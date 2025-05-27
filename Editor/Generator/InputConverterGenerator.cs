namespace Numeira;

internal static class InputConverterGenerator
{
    public static VirtualLayer Generate(BuildContext context)
    {
        return GenerateVRChatLayer(context);
    }

    private static VirtualLayer GenerateVRChatLayer(BuildContext context)
    {
        var vcc = context.GetVirtualControllerContext();
        var layer = new AnimatorControllerLayer() { name = "[ModEmo] Input Converter" };
        var stateMachine = layer.stateMachine = new();

        var array = (Enum.GetValues(typeof(Gesture)) as Gesture[])!;

        var data = context.GetData();
        data.Parameters.Add(new("GestureLeft", 0f));
        data.Parameters.Add(new("GestureRight", 0f));
        data.Parameters.Add(new(ParameterNames.Expression.Index, 0f));
        data.Parameters.Add(new(ParameterNames.Internal.Input.Switch, 1f));
        data.Parameters.Add(new(ParameterNames.Internal.Input.Override, 0f));

        var dbt = new DirectBlendTree();

        var @lock = dbt.AddBlendTree("Lock");
        @lock.BlendParameter = ParameterNames.Internal.Input.Override;

        var @switch = @lock.AddBlendTree("Switch");
        @switch.BlendParameter = ParameterNames.Internal.Input.Switch;
        var sides = new[] { "Left", "Right", "Left" };

        Dictionary<int, (AnimationClip Min, AnimationClip Max)> cache = new();

        for (int i = 0; i < 2; i++)
        {
            var side = sides[i];
            var root = @switch.AddBlendTree(side);
            root.BlendParameter = $"Gesture{side}";

            foreach (var gesture in array)
            {
                var tree = root.AddBlendTree($"{gesture}", (int)gesture);
                tree.BlendParameter = $"Gesture{sides[i + 1]}";

                int baseLine = (int)gesture * array.Length;
                if (!cache.TryGetValue(baseLine, out var clips))
                {
                    var min = new AnimationClip() { name = $"{array[0]}" };
                    var max = new AnimationClip() { name = $"{array[^1]}" };
                    AnimationUtility.SetEditorCurve(min, AnimationUtils.CreateAAPBinding(ParameterNames.Expression.Index), AnimationCurve.Constant(0, 0, 1 + baseLine));
                    AnimationUtility.SetEditorCurve(max, AnimationUtils.CreateAAPBinding(ParameterNames.Expression.Index), AnimationCurve.Constant(0, 0, 8 + baseLine));
                    clips = (min, max);
                    cache.Add(baseLine, clips);
                }
                tree.AddMotion(clips.Min, 0);
                tree.AddMotion(clips.Max, 7);
            }
        }

        {
            var tree = @lock.AddBlendTree("Lock");
            tree.BlendParameter = ParameterNames.Expression.Index;
            var min = new AnimationClip() { name = $"0" };
            var max = new AnimationClip() { name = $"63" };
            AnimationUtility.SetEditorCurve(min, AnimationUtils.CreateAAPBinding(ParameterNames.Expression.Index), AnimationCurve.Constant(0, 0, 0));
            AnimationUtility.SetEditorCurve(max, AnimationUtils.CreateAAPBinding(ParameterNames.Expression.Index), AnimationCurve.Constant(0, 0, 63));
            tree.AddMotion(min, 0);
            tree.AddMotion(max, 63);
        }


        {  
            var state = stateMachine.AddState("(WD ON)");
            state.motion = dbt.Build(context.AssetContainer);
        }

        return vcc.Clone(layer, 0);
        /*
        var vcc = context.GetVirtualControllerContext();
        var layer = VirtualLayer.Create(vcc.CloneContext, "[ModEmo] Input Converter");
        var data = context.GetData();

        var array = (Enum.GetValues(typeof(Gesture)) as Gesture[])!;

        var stateMachine = layer.StateMachine!;

        var rootTree = new DirectBlendTree();
        var min = new AnimationClip() { name = $"Input Min" };
        var max = new AnimationClip() { name = $"Input Max" };
        

        var left = rootTree.AddBlendTree("GestureLeft");
        left.BlendParameter = "GestureLeft";

        left.AddMotion(min, 0);
        var right = left.AddBlendTree("GestureRight", array.Length - 1);
        right.BlendParameter = "GestureRight";
        right.AddMotion(min, 0);
        right.AddMotion(max, array.Length - 1);

        var bind = AnimationUtils.CreateAAPBinding(ParameterNames.Internal.Expression.Index);
        AnimationUtility.SetEditorCurve(min, bind, AnimationCurve.Constant(0, 0, 0));
        AnimationUtility.SetEditorCurve(max, bind, AnimationCurve.Constant(0, 0, array.Length * array.Length));


        data.Parameters.Add(new(left.BlendParameter, 0f));
        data.Parameters.Add(new(right.BlendParameter, 0f));
        data.Parameters.Add(new(bind.propertyName, 0));

        layer.StateMachine!.AddState("DirectBlendTree (WD On)", vcc.Clone(rootTree.Build(context.AssetContainer)));
        return layer;
        //*/
    }
}
