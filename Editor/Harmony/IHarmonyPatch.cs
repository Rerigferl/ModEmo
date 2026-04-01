using System.Reflection;
using HarmonyLib;

namespace Numeira.HarmonyPatch;

internal interface IHarmonyPatch
{
    void Patch(Harmony harmony);
}

internal abstract class HarmonyPatch<TSelf> : IHarmonyPatch where TSelf : HarmonyPatch<TSelf>
{
    protected abstract void Patch(Harmony harmony);

    void IHarmonyPatch.Patch(Harmony harmony)
    {
        Patch(harmony);
    }

    protected static HarmonyMethod GetPatchMethod(string name)
    {
        return new(typeof(TSelf).GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
    }
}