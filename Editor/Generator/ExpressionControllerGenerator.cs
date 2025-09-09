using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Numeira.Animation;
using Debug = UnityEngine.Debug;

namespace Numeira;
internal static class ExpressionControllerGenerator
{
    public static VirtualLayer Generate(BuildContext context)
    {
        var vcc = context.GetVirtualControllerContext();
        var layer = new AnimatorControllerLayer() { name = "[ModEmo] Expressions" };
        var stateMachine = layer.stateMachine = new();

        var modEmo = context.GetModEmoContext().Root;
        var data = context.GetData();

        var expressionPatterns = modEmo.ExportExpressions();
        var usageMap = expressionPatterns
            .SelectMany(x => x)
            .SelectMany(x => x.Frames)
            .SelectMany(x => x.GetBlendShapes())
            .Where(x => data.BlendShapes.TryGetValue(x.Name, out var value) && value.Value != x.Value)
            .Select(x => x.Name)
            .ToHashSet();

        data.Parameters.Add(new(ParameterNames.Expression.Pattern, 0));

        foreach(var (pattern, patternIdx) in expressionPatterns.Index())
        {
            var mac = stateMachine.AddStateMachine(pattern.Key.Name);
        }

        /*
         * 
        var registeredClips = new AnimationData[65];

        foreach ( var (pattern, patternIdx) in expressionPatterns.Index() )
        {
            var defState = stateMachine.AddState(pattern.Key.Name);
            if (pattern.Key.DefaultExpression is null)
            {
                var clip = new AnimationClip() { name = pattern.Key.Name };
                defState.motion = clip;
                registeredClips[1].Clip = clip;

                foreach (var blendShape in usageMap)
                {
                    var bind = new EditorCurveBinding() { path = "", type = typeof(Animator), propertyName = $"{ParameterNames.BlendShapes.Prefix}{blendShape}" };
                    if (!data.BlendShapes.TryGetValue(blendShape, out var defaultValue))
                        defaultValue = new BlendShapeInfo(0, 100);
                    AnimationUtility.SetEditorCurve(clip, bind, AnimationCurve.Constant(0, 0, defaultValue.Value / defaultValue.Max /* TODO: デフォルト値を入れる /));
                }
            }
            {
                var t = stateMachine.AddAnyStateTransition(defState);
                t.canTransitionToSelf = false;
                t.duration = 0.1f;
                t.hasFixedDuration = true;
                t.hasExitTime = false;
                t.AddCondition(AnimatorConditionMode.Equals, patternIdx, ParameterNames.Expression.Pattern);
                t.AddCondition(AnimatorConditionMode.Greater, 1 - 0.01f, ParameterNames.Expression.Index);
                t.AddCondition(AnimatorConditionMode.Less, 1 + 0.01f, ParameterNames.Expression.Index);
            }

            var registeredIndexes = new HashSet<int>();
            var expressions = ((IEnumerable<IModEmoExpression>)pattern);

            registeredClips.AsSpan().Clear();

            var gestures = Enum.GetValues(typeof(Gesture)).Cast<Gesture>();
            var maskAndIndexes = gestures.SelectMany(x => gestures.Select(y => (Left: x, Right: y)))
                .Select(x =>
                {
                    return (Mask: GestureToMask(x.Left, x.Right), Index: GestureToIndex(x.Left, x.Right));
                    static uint GestureToMask(Gesture left, Gesture right) => (0b_0000_0001_0000_0000u << (int)right | 0b_0000_0000_0000_0001u << (int)left);
                    static int GestureToIndex(Gesture left, Gesture right) => 1 + (int)(left) + (int)(right) * 8;
                })
                .ToArray();

            foreach (var expression in expressions.Where(x => x.Mode == ExpressionMode.Default))
            {
                if (expression.Settings.ConditionFolder is { } conditionFolder && conditionFolder.GetComponentsInDirectChildren<IModEmoConditionProvider>()?.FirstOrDefault() is { } condition)
                {
                    var conditionMask = condition.GetConditionMask();
                    var indexes = maskAndIndexes.Where(x => (conditionMask & x.Mask) == conditionMask).Select(x => x.Index);

                    AnimationClip? clip = null;

                    foreach (var index in indexes)
                    {
                        _ = index < registeredClips.Length;
                        if (registeredClips[index].Clip != null)
                            continue;

                        clip ??= MakeAnimationClip(expression);
                        registeredClips[index].Clip = clip;
                        registeredClips[index].TimeParameterName = expression.GameObject?.GetComponent<IModEmoMotionTimeProvider>()?.ParameterName;
                    }
                }

            }

            foreach (var expression in expressions.Where(x => x.Mode == ExpressionMode.Combine))
            {
                if (expression.Settings.ConditionFolder is { } conditionFolder && conditionFolder.GetComponentsInDirectChildren<IModEmoConditionProvider>()?.FirstOrDefault() is { } condition)
                {
                    var conditionMask = condition.GetConditionMask();
                    var indexes = maskAndIndexes.Where(x => (conditionMask & x.Mask) != 0).Select(x => x.Index);
                    AnimationClip? clip = null;
                    Dictionary<AnimationClip, AnimationClip> combinedCache = new();

                    foreach (var index in indexes)
                    {
                        _ = index < registeredClips.Length;
                        ref var registered = ref registeredClips.AsSpan()[index].Clip;
                        if (registered != null)
                        {
                            if (!combinedCache.TryGetValue(registered, out var combined))
                            {
                                combined = MakeAnimationClip(expression, registered);
                                combinedCache.Add(registered, combined);
                            }

                            registered = combined;
                        }
                        else
                        {
                            clip ??= MakeAnimationClip(expression);
                            registered = clip;
                        }
                    }
                }
            }

            AnimationClip MakeAnimationClip(IModEmoExpression expression, AnimationClip? source = null)
            {
                var generator = new AnimationClipGenerator() { Name = source == null ? expression.Name : $"{source.name} + {expression.Name}" };
                if (source != null)
                {
                    foreach (var bind in AnimationUtility.GetCurveBindings(source))
                    {
                        if (!data.BlendShapes.TryGetValue(bind.propertyName, out var value))
                            continue;

                        if (!usageMap.Contains(bind.propertyName))
                            continue;

                        var curve = AnimationUtility.GetEditorCurve(source, bind);
                        var keys = curve.keys;
                        if (!keys.Any(x => x.value != value.Value))
                            continue;

                        foreach(var x in keys)
                        {
                            var bind2 = new EditorCurveBinding() { path = "", type = typeof(Animator), propertyName = $"{ParameterNames.BlendShapes.Prefix}{bind.propertyName}" };

                            generator.Add(bind, x.time, x.value);
                        }
                    }
                }
                else
                {
                    foreach (var blendShape in usageMap)
                    {
                        var bind = new EditorCurveBinding() { path = "", type = typeof(Animator), propertyName = $"{ParameterNames.BlendShapes.Prefix}{blendShape}" };

                        if (!data.BlendShapes.TryGetValue(blendShape, out var defaultValue))
                            defaultValue = new BlendShapeInfo(0, 100);

                        generator.Add(bind, 0, defaultValue.Value / defaultValue.Max);
                    }
                }

                foreach(var frame in expression.Frames)
                {
                    foreach(var blendShape in frame.BlendShapes)
                    {
                        var bind = AnimationUtils.CreateAAPBinding($"{ParameterNames.BlendShapes.Prefix}{blendShape.Name}");
                        if (blendShape.Cancel)
                            bind = AnimationUtils.CreateAAPBinding($"{ParameterNames.Internal.BlendShapes.DisablePrefix}{blendShape.Name}");
                        if (!data.BlendShapes.TryGetValue(blendShape.Name, out var defaultValue))
                            defaultValue = new BlendShapeInfo(0, 100);

                        generator.Add(bind, frame.Keyframe, blendShape.Value / defaultValue.Max);
                    }


                    if (frame.Publisher is { } publisher && publisher.GameObject?.GetComponent<IModEmoExpressionBlinkControl>() is { } blinkCtrl)
                    {
                        var bind = AnimationUtils.CreateAAPBinding(ParameterNames.Blink.Disable);
                        generator.Add(bind, frame.Keyframe, blinkCtrl.Enable ? 0 : 1);
                    }
                }

                return generator.Export();
            }

            foreach(var x in registeredClips.Index().Where(x => x.Value.Clip != null).GroupBy(x => x.Value, x => x.Index, AnimationData.EqualityComparer.Default))
            {
                var state = stateMachine.AddState(x.Key.Clip!.name);
                state.motion = x.Key.Clip!;

                if (x.Key.TimeParameterName != default)
                {
                    state.timeParameterActive = true;
                    state.timeParameter = x.Key.TimeParameterName;
                }

                foreach (var index in x)
                {
                    var t = stateMachine.AddAnyStateTransition(state);
                    t.canTransitionToSelf = false;
                    t.duration = 0.1f;
                    t.hasFixedDuration = true;
                    t.hasExitTime = false;
                    t.AddCondition(AnimatorConditionMode.Equals, patternIdx, ParameterNames.Expression.Pattern);
                    t.AddCondition(AnimatorConditionMode.Greater, index - 0.01f, ParameterNames.Expression.Index);
                    t.AddCondition(AnimatorConditionMode.Less, index + 0.01f, ParameterNames.Expression.Index);
                }
            }

        }

         */
        return vcc.Clone(layer, 0);
    }

    private static IntPtr GetNativePtr<T>(this T obj) where T : Object => Unsafe.As<Tuple<IntPtr, int>>(obj).Item1;

    public unsafe static void Test()
    {
        var blendTree = AssetDatabase.LoadAssetAtPath<BlendTree>(AssetDatabase.GUIDToAssetPath("e6f0338b60cc8f84daf9f95ea5b7812b"));
        blendTree.maxThreshold = float.Epsilon;
        var pointer = Unsafe.As<Tuple<IntPtr, int>>(blendTree).Item1;
        ref var nativeMotion = ref Unsafe.AsRef<NativeObjectLayout>(pointer.ToPointer());

        var ptr = (byte*)Unsafe.AsPointer(ref nativeMotion.First);
        int limit = 512;
        var p = ptr;
        while (limit > 0)
        {
            if (*(float*)p == blendTree.maxThreshold)
            { break; }
            p++;
            limit--;
        }
        Debug.LogError(p - ptr);
        return;
        using var so = new SerializedObject(blendTree);
        var normalizedBlendValues = so.FindProperty("m_NormalizedBlendValues");
        var flag = normalizedBlendValues.boolValue;
        Debug.LogError(flag);
        normalizedBlendValues.boolValue = !flag;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void Generate(BuildContext context, AnimatorControllerBuilder animatorController)
    {
        var modEmo = context.GetModEmoContext().Root;
        var data = context.GetData();

        var expressionPatterns = modEmo.ExportExpressions();
        var usageMap = expressionPatterns
            .SelectMany(x => x)
            .SelectMany(x => x.Frames)
            .SelectMany(x => x.GetBlendShapes())
            .Where(x => data.BlendShapes.TryGetValue(x.Name, out var value) && value.Value != x.Value)
            .Select(x => x.Name)
            .ToHashSet();

        var layer = animatorController.AddLayer("[ModEmo] Expressions");
        var stateMachine = layer.StateMachine.WithDefaultMotion(data.BlankClip);

        animatorController.Parameters.AddFloat(ParameterNames.Expression.Pattern, 0);
        animatorController.Parameters.AddFloat(ParameterNames.Expression.Index, 0);

        DirectBlendTree.DefaultDirectBlendParameter = ParameterNames.Internal.One;
        var blendTree = new DirectBlendTree();

        var patternSwitch = blendTree.AddBlendTree("Pattern Switch");
        patternSwitch.BlendParameter = ParameterNames.Expression.Pattern;
        var patternFallback = new MotionBranch(data.BlankClip);

        const float Epsilon = 0.005f;

        var sw = new Stopwatch();

        foreach (var (pattern, patternIdx) in expressionPatterns.Index())
        {
            var patternTree = patternSwitch.AddDirectBlendTree(pattern.Key.Name, patternIdx);
                        
            IBlendTree nextTree = patternTree;

            foreach(var (expression, expressionIdx) in pattern.Index())
            {
                sw.Restart();
                var expressionTree = nextTree.AddDirectBlendTree(expression.Name);
                nextTree = expressionTree;

                var expressionMotion = expression.MakeDirectAnimationClip("Body");
                var branch = new MotionBranch(expressionMotion);
                foreach (var conditionsOr in expression.Conditions)
                {
                    float? prevValue = null;
                    var fallbackTree = new DirectBlendTree() { Name = "Fallback" };
                    foreach(var conditionsAnd in conditionsOr)
                    {
                        animatorController.Parameters.AddFloat(conditionsAnd.Parameter.Name);

                        var tree = nextTree.AddBlendTree(conditionsAnd.Parameter.Name, prevValue);
                        tree.BlendParameter = conditionsAnd.Parameter.Name;
                        float value = conditionsAnd.Parameter.Value;

                        tree.Append(fallbackTree, value - Epsilon);
                        tree.Append(fallbackTree, value + Epsilon);

                        prevValue = value;
                        nextTree = tree;
                    }
                    nextTree.Append(branch, prevValue);

                    nextTree = fallbackTree;
                }

                sw.Stop();
                Debug.LogError($"{expression.Name} - {sw.ElapsedMilliseconds}ms");
            }

            nextTree.AddMotion(null!);
        }

        var idleState = stateMachine.AddState("DBT (DO NOT OPEN IN EDITOR!) (WD On)");
        idleState.Motion = blendTree.Build();

    }

    private sealed class ConditionExpressionComparer : IEqualityComparer<IModEmoExpression>
    {
        public static ConditionExpressionComparer Instance { get; } = new();
        public bool Equals(IModEmoExpression x, IModEmoExpression y)
            => GetHashCode(x) == GetHashCode(y);

        public int GetHashCode(IModEmoExpression obj)
        {
            HashCode hash = new();
            foreach(var x in obj.Conditions)
            {
                foreach(var y in x)
                {
                    hash.Add(y);
                }
            }
            return hash.ToHashCode();
        }
    }

    private static void ApplyTransitionWithCondition(StateBuilder state, AnimatorParameterCondition condition, Func<StateBuilder, TransitionBuilder> transitionBuilder)
    {
        var mode = condition.Mode;
        var name = condition.Parameter.Name;
        switch (condition.Parameter.Value.Type)
        {
            case AnimatorControllerParameterType.Bool:
                {
                    var value = (bool)condition.Parameter.Value;
                    if (mode is ConditionMode.Equals)
                    {
                        transitionBuilder(state).AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, name, 0);
                    }
                    else
                    {
                        transitionBuilder(state).AddCondition(value ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, name, 0);
                    }
                }
                break;
            case AnimatorControllerParameterType.Int:
                {
                    if (mode.HasFlag(ConditionMode.Equals))
                    {
                        transitionBuilder(state).AddCondition(AnimatorConditionMode.Equals, name, condition.Parameter.Value);
                    }
                    else if (mode.HasFlag(ConditionMode.NotEqual))
                    {
                        transitionBuilder(state).AddCondition(AnimatorConditionMode.NotEqual, name, condition.Parameter.Value);
                    }
                    if (mode.HasFlag(ConditionMode.GreaterThan))
                    {
                        transitionBuilder(state).AddCondition(AnimatorConditionMode.Greater, name, condition.Parameter.Value);
                    }
                    else if (mode.HasFlag(ConditionMode.LessThan))
                    {
                        transitionBuilder(state).AddCondition(AnimatorConditionMode.Greater, name, condition.Parameter.Value);
                    }
                }
                break;
            case AnimatorControllerParameterType.Float:
                {
                    if (mode.HasFlag(ConditionMode.Equals))
                    {
                        transitionBuilder(state)
                            .AddCondition(AnimatorConditionMode.Less, name, condition.Parameter.Value + float.Epsilon)
                            .AddCondition(AnimatorConditionMode.Greater, name, condition.Parameter.Value - float.Epsilon);
                    }
                    else if (mode.HasFlag(ConditionMode.NotEqual))
                    {
                        transitionBuilder(state).AddCondition(AnimatorConditionMode.Less, name, condition.Parameter.Value - float.Epsilon);
                        transitionBuilder(state).AddCondition(AnimatorConditionMode.Greater, name, condition.Parameter.Value + float.Epsilon);
                    }
                    if (mode.HasFlag(ConditionMode.GreaterThan))
                    {
                        transitionBuilder(state).AddCondition(AnimatorConditionMode.Greater, name, condition.Parameter.Value);
                    }
                    else if (mode.HasFlag(ConditionMode.LessThan))
                    {
                        transitionBuilder(state).AddCondition(AnimatorConditionMode.Greater, name, condition.Parameter.Value);
                    }
                }
                break;
        }
    }

    private struct AnimationData
    {
        public AnimationData(AnimationClip clip, string? timeParameterName = null)
        {
            Clip = clip;
            TimeParameterName = timeParameterName;
        }

        public AnimationClip? Clip;
        public string? TimeParameterName;

        public sealed class EqualityComparer : IEqualityComparer<AnimationData>
        {
            public static EqualityComparer Default => new();

            bool IEqualityComparer<AnimationData>.Equals(AnimationData x, AnimationData y) => x.Clip == y.Clip;

            int IEqualityComparer<AnimationData>.GetHashCode(AnimationData obj) => obj.Clip!.GetHashCode();
        }
    }
}



[StructLayout(LayoutKind.Sequential)]
internal record struct NativeObjectLayout
{
    private NamedObject Base;
    public byte First;

    public nint m_Childs; // 248
    public nint m_BlendParameter; // 256
    public nint m_BlendParameterY; // 264
    public float m_MinThreshold; //272
    public float m_MaxThreshold; // 276

    public bool m_UseAutomaticThresholds; // <- 280...?
    public bool m_NormalizedBlendValues;
    public int m_BlendType;

    [StructLayout(LayoutKind.Sequential, Size = 248)]
    private struct Blackbox248Bytes
    {

    }

    public struct NonCopyable
    {
        public IntPtr MethodTable;
    }

    public struct AllocationRootWithSalt
    {
        public uint m_Salt;
        public uint m_RootReferenceIndex;
    }

    public struct MemLabelId
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public AllocationRootWithSalt m_RootReferenceWithSalt;
#endif

        public int identifier;
    }

    public struct ScriptingGCHandle
    {
        public IntPtr m_Handle;
        public int m_Weakness;
        public IntPtr m_Object;
    }

    public struct NativeObject
    {
        public NonCopyable Base;
        public int m_InstanceID;

        // This is represented on the C++ side as several bit-fields.
        public int m_BitFields;

        public IntPtr m_EventIndex;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public MemLabelId m_FullMemoryLabel;
#endif

        public ScriptingGCHandle m_MonoReference;

#if UNITY_EDITOR
        public uint m_DirtyIndex;
        public ulong m_FileIDHint;
        public bool m_IsPreviewSceneObject;
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public int m_ObjectProfilerListIndex;
#endif
    }

    public struct PPtr<T> where T : unmanaged
    {
        public int m_InstanceID;
    }

    public struct NativePrefab
    {
        public NativeObject Base;
        // ...
    }

    public struct NativePrefabInstance
    {
        public NativeObject Base;
        // ...
    }

    public struct EditorExtension
    {
        public NativeObject Base;

#if UNITY_EDITOR
        public PPtr<EditorExtension> m_CorrespondingSourceObject;
        public PPtr<EditorExtension> m_DeprecatedExtensionPtr;
        public PPtr<NativePrefab> m_PrefabAsset;
        public PPtr<NativePrefabInstance> m_PrefabInstance;
        public bool m_IsClonedFromPrefabObject;
#endif
    }

    public struct ConstantString
    {
        public nint m_Buffer;
    }

    public struct NamedObject
    {
        public EditorExtension Base;
        public ConstantString m_Name;

    }
}