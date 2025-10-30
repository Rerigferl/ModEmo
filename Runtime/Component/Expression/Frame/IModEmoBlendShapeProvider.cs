namespace Numeira
{
    internal interface IModEmoBlendShapeProvider : IModEmoComponent
    {
        public void CollectBlendShapes(in BlendShapeCurveWriter writer);
    }

    internal readonly struct BlendShapeCurveWriter
    {
        private readonly Dictionary<(string Name, bool Cancel), List<Keyframe>> innerDictionary;

        private BlendShapeCurveWriter(Dictionary<(string Name, bool Cancel), List<Keyframe>> innerDictionary)
        {
            this.innerDictionary = innerDictionary;
        }

        internal static BlendShapeCurveWriter Create()
        {
            return new BlendShapeCurveWriter(new());
        }

        public readonly void Write(float time, BlendShape blendShape) => Write(time, blendShape.Name, blendShape.Value, blendShape.Cancel);

        public readonly void Write(float time, string name, float value, bool cancel = false)
        {
            var list = innerDictionary.GetOrAdd((name, cancel), _ => new());
            list.Add(new(time, value, 0, 0));
        }
        
        public readonly void ModifyCurveTimes(ModifyCurveTimeDelegate factory)
        {
            foreach(var (key, list) in innerDictionary)
            {
                foreach(ref var x in list.AsSpan())
                {
                    x.time = factory(key.Name, key.Cancel, x.time);
                }
            }
        }

        internal readonly IEnumerable<CurveBlendShape> Export()
        {
            foreach (var (key, value) in innerDictionary)
            {
                yield return new CurveBlendShape(key.Name, new AnimationCurve(value.ToArray()), key.Cancel);
            }
        }

        public delegate float ModifyCurveTimeDelegate(string blendShapeName, bool isCancelBlendShape, float time);
    }
}