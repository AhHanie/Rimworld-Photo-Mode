using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public class PhotoWeatherOverlayRenderParticipant : IRenderOverrideParticipant
    {
        private static readonly int RenderLayer = LayerMask.NameToLayer("GravshipExclude");

        private readonly Dictionary<WeatherOverlayDualPanner, Material> materialCache = new Dictionary<WeatherOverlayDualPanner, Material>();

        public void Apply(Camera camera)
        {
            PhotoModeManager manager = PhotoModeManager.Current;
            if (manager == null || !manager.State.Active)
            {
                return;
            }

            PhotoEnvironmentState environment = manager.State.Environment;
            if (environment.UseRealWeather || environment.VisualWeather == null || !environment.OverlayParticlesEnabled)
            {
                return;
            }

            Map map = manager.State.TargetMap;
            if (map == null || map.Disposed)
            {
                return;
            }

            if (map.Biome != null && map.Biome.disableSkyLighting)
            {
                return;
            }

            WeatherDef weatherDef = environment.VisualWeather;
            float hours = environment.UseRealTime ? GenLocalDate.HourFloat(map) : environment.TimeHours;
            PhotoSkyRenderState render = PhotoSkyCalculator.Compute(map, hours, weatherDef);

            Color overlayColor = render.OverlayColor;
            overlayColor.a *= Mathf.Clamp01(environment.OverlayIntensity);

            List<SkyOverlay> overlays = weatherDef.Worker.overlays;
            for (int i = 0; i < overlays.Count; i++)
            {
                if (!(overlays[i] is WeatherOverlayDualPanner dualPanner) || dualPanner.worldOverlayMat == null)
                {
                    continue;
                }

                Material clone = GetOrCreateClone(dualPanner);
                clone.color = overlayColor;
                SkyOverlay.DrawWorldOverlay(map, clone, RenderLayer);
            }
        }

        public void Restore(Camera camera)
        {
        }

        public void ClearMaterialCache()
        {
            foreach (KeyValuePair<WeatherOverlayDualPanner, Material> entry in materialCache)
            {
                if (entry.Value != null)
                {
                    Object.Destroy(entry.Value);
                }
            }

            materialCache.Clear();
        }

        private Material GetOrCreateClone(WeatherOverlayDualPanner overlay)
        {
            if (!materialCache.TryGetValue(overlay, out Material clone) || clone == null)
            {
                clone = new Material(overlay.worldOverlayMat);
                materialCache[overlay] = clone;
            }

            return clone;
        }
    }
}
