using System.Diagnostics;
using System.Reflection;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf.vrchat;
using Numeira.Animation;
using VRC.SDK3.Avatars.Components;
using Debug = UnityEngine.Debug;

[assembly: ExportsPlugin(typeof(Numeira.ModEmoPluginDefinition))]

namespace Numeira;

internal sealed class ModEmoPluginDefinition : Plugin<ModEmoPluginDefinition>
{
    internal const string ArtifactCachePath = "Packages/numeira.mod-emo/__Generated/";
    protected override void Configure()
    {
        InPhase(BuildPhase.Transforming)
            .BeforePlugin("nadena.dev.modular-avatar")
            .AfterPlugin("net.rs64.tex-trans-tool")
            .WithRequiredExtensions(new[] { typeof(ModEmoContext) }, 
            sequence =>
            {
                sequence
                    .Run(GeneratingPass.Instance)
                    .PreviewingWith(new ExpressionPreview());
            });
    }

    [DependsOnContext(typeof(VirtualControllerContext))]
    public sealed class ModEmoContext : IExtensionContext
    {
        private ModEmoTagComponent[]? components;

        public ReadOnlySpan<ModEmoTagComponent> Components => components;

        public ModEmo Root { get; private set; } = null!;

        public void OnActivate(BuildContext context)
        {
            var components = this.components = context.AvatarRootObject.GetComponentsInChildren<ModEmoTagComponent>(true);
            Root = (components.FirstOrDefault(x => x is ModEmo) as ModEmo)!;
        }

        public void OnDeactivate(BuildContext context)
        {
            if (components is null)
                return;

            foreach(var component in components.OrderByDescending(x => x.GetType().GetCustomAttribute<RequireComponent>(true) != null ? 1 : 0))
            {
                Object.DestroyImmediate(component);
            }
        }

    }

    public sealed class GeneratingPass : Pass<GeneratingPass>
    {
        protected override void Execute(BuildContext context)
        {
            var modEmo = context.GetModEmoContext().Root;
            if (modEmo == null || !modEmo.gameObject.activeInHierarchy || !modEmo.enabled )
                return;

            int? modEmoHash = modEmo.Settings.UseCache ? modEmo.GetHashCode() : null;
            AnimatorController? animatorController = null;
            if (modEmoHash is not null)
            {
                var cache = ModEmoGeneratedArtifactsCache.FindCache(modEmoHash.Value);
                animatorController = cache == null ? null : AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(cache));
                if (animatorController != null)
                    Debug.LogError($"[ModEmo] Cache found, Hash: {modEmoHash}");
                else
                    Debug.LogError($"[ModEmo] Cache not found, Hash: {modEmoHash}");
            }

            if (animatorController == null)
            {
                var builder = new AnimatorControllerBuilder() { Name = "ModEmo" };
                builder.Parameters
                    .AddFloat(ParameterNames.Internal.One, 1f)
                    .AddFloat(ParameterNames.Internal.SmoothAmount, modEmo.Settings.SmoothFactor);

                if (context.PlatformProvider.QualifiedName == WellKnownPlatforms.VRChatAvatar30)
                {
                    GestureWeightSmootherGenerator.Generate(context, builder);
                }
                else
                {
                    throw new NotImplementedException($"TargetPlatform `{context.PlatformProvider.DisplayName}` is not supported");
                }

                ExpressionControllerGenerator.Generate(context, builder);
                BlendShapeControllerGenerator.Generate(context, builder);

                var assetContainer = new AssetContainer();
                Stopwatch sw = Stopwatch.StartNew();
                animatorController = builder.ToAnimatorController(assetContainer);
                sw.Stop();
                Debug.LogError($"Build AC: {sw.ElapsedMilliseconds}ms");

                if (modEmoHash is not null)
                {
                    AssetDatabase.StartAssetEditing();
                    try
                    {
                        System.IO.Directory.CreateDirectory(ArtifactCachePath);
                        var cache = ScriptableObject.CreateInstance<ModEmoGeneratedArtifactsCache>();
                        cache.HashCode = modEmoHash.Value;
                        cache.Expires = DateTime.Now.AddDays(2);
                        var path = AssetDatabase.GenerateUniqueAssetPath($"{ArtifactCachePath}ModEmo-{context.AvatarRootObject.name}-{(uint)modEmoHash.Value}.asset");
                        AssetDatabase.CreateAsset(cache, path);
                        foreach (var x in assetContainer.Assets)
                        {
                            if (EditorUtility.IsPersistent(x))
                                continue;

                            if (x is not AnimatorController)
                                x.hideFlags |= HideFlags.HideInHierarchy;
                            AssetDatabase.AddObjectToAsset(x, cache);
                        }
                    }
                    finally
                    {
                        AssetDatabase.StopAssetEditing();
                    }
                }
                else
                {
                    context.AssetSaver.SaveAssets(assetContainer.Assets);
                }
            }

            // Disable default
            if (context.PlatformProvider.QualifiedName == WellKnownPlatforms.VRChatAvatar30)
            {
                var descriptor = context.VRChatAvatarDescriptor();
                descriptor.customEyeLookSettings.eyelidsBlendshapes[0] = -1;
            }

            var ma = modEmo.gameObject.AddComponent<ModularAvatarMergeAnimator>();
            ma.pathMode = MergeAnimatorPathMode.Absolute;
            ma.matchAvatarWriteDefaults = true;
            ma.layerType = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.FX;
            ma.animator = animatorController;

            new MenuGenerator(context).Generate();
        }
    }
}

static partial class Ext
{
    public static void AddLayers(this VirtualAnimatorController controller, LayerPriority priority, params VirtualLayer[] layers)
    {
        foreach (var layer in layers)
        {
            controller.AddLayer(priority, layer);
        }
    }
}