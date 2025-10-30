namespace Numeira
{
    internal interface IModEmoBlendShapeProvider : IModEmoComponent
    {
        public void CollectBlendShapes(in BlendShapeCurveWriter writer);

        public IEnumerable<CurveBlendShape> GetBlendShapes()
        {
            var writer = BlendShapeCurveWriter.Create();
            CollectBlendShapes(writer);
            return writer.Export().ToArray();
        }
    }

    internal readonly struct BlendShapeCurveWriter
    {
        private static readonly Dictionary<(string Name, bool Cancel), List<Curve.Keyframe>> sharedDictionary = new();
        private readonly Dictionary<(string Name, bool Cancel), List<Curve.Keyframe>> innerDictionary;

        private BlendShapeCurveWriter(Dictionary<(string Name, bool Cancel), List<Curve.Keyframe>> innerDictionary)
        {
            this.innerDictionary = innerDictionary;
        }

        internal static BlendShapeCurveWriter Create(bool useSharedDictionary = true)
        {
            if (useSharedDictionary)
            {
                var dict = sharedDictionary;
                foreach (var value in dict.Values)
                {
                    value.Clear();
                }
                return new BlendShapeCurveWriter(dict);
            }
            return new BlendShapeCurveWriter(new());
        }

        public readonly void Write(float time, BlendShape blendShape) => Write(time, blendShape.Name, blendShape.Value, blendShape.Cancel);

        public readonly void Write(float time, string name, float value, bool cancel = false)
        {
            var list = innerDictionary.GetOrAdd((name, cancel), _ => new());
            list.Add(new(time, value));
        }
        
        public readonly void ModifyCurveTimes(ModifyCurveTimeDelegate factory)
        {
            foreach(var (key, list) in innerDictionary)
            {
                foreach(ref var x in list.AsSpan())
                {
                    x.Time = factory(key.Name, key.Cancel, x.Time);
                }
            }
        }

        internal readonly IEnumerable<CurveBlendShape> Export()
        {
            foreach (var (key, value) in innerDictionary)
            {
                if (value.Count == 0)
                    continue;

                yield return new CurveBlendShape(key.Name, new (value), key.Cancel);
            }
        }

        public delegate float ModifyCurveTimeDelegate(string blendShapeName, bool isCancelBlendShape, float time);
    }
}