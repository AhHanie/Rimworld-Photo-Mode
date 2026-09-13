using HarmonyLib;
using RimWorld;
using Verse;

namespace Photo_Mode
{
    [HarmonyPatch(typeof(MapInterface), nameof(MapInterface.HandleMapClicks))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_MapInterface_HandleMapClicks
    {
        private static bool Prefix()
        {
            return !PhotoModeManager.IsRenderingThisCurrentMap;
        }
    }

    [HarmonyPatch(typeof(MapInterface), nameof(MapInterface.HandleLowPriorityInput))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_MapInterface_HandleLowPriorityInput
    {
        private static bool Prefix()
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            PhotoModeManager manager = PhotoModeManager.Current;
            manager.MapInteraction.HandleInput(manager);
            return false;
        }
    }

    [HarmonyPatch(typeof(DesignatorManager), nameof(DesignatorManager.ProcessInputEvents))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_DesignatorManager_ProcessInputEvents
    {
        private static bool Prefix()
        {
            return !PhotoModeManager.IsRenderingThisCurrentMap;
        }
    }
}
