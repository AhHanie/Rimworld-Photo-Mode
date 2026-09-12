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

            PhotoModeManager manager = PhotoModeManager.Current;
            if (!manager.CaptureService.TryGetCaptureAspect(out float aspect))
            {
                aspect = (float)UI.screenWidth / UI.screenHeight;
            }

            float rollDegrees = manager.State.Camera.RollDegrees;
            float rollRadians = rollDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Abs(Mathf.Cos(rollRadians));
            float sin = Mathf.Abs(Mathf.Sin(rollRadians));

            Vector3 position = Find.Camera.transform.position;
            float rootSize = __instance.RootSize;
            float halfWidth = rootSize * aspect;
            float halfHeight = rootSize;

            float rolledHalfX = cos * halfWidth + sin * halfHeight;
            float rolledHalfZ = sin * halfWidth + cos * halfHeight;

            CellRect rect = default;
            rect.minX = Mathf.FloorToInt(position.x - rolledHalfX - 1f);
            rect.maxX = Mathf.CeilToInt(position.x + rolledHalfX);
            rect.minZ = Mathf.FloorToInt(position.z - rolledHalfZ - 1f);
            rect.maxZ = Mathf.CeilToInt(position.z + rolledHalfZ);

            __result = rect;
            return false;
        }
    }
}
