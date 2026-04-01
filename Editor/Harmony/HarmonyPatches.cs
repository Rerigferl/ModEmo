using HarmonyLib;

namespace Numeira.HarmonyPatch;

[InitializeOnLoad]
internal static class HarmonyPatches
{
    private static readonly IHarmonyPatch[] Patches = 
    {
        new AddComponentMenuPatch()
    };

    static HarmonyPatches()
    {
        const string ID = "numeira.mod-emo.harmony-patch";
        Harmony harmony = new(ID);

        foreach(var patch in Patches)
        {
            patch.Patch(harmony);
        }

        AssemblyReloadEvents.beforeAssemblyReload += () => { harmony.UnpatchAll(ID); };
    }
}
