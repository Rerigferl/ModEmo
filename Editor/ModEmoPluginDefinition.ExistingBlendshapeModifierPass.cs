#if ZATOOLS
using System.Buffers;
using KusakaFactory.Zatools.Runtime;

namespace Numeira;

internal sealed partial class ModEmoPluginDefinition
{
    public sealed class ExistingBlendshapeModifierPass : Pass<ExistingBlendshapeModifierPass>
    {
        protected override void Execute(BuildContext context)
        {
            var allModifiers = context.AvatarRootObject.GetComponentsInChildren<ModEmoExistingBlendShapeModifier>(includeInactive: true);

            var modifiers = allModifiers.Where(x => x.gameObject.activeInHierarchy).ToArray();
            if (modifiers.Length == 0)
                return;

            var faceRenderer = AvatarUtils.GetFaceRenderer(context.AvatarRootTransform);
            if (faceRenderer == null)
                return;

            var mesh = faceRenderer.sharedMesh;
            if (mesh == null)
                return;

            var newMesh = Object.Instantiate(mesh);
            ObjectRegistry.RegisterReplacedObject(mesh, newMesh);

            newMesh.ClearBlendShapes();

            int count = mesh.blendShapeCount;

            var dict = modifiers.ToDictionary(x => x.TargetBlendShapeName, x => x);

            Vector3[] verticies1 = new Vector3[mesh.vertexCount];
            Vector3[] verticies2 = new Vector3[mesh.vertexCount];

            for (int index = 0; index < count; index++)
            {
                var name = mesh.GetBlendShapeName(index);
                if (!dict.TryGetValue(name, out var modifier))
                {
                    mesh.GetBlendShapeFrameVertices(index, 0, verticies1, null, null);
                }
                else
                {
                    verticies1.AsSpan().Clear();
                    foreach (var blendshape in modifier.GetBlendShapes())
                    {
                        int index2 = mesh.GetBlendShapeIndex(blendshape.Name);
                        if (index2 == -1)
                            continue;

                        mesh.GetBlendShapeFrameVertices(index2, 0, verticies2, null, null);

                        var v = blendshape.Value;
                        var w = mesh.GetBlendShapeFrameWeight(index2, 0);
                        v /= w;

                        var origweight = faceRenderer.GetBlendShapeWeight(index2) / w;
                        float cancel = blendshape.Cancel ? -origweight : 1;

                        for (int i = 0; i < verticies1.Length; i++)
                        {
                            var v1 = verticies1[i];
                            var v2 = verticies2[i];
                            verticies1[i] = Vector3.Lerp(v1, v1 + v2 * cancel, v);
                        }
                    }
                }

                newMesh.AddBlendShapeFrame(name, mesh.GetBlendShapeFrameWeight(index, 0), verticies1, null, null);
            }
            faceRenderer.sharedMesh = newMesh;

            foreach (var x in allModifiers)
            {
                Object.Destroy(x.gameObject);
            }
        }

        internal sealed class BlendShapeModifier
        {
            public Mesh Source { get; }

            public BlendShapeModifier(Mesh source)
            {
                Source = source;
            }

            private Dictionary<string, Dictionary<string, float>> blendFactors = new();

            public void Add(string targetBlendshapeName, string sourceBlendshapeName, float factor) 
                => blendFactors.GetOrAdd(targetBlendshapeName, _ => new())[sourceBlendshapeName] = factor;

            public Mesh Export()
            {
                var sourceMesh = this.Source;
                var newMesh = Object.Instantiate(sourceMesh);

                newMesh.ClearBlendShapes();

                var vertexBuffer = new Vector3[sourceMesh.vertexCount];
                var vertexBuffer2 = new Vector3[sourceMesh.vertexCount];

                int count = sourceMesh.blendShapeCount;
                for (int shapeIndex = 0; shapeIndex < count; shapeIndex++)
                {
                    int frameCount = sourceMesh.GetBlendShapeFrameCount(shapeIndex);
                    string shapeName = sourceMesh.GetBlendShapeName(shapeIndex);

                    for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                    {
                        if (blendFactors.TryGetValue(shapeName, out var dict) && dict.Count != 0)
                        {
                            vertexBuffer.AsSpan().Clear();
                            foreach (var (blendShapeName, blendFactor) in dict)
                            {
                                int index = sourceMesh.GetBlendShapeIndex(blendShapeName);
                                if (index == -1)
                                    continue;
                                Blend(index, frameIndex, blendFactor, vertexBuffer);
                            }
                        }
                        else
                        {
                            sourceMesh.GetBlendShapeFrameVertices(shapeIndex, frameIndex, vertexBuffer, null, null);
                        }

                        float weight = sourceMesh.GetBlendShapeFrameWeight(shapeIndex, frameIndex);
                        newMesh.AddBlendShapeFrame(shapeName, weight, vertexBuffer, null, null);
                    }
                }

                void Blend(int shapeIndex, int frameIndex, float factor, Vector3[] buffer)
                {
                    var targetShapeFrameCount = sourceMesh.GetBlendShapeFrameCount(shapeIndex);
                    sourceMesh.GetBlendShapeFrameVertices(shapeIndex, frameIndex, vertexBuffer2, null, null);

                    Debug.Assert(buffer.Length == vertexBuffer2.Length);

                    // TODO: Burstとか使おう

                    float cancel = factor < 0 ? -1 : 1;
                    float weight = Mathf.Abs(factor);

                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = Vector3.LerpUnclamped(buffer[i], buffer[i] + vertexBuffer2[i] * cancel, weight);
                    }
                }

                return newMesh;
            }
        }
    }
}
#endif