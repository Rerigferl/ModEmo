using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "FaceTracking Expression Marker")]
    internal sealed class FaceTrackingExpressionMarker : ModEmoTagComponent, IFaceTrackingExpressionMarker
    {
        public FaceTrackingMarkerRole Role;

        FaceTrackingMarkerRole IFaceTrackingExpressionMarker.Role => Role;
    }

    internal interface IFaceTrackingExpressionMarker : IModEmoComponent, ISubComponent<IFaceTrackingExpressionMarker>, IOwnerComponent<IModEmoExpression>
    {
        public IModEmoExpression? Expression => this.GetOwnedComponents().FirstOrDefault();
        public FaceTrackingMarkerRole Role { get; }
    }

    [Flags]
    internal enum FaceTrackingMarkerRole : uint
    {
        None = 0u,

        Left = 1u << 30,
        Right = 1u << 31,

        Close = 1u << 0,
        LookX = 1u << 1,
        LookY = 1u << 2,
    }

#if UNITY_EDITOR
    static partial class RuntimeEditor
    {
        public sealed class FaceTrackingExpressionMarkerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
            }
        }
    }
#endif
}