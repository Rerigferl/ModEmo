namespace Numeira;

internal static class AnimationCurveExt
{
    public static bool Equals(this AnimationCurve curve, float value)
    {
        if (curve == null || curve.length == 0)
            return false;

        foreach (var key in curve.keys)
        {
            if (key.value != value)
                return false;
        }

        return true;
    }
}