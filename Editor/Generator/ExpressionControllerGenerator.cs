using System;
using System.Linq.Expressions;
using System.Reflection.Emit;
using nadena.dev.ndmf.util;
using Numeira.Animation;
using Debug = UnityEngine.Debug;

namespace Numeira;
internal static class ExpressionControllerGenerator
{
    private const float Epsilon = 0.005f;
    public static void Generate(BuildContext context, AnimatorControllerBuilder animatorController)
    {
        animatorController.Parameters.AddFloat(ParameterNames.Expression.Pattern, 0);
        animatorController.Parameters.AddFloat(ParameterNames.Expression.Page, 0);
        animatorController.Parameters.AddFloat(ParameterNames.Expression.Index, 0);
        animatorController.Parameters.AddFloat(ParameterNames.Expression.Lock, 0);

        List<ExpressionData> expressions = new();
        //GenerateIndexSelectorBlendTree(context, animatorController, expressions);
        GenerateIndexSelectorStates(context, animatorController, expressions);
        context.GetData().Expressions = expressions;

        GenerateMouthMorphCancellar(context, animatorController);
        GenerateExpressionSelector(context, animatorController, expressions);
        GenerateBlinkController(context, animatorController);
    }

    //private static void GenerateIndexSelectorBlendTree(BuildContext context, AnimatorControllerBuilder animatorController, List<ExpressionData> expressions)
    //{
    //    var modEmo = context.GetModEmoContext().Root;
    //    var data = context.GetData();

    //    var expressionPatterns = modEmo.ExportExpressions();

    //    var layer = animatorController.AddLayer("[ModEmo] Conditions");
    //    var stateMachine = layer.StateMachine.WithDefaultMotion(data.BlankClip);

    //    var blendTree = new DirectBlendTreeBuilder
    //    {
    //        Name = "ModEmo Expression Controlller",
    //        DefaultDirectBlendParameter = ParameterNames.Internal.One
    //    };

    //    var idleState = stateMachine.AddState("DBT (DO NOT OPEN IN EDITOR!) (WD On)");
    //    idleState.Motion = blendTree;

    //    var lockTree = blendTree.AddBlendTree("Lock").Motion;
    //    lockTree.BlendParameter = ParameterNames.Expression.Lock;

    //    if (!modEmo.Settings.DebugSettings.SkipExpressionController)
    //    {
    //        var patternSwitch = lockTree.AddBlendTree("Pattern Switch").Motion;
    //        patternSwitch.BlendParameter = ParameterNames.Expression.Pattern;

    //        foreach (var (pattern, patternIdx) in expressionPatterns.Index())
    //        {
    //            var patternTree = patternSwitch.AddDirectBlendTree(pattern.Key.Name).WithThreshold(patternIdx).Motion;

    //            var patternAnimation = new AnimationClipBuilder() { Name = pattern.Key.Name };
    //            patternAnimation.AddAnimatedParameter(ParameterNames.Expression.Index, 0, expressions.Count);
    //            expressions.Add(new(pattern.Key, patternIdx, 0, 0));

    //            BlendTreeBuilder nextTree = patternTree;
    //            BlendTreeBuilder? fallback = null;

    //            foreach (var (expression, expressionIdx) in pattern.Index())
    //            {
    //                var expressionTree = nextTree == fallback ? nextTree.WithName($"Fallback: {expression.Name}") : nextTree.AddDirectBlendTree(expression.Name).Motion;
    //                nextTree = expressionTree;

    //                var expressionAnimation = new AnimationClipBuilder() { Name = expression.Name };
    //                expressionAnimation.AddAnimatedParameter(ParameterNames.Expression.Index, 0, expressions.Count);
    //                expressions.Add(new());

    //                foreach (var conditionsOr in expression.Conditions)
    //                {
    //                    float? prevValue = null;
    //                    var fallbackTree = new DirectBlendTreeBuilder() { Name = "Fallback" };
    //                    foreach (var conditionsAnd in conditionsOr)
    //                    {
    //                        animatorController.Parameters.AddFloat(conditionsAnd.Parameter.Name);

    //                        var tree = nextTree.AddBlendTree(conditionsAnd.Parameter.Name).WithThreshold(prevValue ?? 0).Motion;

    //                        tree.BlendParameter = conditionsAnd.Parameter.Name;
    //                        float value = conditionsAnd.Parameter.Value;

    //                        tree.Append(fallbackTree).WithThreshold(value - Epsilon);
    //                        tree.Append(fallbackTree).WithThreshold(value + Epsilon);

    //                        prevValue = value;
    //                        nextTree = tree;
    //                    }
    //                    nextTree.Append(expressionAnimation).WithThreshold(prevValue ?? 0);

    //                    nextTree = fallbackTree;
    //                    fallback = fallbackTree;
    //                }
    //            }

    //            nextTree.Append(patternAnimation);
    //        }
    //    }
    //    else
    //    {
    //        foreach (var (pattern, patternIdx) in expressionPatterns.Index())
    //        {
    //            expressions.Add(pattern.Key);
    //            foreach (var (expression, expressionIdx) in pattern.Index())
    //            {
    //                expressions.Add(expression);
    //            }
    //        }
    //    }

    //    {
    //        var preserve = lockTree.AddBlendTree("").Motion;
    //        preserve.BlendParameter = ParameterNames.Expression.Index;

    //        preserve.AddAnimationClip("0").Motion.AddAnimatedParameter(ParameterNames.Expression.Index, 0, 0);
    //        preserve.AddAnimationClip($"{expressions.Count}").WithThreshold(expressions.Count).Motion.AddAnimatedParameter(ParameterNames.Expression.Index, 0, expressions.Count);
    //    }
    //}

    private static void GenerateIndexSelectorStates(BuildContext context, AnimatorControllerBuilder animatorController, List<ExpressionData> expressions)
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
                patternAnimation.AddAnimatedParameter(ParameterNames.Expression.Index, 0, 0);
                expressions.Add(new(pattern.Key, patternIdx, pattern.Key, null));

                foreach (var (expression, expressionIdx) in pattern.Index())
                {
                    int index = expressions.Count;
                    var id = expression.GetID();
                    expressions.Add(new (pattern.Key, patternIdx, expression, id));

                    var expressionTree = patternTree.AddDirectBlendTree(expression.Name).Motion;

                    BlendTreeBuilder parent = expressionTree;


                    var fallback = new AnimationClipBuilder() { Name = "Fallback" };
                    fallback.AddAnimatedParameter($"{ParameterNames.Expression.Index}/{id}", 0, 0);

                    var expressionAnimation = new AnimationClipBuilder() { Name = expression.Name };
                    expressionAnimation.AddAnimatedParameter($"{ParameterNames.Expression.Index}/{id}", 0, 1);
                    animatorController.Parameters.AddFloat($"{ParameterNames.Expression.Index}/{id}", 0);
                    foreach (var conditionGroup in expression.Conditions)
                    {
                        float? lastValue = null;

                        foreach(var condition in conditionGroup)
                        {
                            var tree = parent.AddBlendTree(condition.Parameter.Name).WithThreshold(lastValue ?? 0).Motion;
                            tree.BlendParameter = condition.Parameter.Name;
                            animatorController.AddParameter(tree.BlendParameter, AnimatorControllerParameterType.Float);
                            var value = condition.Parameter.Value;
                            lastValue = value;

                            if (condition.Mode is ConditionMode.Equals)
                            {
                                tree.Append(fallback, threshold: value - Epsilon);
                                tree.Append(fallback, threshold: value + Epsilon);
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }

                            parent = tree;
                        }

                        parent.Append(expressionAnimation).WithThreshold(lastValue ?? 0);
                    }
                }
            }
        }
        else
        {
            foreach (var (pattern, patternIdx) in expressionPatterns.Index())
            {
                expressions.Add(new(pattern.Key, patternIdx, pattern.Key, null));
                foreach (var (expression, expressionIdx) in pattern.Index())
                {
                    expressions.Add(new(pattern.Key, patternIdx, expression, expression.GetID()));
                }
            }
        }

        {
            var preserve = lockTree.AddDirectBlendTree("").Motion;

            foreach (var expression in expressions.AsSpan())
            {
                if (expression.Id is null)
                    continue;

                var tree = preserve.AddBlendTree(expression.Expression.Name).Motion;
                tree.BlendParameter = $"{ParameterNames.Expression.Index}/{expression.Id}";

                tree.AddAnimationClip("0").Motion.AddAnimatedParameter(tree.BlendParameter, 0, 0);
                tree.AddAnimationClip("1").Motion.AddAnimatedParameter(tree.BlendParameter, 0, 1);
            }
        }
    }

    private static void GenerateExpressionSelector(BuildContext context, AnimatorControllerBuilder animatorController, List<ExpressionData> expressions)
    {
        var data = context.GetData();

        var layer = animatorController.AddLayer("[ModEmo] Expressions");
        var stateMachine = layer.StateMachine;

        int stateCount = 0;

        var defaultState = stateMachine.AddState("Default", new Vector2((stateMachine.EntryPosition.x + stateMachine.ExitPosition.x) / 2, 120 + 70 * stateCount++));
        
        foreach (var group in expressions.GroupBy(x => x.Pattern))
        {
            var pattern = group.Key;
            var array = group.ToArray();

            for (int i = 0; i < array.Length; i++)
            {
                var expData = array[i];
                var expression = expData.Expression;

                var state = stateMachine.AddState(expression.Name, new Vector2(defaultState.Position!.Value.x, 120 + 70 * stateCount++));

                if (i == 0)
                {
                    var t = stateMachine.AddEntryTransition(state).WithDuration(0.1f);
                    t.AddCondition(AnimatorConditionMode.Less, ParameterNames.Expression.Pattern, expData.PatternIndex + Epsilon);
                    t.AddCondition(AnimatorConditionMode.Greater, ParameterNames.Expression.Pattern, expData.PatternIndex - Epsilon);
                    for (int i2 = 1; i2 < array.Length; i2++)
                    {
                        t.AddCondition(AnimatorConditionMode.Less, $"{ParameterNames.Expression.Index}/{array[i2].Id}", 1);

                        state.AddExitTransition().WithDuration(0.1f)
                            .AddCondition(AnimatorConditionMode.Greater, $"{ParameterNames.Expression.Index}/{array[i2].Id}", 0);

                        state.AddExitTransition().WithDuration(0.1f).AddCondition(AnimatorConditionMode.Greater, ParameterNames.Expression.Pattern, expData.PatternIndex);
                        state.AddExitTransition().WithDuration(0.1f).AddCondition(AnimatorConditionMode.Less, ParameterNames.Expression.Pattern, expData.PatternIndex);
                    }
                }
                else
                {
                    defaultState.AddExitTransition().WithDuration(0.1f).AddCondition(AnimatorConditionMode.Greater, $"{ParameterNames.Expression.Index}/{expData.Id}", 0);

                    var t = stateMachine.AddEntryTransition(state).WithDuration(0.1f)
                        .AddCondition(AnimatorConditionMode.Less, ParameterNames.Expression.Pattern, expData.PatternIndex + Epsilon)
                        .AddCondition(AnimatorConditionMode.Greater, ParameterNames.Expression.Pattern, expData.PatternIndex - Epsilon)
                        .AddCondition(AnimatorConditionMode.Greater, $"{ParameterNames.Expression.Index}/{expData.Id}", 0);

                    state.AddExitTransition().AddCondition(AnimatorConditionMode.Less, $"{ParameterNames.Expression.Index}/{expData.Id}", 1).WithDuration(0.1f);
                    state.AddExitTransition().WithDuration(0.1f).AddCondition(AnimatorConditionMode.Greater, ParameterNames.Expression.Pattern, expData.PatternIndex);
                    state.AddExitTransition().WithDuration(0.1f).AddCondition(AnimatorConditionMode.Less, ParameterNames.Expression.Pattern, expData.PatternIndex);

                    for (int i2 = i - 1; i2 >= 1; i2--)
                    {
                        t.AddCondition(AnimatorConditionMode.Less, $"{ParameterNames.Expression.Index}/{array[i2].Id}", 1);

                        state.AddExitTransition().WithDuration(0.1f)
                            .AddCondition(AnimatorConditionMode.Greater, $"{ParameterNames.Expression.Index}/{array[i2].Id}", 0);
                    }
                }



                state.Motion = expression.MakeAnimationClip(data);
                defaultState.Motion ??= state.Motion;
                state.MotionTime = expression.MotionTime;

                var tr = state.AddTrackingControl();
                tr.Eyes = TrackingType.Animation;
                tr.Mouth = expression.LipSync ? TrackingType.Tracking : TrackingType.Animation;
            }
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