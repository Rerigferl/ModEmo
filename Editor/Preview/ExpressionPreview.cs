using System.Collections.Immutable;
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

    public static float PreviewTime { get; set; } = 0;
    public static bool AutoPlay { get; set; } = false;

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

        private readonly ComputeContext context;
        private readonly ModEmo rootComponent;
        private readonly ImmutableDictionary<string, BlendShapeInfo> blendShapeInfos;
        private readonly Renderer originalRenderer;
        private IModEmoExpression? selectedExpression;
        private DateTime selectionChangedTime;
        private IDisposable? sceneReflesher;

        private readonly Dictionary<string, int> blendShapeIndexCache = new();

        public Node(RenderGroup renderGroup, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            this.context = context;
            originalRenderer = proxyPairs.FirstOrDefault().Item1;
            blendShapeInfos = ModEmoData.GetBlendShapeInfos(renderGroup.Renderers[0] as SkinnedMeshRenderer);
            rootComponent = renderGroup.GetData<ModEmo>();
        }

        public Node(Node source, ComputeContext context)
        {
            this.context = context;
            originalRenderer = source.originalRenderer;
            blendShapeInfos = source.blendShapeInfos;
            rootComponent = source.rootComponent;
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
            if (proxy is not SkinnedMeshRenderer smr || original is not SkinnedMeshRenderer origSmr || smr.sharedMesh is not { } mesh || mesh == null)
                return;

            if (selectedExpression is not { } expression)
                return;

            float time = (float)(DateTime.Now - selectionChangedTime).TotalSeconds;
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

            var blendShapes = selectedExpression.BlendShapes;
            foreach(var shape in blendShapes)
            {
                int index = GetBlendShapeIndex(mesh, shape.Name);
                if (index == -1)
                    continue;

                var lastTime = shape.Value.keys[shape.Value.length - 1].time;
                var value = shape.Value.Evaluate(time * lastTime);

                if (shape.Cancel)
                {
                    float orig = origSmr.GetBlendShapeWeight(index);
                    var weight = value / 100f;
                    value = orig * (1 - weight);
                }

                smr.SetBlendShapeWeight(index, value);
            }

            if (TemporaryPreviewBlendShape.Value != null)
            {
                int index = GetBlendShapeIndex(mesh, TemporaryPreviewBlendShape.Value);
                if (index != -1)
                {
                    smr.SetBlendShapeWeight(index, 100);
                }
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

                if (selectedExpression != null)
                {
                    if (selectedExpression.BlendShapes.Select(x => x.Value.length).MaxOrDefault() > 1)
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
            sceneReflesher?.Dispose();
        }
    }
}
