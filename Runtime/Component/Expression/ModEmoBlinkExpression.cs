
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Blink Expression")]
    internal sealed class ModEmoBlinkExpression : ModEmoExpression, IModEmoLoopControl, ISerializationCallbackReceiver
    {
        [HideInInspector]
        [Obsolete]
        public BlendShape[] BlendShapes = { };

        public IEnumerable<BlendShape> GetBlendShapes()
        {
            foreach(var x in this.GetComponentsInDirectChildren<IModEmoExpressionFrameProvider>(includeSelf: true))
            {
                foreach(var frame in x.GetFrames())
                {
                    foreach (var blendShape in frame.GetBlendShapes())
                    {
                        yield return blendShape; 
                    }
                }
            }
        }

        public override IEnumerable<ExpressionFrame> Frames
        { 
            get
            {
                var blendShapes = GetBlendShapes();
                var zero = blendShapes.Select(x => x with { Value = 0 });
                yield return ExpressionFrame.Create(this, 0 / 60f, zero);
                yield return ExpressionFrame.Create(this, 60 / 60f, zero);
                yield return ExpressionFrame.Create(this, 65 / 60f, blendShapes);
                yield return ExpressionFrame.Create(this, 67 / 60f, blendShapes);
                yield return ExpressionFrame.Create(this, 80 / 60f, zero);
                yield return ExpressionFrame.Create(this, 300 / 60f, zero);
            }
        }

        public bool IsLoop => true;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach (var b in GetBlendShapes())
                hashCode.Add(b);

            base.CalculateContentHash(ref hashCode);
        }

#if UNITY_EDITOR

        public void OnBeforeSerialize() => OnValidate();

        public void OnAfterDeserialize() => OnValidate();

#pragma warning disable CS0612
        public void OnValidate()
        {
            if ((BlendShapes?.Length ?? 0) == 0)
                return;

            var component = this.gameObject.AddComponent<ModEmoBlendShapeSelector>();
            component.BlendShapes.AddRange(BlendShapes);

            BlendShapes = Array.Empty<BlendShape>();
        }
#pragma warning restore CS0612
#endif
    }
}