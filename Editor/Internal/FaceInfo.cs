using System.Collections.Immutable;

namespace Numeira;

internal sealed class FaceInfo
{
    private const string UncategorizedGroupName = "Uncategorized";

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
            if (UnsafeMeshUtils.IsMarkerBlendShape(mesh, i) && !IsReservedBlendShape(name))
            {
                groupRangeMaps.TryAdd(currentGroup ?? UncategorizedGroupName, new Range(groupStartIndex, blendShapes.Count));
                groupStartIndex = blendShapes.Count;
                currentGroup = component.Settings.SeparatorStringRegEx.Replace(name, "");
            }
            else
            {
                blendShapes.Add(new(renderer, currentGroup, name, i, mesh.GetBlendShapeFrameWeight(i, 0)));
            }
        }
        groupRangeMaps.TryAdd(currentGroup ?? UncategorizedGroupName, new Range(groupStartIndex, blendShapes.Count));

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
                using var x = renderer.GetBlendshapeWeights();
                return x.Span[Index];
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
            public bool UseCancelGate;
            public bool UseEnableGate;
            public bool UseControlGate;
            public bool UseOverrideGate;

            public readonly bool AllowControl => UseControlGate | UseOverrideGate;
        }
    }
}
