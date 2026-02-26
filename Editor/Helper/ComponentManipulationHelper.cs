using UnityEditorInternal;

namespace Numeira;

internal static class ComponentManipulationHelper
{
    private const string PasteComponentAtNextIndexPath = "CONTEXT/Component/Paste Component At Next Index";

    [MenuItem(PasteComponentAtNextIndexPath, false, secondaryPriority = 14)]
    public static void PasteComponentAtNextIndex(MenuCommand command)
    {
        var context = command.context as Component;
        var obj = context?.gameObject;
        if (context == null || obj == null)
            return;

        if (!ComponentUtility.PasteComponentAsNew(obj))
            return;

        var components = obj.GetComponentsInChildren(typeof(Component), true);
        int targetIndex = -1;
        Component? targetComponent = components[^1];

        for (int i = 0; i < components.Length - 1; i++)
        {
            if (components[i] == context)
            {
                targetIndex = i;
                break;
            }
        }


        if (targetIndex == -1)
            return;

        targetIndex += 1;


        for (int i = 0; i < components.Length - 1 - targetIndex; i++)
        {
            ComponentUtility.MoveComponentUp(targetComponent);
        }
    }
}