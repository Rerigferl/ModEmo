using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Numeira;

internal static class ModEmoBlendShapeSelectorHelper
{
    [MenuItem($"CONTEXT/{nameof(ModEmoBlendShapeSelector)}/Sort Blendshapes by Category", false, 100)]
    internal static void SortByCategoryName(MenuCommand command)
    {
        if (command.context is not ModEmoBlendShapeSelector selector)
            return;

        var list = selector.BlendShapes;
        if (list.Count < 2)
            return;

        if (selector.GetComponentInParent<ModEmo>(true) is not { } root)
            return;

        if (ModEmoData.GetCategorizedBlendShapes(root) is not { } data)
            return;

        Undo.RecordObject(selector, "Sort Blendshapes by Category");

        list.Sort(new CategorizedBlendShapeComparer(data.CategorizedBlendShapeNames));

        EditorUtility.SetDirty(selector);
    }

    [MenuItem($"CONTEXT/{nameof(ModEmoBlendShapeSelector)}/Split Blendshapes", false, 200)]
    internal static void SplitBlendshapes(MenuCommand command)
    {
        if (command.context is not ModEmoBlendShapeSelector selector)
            return;

        var list = selector.BlendShapes;

        if (selector.GetComponentInParent<ModEmo>(true) is not { } root)
            return;

        if (ModEmoData.GetBlendShapeInfos(root.GetFaceRenderer()) is not { } infos)
            return;

        ModEmoBlendShapeSelector? newLeft = null;
        ModEmoBlendShapeSelector? newRight = null;

        Undo.SetCurrentGroupName("Split Blendshapes");
        var uid = Undo.GetCurrentGroup();

        Undo.RecordObject(selector, "Split Blendshapes");

        bool listHasChanged = false;

        foreach(ref var item in list.AsSpan())
        {
            var name = item.Name;
            bool flag = false;

            if (infos.ContainsKey($"{name}_L"))
            {
                flag = true;
                (newLeft ??= Undo.AddComponent<ModEmoBlendShapeSelector>(selector.gameObject)).BlendShapes.Add(item with { Name = $"{name}_L" });
            }

            if (infos.ContainsKey($"{name}_R"))
            {
                flag = true;
                (newRight ??= Undo.AddComponent<ModEmoBlendShapeSelector>(selector.gameObject)).BlendShapes.Add(item with { Name = $"{name}_R" });
            }

            if (flag)
            {
                item = default;
            }

            listHasChanged |= flag;
        }

        if (newLeft != null)
            newLeft.Keyframe = selector.Keyframe;

        if (newRight != null)
            newRight.Keyframe = selector.Keyframe;

        if (listHasChanged)
        {
            list.RemoveAll(x => string.IsNullOrEmpty(x.Name));
        }

        Undo.CollapseUndoOperations(uid);
        EditorUtility.SetDirty(selector);
    }

    [MenuItem($"CONTEXT/{nameof(ModEmoBlendShapeSelector)}/Flip Blendshapes", false, 201)]
    internal static void FlipBlendshapes(MenuCommand command)
    {
        if (command.context is not ModEmoBlendShapeSelector selector)
            return;

        var list = selector.BlendShapes;

        Undo.RecordObject(selector, "Split Blendshapes");

        foreach (ref var item in list.AsSpan())
        {
            ref var name = ref item.Name;

            if (name.EndsWith("_L"))
                name = $"{name[..^2]}_R";
            else if (name.EndsWith("_R"))
                name = $"{name[..^2]}_L";
        }

        EditorUtility.SetDirty(selector);
    }
    
    [MenuItem($"CONTEXT/{nameof(ModEmoBlendShapeSelector)}/Copy Previous Frame", false, 202)]
    internal static void CopyPreviousFrame(MenuCommand command)
    {
        if (command.context is not ModEmoBlendShapeSelector selector)
            return;

        var selectors = selector.gameObject.GetComponents<ModEmoBlendShapeSelector>().Where(x => x.Keyframe < selector.Keyframe).SelectMany(x => x.BlendShapes).GroupBy(x => (x.Name, x.Cancel)).Select(x => x.FirstOrDefault());
        var list = selector.BlendShapes;
        Undo.RecordObject(selector, "Copy Previous Frame");
        foreach (var x in selectors)
        {
            list.Add(x);
        }

        EditorUtility.SetDirty(selector);
    }
}
