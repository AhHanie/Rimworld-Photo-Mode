using System.Collections.Generic;
using Verse;

namespace Photo_Mode
{
    public enum WeatherOverlaySupportStatus
    {
        UsingRealWeather,
        NoOverlay,
        FullySupported,
        PartiallySupported,
        Unsupported
    }

    public static class PhotoWeatherOverlaySupport
    {
        public static bool IsSupported(SkyOverlay overlay)
        {
            return overlay is WeatherOverlayDualPanner dualPanner && dualPanner.worldOverlayMat != null;
        }

        public static WeatherOverlaySupportStatus Classify(WeatherDef weatherDef)
        {
            if (weatherDef == null)
            {
                return WeatherOverlaySupportStatus.NoOverlay;
            }

            List<SkyOverlay> overlays = weatherDef.Worker.overlays;
            if (overlays.Count == 0)
            {
                return WeatherOverlaySupportStatus.NoOverlay;
            }

            bool hasSupported = false;
            bool hasUnsupported = false;
            for (int i = 0; i < overlays.Count; i++)
            {
                if (IsSupported(overlays[i]))
                {
                    hasSupported = true;
                }
                else
                {
                    hasUnsupported = true;
                }
            }

            if (hasSupported && hasUnsupported)
            {
                return WeatherOverlaySupportStatus.PartiallySupported;
            }

            return hasSupported ? WeatherOverlaySupportStatus.FullySupported : WeatherOverlaySupportStatus.Unsupported;
        }
    }
}
