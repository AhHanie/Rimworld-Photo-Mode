using HarmonyLib;
using Verse;

namespace Photo_Mode
{
    [HarmonyPatch(typeof(CameraDriver), nameof(CameraDriver.OnPreCull))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_CameraDriver_OnPreCull
    {
        private static void Postfix(CameraDriver __instance)
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return;
            }

            if (__instance != Find.CameraDriver)
            {
                return;
            }

            PhotoModeManager.Current.RenderScope.HandlePreCull(Find.Camera);
            PhotoPostProcess.EnsureAttached(Find.Camera);
        }
    }
}
