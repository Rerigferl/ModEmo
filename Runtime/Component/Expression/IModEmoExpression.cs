namespace Numeira
{
    internal interface IModEmoExpression : IModEmoComponent
    {
        string Name { get; }

        ExpressionMode Mode { get; }

        IEnumerable<ExpressionFrame> Frames { get; }

        IEnumerable<CurveBlendShape> BlendShapes
        {
            get
            {
                Dictionary<(string, bool), AnimationCurve> dict = new();
                var timeMax = Frames.Any() ? Frames.Max(x => x.Time) : 1f;
                foreach (var frame in Frames)
                {
                    foreach (var blendShape in frame.GetBlendShapes())
                    {
                        var curve = dict.GetOrAdd((blendShape.Name, blendShape.Cancel), _ => new());
                        curve.AddKey(new Keyframe(frame.Time / timeMax, blendShape.Value, 0, 0));
                    }
                }

                return dict.Select(x => new CurveBlendShape(x.Key.Item1, x.Value, x.Key.Item2));
            }
        }
        private static float Tangent(float timeStart, float timeEnd, float valueStart, float valueEnd)
        {
            return (valueEnd - valueStart) / (timeEnd - timeStart);
        }

        IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> Conditions => GetConditions();

        private IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions()
        {
            var self = this.GameObject.GetComponents<IModEmoConditionProvider>();
            if (self.Length != 0)
            {
                yield return Group.Create(self[0], self.SelectMany(x => x.GetConditions()).SelectMany(x => x));
            }

            foreach(var x in Component.GetComponentsInDirectChildren<IModEmoConditionProvider>().SelectMany(x => x.GetConditions()))
                yield return x;
        }

        bool IsLoop => Component.GetComponent<IModEmoLoopControl>()?.IsLoop is true;

        string? MotionTime => Component.GetComponent<IModEmoMotionTimeProvider>()?.ParameterName;

        bool Blink => Component.GetComponent<IModEmoBlinkControl>()?.Enable ?? true;

        bool LipSync => Component.GetComponent<IModEmoLipSyncConttrol>()?.Enable ?? true;

        bool EnableMouthMorphCancel => Component.GetComponent<IModEmoMouthMorphCancelControl>()?.Enable ?? false;
    }
}