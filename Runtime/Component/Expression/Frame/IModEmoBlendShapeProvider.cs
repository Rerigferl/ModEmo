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
        private readonly Dictionary<(string Name, bool Cancel), List<Curve.Keyframe>> innerDictionary;
        private readonly Stack<Func<KeyframeInfo, float>> keyframeFactories;

        private BlendShapeCurveWriter(Dictionary<(string Name, bool Cancel), List<Curve.Keyframe>> innerDictionary)
        {
            this.innerDictionary = innerDictionary;
            this.keyframeFactories = new();
        }

        internal static BlendShapeCurveWriter Create()
        {
            return new BlendShapeCurveWriter(new());
        }

        public readonly void Clear()
        {
            foreach (var (_, list) in innerDictionary)
            {
                list.Clear();
            }
        }

        public readonly void Write(float time, BlendShape blendShape) => Write(time, blendShape.Name, blendShape.Value, blendShape.Cancel);

        public readonly void Write(float time, string name, float value, bool cancel = false)
        {
            var list = innerDictionary.GetOrAdd((name, cancel), _ => new());
            foreach (var factory in keyframeFactories)
            {
                time = factory(new(name, cancel, time));
            }
            list.Add(new(time, value));
        }

        public readonly void BeginModifyCurveTime(Func<KeyframeInfo, float> factory)
        {
            keyframeFactories.Push(factory);
        }

        public readonly void EndModifyCurveTime() => keyframeFactories.TryPop(out _);
        
        internal readonly IEnumerable<CurveBlendShape> Export()
        {
            foreach (var (key, value) in innerDictionary)
            {
                if (value.Count == 0)
                    continue;

                yield return new CurveBlendShape(key.Name, new (value), key.Cancel);
            }
        }

        public readonly struct KeyframeInfo
        {
            public readonly string BlendShapeName;
            public readonly bool IsCancelBlendShape;
            public readonly float Time;

            public KeyframeInfo(string blendShapeName, bool isCancelBlendShape, float time)
            {
                BlendShapeName = blendShapeName;
                IsCancelBlendShape = isCancelBlendShape;
                Time = time;
            }
        }
    }
}