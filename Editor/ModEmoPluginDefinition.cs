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

            var animatorAPI = context.GetVirtualControllerContext();

            // TODO: マルチプラットフォーム化の時にいい感じにする
            var fx = animatorAPI.Controllers[VRCAvatarDescriptor.AnimLayerType.FX];

            fx.AddLayer(LayerPriority.Default, BlendShapeControllerGenerator.Generate(context));

            fx.Parameters = fx.Parameters.AddRange(data.Parameters.Select(x => (AnimatorControllerParameter)x).Select(x => KeyValuePair.Create(x.name, x)));
        }
    }
}
