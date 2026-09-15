using System.Text;
using nadena.dev.ndmf.util;
using Numeira.Animation;
using UnityEngine.TextCore;

namespace Numeira;

partial class ModEmoExpressionExt
{
    public static MotionBuilder ToMotion<T>(this T expression, BuildContext context, int layerIndex = 0, bool writeDefaultValues = true, bool writeBlink = true) where T : IModEmoExpression, IAnimationSourceComponent
    {
        var data = context.GetData();
        var registry = new Registry(data.FaceInfo, layerIndex, expression.Name);

        var options = new AnimationGeneratorOptions()
        {
            AnimationName = $"{expression.Name}",
            FaceObject = data.Face.transform,
            AvatarRootTransform = context.AvatarRootTransform,
            IsLoop = expression.IsLoop
        };

        if (writeDefaultValues)
        {
            registry.WriteDefaultValues();
        }

        if (writeBlink)
        {
            registry.WriteBlinks(expression.Blink);
        }

        if (expression.EnableMouthMorphCancel)
        {
            registry.WriteMouthMorphCancel(true);
        }

        expression.RegisterAnimations(registry, options);

        return registry.GetResult();
    }

    private sealed partial class Registry : IAnimationRegistry
    {
        public FaceInfo FaceInfo { get; }
        public int LayerIndex { get; }
        private readonly DirectBlendTreeBuilder root;
        private readonly List<IDisposable> disposables;
        private readonly List<IKeyframeWriterContext> writers;
        private AnimationClipBuilder? defaultValueProvider;

        public Registry(FaceInfo faceInfo, int layerIndex, string name)
        {
            this.FaceInfo = faceInfo;
            this.LayerIndex = layerIndex;
            root = new DirectBlendTreeBuilder() { Name = name, DefaultDirectBlendParameter = ParameterNames.Internal.One };
            disposables = new();
            writers = new();
        }

        public Registry(DirectBlendTreeBuilder root, Registry source)
        {
            this.root = root;
            FaceInfo = source.FaceInfo;
            LayerIndex = source.LayerIndex;
            disposables = source.disposables;
            writers = source.writers;
            defaultValueProvider = source.defaultValueProvider;
        }

        public void Append(MotionBuilder motion)
        {
            root.Append(motion);
        }

        public void WriteDefaultValues()
        {
            var anim = defaultValueProvider ??= new();

            foreach (var blendShape in FaceInfo.BlendShapes)
            {
                if (!blendShape.UsageInfo.AllowControl)
                    continue;

                float value = blendShape.Value;
                value /= blendShape.Max;

                if (FaceInfo.RegisterControlBlendshape(blendShape.Name, BlendshapeControlType.Normal, LayerIndex) is { } name)
                    anim.AddAnimatedParameter(name, 0, value);
            }
        }

        public void WriteBlinks(bool enable)
        {
            var anim = defaultValueProvider ??= new();

            anim.AddAnimatedParameter(ParameterNames.Blink.Value, 0, enable ? 1 : 0);
        }

        public void WriteMouthMorphCancel(bool enable)
        {
            var anim = defaultValueProvider ??= new();

            anim.AddAnimatedParameter(ParameterNames.Internal.MouthMorphCancel.Enable, 0, enable ? 1 : 0);
        }

        public IKeyframeWriterContext RegisterAnimation(in AnimationGeneratorOptions options, string? blendParameter = null)
        {
            IKeyframeWriterContext result;
            if (blendParameter is null)
            {
                result = new AnimationClipWriter(this, options);
            }
            else
            {
                result = new BlendTreeWriter(this, options, blendParameter);
            }
            disposables.Add((result as IDisposable)!);
            writers.Add(result);
            return result;
        }

        public ITwoAxisAnimationRegistryFactory RegisterTwoAxisAnimation(string blendParameterX, string blendParameterY)
            => new TwoAxisFactory(this, blendParameterX, blendParameterY);

        public MotionBuilder GetResult()
        {
            var tree = root;
            Print(tree);
            foreach (var disposable in disposables.AsSpan())
                disposable.Dispose();
            disposables.Clear();

            //Optimize(tree);

            static void Optimize(BlendTreeBuilder tree)
            {
                var children = tree.Children.AsSpan();
                bool defrag = false;
                foreach (ref var child in children)
                {
                    var motion = child.Motion;
                    if (motion is BlendTreeBuilder bt)
                    {
                        if (bt.Count == 0)
                        {
                            defrag = true;
                            child = null;
                            continue;
                        }

                        if (bt is DirectBlendTreeBuilder dbt)
                        {
                            if (dbt.Count == 1)
                            {
                                child = dbt.Children[0];
                            }
                        }

                        if (child?.Motion is BlendTreeBuilder bt2)
                            Optimize(bt2);
                    }
                    else if (motion is AnimationClipBuilder acb)
                    {

                    }
                }

                if (defrag)
                {
                    int w = 0;
                    for (int r = 0; r < children.Length; r++)
                    {
                        if (children[r] is not null)
                        {
                            children[w] = children[r];
                            w++;
                        }
                    }
                    tree.Children.SetLength(w);
                }
            }

            if (tree.Children.Count == 1)
                return tree.Children[0].Motion;

            return root;
        }

        private static void Print(DirectBlendTreeBuilder dbt)
        {
            var sb = new StringBuilder();
            Recurse(sb, 0, dbt);
            void Recurse(StringBuilder sb, int indent, MotionBuilder motion)
            {
                sb.Append(' ', indent);

                sb.Append($"{motion.Name}({motion.GetType().Name})");

                if (motion is BlendTreeBuilder bt)
                {
                    foreach(var x in bt.Children)
                    {
                        Recurse(sb, indent + 2, x.Motion);
                    }
                }
                else
                {
                    sb.AppendLine();
                }
            }
        }
    }

    partial class Registry
    {
        private sealed class TwoAxisFactory : ITwoAxisAnimationRegistryFactory
        {
            public TwoDirectionBlendTreeBuilder BlendTree { get; }
            private Registry Parent { get; }

            public TwoAxisFactory(Registry parent, string blendParameterX, string blendParameterY)
            {
                Parent = parent;
                BlendTree = new TwoDirectionBlendTreeBuilder() { Name = $"{parent.root.Name} (Two Axis)", BlendParameterX  = blendParameterX, BlendParameterY = blendParameterY, DefaultDirectBlendParameter = parent.root.DefaultDirectBlendParameter, IsCertein = true, IsFreeform = true };
                parent.Append(BlendTree);
            }

            public IAnimationRegistry GetRegistry(Vector2 position)
            {
                return new Registry(BlendTree.AddDirectBlendTree($"{Parent.root.Name} ({position.x:F2}, {position.y:F2})").WithPosition(position).Motion, Parent);
            }
        }
    }

    partial class Registry
    {
        private class AnimationClipWriter : IKeyframeWriterContext, IDisposable
        {
            protected Registry Parent { get; }
            protected AnimationClipBuilder Clip { get; }

            public AnimationClipWriter(Registry parent, in AnimationGeneratorOptions options)
            {
                Parent = parent;
                var clip = Clip = parent.defaultValueProvider is { } def ? new(def) : new();
                clip.Name = $"{parent.root.Name} - {options.AnimationName}";
                clip.IsLoop = options.IsLoop;
            }

            private readonly DefaultBlendshapeRecords defaultBlendshapeRecords = new();

            private bool IsFaceObject(Transform target) => target == Parent.FaceInfo.Renderer.transform;

            public void AddBlendshape(Transform target, string name, float time, float value)
            {
                bool isFaceObject = IsFaceObject(target);
                var faceInfo = Parent.FaceInfo;

                if (isFaceObject)
                {
                    if (!faceInfo.BlendshapeMap.TryGetValue(name, out var info) || faceInfo.RegisterControlBlendshape(name, BlendshapeControlType.Normal, Parent.LayerIndex) is not { } x)
                        return;

                    // normalize
                    value /= info.Max;
                    
                    Clip.AddAnimatedParameter(x, time, value);
                    defaultBlendshapeRecords.Add(name, time, BlendshapeControlType.Normal, true);
                }
                else
                {
                    Clip.Add(new() { type = typeof(SkinnedMeshRenderer), path = target.AvatarRootPath(), propertyName = $"blendShape.{name}" }, time, value);
                    defaultBlendshapeRecords.Add(name, time, BlendshapeControlType.Normal, false);
                }
            }

            public void AddBlendshapeDefault(Transform target, string name)
            {
                if (defaultBlendshapeRecords.Contains(name, BlendshapeControlType.Normal, IsFaceObject(target)))
                    return;

                float value = 0;
                if (Parent.FaceInfo.BlendshapeMap.TryGetValue(name, out var info))
                    value = info.Value;


                AddBlendshape(target, name, 0, value);
            }
            
            public void AddCancelBlendshape(Transform target, string name, float time, float value)
            {
                var faceInfo = Parent.FaceInfo;
                if (IsFaceObject(target))
                {
                    if (!faceInfo.BlendshapeMap.TryGetValue(name, out var info) || faceInfo.RegisterControlBlendshape(name, BlendshapeControlType.Cancel, Parent.LayerIndex) is not { } x)
                        return;

                    // normalize
                    value /= info.Max;

                    Clip.AddAnimatedParameter(x, time, value);
                    defaultBlendshapeRecords.Add(name, time, BlendshapeControlType.Cancel, true);
                }
            }

            public void AddCancelBlendshapeDefault(Transform target, string name)
            {
                if (!IsFaceObject(target) || defaultBlendshapeRecords.Contains(name, BlendshapeControlType.Cancel, true))
                    return;

                AddCancelBlendshape(target, name, 0, 0);
            }

            public void AddRotation(Transform target, float time, Vector3 eularAngle, bool relative = true)
            {
                if (target.AvatarRootPath() is not { } path)
                    return;

                var defAngle = target.localEulerAngles;

                var propertyNameBase = $"m_LocalEulerAngles.";
                var bindBase = new EditorCurveBinding() { type = typeof(Transform), path = path, propertyName = "m_LocalEulerAngles" };

                var eularAxis = "xyz";

                for (int i = 0; i < eularAxis.Length; i++)
                {
                    var bind = bindBase with { propertyName = $"{propertyNameBase}{eularAxis[i]}" };
                    Clip.Add(bind, 0, defAngle[i]);
                    var angle = eularAngle[i];
                    if (relative)
                        angle += defAngle[i];
                    Clip.Add(bind, time, angle);
                }
            }

            public void SetAnimatorParameter<T>(string name, float time, T value)
            {
                Clip.AddAnimatedParameter(name, time, AnimationUtils.ConvertToAnimatorFloat(value));
            }

            public void SetAvatarParameter<T>(string name, T value)
            {
                throw new NotImplementedException();
            }

            public virtual void Dispose()
            {
                Parent.Append(Clip);
            }
        }

        private sealed class BlendTreeWriter : AnimationClipWriter
        {
            private string BlendParameter { get; }
            public BlendTreeWriter(Registry parent, in AnimationGeneratorOptions options, string blendParameter) : base(parent, options)
            {
                BlendParameter = blendParameter;
            }

            public override void Dispose()
            {
                var clip = Clip;
                var bindings = clip.Bindings.ToArray();

                HashSet<float> timesMap = new();
                foreach (var binding in bindings)
                {
                    foreach(var key in clip[binding])
                    {
                        timesMap.Add(key.Time);
                    }
                }

                if (timesMap.Count == 0)
                    return;

                var tree = new OneDirectionBlendTreeBuilder() { Name = clip.Name, BlendParameter = BlendParameter, };

                foreach (var time in timesMap.OrderBy(x => x))
                {
                    var anim = tree.AddAnimationClip($"{tree.Name} (Frame {time:F3})").Motion;
                    foreach (var binding in bindings)
                    {
                        if (clip.Evaluate(binding, time) is { } value)
                            anim.Add(binding, 0, value);
                    }
                }


                Parent.Append(tree);
            }
        }
    }
}
