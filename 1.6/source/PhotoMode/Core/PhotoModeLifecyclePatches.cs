using HarmonyLib;
using Verse;

namespace Photo_Mode
{
    [HarmonyPatch(typeof(Game), nameof(Game.CurrentMap), MethodType.Setter)]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_Game_CurrentMap
    {
        private static void Prefix(Map value)
        {
            PhotoModeManager manager = PhotoModeManager.Current;
            if (manager == null || !manager.State.Active || manager.State.TargetMap == value)
            {
                return;
            }

            manager.Exit("MapChanged");
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.Dispose))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_Game_Dispose
    {
        private static void Prefix()
        {
            PhotoModeManager manager = PhotoModeManager.Current;
            if (manager == null || !manager.State.Active)
            {
                return;
            }

            manager.Exit("GameDisposed");
        }
    }
}
