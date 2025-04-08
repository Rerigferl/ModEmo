
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using nadena.dev.ndmf.util;
using UnityEngine;

namespace Numeira;

internal sealed class ModEmoExpressionGenerator
{
    public ModEmo ModEmo { get; }
    public SkinnedMeshRenderer FaceSMR { get; }
    public Object AssetContainer { get; }
    private DirectBlendTree DirectBlendTree { get; }

    private AnimatorController animator = null!;

    public ModEmoExpressionGenerator(ModEmo modEmo, Object assetContainer)
    {
        ModEmo = modEmo;
        FaceSMR = ModEmo.Settings.Face.Get(modEmo)?.GetComponent<SkinnedMeshRenderer>()!;
        AssetContainer = assetContainer;
        DirectBlendTree = new() { DirectBlendParameter = "1", Name = "ModEmo" };
    }

    public AnimatorController Build()
    {
        var animator = this.animator = new AnimatorController() { name = "ModEmo" };

        CreateBlendShapeAnimations();

        var compiledTree = DirectBlendTree.Build(AssetContainer);
        animator.AddLayer("ModEmo DirectBlendTree");
        animator.layers[^1].defaultWeight = 1;
        animator.layers[^1].stateMachine.AddState(compiledTree.name).motion = compiledTree;
        
        foreach(var param in DirectBlendTree.GetAnimatorParameters().Distinct(AnimatorParameter.ParameterNameEqualityComparer.Instance))
        {
            animator.AddParameter(param.ToUnityParameter());
        }

        return animator;
    }

    private void CreateBlendShapeAnimations()
    {
        var usageMap = ModEmo.ExportExpressions().SelectMany(x => x).SelectMany(x => x.BlendShapes).Select(x => x.Name).ToHashSet();
        var root = DirectBlendTree.AddDirectBlendTree("BlendShape");
        var faceInfo = new FaceInfo(ModEmo);
        foreach (var (key, shapes) in faceInfo.CategorizedBlendShapes.OrderBy(x => x.Key))
        {
            foreach (var (name, blendShape) in shapes)
            {
                if (!usageMap.Contains(name))
                    continue;

                var category = key[key.IndexOf(" ")..];
                var cat = root.Find<DirectBlendTree>(category);
                cat ??= root.AddDirectBlendTree(category);

                var zero = new AnimationClip() { name = $"{name} Min" };
                var one = new AnimationClip() { name = $"{name} Max" };

                var bind = new EditorCurveBinding() { path = ModEmo.Settings.Face.referencePath, propertyName = $"blendShape.{name}", type = typeof(SkinnedMeshRenderer) };
                AnimationUtility.SetEditorCurve(zero, bind, AnimationCurve.Constant(0, 0, 0));
                AnimationUtility.SetEditorCurve(one, bind, AnimationCurve.Constant(0, 0, blendShape.Max));

                var tree = cat.AddBlendTree(name);
                tree.AddMotion(zero);
                tree.AddMotion(one);

                tree.BlendParameter = $"ModEmo/BlendShape/{name}";
            }
        }
    }

    public sealed class FaceInfo
    {
        private const string UncategorizedGroupName = "Uncategorized";
        public FaceInfo(ModEmo modEmo)
        {
            var faceGo = modEmo.Settings.Face.Get(modEmo);
            Face = faceGo.GetComponent<SkinnedMeshRenderer>();

            var mesh = Face.sharedMesh;

            int count = mesh.blendShapeCount;
            Dictionary<string, Dictionary<string, BlendShapeInfo>> info = new();
            string currentGroup = $"0 {UncategorizedGroupName}";
            var regex = new Regex(modEmo.Settings.SeparatorStringRegEx, RegexOptions.CultureInvariant);
            int groupCount = 1;
            for (int i = 0; i < count; i++)
            {
                var name = mesh.GetBlendShapeName(i);
                if (regex.IsMatch(name))
                {
                    currentGroup = $"{groupCount++} {regex.Replace(name, "")}";
                    continue;
                }

                info.GetOrAdd(currentGroup, _ => new()).TryAdd(name, new(Face, i));
            }
            CategorizedBlendShapes = info.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutableDictionary());
        }

        public SkinnedMeshRenderer Face { get; }
        public Mesh Mesh => Face.sharedMesh;

        public ImmutableDictionary<string, ImmutableDictionary<string, BlendShapeInfo>> CategorizedBlendShapes { get; } = null!;

        public readonly struct BlendShapeInfo
        {
            internal BlendShapeInfo(SkinnedMeshRenderer face, int index)
            {
                Value = face.GetBlendShapeWeight(index);
                Max = face.sharedMesh.GetBlendShapeFrameWeight(index, 0);
            }

            public readonly float Value;
            public readonly float Max;
        }
    }

    private void GenerateAnimations()
    {
    }
}