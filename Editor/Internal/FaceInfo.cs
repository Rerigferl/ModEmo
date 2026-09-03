using System.Collections.Immutable;

namespace Numeira;

internal sealed class FaceInfo
{
    public SkinnedMeshRenderer Renderer { get; }
    public ReadOnlySpan<BlendshapeInfo> BlendShapes => blendShapes;
    public ImmutableDictionary<string, ReadOnlyMemory<BlendshapeInfo>> GroupedBlendShapes { get; }
    public ImmutableDictionary<string, BlendshapeInfo> BlendshapeMap { get; }

    private readonly BlendshapeInfo[] blendShapes;

    public FaceInfo(ModEmo component)
    {
        var renderer = component.GetFaceRenderer();
        if (renderer == null)
            throw new Exception("Face is missing");

        Renderer = renderer;
        var mesh = renderer.sharedMesh;
        var count = mesh.blendShapeCount;

        var blendShapes = new List<BlendshapeInfo>(count);
        var groupRangeMaps = new Dictionary<string, Range>();

        string? currentGroup = null;
        int groupStartIndex = 0;

        for (int i  = 0; i < count; i++)
        {
            var name = mesh.GetBlendShapeName(i);
            bool isMarkerBlendshape = component.Settings.MarkerDetectMode switch
            {
                MarkerBlendShapeDetectMode.ByName => component.Settings.SeparatorStringRegEx.IsMatch(name),
                MarkerBlendShapeDetectMode.ByVertex => UnsafeMeshUtils.IsMarkerBlendShape(mesh, i, component.Settings.MarkerBlendshapeVertexCountThreshold, component.Settings.MarkerBlendshapeThreshold) && !IsReservedBlendShape(name),
                _ => false,
            };

            if (isMarkerBlendshape)
            {
                groupRangeMaps.TryAdd(currentGroup ?? component.Settings.DefaultGroupName, new Range(groupStartIndex, blendShapes.Count));
                groupStartIndex = blendShapes.Count;
                currentGroup = component.Settings.SeparatorStringRegEx.Replace(name, "");
            }
            else
            {
                blendShapes.Add(new(renderer, currentGroup, name, i, mesh.GetBlendShapeFrameWeight(i, 0)));
            }
        }
        groupRangeMaps.TryAdd(currentGroup ?? component.Settings.DefaultGroupName, new Range(groupStartIndex, blendShapes.Count));

        this.blendShapes = blendShapes.ToArray();
        GroupedBlendShapes = groupRangeMaps.ToImmutableDictionary(x => x.Key, x =>
        {
            var array = this.blendShapes;
            var (offset, length) = x.Value.GetOffsetAndLength(array.Length);
            return (ReadOnlyMemory<BlendshapeInfo>)array.AsMemory(offset, length);
        });
        BlendshapeMap = this.blendShapes.ToImmutableDictionary(x => x.Name, x => x);
    }

    private bool IsReservedBlendShape(string name)
    {
        return name.StartsWith("vrc.", StringComparison.OrdinalIgnoreCase);
    }

    public string? RegisterControlBlendshape(string name, BlendshapeControlType type, int layer = 0)
    {
        if (!this.BlendshapeMap.TryGetValue(name, out var info))
            return null;

        if (type is BlendshapeControlType.Normal)
        {
            info.UsageInfo.ControlGateLayers[layer] = true;
        }
        else if (type is BlendshapeControlType.Cancel)
        {
            info.UsageInfo.CancelGateLayers[layer] = true;
        }

        DefaultInterpolatedStringHandler handler = new(0, 0, null, stackalloc char[128]);
        handler.AppendLiteral(ParameterNames.Internal.BlendShapes.Prefix);
        handler.AppendFormatted(name);

        handler.AppendLiteral("/");
        handler.AppendFormatted(type switch
        {
            BlendshapeControlType.Normal => "Value",
            BlendshapeControlType.Cancel => "Cancel",
            _ => "",
        });
        handler.AppendLiteral("/");
        handler.AppendFormatted(layer);

        var result = handler.ToStringAndClear();

        Debug.Log($"[ModEmo] Register control blendshape: {name} -> {result}");

        return result;
    }

    public sealed class BlendshapeInfo
    {
        private readonly SkinnedMeshRenderer renderer;

        internal BlendshapeInfo(SkinnedMeshRenderer renderer, string? group, string name, int index, float max)
        {
            this.renderer = renderer;
            Group = group;
            Name = name;
            Index = index;
            Max = max;
        }

        public string? Group { get; }
        public string Name { get; }
        public int Index { get; }
        public float Max { get; }

        public float Value
        {
            get
            {
                return renderer.GetBlendShapeWeight(Index);
            }
        }

        public ref BlendshapeUsageInfo UsageInfo => ref usageInfo;
        private BlendshapeUsageInfo usageInfo;

        public override string ToString()
        {
            return $"({Index}) {Name}";
        }

        public struct BlendshapeUsageInfo
        {
            public readonly bool UseCancelGate => !CancelGateLayers.IsDefault;
            public bool UseEnableGate;
            public readonly bool UseControlGate => !ControlGateLayers.IsDefault;
            public bool UseOverrideGate;

            public BitFlags<uint> CancelGateLayers;
            public BitFlags<uint> ControlGateLayers;

            public readonly bool AllowControl => UseControlGate || UseOverrideGate;
        }
    }
}
