using HarmonyLib;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    [HarmonyPatch(typeof(CameraDriver), nameof(CameraDriver.CurrentViewRect), MethodType.Getter)]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_CameraDriver_CurrentViewRect
    {
        private static bool Prefix(CameraDriver __instance, ref CellRect __result)
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            if (__instance != Find.CameraDriver)
            {
                return true;
            }

            if (!PhotoModeManager.Current.CaptureService.TryGetCaptureAspect(out float aspect))
            {
                return true;
            }

            Vector3 position = Find.Camera.transform.position;
            float rootSize = __instance.RootSize;

            CellRect rect = default;
            rect.minX = Mathf.FloorToInt(position.x - rootSize * aspect - 1f);
            rect.maxX = Mathf.CeilToInt(position.x + rootSize * aspect);
            rect.minZ = Mathf.FloorToInt(position.z - rootSize - 1f);
            rect.maxZ = Mathf.CeilToInt(position.z + rootSize);

            __result = rect;
            return false;
        }
    }
}
