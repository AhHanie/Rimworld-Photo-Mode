using RimWorld;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public class PhotoTimeOfDayRenderParticipant : IRenderOverrideParticipant
    {
        private static readonly int LightsourceShineIntensityId = Shader.PropertyToID("_LightsourceShineIntensity");
        private static readonly int LightsourceShineSizeReductionId = Shader.PropertyToID("_LightsourceShineSizeReduction");
        private static readonly int DayPercentId = Shader.PropertyToID("_DayPercent");

        private bool applied;

        private Color snapshotLightOverlayColor;
        private Color snapshotFogOfWarColor;
        private Color snapshotSunShadowColor;
        private Color snapshotSunShadowFadeColor;
        private float snapshotSaturation;
        private Vector4 snapshotShadowVector;
        private Vector4 snapshotWaterCastVectSun;
        private Vector4 snapshotWaterCastVectMoon;
        private float snapshotShineSizeReduction;
        private float snapshotShineIntensity;
        private float snapshotDayPercent;

        public void Apply(Camera camera)
        {
            applied = false;

            PhotoModeManager manager = PhotoModeManager.Current;
            if (manager == null || !manager.State.Active)
            {
                return;
            }

            PhotoEnvironmentState environment = manager.State.Environment;
            if (environment.UseRealTime && environment.UseRealWeather)
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

            float hours = environment.UseRealTime ? GenLocalDate.HourFloat(map) : environment.TimeHours;
            WeatherDef weatherDef = environment.UseRealWeather ? map.weatherManager.curWeather : environment.VisualWeather ?? map.weatherManager.curWeather;

            PhotoSkyRenderState render = PhotoSkyCalculator.Compute(map, hours, weatherDef);

            snapshotLightOverlayColor = MatBases.LightOverlay.color;
            snapshotFogOfWarColor = MatBases.FogOfWar.color;
            snapshotSunShadowColor = MatBases.SunShadow.color;
            snapshotSunShadowFadeColor = MatBases.SunShadowFade.color;
            snapshotSaturation = Find.CameraColor.saturation;
            snapshotShadowVector = Shader.GetGlobalVector(ShaderPropertyIDs.MapSunLightDirection);
            snapshotWaterCastVectSun = Shader.GetGlobalVector(ShaderPropertyIDs.WaterCastVectSun);
            snapshotWaterCastVectMoon = Shader.GetGlobalVector(ShaderPropertyIDs.WaterCastVectMoon);
            snapshotShineSizeReduction = Shader.GetGlobalFloat(LightsourceShineSizeReductionId);
            snapshotShineIntensity = Shader.GetGlobalFloat(LightsourceShineIntensityId);
            snapshotDayPercent = Shader.GetGlobalFloat(DayPercentId);

            MatBases.LightOverlay.color = render.LightOverlayColor;
            MatBases.FogOfWar.color = render.FogOfWarColor;
            MatBases.SunShadow.color = render.ShadowColor;
            MatBases.SunShadowFade.color = render.ShadowColor;
            Find.CameraColor.saturation = render.Saturation;
            Shader.SetGlobalVector(ShaderPropertyIDs.MapSunLightDirection, render.ShadowVector);
            Shader.SetGlobalVector(ShaderPropertyIDs.WaterCastVectSun, render.WaterCastVectSun);
            Shader.SetGlobalVector(ShaderPropertyIDs.WaterCastVectMoon, render.WaterCastVectMoon);
            Shader.SetGlobalFloat(LightsourceShineSizeReductionId, render.LightsourceShineSizeReduction);
            Shader.SetGlobalFloat(LightsourceShineIntensityId, render.LightsourceShineIntensity);
            Shader.SetGlobalFloat(DayPercentId, render.DayPercent);

            applied = true;
        }

        public void Restore(Camera camera)
        {
            if (!applied)
            {
                return;
            }

            applied = false;

            MatBases.LightOverlay.color = snapshotLightOverlayColor;
            MatBases.FogOfWar.color = snapshotFogOfWarColor;
            MatBases.SunShadow.color = snapshotSunShadowColor;
            MatBases.SunShadowFade.color = snapshotSunShadowFadeColor;
            Find.CameraColor.saturation = snapshotSaturation;
            Shader.SetGlobalVector(ShaderPropertyIDs.MapSunLightDirection, snapshotShadowVector);
            Shader.SetGlobalVector(ShaderPropertyIDs.WaterCastVectSun, snapshotWaterCastVectSun);
            Shader.SetGlobalVector(ShaderPropertyIDs.WaterCastVectMoon, snapshotWaterCastVectMoon);
            Shader.SetGlobalFloat(LightsourceShineSizeReductionId, snapshotShineSizeReduction);
            Shader.SetGlobalFloat(LightsourceShineIntensityId, snapshotShineIntensity);
            Shader.SetGlobalFloat(DayPercentId, snapshotDayPercent);
        }
    }
}
