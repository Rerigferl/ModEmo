using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using VRC.SDK3.Avatars.Components;

[assembly: ExportsPlugin(typeof(Numeira.ModEmoPluginDefinition))]

namespace Numeira;

internal sealed class ModEmoPluginDefinition : Plugin<ModEmoPluginDefinition>
{
    protected override void Configure()
    {
        InPhase(BuildPhase.Generating).WithRequiredExtensions(new[] { typeof(ExtensionContext) }, sequence => sequence.Run(GeneratingPass.Instance));
    }

    public sealed class ExtensionContext : IExtensionContext
    {
        private ModEmoTagComponent[]? components;
        public ReadOnlySpan<ModEmoTagComponent> Components => components;
        public ModEmo Root { get; private set; } = null!;
        private static Guid seed;

        public void OnActivate(BuildContext context)
        {
            seed = Guid.NewGuid();
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
            var modEmo = context.Extension<ExtensionContext>().Root;
            if (modEmo == null)
                return;

            var generator = new ModEmoExpressionGenerator(modEmo, context.AssetContainer);
            var compiled = generator.Build();
            context.AssetSaver.SaveAsset(compiled);

            var mama = modEmo.gameObject.AddComponent<ModularAvatarMergeAnimator>();
            mama.animator = compiled;
            mama.matchAvatarWriteDefaults = false;
            mama.pathMode = MergeAnimatorPathMode.Absolute;
            mama.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
        }
    }
}
