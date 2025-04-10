namespace Numeira;

internal static class BlendShapeControllerGenerator
{
    public static VirtualLayer Generate(BuildContext context)
    {
        var rootTree = new DirectBlendTree();
        var vcc = context.GetVirtualControllerContext();
        var layer = VirtualLayer.Create(vcc.CloneContext, "[ModEmo] Face BlendShapes");
        var modEmo = context.GetModEmoContext().Root;
        var data = context.GetData();

        var usageMap = modEmo.ExportExpressions().SelectMany(x => x).SelectMany(x => x.BlendShapes).Select(x => x.Name).ToHashSet();

        foreach (var (key, shapes) in data.CategorizedBlendShapes.OrderBy(x => x.Key))
        {
            foreach (var (name, blendShape) in shapes)
            {
                if (!usageMap.Contains(name))
                    continue;

                var categoryKey = key[key.IndexOf(" ")..];
                var category = rootTree.Find<DirectBlendTree>(categoryKey);
                category ??= rootTree.AddDirectBlendTree(categoryKey);

                var root = category.AddDirectBlendTree(name);

                var parameterName = $"{ParameterNames.BlendShapes.Prefix}{name}";
                var overrideParameterName = $"{ParameterNames.Internal.BlendShapes.OverridePrefix}{name}";
                var controlParameterName = $"{ParameterNames.Internal.BlendShapes.ControlPrefix}{name}";
                var disableParameterName = $"{ParameterNames.Internal.BlendShapes.DisablePrefix}{name}";
                
                data.Parameters.Add(new(parameterName, 0f));
                data.Parameters.Add(new(overrideParameterName, 0f, AnimatorParameterType.Float));
                data.Parameters.Add(new(controlParameterName, 0f));
                data.Parameters.Add(new(disableParameterName, 0f));

                // ParameterMix
                {
                    var zero = new AnimationClip() { name = $"{name} Min" };
                    var one = new AnimationClip() { name = $"{name} Max" };

                    var bind = new EditorCurveBinding() { path = "", propertyName = $"{controlParameterName}", type = typeof(Animator) };
                    AnimationUtility.SetEditorCurve(zero, bind, AnimationCurve.Constant(0, 0, 0));
                    AnimationUtility.SetEditorCurve(one, bind, AnimationCurve.Constant(0, 0, 1));

                    var gate = root.AddBlendTree("Gate");
                    gate.BlendParameter = overrideParameterName;

                    var disableGate = gate.AddBlendTree("Disable Gate", 0);
                    disableGate.BlendParameter = disableParameterName;

                    var normal = disableGate.AddBlendTree("Normal", 0);
                    normal.BlendParameter = parameterName;
                    normal.AddMotion(zero);
                    normal.AddMotion(one);

                    disableGate.AddMotion(zero, 1f);

                    var @override = gate.AddBlendTree("Override", 0.0001f);
                    @override.BlendParameter = gate.BlendParameter;
                    @override.AddMotion(zero);
                    @override.AddMotion(one);
                }

                // Control
                {
                    var zero = new AnimationClip() { name = $"{name} Min" };
                    var one = new AnimationClip() { name = $"{name} Max" };

                    var bind = new EditorCurveBinding() { path = modEmo.Settings.Face.referencePath, propertyName = $"blendShape.{name}", type = typeof(SkinnedMeshRenderer) };
                    AnimationUtility.SetEditorCurve(zero, bind, AnimationCurve.Constant(0, 0, 0));
                    AnimationUtility.SetEditorCurve(one, bind, AnimationCurve.Constant(0, 0, blendShape.Max));

                    var tree = root.AddBlendTree("Control");
                    tree.AddMotion(zero);
                    tree.AddMotion(one);

                    tree.BlendParameter = controlParameterName;
                }
            }
        }

        layer.StateMachine!.AddState("DirectBlendTree (WD On)", vcc.Clone(rootTree.Build(context.AssetContainer)));
        return layer;
    }
}

internal static class InputConverterGenerator
{

    public static VirtualLayer Generate(BuildContext context)
    {
        var rootTree = new DirectBlendTree();
        var vcc = context.GetVirtualControllerContext();
        var layer = VirtualLayer.Create(vcc.CloneContext, "[ModEmo] Input Converter");
        var data = context.GetData();

        string[] dirs = { "Left", "Right" };

        foreach (var dir in dirs)
        {
            var tree = rootTree.AddBlendTree(dir);
            tree.BlendParameter = $"Gesture{dir}";

            var zero = new AnimationClip() { name = $"Input {dir} Min" };
            var one = new AnimationClip() { name = $"Input {dir} Max" };

            var bind = new EditorCurveBinding() { path = "", propertyName = $"{ParameterNames.Internal.Input.Prefix}{dir}", type = typeof(Animator) };
            AnimationUtility.SetEditorCurve(zero, bind, AnimationCurve.Constant(0, 0, 0));
            AnimationUtility.SetEditorCurve(one, bind, AnimationCurve.Constant(0, 0, 7));

            tree.AddMotion(zero, 0);
            tree.AddMotion(one, 7);

            data.Parameters.Add(new(tree.BlendParameter, 0));
            data.Parameters.Add(new(bind.propertyName, 0f));
        }

        layer.StateMachine!.AddState("DirectBlendTree (WD On)", vcc.Clone(rootTree.Build(context.AssetContainer)));
        return layer;
    }
}