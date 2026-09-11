using HarmonyLib;
using Verse;

namespace Photo_Mode
{
    [HarmonyPatch(typeof(WeatherWorker), nameof(WeatherWorker.DrawWeather))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_WeatherWorker_DrawWeather
    {
        private static bool Prefix()
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            return PhotoModeManager.Current.State.Environment.UseRealWeather;
        }
    }
}
