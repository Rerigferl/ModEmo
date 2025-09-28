using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using nadena.dev.modular_avatar.core;
using Numeira.Animation;
using UnityEngine;

namespace Numeira;

internal sealed class ModEmoData
{
    private const string UncategorizedGroupName = "Uncategorized";

    public SkinnedMeshRenderer Face { get; }

    public List<ExpressionData>? Expressions { get; set; }

    public List<KeyValuePair<string, List<string>>> CategorizedBlendShapes { get; }

    public ImmutableDictionary<string, BlendShapeInfo> BlendShapes { get; }

    public HashSet<AvatarParameter> Parameters { get; } = new(AvatarParameter.ParameterNameEqualityComparer.Instance);

    public MotionBuilder BlankClip { get; } = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath("3107326e8ebb7da42981f107a7207199"));

    public ImmutableHashSet<string>? GeneratedBlendshapeControls { get; set; }

    public ImmutableHashSet<string> UsageBlendShapeMap { get; }

    internal static ModEmoData Init(BuildContext context) => new(context);

    private ModEmoData(BuildContext context)
    {
        var component = context.GetModEmoContext().Root;
        Face = component.GetFaceRenderer() ?? throw new MissingReferenceException("Face object is missing");

        var mesh = Face.sharedMesh;

        (CategorizedBlendShapes, BlendShapes) = GetCategorizedBlendShapes(component) ?? default;

        List<IModEmoExpression> expressions = new();
        foreach(var x in component.ExportExpressions())
        {
            expressions.Add(x.Key);
            foreach(var y in x)
            {
                expressions.Add(y);
            }
        }

        if (component.GetBlinkExpression() is { } blink)
            expressions.Add(blink);

        UsageBlendShapeMap = 
            expressions.SelectMany(x => x.Frames.SelectMany(x => x.GetBlendShapes()))
            .Where(x => BlendShapes.TryGetValue(x.Name, out var value) && value.Value != x.Value)
            .Select(x => x.Name)
            .ToImmutableHashSet();
    }

    public static ImmutableDictionary<string, BlendShapeInfo> GetBlendShapeInfos(SkinnedMeshRenderer? renderer)
    {
        if (renderer is null)
            return ImmutableDictionary<string, BlendShapeInfo>.Empty;

        var mesh = renderer.sharedMesh;
        int count = mesh.blendShapeCount;

        Dictionary<string, BlendShapeInfo> info = new();
        for (int i = 0; i < count; i++)
        {
            var name = mesh.GetBlendShapeName(i);
            info.TryAdd(name, new(renderer, i));
        }
        return info.ToImmutableDictionary();
    }

    public static (List<KeyValuePair<string, List<string>>> CategorizedBlendShapeNames, ImmutableDictionary<string, BlendShapeInfo> BlendShapeInfos)? GetCategorizedBlendShapes(ModEmo component)
    {
        var face = component.GetFaceRenderer();
        var mesh = face?.sharedMesh;
        if (face == null || mesh == null)
            return default;

        List<KeyValuePair<string, List<string>>> groups = new();
        string currentGroup = $"{UncategorizedGroupName}";
        List<string> currentGroupList = new();

        groups.Add(KeyValuePair.Create(currentGroup, currentGroupList));

        Dictionary<string, BlendShapeInfo> blendShapes = new();

        var regex = new Regex(component.Settings.SeparatorStringRegEx, RegexOptions.CultureInvariant);
        int count = mesh.blendShapeCount;
        for (int i = 0; i < count; i++)
        {
            var name = mesh.GetBlendShapeName(i);
            if (regex.IsMatch(name))
            {
                currentGroup = $"{regex.Replace(name, "")}";
                currentGroupList = new();
                groups.Add(KeyValuePair.Create(currentGroup, currentGroupList));
                continue;
            }

            blendShapes.TryAdd(name, new(face, i));
            currentGroupList.Add(name);
        }

        return (groups, blendShapes.ToImmutableDictionary());

    }
}
