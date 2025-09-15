using Numeira.Animation;
using Debug = UnityEngine.Debug;

namespace Numeira;
internal static class ExpressionControllerGenerator
{
    private const float Epsilon = 0.005f;
    public static void Generate(BuildContext context, AnimatorControllerBuilder animatorController)
    {
        animatorController.Parameters.AddFloat(ParameterNames.Expression.Pattern, 0);
        animatorController.Parameters.AddFloat(ParameterNames.Expression.Index, 0);
        animatorController.Parameters.AddFloat(ParameterNames.Expression.Lock, 0);

        List<IModEmoExpression> expressions = new();
        GenerateIndexSelectorBlendTree(context, animatorController, expressions);
        context.GetData().Expressions = expressions;

        GenerateMouthMorphCancellar(context, animatorController);
        GenerateExpressionSelector(context, animatorController, expressions.AsSpan());
        GenerateBlinkController(context, animatorController);
    }

    private static List<IModEmoExpression> GenerateIndexSelectorBlendTree(BuildContext context, AnimatorControllerBuilder animatorController, List<IModEmoExpression> expressions)
    {
        var modEmo = context.GetModEmoContext().Root;
        var data = context.GetData();

        var expressionPatterns = modEmo.ExportExpressions();

        var layer = animatorController.AddLayer("[ModEmo] Conditions");
        var stateMachine = layer.StateMachine.WithDefaultMotion(data.BlankClip);

        var blendTree = new DirectBlendTreeBuilder
        {
            Name = "ModEmo Expression Controlller",
            DefaultDirectBlendParameter = ParameterNames.Internal.One
        };

        var idleState = stateMachine.AddState("DBT (DO NOT OPEN IN EDITOR!) (WD On)");
        idleState.Motion = blendTree;

        var lockTree = blendTree.AddBlendTree("Lock").Motion;
        lockTree.BlendParameter = ParameterNames.Expression.Lock;

        if (!modEmo.Settings.DebugSettings.SkipExpressionController)
        {
            var patternSwitch = lockTree.AddBlendTree("Pattern Switch").Motion;
            patternSwitch.BlendParameter = ParameterNames.Expression.Pattern;

            foreach (var (pattern, patternIdx) in expressionPatterns.Index())
            {
                var patternTree = patternSwitch.AddDirectBlendTree(pattern.Key.Name).WithThreshold(patternIdx).Motion;

                var patternAnimation = new AnimationClipBuilder() { Name = pattern.Key.Name };
                patternAnimation.AddAnimatedParameter(ParameterNames.Expression.Index, 0, expressions.Count);
                expressions.Add(pattern.Key);

                BlendTreeBuilder nextTree = patternTree;
                BlendTreeBuilder? fallback = null;

                foreach (var (expression, expressionIdx) in pattern.Index())
                {
                    var expressionTree = nextTree == fallback ? nextTree.WithName($"Fallback: {expression.Name}") : nextTree.AddDirectBlendTree(expression.Name).Motion;
                    nextTree = expressionTree;

                    var expressionAnimation = new AnimationClipBuilder() { Name = expression.Name };
                    expressionAnimation.AddAnimatedParameter(ParameterNames.Expression.Index, 0, expressions.Count);
                    expressions.Add(expression);

                    foreach (var conditionsOr in expression.Conditions)
                    {
                        float? prevValue = null;
                        var fallbackTree = new DirectBlendTreeBuilder() { Name = "Fallback" };
                        foreach (var conditionsAnd in conditionsOr)
                        {
                            animatorController.Parameters.AddFloat(conditionsAnd.Parameter.Name);

                            var tree = nextTree.AddBlendTree(conditionsAnd.Parameter.Name).WithThreshold(prevValue ?? 0).Motion;

                            tree.BlendParameter = conditionsAnd.Parameter.Name;
                            float value = conditionsAnd.Parameter.Value;

                            tree.Append(fallbackTree).WithThreshold(value - Epsilon);
                            tree.Append(fallbackTree).WithThreshold(value + Epsilon);

                            prevValue = value;
                            nextTree = tree;
                        }
                        nextTree.Append(expressionAnimation).WithThreshold(prevValue ?? 0);

                        nextTree = fallbackTree;
                        fallback = fallbackTree;
                    }
                }

                nextTree.Append(patternAnimation);
            }
        }
        else
        {
            foreach (var (pattern, patternIdx) in expressionPatterns.Index())
            {
                expressions.Add(pattern.Key);
                foreach (var (expression, expressionIdx) in pattern.Index())
                {
                    expressions.Add(expression);
                }
            }
        }

        {
            var preserve = lockTree.AddBlendTree("").Motion;
            preserve.BlendParameter = ParameterNames.Expression.Index;

            preserve.AddAnimationClip("0").Motion.AddAnimatedParameter(ParameterNames.Expression.Index, 0, 0);
            preserve.AddAnimationClip($"{expressions.Count}").WithThreshold(expressions.Count).Motion.AddAnimatedParameter(ParameterNames.Expression.Index, 0, expressions.Count);
        }

        return expressions;
    }

    private static void GenerateExpressionSelector(BuildContext context, AnimatorControllerBuilder animatorController, ReadOnlySpan<IModEmoExpression> expressions)
    {
        var data = context.GetData();

        var layer = animatorController.AddLayer("[ModEmo] Expressions");
        var stateMachine = layer.StateMachine;

        for (int i = 0; i < expressions.Length; i++)
        {
            var expression = expressions[i];

            var state = stateMachine.AddState(expression.Name, new Vector2((stateMachine.EntryPosition.x + stateMachine.ExitPosition.x) / 2, 120 + 70 * i));
            stateMachine.AddEntryTransition(state)
                .AddCondition(AnimatorConditionMode.Greater, ParameterNames.Expression.Index, i - Epsilon)
                .AddCondition(AnimatorConditionMode.Less, ParameterNames.Expression.Index, i + Epsilon);
            state.AddExitTransition().AddCondition(AnimatorConditionMode.Greater, ParameterNames.Expression.Index, i).WithDuration(0.1f);
            state.AddExitTransition().AddCondition(AnimatorConditionMode.Less, ParameterNames.Expression.Index, i).WithDuration(0.1f);
            state.Motion = expression.MakeAnimationClip(data);
            state.MotionTime = expression.MotionTime;

            var tr = state.AddTrackingControl();
            tr.Eyes = TrackingType.Animation;
            tr.Mouth = expression.LipSync ? TrackingType.Tracking : TrackingType.Animation;
        }
    }

    private static void GenerateBlinkController(BuildContext context, AnimatorControllerBuilder animatorController)
    {
        var modEmo = context.GetModEmoContext().Root;
        var expression = modEmo.GetBlinkExpression();
        if (expression is null)
            return;

        animatorController.Parameters.AddFloat(ParameterNames.Blink.Value, 1);
        animatorController.Parameters.AddFloat(ParameterNames.Blink.Sync, 1);

        var layer = animatorController.AddLayer("[ModEmo] Blink");
        var stateMachine = layer.StateMachine;

        var off = stateMachine.AddState("OFF");
        off.Motion = context.GetData().BlankClip;

        var on = stateMachine.AddState("ON");
        on.Motion = expression.MakeAnimationClip(context.GetData(), writeDefault: false, writeBlink: false);

        off.AddTransition(on)
            .AddCondition(AnimatorConditionMode.Greater, ParameterNames.Blink.Value, 1 - Epsilon)
            .AddCondition(AnimatorConditionMode.Greater, ParameterNames.Blink.Sync, 1 - Epsilon);

        on.AddTransition(off).AddCondition(AnimatorConditionMode.Less, ParameterNames.Blink.Value, 1);
        on.AddTransition(off).AddCondition(AnimatorConditionMode.Less, ParameterNames.Blink.Sync, 1);
    }

    private static void GenerateMouthMorphCancellar(BuildContext context, AnimatorControllerBuilder animatorController)
    {
        animatorController.Parameters.AddFloat(ParameterNames.MouthMorphCancel.Enable, 0);

        var modEmo = context.GetModEmoContext().Root;
        if (modEmo.MouthMorphCanceller is not { } cancellar)
            return;

        var blendTree = new OneDirectionBlendTreeBuilder() 
        {
            Name = "", 
            DefaultDirectBlendParameter = ParameterNames.Internal.One,
            BlendParameter = ParameterNames.MouthMorphCancel.Enable,
        };

        animatorController.Parameters.AddFloat("Voice", 0);

        animatorController.AddLayer("[ModEmo] Morph Cancellar").StateMachine
            .WithDefaultMotion(blendTree)
            .AddState("(WD On)");

        var disable = blendTree.AddAnimationClip("Disable").Motion;
        var voiceSwitch = blendTree.AddBlendTree("Voice").Motion;
        voiceSwitch.BlendParameter = "Voice";
        voiceSwitch.Append(disable, threshold: 0);

        var enable = voiceSwitch.AddAnimationClip("Enable").WithThreshold(float.Epsilon).Motion;

        foreach(var blendShape in cancellar.GetFrames().SelectMany(x => x.GetBlendShapes()))
        {
            var name = $"{ParameterNames.Internal.BlendShapes.Prefix}{blendShape.Name}/Enable";
            disable.AddAnimatedParameter(name, 0, 1);
            enable.AddAnimatedParameter(name, 0, 0);
        }
    }
}