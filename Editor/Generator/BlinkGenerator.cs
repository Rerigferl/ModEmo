using Numeira.Animation;

namespace Numeira;

internal static class BlinkGenerator
{
    public static void GenerateBlinkController(BuildContext context, AnimatorControllerBuilder animatorController)
    {
        var modEmo = context.GetModEmoContext().Root;
        var expression = modEmo.GetBlinkExpression();
        if (expression is null)
            return;

        animatorController.Parameters.AddFloat(ParameterNames.Blink.Value, 1);
        animatorController.Parameters.AddFloat(ParameterNames.Blink.Sync, 1);

        var layer = animatorController.AddLayer("[ModEmo] Blink");
        var stateMachine = layer.StateMachine;

        var state = stateMachine.AddState("Blink (WD On)");
        state.WriteDefaults = true;
        var dbt = new DirectBlendTreeBuilder() { Name = "Blink", DefaultDirectBlendParameter = ParameterNames.Internal.One, };
        state.Motion = dbt;

        var syncGate = dbt.AddDirectBlendTree("Sync").WithDirectBlendParameter(ParameterNames.Blink.Sync).Motion;
        var valueGate = syncGate.AddDirectBlendTree("Value").WithDirectBlendParameter(ParameterNames.Blink.Value).Motion;

        if (expression is ModEmoFaceTrackingExpression ft)
        {
            EyeTrackingGenerator.Generate(context, animatorController, ft, valueGate);
            return;
        }

        MakeTimerState(context, animatorController, useEyeClose: expression is IModEmoBlinkExpression { UseEyeCloseToBlinkTimer: true });

        valueGate.Append(expression.ToMotion(context, layerIndex: 1, writeDefaultValues: false, writeBlink: false));

        foreach (var t in expression.GetOwnedComponents<IModEmoMotionTimeProvider>())
        {
            if (t.ParameterName is null)
                continue;
            animatorController.Parameters.AddFloat(t.ParameterName);
        }
    }

    private static void MakeTimerState(BuildContext context, AnimatorControllerBuilder animatorController, bool useEyeClose = false)
    {
        animatorController.Parameters.AddFloat(ParameterNames.Blink.Timer, 0);
        var layer = animatorController.AddLayer("[ModEmo] Blink Timer");
        var stateM = layer.StateMachine;
        var timerState = stateM.AddState("Timer");

        var timer = new AnimationClipBuilder
        {
            Name = "Timer",
            IsLoop = true
        };

        timer.AddAnimatedParameter(ParameterNames.Blink.Timer, 0 / 60f, 0);
        timer.AddAnimatedParameter(ParameterNames.Blink.Timer, 60 / 60f, 0);
        timer.AddAnimatedParameter(ParameterNames.Blink.Timer, 65 / 60f, 1);
        timer.AddAnimatedParameter(ParameterNames.Blink.Timer, 67 / 60f, 1);
        timer.AddAnimatedParameter(ParameterNames.Blink.Timer, 80 / 60f, 0);
        timer.AddAnimatedParameter(ParameterNames.Blink.Timer, 300 / 60f, 0);

        timerState.Motion = timer;

        if (!useEyeClose)
        {
            return;
        }

        var eyeCloseState = stateM.AddState("EyeClose");
        var blendTree = new DirectBlendTreeBuilder
        {
            Name = "EyeClose",
            DefaultDirectBlendParameter = ParameterNames.Internal.One
        };

        var smoothed = eyeCloseState.MotionTime = ParameterNames.FaceTracking.EyeClosed + "/Smooth";
        
        {
            var tree = blendTree.AddBlendTree("Smoothing").Motion;
            tree.BlendParameter = ParameterNames.Internal.SmoothAmount;
            var i = tree.AddBlendTree("Input").WithThreshold(0).Motion;
            var zero = i.AddAnimationClip("0").WithThreshold(0).Motion.AddAnimatedParameter(smoothed, 0, 0);
            var one = i.AddAnimationClip("1").WithThreshold(1).Motion.AddAnimatedParameter(smoothed, 0, 1);

            var o = tree.AddBlendTree("Output").WithThreshold(1).Motion;
            o.Append(zero, 0);
            o.Append(one, 1);

            i.BlendParameter = ParameterNames.FaceTracking.EyeClosed;
            o.BlendParameter = smoothed;
        }

        {
            var tree = blendTree.AddBlendTree("Timer").Motion;
            tree.BlendParameter = smoothed;
            tree.AddAnimationClip("0").WithThreshold(0).Motion.AddAnimatedParameter(ParameterNames.Blink.Timer, 0, 0);
            tree.AddAnimationClip("1").WithThreshold(1).Motion.AddAnimatedParameter(ParameterNames.Blink.Timer, 0, 1);
        }

        eyeCloseState.Motion = blendTree;
        animatorController.Parameters.AddFloat(ParameterNames.FaceTracking.EyeClosed);
        animatorController.Parameters.AddFloat(smoothed);
        context.GetData().Parameters.Add(new(ParameterNames.FaceTracking.EyeClosed, 0f, AnimatorParameterType.Float, false, false));

        timerState.AddTransition(eyeCloseState).AddCondition(AnimatorConditionMode.Greater, smoothed, 0);
        eyeCloseState.AddTransition(timerState).AddCondition(AnimatorConditionMode.Less, smoothed, ModEmoConstants.Epsilon);
    }
}
