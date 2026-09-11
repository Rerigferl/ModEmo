

namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Transform")]
    internal sealed class ModEmoTransform : ModEmoTagComponent, IKeyframeWriterComponent
    {
        public float Keyframe = 0;

        public AvatarObjectReference Target = new();

        public bool UsePosition;
        public bool UseRotation;

        public Vector3 Position;
        public Vector3 Rotation;

        public void OnEnable() { }

        public void WriteKeyframes(IKeyframeWriterContext context, in AnimationGeneratorOptions options)
        {
            if (!enabled || Target.Get(this) is not { } target)
            {
                return;
            }

            if (UsePosition)
                throw new NotImplementedException("Position mada dekitenai");

            if (UseRotation)
            context.AddRotation(target.transform, Keyframe, Rotation, true);
        }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(Target);

            hashCode.Add(UsePosition);
            if (UsePosition)
                hashCode.Add(Position);

            hashCode.Add(UseRotation);
            if (UseRotation)
                hashCode.Add(Rotation);
        }
    }

}