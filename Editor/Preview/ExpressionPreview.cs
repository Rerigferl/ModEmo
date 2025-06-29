using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BestHTTP;
using nadena.dev.ndmf.preview;

namespace Numeira;

internal sealed class ExpressionPreview : IRenderFilter
{
    public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
    {
        var result = Iterate(context).ToImmutableList();
        
        return result;
        static IEnumerable<RenderGroup> Iterate(ComputeContext context)
        {
            foreach(var root in context.GetAvatarRoots())
            {
                if (!context.ActiveInHierarchy(root))
                    continue;

                var component = context.GetComponentsInChildren<ModEmo>(root, true).FirstOrDefault(x => context.ActiveAndEnabled(x));
                if (component == null)
                    continue;

                var renderer = context.Observe(component, x => x.Settings.Face.Clone().Get(component)?.GetComponent<SkinnedMeshRenderer>());
                if (renderer == null)
                    continue;

                yield return RenderGroup.For(renderer).WithData(component);
            }
            yield break;
        }
    }

    public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
    {
        return Task.FromResult<IRenderFilterNode>(new Node(group, proxyPairs, context));
    }

    internal sealed class Node : IRenderFilterNode
    {
        public RenderAspects WhatChanged => RenderAspects.Shapes;

        private readonly ModEmo rootComponent;
        private readonly ImmutableDictionary<string, ModEmoData.BlendShapeInfo> blendShapeInfos;
        private readonly Renderer originalRenderer;
        private IModEmoExpression? selectedExpression;
        private DateTime selectionChangedTime;
        private AnimationClip? animaton;
        private IDisposable? sceneReflesher;

        public Node(RenderGroup renderGroup, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            originalRenderer = proxyPairs.FirstOrDefault().Item1;
            blendShapeInfos = ModEmoData.GetBlendShapeInfos(renderGroup.Renderers[0] as SkinnedMeshRenderer);
            rootComponent = renderGroup.GetData<ModEmo>();

            foreach (var x in context.GetComponentsInChildren<IModEmoExpressionFrame>(rootComponent.gameObject, true))
            {
                context.Observe(x as ModEmoTagComponent, x => x?.GetHashCode() ?? 0);
            }
        }

        public Node(Node source, ComputeContext context)
        {
            originalRenderer = source.originalRenderer;
            blendShapeInfos = source.blendShapeInfos;
            rootComponent = source.rootComponent;

            foreach (var x in context.GetComponentsInChildren<IModEmoExpressionFrame>(rootComponent.gameObject, true))
            {
                context.Observe(x as ModEmoTagComponent, x => x?.GetHashCode() ?? 0);
            }
        }

        public void OnFrame(Renderer original, Renderer proxy)
        {
            if (selectedExpression is not { } expression)
                return;

            if (animaton is not { } clip || clip == null)
                return;

            if (proxy is not SkinnedMeshRenderer smr || smr.sharedMesh is not { } mesh || mesh == null)
                return;

            static IEnumerable<(string PropertyName, float Value)> Sample(AnimationClip clip, float normalizedTime)
            {
                float time = normalizedTime * clip.length;

                foreach (var bind in AnimationUtility.GetCurveBindings(clip))
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, bind);
                    if (curve == null)
                        continue;
                    float value = curve.Evaluate(time);

                    var propertyName = bind.propertyName;
                    yield return ($"{propertyName}", value);
                }
            }

            var time = (DateTime.Now - selectionChangedTime).TotalSeconds;
            if (selectedExpression.Loop)
            {
                time = (time * 0.5) % 1;
            }
            else
            {
                time = (Math.Clamp(Math.Sin(time), -0.2, 0.2) + 0.2) / 0.4;
            }

            foreach (var (name, value) in Sample(clip, (float)time))
            {
                int index = mesh.GetBlendShapeIndex(name);
                if (index == -1)
                    continue;
                smr.SetBlendShapeWeight(index, value);
            }
        }

        public Task<IRenderFilterNode?> Refresh(IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context, RenderAspects updatedAspects)
        {
            if (originalRenderer == proxyPairs.FirstOrDefault().Item1)
            {
                sceneReflesher?.Dispose();
                sceneReflesher = null;
                return Task.FromResult<IRenderFilterNode?>(new Node(this, context));
            }

            return Task.FromResult<IRenderFilterNode?>(null);
        }

        public void OnFrameGroup()
        {
            var selectedExpression = GetSelectedExpression();

            if (selectedExpression != this.selectedExpression)
            {
                selectionChangedTime = DateTime.Now;
                sceneReflesher?.Dispose();
                sceneReflesher = null;

                if (animaton != null)
                    Object.DestroyImmediate(animaton);

                if (selectedExpression != null)
                {
                    animaton = selectedExpression.MakeAnimationClip(blendShapeInfos, forPreviewMode: true);
                    sceneReflesher = SceneViewReflesher.BeginReflesh();
                }
            }    
            
            this.selectedExpression = selectedExpression;

            IModEmoExpression? GetSelectedExpression()
            {
                var active = Selection.activeGameObject;
                if (active == null) 
                    return null;

                if (active.GetComponentInParent<IModEmoExpression>() is not { } expression)
                    return null;

                if (expression.Component!.GetComponentInParent<ModEmo>() != rootComponent)
                    return null;

                return expression;
            }
        }

        public void Dispose()
        {
            if (animaton != null)
                Object.DestroyImmediate(animaton);

            sceneReflesher?.Dispose();
        }
    }
}

internal static class SceneViewReflesher
{
    private static Stack<bool> refleshStack = new();

    static SceneViewReflesher()
    {
        EditorApplication.update += () =>
        {
            if (refleshStack.Count == 0)
                return;

            if (!EditorApplication.isFocused)
                return;

            SceneView.RepaintAll();
        };
    }

    public static IDisposable BeginReflesh()
    {
        refleshStack.Push(true);
        return RefleshDisposer.Instance;
    }

    private sealed class RefleshDisposer : IDisposable
    {
        public static readonly RefleshDisposer Instance = new();
        public void Dispose()
        {
            refleshStack.Pop();
        }
    }
}