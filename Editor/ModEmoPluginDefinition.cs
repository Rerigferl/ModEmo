using System.Collections.Immutable;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using VRC.SDK3.Avatars.Components;

[assembly: ExportsPlugin(typeof(Numeira.ModEmoPluginDefinition))]

namespace Numeira;

internal sealed class ModEmoPluginDefinition : Plugin<ModEmoPluginDefinition>
{
    protected override void Configure()
    {
        InPhase(BuildPhase.Generating)
            .WithRequiredExtensions(new[] { typeof(ExtensionContext), typeof(VirtualControllerContext) }, 
            sequence =>
            {
                sequence
                    .Run(GeneratingPass.Instance);
            });
    }

    public sealed class ExtensionContext : IExtensionContext
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

            foreach (var component in components)
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
            if (modEmo == null)
                return;


            var data = context.GetData();
            data.Parameters.Add(new(ParameterNames.Internal.One, 1f));
            DirectBlendTree.DefaultDirectBlendParameter = ParameterNames.Internal.One;

            data.Parameters.Add(new(ParameterNames.Internal.Input.Left, 0));
            data.Parameters.Add(new(ParameterNames.Internal.Input.Right, 0));

            var animatorAPI = context.GetVirtualControllerContext();

            // TODO: マルチプラットフォーム化の時にいい感じにする
            var fx = animatorAPI.Controllers[VRCAvatarDescriptor.AnimLayerType.FX];

            var priority = new LayerPriority(1);

            fx.AddLayer(priority, InputConverterGenerator.Generate(context));
            fx.AddLayer(priority, ExpressionControllerGenerator.Generate(context));
            fx.AddLayer(priority, BlendShapeControllerGenerator.Generate(context));

            fx.Parameters = fx.Parameters.AddRange(data.Parameters.Select(x => (AnimatorControllerParameter)x).Select(x => KeyValuePair.Create(x.name, x)));

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
