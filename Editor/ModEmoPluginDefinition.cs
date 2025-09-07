using nadena.dev.modular_avatar.core;
using Numeira.Animation;

[assembly: ExportsPlugin(typeof(Numeira.ModEmoPluginDefinition))]

namespace Numeira;

internal sealed class ModEmoPluginDefinition : Plugin<ModEmoPluginDefinition>
{
    protected override void Configure()
    {
        InPhase(BuildPhase.Transforming)
            .BeforePlugin("nadena.dev.modular-avatar")
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

            foreach(var x in components.Select(x => x.gameObject).Distinct().ToArray())
            {
                //x.RemoveComponents<ModEmoTagComponent>();
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

            var animatorController = new AnimatorControllerBuilder() { Name = "ModEmo" };
            animatorController.Parameters
                .AddFloat(ParameterNames.Internal.One, 1f)
                .AddFloat(ParameterNames.Internal.SmoothAmount, 0.65f);

            if (context.PlatformProvider.QualifiedName == WellKnownPlatforms.VRChatAvatar30)
            {

            }
            else
            {
                throw new NotImplementedException($"TargetPlatform `{context.PlatformProvider.DisplayName}` is not supported");
            }

            ExpressionControllerGenerator.Generate(context, animatorController);

            var assetContainer = new AssetContainer();
            var compiledAnimatorController = animatorController.ToAnimatorController(assetContainer);
            var ma = modEmo.gameObject.AddComponent<ModularAvatarMergeAnimator>();
            ma.pathMode = MergeAnimatorPathMode.Absolute;
            ma.matchAvatarWriteDefaults = true;
            ma.layerType = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.FX;
            ma.animator = compiledAnimatorController;

            //modEmo.AnimatorController =

            //avatarController.AddLayer(priority, InputConverterGenerator.Generate(context));
            //avatarController.AddLayer(priority, GestureWeightSmootherGenerator.Generate(context));
            //avatarController.AddLayer(priority, ExpressionControllerGenerator.Generate(context));
            //if (ExpressionControllerGenerator.GenerateBlinkController(context) is { } blink)
            //    avatarController.AddLayer(priority, blink);
            //avatarController.AddLayer(priority, BlendShapeControllerGenerator.Generate(context));

            //avatarController.Parameters = avatarController.Parameters.AddRange(data.Parameters.Select(x => (AnimatorControllerParameter)x).Select(x => KeyValuePair.Create(x.name, x)));
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