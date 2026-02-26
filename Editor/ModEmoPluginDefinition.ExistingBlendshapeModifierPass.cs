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

            var mix = faceRenderer.gameObject.AddComponent<AdHocBlendShapeMix>();
            mix.Replace = true;
            var list = new List<BlendShapeMixDefinition>();

            foreach (var group in modifiers.GroupBy(x => x.TargetBlendShapeName, x => x.GetBlendShapes()))
            {
                list.Add(new()
                {
                    ToBlendShape = group.Key,
                    FromBlendShape = group.Key,
                    MixWeight = -1
                });

                foreach (var x in group.SelectMany(x => x))
                {
                    list.Add(new()
                    {
                        ToBlendShape = group.Key,
                        FromBlendShape = x.Name,
                        MixWeight = (x.Value / 100f) * (x.Cancel ? -1 : 1),
                    });
                }
            }

            mix.MixDefinitions = list.ToArray();


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