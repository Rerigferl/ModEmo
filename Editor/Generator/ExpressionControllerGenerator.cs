using System.Linq.Expressions;
using VRC.SDK3.Avatars.Components;

namespace Numeira;

internal static class ExpressionControllerGenerator
{
    public static VirtualLayer Generate(BuildContext context)
    {
        var vcc = context.GetVirtualControllerContext();
        var layer = new AnimatorControllerLayer() { name = "[ModEmo] Expressions" };
        var stateMachine = layer.stateMachine = new();

        var idle = stateMachine.AddState("Idle");


        var modEmo = context.GetModEmoContext().Root;
        var data = context.GetData();

        var expressionPatterns = modEmo.ExportExpressions();

        var blendTree = new DirectBlendTree();

        foreach( var pattern in expressionPatterns )
        {
            foreach (var expression in pattern)
            {
                foreach(var condition in expression.Conditions.SelectMany(x => x.ToAnimatorConditions()))
                {

                }

                var state = stateMachine.AddState(expression.Name);
                var t = stateMachine.AddAnyStateTransition(state);
                t.canTransitionToSelf = false;
                t.conditions = expression.Conditions.SelectMany(x => x.ToAnimatorConditions()).ToArray();

                var clip = new AnimationClip() { name = expression.Name };
                state.motion = clip;

                foreach (var blendShape in expression.BlendShapes)
                {
                    var bind = new EditorCurveBinding() { path = "", type = typeof(Animator), propertyName = $"{ParameterNames.BlendShapes.Prefix}{blendShape.Name}" };
                    AnimationUtility.SetEditorCurve(clip, bind, AnimationCurve.Constant(0,0,blendShape.Value));
                }

            }
            break;
        }

        return vcc.Clone(layer, 0);
    }
}
