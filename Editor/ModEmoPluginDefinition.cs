using System.Diagnostics;
using System.Reflection;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf.vrchat;
using Numeira.Animation;
using Debug = UnityEngine.Debug;

[assembly: ExportsPlugin(typeof(Numeira.ModEmoPluginDefinition))]

namespace Numeira;

internal sealed partial class ModEmoPluginDefinition : Plugin<ModEmoPluginDefinition>
{
    public override string DisplayName => "ModEmo";
    public override string QualifiedName => "numeira.mod-emo";

    internal const string ArtifactCachePath = "Packages/numeira.mod-emo/__Generated/";
    protected override void Configure()
    {
#if ZATOOLS
        InPhase(BuildPhase.Transforming)
            .BeforePlugin("org.kb10uy.zatools")
            .Run(ExistingBlendshapeModifierPass.Instance)
            .PreviewingWith(new BlendShapeModifierPreview());
#endif
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

            foreach (var component in components.OrderByDescending(x => x.GetType().GetCustomAttribute<RequireComponent>(true) != null ? 1 : 0))
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
            if (modEmo == null || !modEmo.gameObject.activeInHierarchy || !modEmo.enabled)
                return;

            var builder = new AnimatorControllerBuilder() { Name = "ModEmo" };
            builder.Parameters
                .AddFloat(ParameterNames.Internal.One, 1f)
                .AddFloat(ParameterNames.IsLocal, 1f)
                .AddFloat(ParameterNames.Internal.SmoothAmount, modEmo.Settings.SmoothFactor);

            if (context.PlatformProvider.QualifiedName == WellKnownPlatforms.VRChatAvatar30)
            {
                builder.Parameters
                    .AddFloat("GestureLeft", 0)
                    .AddFloat("GestureRight", 0);

                GestureWeightSmootherGenerator.Generate(context, builder);
            }
            else
            {
                throw new NotImplementedException($"TargetPlatform `{context.PlatformProvider.DisplayName}` is not supported");
            }

            ExpressionControllerGenerator.Generate(context, builder);
            BlendShapeControllerGenerator.Generate(context, builder);
            BlendShapeControllerGenerator.GenerateBlendshapeSyncController(context, builder);
            MenuGenerator.Generate(context, builder);

            var assetContainer = new AssetContainer();
            var animatorController = builder.ToAnimatorController(assetContainer);

            context.AssetSaver.SaveAssets(assetContainer.Assets);

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

            var mp = modEmo.gameObject.GetOrAddComponent<ModularAvatarParameters>();
            foreach(var x in context.GetData().Parameters)
            {
                mp.parameters.Add(new()
                {
                    nameOrPrefix = x.Name,
                    defaultValue = x.Value,
                    localOnly = x.IsLocal,
                    saved = x.Saved,
                    syncType = x.SyncType switch
                    {
                        AnimatorParameterType.Int => ParameterSyncType.Int,
                        AnimatorParameterType.Float => ParameterSyncType.Float,
                        AnimatorParameterType.Bool => ParameterSyncType.Bool,
                        _ => ParameterSyncType.NotSynced,
                    },
                });
            }

            //new MenuGenerator(context).Generate();
        }
    }
}