#if ZATOOLS
using KusakaFactory.Zatools.Runtime;

namespace Numeira;

internal sealed partial class ModEmoPluginDefinition
{
    public sealed class ExistingBlendshapeModifierPass : Pass<ExistingBlendshapeModifierPass>
    {
        protected override void Execute(BuildContext context)
        {
            var allModifiers = context.AvatarRootObject.GetComponentsInChildren<ModEmoExistingBlendShapeModifier>(includeInactive: true);

            var modifiers = allModifiers.Where(x => x.gameObject.activeInHierarchy).ToArray();
            if (modifiers.Length == 0)
                return;

            var faceRenderer = AvatarUtils.GetFaceRenderer(context.AvatarRootTransform);
            if (faceRenderer == null)
                return;

            var mix = faceRenderer.gameObject.AddComponent<AdHocBlendShapeMix>();
            mix.Replace = true;
            var list = new List<BlendShapeMixDefinition>();

            foreach (var group in modifiers.GroupBy(x => x.TargetBlendShapeName, x => x.GetBlendShapes()))
            {
                list.Add(new()
                {
                    ToBlendShape = group.Key,
                    FromBlendShape = group.Key,
                    MixWeight = -1
                });

                foreach (var x in group.SelectMany(x => x))
                {
                    list.Add(new()
                    {
                        ToBlendShape = group.Key,
                        FromBlendShape = x.Name,
                        MixWeight = (x.Value / 100f) * (x.Cancel ? -1 : 1),
                    });
                }
            }

            mix.MixDefinitions = list.ToArray();

            foreach (var modifier in allModifiers)
            {
                Object.Destroy(modifier.gameObject);
            }
        }
    }
}
#endif