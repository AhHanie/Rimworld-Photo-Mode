using HarmonyLib;
using Verse;

namespace Photo_Mode
{
    [HarmonyPatch(typeof(DynamicDrawManager), nameof(DynamicDrawManager.DrawDynamicThings))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_DynamicDrawManager_DrawDynamicThings
    {
        private static void Postfix(DynamicDrawManager __instance)
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return;
            }

            Map targetMap = PhotoModeManager.Current.State.TargetMap;
            if (targetMap == null || targetMap.dynamicDrawManager != __instance)
            {
                return;
            }

            PhotoSceneRenderer.RenderAll(targetMap);
        }
    }
}
