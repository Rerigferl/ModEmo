using System.Collections.Immutable;
using System.Threading.Tasks;
using nadena.dev.ndmf.preview;
using Numeira.Animation;

namespace Numeira;

internal sealed class ExpressionPreview : IRenderFilter
{
    public static float PreviewTime { get; set; } = 0;
    public static PublishedValue<string?> TemporaryPreviewBlendShape { get; } = new(null);

    public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
    {
        var result = Iterate(context).ToImmutableList();
        
        return result;
        static IEnumerable<RenderGroup> Iterate(ComputeContext context)
        {
            foreach(var root in context.GetAvatarRoots())
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
    }

    public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
    {
        return Task.FromResult<IRenderFilterNode>(new Node(group, proxyPairs, context));
    }

    internal sealed class Node : IRenderFilterNode
    {
        public RenderAspects WhatChanged => RenderAspects.Shapes;

        private readonly ModEmo rootComponent;
        private readonly ImmutableDictionary<string, BlendShapeInfo> blendShapeInfos;
        private readonly Renderer originalRenderer;
        private IModEmoExpression? selectedExpression;
        private DateTime selectionChangedTime;
        private AnimationClip? animaton;
        private IDisposable? sceneReflesher;

        private Dictionary<string, int> blendShapeIndexCache = new();

        public Node(RenderGroup renderGroup, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            originalRenderer = proxyPairs.FirstOrDefault().Item1;
            blendShapeInfos = ModEmoData.GetBlendShapeInfos(renderGroup.Renderers[0] as SkinnedMeshRenderer);
            rootComponent = renderGroup.GetData<ModEmo>();

            context.Observe(TemporaryPreviewBlendShape);

            foreach (var x in context.GetComponentsInChildren<IModEmoExpressionFrameProvider>(rootComponent.gameObject, true))
            {
                context.Observe(x.Component, x =>
                {
                    var hashCode = new DeterministicHashCode();
                    (x as IModEmoComponent)?.CalculateContentHash(ref hashCode);
                    return hashCode.ToHashCode();
                });
            }
        }

        public Node(Node source, ComputeContext context)
        {
            originalRenderer = source.originalRenderer;
            blendShapeInfos = source.blendShapeInfos;
            rootComponent = source.rootComponent;

            context.Observe(TemporaryPreviewBlendShape);

            foreach (var x in context.GetComponentsInChildren<IModEmoExpressionFrameProvider>(rootComponent.gameObject, true))
            {
                context.Observe(x.Component, x =>
                {
                    var hashCode = new DeterministicHashCode();
                    (x as IModEmoComponent)?.CalculateContentHash(ref hashCode);
                    return hashCode.ToHashCode();
                });
            }
        }

        private int GetBlendShapeIndex(Mesh mesh, string name)
        {
            if (blendShapeIndexCache.TryGetValue(name, out var index)) return index;
            index = mesh.GetBlendShapeIndex(name);
            blendShapeIndexCache.Add(name, index);
            return index;
        }

        public void OnFrame(Renderer original, Renderer proxy)
        {
            if (proxy is not SkinnedMeshRenderer smr || smr.sharedMesh is not { } mesh || mesh == null)
                return;

            if (TemporaryPreviewBlendShape.Value != null)
            {
                int index = GetBlendShapeIndex(mesh, TemporaryPreviewBlendShape.Value);
                if (index != -1)
                {
                    smr.SetBlendShapeWeight(index, 100);
                }
            }

            if (selectedExpression is not { } expression)
                return;

            if (animaton is not { } clip || clip == null)
                return;

            static IEnumerable<(string PropertyName, float Value)> Sample(AnimationClip clip, float normalizedTime)
            {
                float time = normalizedTime * clip.length;

                foreach (var bind in AnimationUtility.GetCurveBindings(clip))
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, bind);
                    if (curve == null)
                        continue;
                    float value = curve.Evaluate(time);

                    var propertyName = bind.propertyName;
                    yield return ($"{propertyName}", value);
                }
            }

            float time = (float)(DateTime.Now - selectionChangedTime).TotalSeconds;
            if (selectedExpression.IsLoop)
            {
                time = (time * 0.5f) % 1;
            }
            else
            {
                time = (Math.Clamp((float)Math.Sin(time), -0.2f, 0.2f) + 0.2f) / 0.4f;
            }

            foreach (var (name, value) in Sample(clip, (float)time))
            {
                int index = GetBlendShapeIndex(mesh, name);
                if (index == -1)
                    continue;
                smr.SetBlendShapeWeight(index, value);
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

        public void OnFrameGroup()
        {
            var selectedExpression = GetSelectedExpression();

            if (selectedExpression != this.selectedExpression)
            {
                selectionChangedTime = DateTime.Now;
                sceneReflesher?.Dispose();
                sceneReflesher = null;

                if (animaton != null)
                    Object.DestroyImmediate(animaton);

                if (selectedExpression != null)
                {
                    animaton = selectedExpression.MakeAnimationClip(blendShapeInfos, null, writeDefault: false, previewMode: true).ToMotion(AssetContainer.Empty) as AnimationClip;
                    if (selectedExpression.Frames.Count() > 1)
                    sceneReflesher = SceneViewReflesher.BeginReflesh();
                }
            }    
            
            this.selectedExpression = selectedExpression;

            IModEmoExpression? GetSelectedExpression()
            {
                var active = Selection.activeGameObject;
                if (active == null) 
                    return null;

                if (active.GetComponentInParent<IModEmoExpression>() is not { } expression)
                    return null;

                if (expression.Component!.GetComponentInParent<ModEmo>() != rootComponent)
                    return null;

                return expression;
            }
        }

        public void Dispose()
        {
            if (animaton != null)
                Object.DestroyImmediate(animaton);

            sceneReflesher?.Dispose();
        }
    }
}

internal static class SceneViewReflesher
{
    private static Stack<bool> refleshStack = new();

    static SceneViewReflesher()
    {
        EditorApplication.update += () =>
        {
            if (refleshStack.Count == 0)
                return;

            if (!EditorApplication.isFocused)
                return;

            SceneView.RepaintAll();
        };
    }

    public static IDisposable BeginReflesh()
    {
        refleshStack.Push(true);
        return RefleshDisposer.Instance;
    }

    private sealed class RefleshDisposer : IDisposable
    {
        public static readonly RefleshDisposer Instance = new();
        public void Dispose()
        {
            refleshStack.Pop();
        }
    }
}