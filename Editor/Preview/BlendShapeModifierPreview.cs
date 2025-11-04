using System.Collections.Immutable;
using System.Threading.Tasks;
using nadena.dev.ndmf.preview;

namespace Numeira;

internal sealed class BlendShapeModifierPreview : IRenderFilter
{
    static TogglablePreviewNode EnableNode = TogglablePreviewNode.Create(
        () => "Existing Blendshape Modifier",
        qualifiedName: "numeira.mod-emo/BlendShapeModifierPreview",
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
        private ModEmoExistingBlendShapeModifier? selectedExpression;

        private readonly Dictionary<string, int> blendShapeIndexCache = new();

        public Node(RenderGroup renderGroup, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            originalRenderer = proxyPairs.FirstOrDefault().Item1;
            blendShapeInfos = ModEmoData.GetBlendShapeInfos(renderGroup.Renderers[0] as SkinnedMeshRenderer);
            rootComponent = renderGroup.GetData<ModEmo>();
        }

        public Node(Node source, ComputeContext context)
        {
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
            if (proxy is not SkinnedMeshRenderer smr || smr.sharedMesh is not { } mesh || mesh == null)
                return;

            if (selectedExpression is not { } modifier)
                return;

            foreach (var x in modifier.GetBlendShapes())
            {
                int index = GetBlendShapeIndex(mesh, x.Name);
                if (index == -1)
                    continue;

                float value = x.Value;
                if (x.Cancel)
                {
                    float orig = smr.GetBlendShapeWeight(index);
                    var weight = value / 100f;
                    value = orig * (1 - weight);
                }

                smr.SetBlendShapeWeight(index, value);
            }

            if (ExpressionPreview.TemporaryPreviewBlendShape.Value != null)
            {
                int index = GetBlendShapeIndex(mesh, ExpressionPreview.TemporaryPreviewBlendShape.Value);
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
                return Task.FromResult<IRenderFilterNode?>(new Node(this, context));
            }

            return Task.FromResult<IRenderFilterNode?>(null);
        }

        public void OnFrameGroup()
        {
            var selectedExpression = GetSelectedExpression();

            if (selectedExpression != this.selectedExpression)
            {
                if (selectedExpression != null)
                {

                }
            }

            this.selectedExpression = selectedExpression;

            ModEmoExistingBlendShapeModifier? GetSelectedExpression()
            {
                var active = Selection.activeGameObject;
                if (active == null)
                    return null;

                if (active.GetComponentInParent<ModEmoExistingBlendShapeModifier>() is not { } expression)
                    return null;

                if (expression.GetComponentInParent<ModEmo>() != rootComponent)
                    return null;

                return expression;
            }
        }

        public void Dispose()
        {

        }
    }
}