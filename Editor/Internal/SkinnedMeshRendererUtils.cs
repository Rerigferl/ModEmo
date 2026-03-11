using System.Runtime.InteropServices;

namespace Numeira;

internal static class SkinnedMeshRendererUtils
{
    public static BlendshapeWeights GetBlendshapeWeights(this SkinnedMeshRenderer renderer) => new(renderer);

    public readonly ref struct BlendshapeWeights
    {
        public BlendshapeWeights(SkinnedMeshRenderer renderer)
        {
            Renderer = renderer;
#if !UNITY_2022_3_22

            var mesh = renderer.sharedMesh;

            int count = mesh.blendShapeCount;
            blendshapeWeightCount = count;

            var array = ArrayPool<float>.Shared.Rent(count);
            var span = array.AsSpan(0, count);
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = renderer.GetBlendShapeWeight(i);
            }
            buffer = array;
#endif
        }

        public SkinnedMeshRenderer Renderer { get; }

        public ReadOnlySpan<float> Span => GetSpanInternal();

#if UNITY_2022_3_22
        private unsafe ReadOnlySpan<float> GetSpanInternal()
        {
            var ptr = (byte*)Unsafe.As<UnityObject>(Renderer).Pointer.ToPointer();

            var arrayPtr = *(float**)(ptr + 0x2f0);
            var count = *(ulong*)(ptr + 0x308);

            return new(arrayPtr, (int)count);
        }

        private sealed class UnityObject
        {
            public IntPtr Pointer;
        }

        public void Dispose()
        {
        }
#else
        private readonly float[]? buffer;
        private readonly int blendshapeWeightCount;

        private ReadOnlySpan<float> GetSpanInternal()
        {
            return buffer.AsSpan(0, blendshapeWeightCount);
        }

        public void Dispose()
        {
            if (buffer == null)
                return;

            ArrayPool<float>.Shared.Return(buffer);
        }
#endif
    }
}

internal static unsafe class UnsafeMeshUtils
{
    public static ReadOnlySpan<BlendShapeVertex> GetBlendshapeVerticies(Mesh mesh, int index)
    {
        var data = GetBlendShapeData(mesh);
        var span = data.Vertex;
        var shape = data.Shapes[index];
        return span.Slice(shape.FirstVertex, shape.VertexCount);
    }

    public static bool IsMarkerBlendShape(Mesh mesh, int index, int vertexCount = 2, float threshold = 5e-08f)
    {
        var data = GetBlendShapeData(mesh);
        var shape = data.Shapes[index];
        
        if (shape.VertexCount == 0)
            return true;

        if (shape.VertexCount <= vertexCount)
        {
            // まれにマーカーシェイプキーが2頂点動かしてるアバターがいる?
            // エクちゃんとしなのさんで確認

            foreach(var vertex in data.Vertex.Slice(shape.FirstVertex, shape.VertexCount))
            {
                if (vertex.Vertex.sqrMagnitude > threshold)
                    return false;
            }

            return true;
        }

        return false;
    }

    public static ref BlendShapeData GetBlendShapeData(Mesh mesh)
    {
        var ptr = (byte*)Unsafe.As<UnityObject>(mesh).Pointer.ToPointer();
        var x = *(nint*)(ptr + 0x98);
        return ref Unsafe.AsRef<BlendShapeData>(*(void**)(x + 0x1d8));
    }

    private sealed class UnityObject
    {
        public IntPtr Pointer;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BlendShapeData
    {
        private Vector<BlendShapeVertex> vertex;
        private Vector<MeshBlendShape> shapes;
        private Vector<BlendShapeChannel> channels;
        private Vector<float> fullWeights;

        public readonly Span<BlendShapeVertex> Vertex => new(vertex.Pointer, vertex.Count);
        public readonly Span<MeshBlendShape> Shapes => new(shapes.Pointer, shapes.Count);
        public readonly Span<BlendShapeChannel> Channels => new(channels.Pointer, channels.Count);
        public readonly Span<float> FullWeights => new(fullWeights.Pointer, fullWeights.Count);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BlendShapeVertex
    {
        public int Index;
        public Vector3 Vertex;
        public Vector3 Normal;
        public Vector3 Tangent;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MeshBlendShape
    {
        public int FirstVertex;
        public int VertexCount;
        public bool HasNormals;
        public bool HasTangents;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BlendShapeChannel
    {
        public nuint Name;
        public uint NameHash;
        public int FrameIndex;
        public int FrameCount;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x28)]
    private unsafe struct Vector<T> where T : unmanaged
    {
        public T* Pointer;

        private nint reserved1;
        private nint reserved2;

        public int Count;

        public readonly Span<T> AsSpan() => new(Pointer, Count);
    }
}