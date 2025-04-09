using System.Reflection;
using System.Reflection.Emit;

namespace Numeira;
public class DirectBlendTree : BlendTreeBase
{
    public string DirectBlendParameter { get; set; } = "1";

    public BlendTree Build(Object? assetContainer = null)
    {
        if (string.IsNullOrEmpty(DirectBlendParameter))
            DirectBlendParameter = "1";
        var tree = CreateDirectBlendTree();
        tree.name = $"{tree.name}(WD On)";
        void Recursive(BlendTree tree)
        {
            var children = tree.children;
            foreach (ref var x in children.AsSpan())
            {
                if (x.motion is BlendTree bt)
                {
                    bt.hideFlags |= HideFlags.HideInHierarchy;
                    if (assetContainer != null)
                        AssetDatabase.AddObjectToAsset(bt, assetContainer);

                    Recursive(bt);
                }
            }
            tree.children = children;
        }
        Recursive(tree);
        return tree;
    }

    protected override void Build(BlendTree blendTree, float? threshold = null)
    {
        blendTree.AddChild(CreateDirectBlendTree(), threshold ?? 0);
    }

    private BlendTree CreateDirectBlendTree()
    {
        var blendTree = new BlendTree();
        blendTree.name = Name;
        blendTree.blendType = BlendTreeType.Direct;
        SetNormalizedBlendValues(blendTree, false);
        foreach (var (child, _) in Children)
        {
            child.Build(blendTree);
        }
        int count = blendTree.children.Length;
        for (int i = 0; i < count; i++)
        {
            blendTree.SetDirectBlendTreeParameter(i, DirectBlendParameter);
        }
        return blendTree;
    }

    private static void SetNormalizedBlendValues(BlendTree blendTree, bool value)
    {
        using var so = new SerializedObject(blendTree);
        so.FindProperty("m_NormalizedBlendValues").boolValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public IEnumerable<AnimatorParameter> GetAnimatorParameters()
    {
        var list = new List<AnimatorParameter>();
        GetAnimatorParameters(list);
        return list;
    }

    public void GetAnimatorParameters(List<AnimatorParameter> result)
    {
        CorrectUsageAnimatorParameters(result);
    }

    protected override void CorrectUsageAnimatorParameters(List<AnimatorParameter> result)
    {
        result.Add(new(DirectBlendParameter, 1f));
        base.CorrectUsageAnimatorParameters(result);
    }

}

internal static class BlendTreeAccessor
{
    private delegate void SetDirectBlendTreeParameterDelegate(BlendTree blendTree, int index, string name);
    private static readonly SetDirectBlendTreeParameterDelegate _SetDirectBlendTreeParameter;

    static BlendTreeAccessor()
    {
        _SetDirectBlendTreeParameter = MakeAccessor<SetDirectBlendTreeParameterDelegate>(nameof(SetDirectBlendTreeParameter));
    }

    public static void SetDirectBlendTreeParameter(this BlendTree blendTree, int index, string name)
        => _SetDirectBlendTreeParameter.Invoke(blendTree, index, name);

    private static T MakeAccessor<T>(string name, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) where T : Delegate
    {
        var invoke = typeof(T).GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var parameters = invoke.GetParameters().Select(x => x.ParameterType).ToArray();
        var method = new DynamicMethod(typeof(T).Name, invoke.ReturnType, parameters);
        var original = typeof(BlendTree).GetMethod(name, bindingFlags);
        var il = method.GetILGenerator();
        for( var i = 0; i < parameters.Length; i++)
        {
            if (i < 4)
            {
                il.Emit(i switch
                {
                    0 => OpCodes.Ldarg_0,
                    1 => OpCodes.Ldarg_1,
                    2 => OpCodes.Ldarg_2,
                    _ => OpCodes.Ldarg_3,
                });
            }
            else
            {
                il.Emit(i switch
                {
                    < byte.MaxValue => OpCodes.Ldarg_S,
                    _ => OpCodes.Ldarg,
                }, i);
            }
        }

        il.Emit(original.IsStatic || !original.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, original);
        il.Emit(OpCodes.Ret);

        return (T)method.CreateDelegate(typeof(T));
    }
}
