﻿using System.Collections.Immutable;
using System.Text.RegularExpressions;
using nadena.dev.modular_avatar.core;

namespace Numeira;

internal sealed class ModEmoData
{
    private const string UncategorizedGroupName = "Uncategorized";


    public SkinnedMeshRenderer Face { get; }

    public ImmutableDictionary<string, ImmutableDictionary<string, BlendShapeInfo>> CategorizedBlendShapes { get; }

    public HashSet<AnimatorParameter> Parameters { get; } = new(AnimatorParameter.ParameterNameEqualityComparer.Instance);


    internal static ModEmoData Init(BuildContext context) => new(context.GetModEmoContext().Root);

    private ModEmoData(ModEmo component)
    {
        Face = 
            component.Settings.Face.Get(component)?.GetComponent<SkinnedMeshRenderer>() 
            ?? throw new MissingReferenceException("Face object is missing");

        var mesh = Face.sharedMesh;

        int count = mesh.blendShapeCount;
        Dictionary<string, Dictionary<string, BlendShapeInfo>> info = new();
        string currentGroup = $"0 {UncategorizedGroupName}";
        var regex = new Regex(component.Settings.SeparatorStringRegEx, RegexOptions.CultureInvariant);
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
