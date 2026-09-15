using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using nadena.dev.ndmf.preview;
using Numeira.Animation;

namespace Numeira;

internal sealed class ExpressionPreview : IRenderFilter
{
    static ExpressionPreview()
    {
        TemporaryPreviewBlendShape.OnChange += _ => SceneView.RepaintAll();
    }

    static TogglablePreviewNode EnableNode = TogglablePreviewNode.Create(
        () => "Expression",
        qualifiedName: "numeira.mod-emo/ExpressionPreview",
        true
    );

    public IEnumerable<TogglablePreviewNode> GetPreviewControlNodes()
    {
        yield return EnableNode;
    }

    public bool IsEnabled(ComputeContext context)
    {
        return context.Observe(EnableNode.IsEnabled);
    }

    public static float PreviewTime
    {
        get => ModEmoPreviewPrefs.instance.FrameTime;
        set => ModEmoPreviewPrefs.instance.FrameTime = value;
    }

    public static bool AutoPlay
    {
        get => ModEmoPreviewPrefs.instance.AutoPlay;
        set => ModEmoPreviewPrefs.instance.AutoPlay = value;
    }

    public static PublishedValue<string?> TemporaryPreviewBlendShape { get; } = new(null);

    private readonly static PropCache<int, GameObject?> SelectionCache = new("numeira.mod-emo.expression-preview.selection-cache", (context, _) => context.Observe(SelectionMonitor.Active, x => x, (x, y) => x == y), (x, y) => x == y);
    private static PropCache<ModEmo, IPreviewable?> SelectedExpression { get; } = new("numeira.mod-emo.expression-preview.selected-expression", (context, component) =>
    {
        var active = SelectionCache.Get(context, 0);
        if (active != null && active.GetComponentInParent<IPreviewable>() is { } expression)
            return expression;

        if (context.Observe(component, x => x.Settings.PreviewExpression?.Get(x), (x, y) => x == y) is { } defaultPreview)
        {
            if (context.GetComponent<IPreviewable>(defaultPreview) is { } exp)
                return exp;
        }

        return null;
    }, (x, y) => x == y);

    public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
    {
        var result = Iterate(context).ToImmutableList();
        return result;

        static IEnumerable<RenderGroup> Iterate(ComputeContext context)
        {
            foreach (var root in context.GetAvatarRoots())
            {
                if (!context.ActiveInHierarchy(root))
                    continue;

                var component = context.GetComponentsInChildren<ModEmo>(root, true).FirstOrDefault(x => context.ActiveAndEnabled(x));
                if (component == null)
                    continue;

                var renderer = context.Observe(component, x => x.GetFaceRenderer());
                if (renderer == null)
                    continue;

                yield return RenderGroup.For(renderer).WithData(component, (x, y) => x.GetHashCode() == y.GetHashCode());
            }
            yield break;
        }
    }

    public async Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
    {
        return new Node(group, proxyPairs, context);
    }

    internal sealed class Node : IRenderFilterNode
    {
        public RenderAspects WhatChanged => RenderAspects.Shapes;

        private readonly ComputeContext context;
        private readonly ModEmo rootComponent;
        private readonly Renderer originalRenderer;
        private readonly IPreviewable? selectedExpression;
        private readonly DateTime selectionChangedTime;
        private IDisposable? sceneReflesher;

        private readonly PreviewRegistry registry = new();

        public Node(RenderGroup renderGroup, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            this.context = context;
            originalRenderer = proxyPairs.FirstOrDefault().Item1;
            rootComponent = renderGroup.GetData<ModEmo>();

            selectionChangedTime = DateTime.Now;
            selectedExpression = SelectedExpression.Get(context, rootComponent);
        }

        public Node(Node source, ComputeContext context)
        {
            this.context = context;
            originalRenderer = source.originalRenderer;
            rootComponent = source.rootComponent;
            selectionChangedTime = DateTime.Now;
            selectedExpression = SelectedExpression.Get(context, rootComponent);
        }

        public void OnFrame(Renderer original, Renderer proxy)
        {
            try
            {
                if (proxy is not SkinnedMeshRenderer smr || original is not SkinnedMeshRenderer origSmr || smr.sharedMesh is not { } mesh || mesh == null)
                    return;

                if (selectedExpression is not { } previewable || previewable == null)
                    return;

                if (previewable.Component!.GetComponentInParent<ModEmo>() != rootComponent)
                    return;

                float time = (float)(DateTime.Now - selectionChangedTime).TotalSeconds - 1;
                if (previewable is IModEmoExpression exp && exp.IsLoop)
                {
                    time = (time * 0.5f) % 1;
                }
                else
                {
                    time = (Math.Clamp((float)Math.Sin(time), -0.2f, 0.2f) + 0.2f) / 0.4f;
                }

                if (!AutoPlay)
                    time = PreviewTime;

                registry.Clear();

                var opt = new AnimationGeneratorOptions() { AnimationName = "", AvatarRootTransform = context.GetAvatarRoot(rootComponent.gameObject).transform, FaceObject = originalRenderer.transform, };
                previewable.RegisterAnimations(registry, opt);

                if (TemporaryPreviewBlendShape.Value != null)
                {
                    var c = registry.RegisterAnimation(opt);
                    c.AddBlendshape(proxy.transform, TemporaryPreviewBlendShape.Value, 0, 100);
                }

                registry.Flush(context, origSmr, smr, time);

                if (sceneReflesher == null)
                {
                    if (registry.HasMultiFrame())
                        sceneReflesher = SceneViewReflesher.BeginReflesh();
                }
            }
            catch {}
        }

        public Task<IRenderFilterNode?> Refresh(IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context, RenderAspects updatedAspects)
        {
            if (originalRenderer == proxyPairs.FirstOrDefault().Item1)
            {
                sceneReflesher?.Dispose();
                sceneReflesher = null;
                return Task.FromResult<IRenderFilterNode?>(new Node(this, context));
            }

            return Task.FromResult<IRenderFilterNode?>(null);
        }

        public void Dispose()
        {
            sceneReflesher?.Dispose();
        }
    }

    private sealed class PreviewRegistry : IAnimationRegistry
    {
        private Context context = new();

        public PropCache<SkinnedMeshRenderer, Mesh> meshCache = new("numeira.mod-emo.expression-preview.meshCahce", (context, smr) => context.Observe(smr, x => x.sharedMesh), (x, y) => x == y);

        public void Clear() => context.Clear();

        public IKeyframeWriterContext RegisterAnimation(in AnimationGeneratorOptions options, string? blendParameter = null)
        {
            context.Renderer = options.FaceObject.GetComponent<SkinnedMeshRenderer>();
            return context;
        }

        //public (IKeyframeWriterContext X, IKeyframeWriterContext Y) RegisterTwoAxisAnimation(in AnimationGeneratorOptions options, string blendParameterX, string blendParameterY) => (BlankContext.Instance, BlankContext.Instance);

        //public IKeyframeWriterContext RegisterMultipleConditionAnimation(in AnimationGeneratorOptions options, params string[] blendParameters) => BlankContext.Instance;

        public void Flush(ComputeContext context, SkinnedMeshRenderer original, SkinnedMeshRenderer proxy, float time)
        {
            var clip = this.context.AnimationClip;
            var mesh = meshCache.Get(context, proxy);

            foreach (var binding in clip.Bindings)
            {
                var index = mesh.GetBlendShapeIndex(binding.propertyName);
                if (index < 0)
                    continue;

                if (clip.Evaluate(binding, time) is not { } value)
                    continue;

                if (binding.type == typeof(SkinnedMeshRenderer))
                {
                    // suru-
                }
                else if (binding.type == typeof(MeshRenderer))
                {
                    var origMesh = meshCache.Get(context, original);
                    var index2 = origMesh.GetBlendShapeIndex(binding.propertyName);
                    if (index2 < 0)
                        continue;

                    float orig = original.GetBlendShapeWeight(index);
                    var weight = value / origMesh.GetBlendShapeFrameWeight(index, 0);
                    value = orig * (1 - weight);
                }

                proxy.SetBlendShapeWeight(index, value);
            }
        }

        public bool HasMultiFrame()
        {
            return context.AnimationClip.Length > 0;
        }

        private sealed class Context : IKeyframeWriterContext
        {
            public AnimationClipBuilder AnimationClip { get; } = new();
            public SkinnedMeshRenderer? Renderer { get; set; }

            private readonly DefaultBlendshapeRecords defaultBlendshapeRecords = new();

            public void Clear()
            {
                defaultBlendshapeRecords.Clear();
                AnimationClip.Clear();
            }

            public void AddBlendshape(Transform target, string name, float time, float value)
            {
                defaultBlendshapeRecords.Add(name, time, BlendshapeControlType.Normal, true);
                AnimationClip.Add(new AnimationBinding(typeof(SkinnedMeshRenderer), "", name), time, value);
            }

            public void AddCancelBlendshape(Transform target, string name, float time, float value)
            {
                defaultBlendshapeRecords.Add(name, time, BlendshapeControlType.Cancel, true);
                AnimationClip.Add(new AnimationBinding(typeof(MeshRenderer), "", name), time, value);
            }

            public void AddRotation(Transform target, float time, Vector3 eularAngle, bool relative = true)
            {
                // TODO!!!
            }

            public void AddBlendshapeDefault(Transform target, string name)
            {
                if (defaultBlendshapeRecords.Contains(name, BlendshapeControlType.Normal, true))
                    return;
                float value = 0;
                if (Renderer?.sharedMesh.GetBlendShapeIndex(name) is { } idx)
                {
                    value = Renderer.GetBlendShapeWeight(idx);
                }

                AddBlendshape(target, name, 0, value);
            }

            public void AddCancelBlendshapeDefault(Transform target, string name)
            {
                if (defaultBlendshapeRecords.Contains(name, BlendshapeControlType.Cancel, true))
                    return;
                AddCancelBlendshape(target, name, 0, 0);
            }

            public void SetAnimatorParameter<T>(string name, float time, T value) { }
            public void SetAvatarParameter<T>(string name, T value) { }
        }
    }
}
