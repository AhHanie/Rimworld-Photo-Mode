using HarmonyLib;
using Verse;

namespace Photo_Mode
{
    [HarmonyPatch(typeof(CameraDriver), nameof(CameraDriver.CameraDriverOnGUI))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_CameraDriver_CameraDriverOnGUI
    {
        private static bool Prefix(CameraDriver __instance)
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            if (__instance != Find.CameraDriver)
            {
                return true;
            }

            PhotoModeManager.Current.CameraController.HandleOnGUI();
            return false;
        }
    }

    [HarmonyPatch(typeof(CameraDriver), nameof(CameraDriver.Update))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_CameraDriver_Update
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

            if (LongEventHandler.ShouldWaitForEvent)
            {
                return;
            }

            Map map = Find.CurrentMap;
            if (map == null)
            {
                return;
            }

            PhotoModeManager manager = PhotoModeManager.Current;
            manager.CameraController.HandleUpdate(map, manager.State.Camera);

            __instance.SetRootPosAndSize(manager.CameraController.CurrentPos, manager.CameraController.CurrentSize);

            RememberedCameraPos remembered = map.rememberedCameraPos;
            remembered.rootPos = manager.CameraController.CurrentPos;
            remembered.rootSize = manager.CameraController.CurrentSize;
        }
    }
}
