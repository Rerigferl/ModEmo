using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using nadena.dev.ndmf.preview;
using UnityEngine;

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
    public static PublishedValue<IModEmoExpression?> PreviewTarget { get; } = new(null, "numeira.mod-emo.expression-preview.preview-target");

    private static PropCache<int, ImmutableList<RenderGroup>> RendererCache { get; } = new("numeira.mod-emo.expression-preview.renderer-cache", static (context, go) =>
    {
        var result = Iterate(context).OrderBy(x => x.GetData<ModEmo>().GetInstanceID()).ToImmutableList();
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

                yield return RenderGroup.For(renderer).WithData(component);
            }
            yield break;
        }
    }, (left, right) => left.SequenceEqual(right));

    private readonly static PropCache<int, GameObject?> SelectionCache = new("numeira.mod-emo.expression-preview.selection-cache", (context, _) => context.Observe(SelectionMonitor.Active, x => x, (x, y) => x == y), (x, y) => x == y);
    private readonly static PropCache<int, IModEmoExpression?> SelectedExpression = new("numeira.mod-emo.expression-preview.selected-expression", (context, _) =>
    {
        if (context.Observe(PreviewTarget, x => x, (x, y) => x == y) is { } locked)
            return locked;

        var active = SelectionCache.Get(context, 0);
        if (active == null)
            return null;

        if (active.GetComponentInParent<IModEmoExpression>() is not { } expression)
            return null;

        return expression;
    }, (x, y) => x == y);

    public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
    {
        return RendererCache.Get(context, 0);
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
        private readonly IModEmoExpression? selectedExpression;
        private readonly DateTime selectionChangedTime;
        private IDisposable? sceneReflesher;

        private static readonly PreviewWriter previewWriter = new();

        public Node(RenderGroup renderGroup, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            this.context = context;
            originalRenderer = proxyPairs.FirstOrDefault().Item1;
            rootComponent = renderGroup.GetData<ModEmo>();

            selectionChangedTime = DateTime.Now;
            selectedExpression = SelectedExpression.Get(context, 0);
        }

        public Node(Node source, ComputeContext context)
        {
            this.context = context;
            originalRenderer = source.originalRenderer;
            rootComponent = source.rootComponent;
            selectionChangedTime = DateTime.Now;
            selectedExpression = SelectedExpression.Get(context, 0);

        }

        public void OnFrame(Renderer original, Renderer proxy)
        {
            if (proxy is not SkinnedMeshRenderer smr || original is not SkinnedMeshRenderer origSmr || smr.sharedMesh is not { } mesh || mesh == null)
                return;

            if (selectedExpression is not { } expression)
                return;

            if (expression.Component!.GetComponentInParent<ModEmo>() != rootComponent)
                return;

            float time = (float)(DateTime.Now - selectionChangedTime).TotalSeconds - 1;
            if (selectedExpression.IsLoop)
            {
                time = (time * 0.5f) % 1;
            }
            else
            {
                time = (Math.Clamp((float)Math.Sin(time), -0.2f, 0.2f) + 0.2f) / 0.4f;
            }

            if (!AutoPlay)
                time = PreviewTime;

            previewWriter.Renderer = smr;
            previewWriter.Reset();
            selectedExpression.CollectAnimation(previewWriter, default);

            if (sceneReflesher == null)
            {
                if (previewWriter.Curves.Select(x => x.Value.Length).MaxOrDefault() > 1)
                    sceneReflesher = SceneViewReflesher.BeginReflesh();
            }

            foreach (var kvp in previewWriter.Curves)
            {
                var (index, curve) = kvp;
                if (curve.Length == 0)
                    continue;

                bool isCancel = index < 0;
                if (isCancel)
                    index = ~index;

                var lastTime = curve.Keys.Select(x => x.Time).MaxOrDefault();
                var value = curve.Evaluate(time * lastTime);

                if (isCancel)
                {
                    float orig = origSmr.GetBlendShapeWeight(index);
                    var weight = value / 100f;
                    value = orig * (1 - weight);
                }

                smr.SetBlendShapeWeight(index, value);
            }

            if (TemporaryPreviewBlendShape.Value != null)
            {
                if (previewWriter.GetBlendShapeIndex(TemporaryPreviewBlendShape.Value) is {} index)
                    smr.SetBlendShapeWeight(index, 100);
            }
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

        private sealed class PreviewWriter : BlendshapeCollector
        {
            private readonly Dictionary<uint, int> blendShapeIndexCache = new();

            public SkinnedMeshRenderer? Renderer { get; set; }

            public IEnumerable<KeyValuePair<int, Curve>> Curves => curves;

            private readonly Dictionary<int, Curve> curves = new();

            public void Reset()
            {
                foreach (var x in curves.Values)
                    x.Reset();
            }

            protected override void WriteDefaultValue(AnimationBinding binding, float value)
            {
                var name = GetTargetBlendshapeName(binding, out _);
                if (name.IsEmpty)
                    return;

                if (GetBlendShapeIndex(name) is not {} index)
                    return;

                curves.GetOrAdd(index, _ => new()).AddKey(default, false);

            }

            protected override void WriteWithBlendshape(AnimationBinding binding, Curve.Keyframe keyframe, ReadOnlySpan<char> blendShapeName, bool isCancel)
            {
                if (GetBlendShapeIndex(blendShapeName) is not {} index)
                    return;
                
                if (isCancel)
                    index = ~index;

                curves.GetOrAdd(index, _ => new()).AddKey(keyframe);
            }

            public int? GetBlendShapeIndex(ReadOnlySpan<char> name)
            {
                if (Renderer is not { } renderer || renderer.sharedMesh is not { } mesh)
                    return null;

                var nameHash = FarmHash.Hash32(MemoryMarshal.AsBytes(name));
                if (!blendShapeIndexCache.TryGetValue(nameHash, out var index))
                {
                    index = mesh.GetBlendShapeIndex(name.ToString());
                    blendShapeIndexCache.Add(nameHash, index);
                }

                return index;
            }
        }
    }
}
