

namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Transform")]
    internal sealed class ModEmoTransform : ModEmoTagComponent, IModEmoAnimationProvider
    {
        public float Keyframe = 0;

        public AvatarObjectReference Target = new();

        public bool UsePosition;
        public bool UseRotation;

        public Vector3 Position;
        public Vector3 Rotation;

        public void OnEnable() { }

        public void WriteAnimation(IAnimationWriter writer, in AnimationWriterContext context)
        {
            if (!enabled || Target.Get(this) is not { } target)
            {
                return;
            }

            var tr = target.transform;

            var bindBase = new AnimationBinding(typeof(Transform), Target.referencePath, "");

            if (UsePosition)
            {
                var def = tr.localPosition;
                writer.WriteDefaultValue(bindBase with { PropertyName = "localPosition.x" }, def.x);
                writer.WriteDefaultValue(bindBase with { PropertyName = "localPosition.y" }, def.y);
                writer.WriteDefaultValue(bindBase with { PropertyName = "localPosition.z" }, def.z);

                var aft = def + Position;

                writer.Write(bindBase with { PropertyName = "localPosition.x" }, Keyframe, aft.x);
                writer.Write(bindBase with { PropertyName = "localPosition.y" }, Keyframe, aft.y);
                writer.Write(bindBase with { PropertyName = "localPosition.z" }, Keyframe, aft.z);
            }

            if (UseRotation)
            {
                var def = tr.localEulerAngles;

                writer.WriteDefaultValue(bindBase with { PropertyName = "localEulerAngles.x" }, def.x);
                writer.WriteDefaultValue(bindBase with { PropertyName = "localEulerAngles.y" }, def.y);
                writer.WriteDefaultValue(bindBase with { PropertyName = "localEulerAngles.z" }, def.z);

                var aft = def + Rotation;

                writer.Write(bindBase with { PropertyName = "localEulerAngles.x" }, Keyframe, aft.x);
                writer.Write(bindBase with { PropertyName = "localEulerAngles.y" }, Keyframe, aft.y);
                writer.Write(bindBase with { PropertyName = "localEulerAngles.z" }, Keyframe, aft.z);
            }
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