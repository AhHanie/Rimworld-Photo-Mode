using RimWorld;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public struct PhotoSkyRenderState
    {
        public Color LightOverlayColor;
        public Color OverlayColor;
        public Color FogOfWarColor;
        public Color ShadowColor;
        public float Saturation;
        public Vector4 ShadowVector;
        public Vector4 WaterCastVectSun;
        public Vector4 WaterCastVectMoon;
        public float LightsourceShineSizeReduction;
        public float LightsourceShineIntensity;
        public float DayPercent;
    }

    public static class PhotoSkyCalculator
    {
        private struct SkyThreshold
        {
            public SkyColorSet Colors;
            public float CelGlowThreshold;
        }

        private static readonly Color FogOfWarBaseColor = new Color32(77, 69, 66, byte.MaxValue);

        public static int ComputeVisualTicksAbs(int realTicksAbs, float longitude, float hours)
        {
            int desiredDayTick = Mathf.Clamp(Mathf.RoundToInt(Mathf.Repeat(hours, 24f) / 24f * 60000f), 0, 59999);
            int realDayTick = GenDate.DayTick(realTicksAbs, longitude);
            return realTicksAbs - realDayTick + desiredDayTick;
        }

        public static PhotoSkyRenderState Compute(Map map, float hours, WeatherDef weatherDef)
        {
            float longitude = Find.WorldGrid.LongLatOf(map.Tile).x;
            int realTicksAbs = Find.TickManager.TicksAbs;
            int visualTicksAbs = ComputeVisualTicksAbs(realTicksAbs, longitude, hours);

            float virtualGlow = GenCelestial.CelestialSunGlow(map.Tile, visualTicksAbs);
            float virtualDayPercent = GenDate.DayPercent(visualTicksAbs, longitude);
            float shadowStrength = CurShadowStrength(virtualGlow);

            SkyTarget skyTarget = CurSkyTarget(weatherDef, virtualGlow);
            GenCelestial.LightInfo shadowInfo = GetLightSourceInfo(virtualDayPercent, virtualGlow, GenCelestial.LightType.Shadow);
            GenCelestial.LightInfo sunInfo = GetLightSourceInfo(virtualDayPercent, virtualGlow, GenCelestial.LightType.LightingSun);
            GenCelestial.LightInfo moonInfo = GetLightSourceInfo(virtualDayPercent, virtualGlow, GenCelestial.LightType.LightingMoon);

            Color fogOfWarColor = skyTarget.colors.sky;
            fogOfWarColor.a = 1f;
            fogOfWarColor *= map.FogOfWarColor ?? FogOfWarBaseColor;

            Color shadowColor = Color.Lerp(Color.white, skyTarget.colors.shadow, shadowStrength);

            return new PhotoSkyRenderState
            {
                LightOverlayColor = skyTarget.colors.sky,
                OverlayColor = skyTarget.colors.overlay,
                Saturation = skyTarget.colors.saturation,
                FogOfWarColor = fogOfWarColor,
                ShadowColor = shadowColor,
                ShadowVector = new Vector4(shadowInfo.vector.x, 0f, shadowInfo.vector.y, shadowStrength),
                WaterCastVectSun = new Vector4(sunInfo.vector.x, 0f, sunInfo.vector.y, sunInfo.intensity),
                WaterCastVectMoon = new Vector4(moonInfo.vector.x, 0f, moonInfo.vector.y, moonInfo.intensity),
                LightsourceShineSizeReduction = 20f * (1f / skyTarget.lightsourceShineSize),
                LightsourceShineIntensity = skyTarget.lightsourceShineIntensity,
                DayPercent = virtualDayPercent
            };
        }

        private static SkyTarget CurSkyTarget(WeatherDef weatherDef, float virtualGlow)
        {
            SkyThreshold[] skyTargets =
            {
                new SkyThreshold { Colors = weatherDef.skyColorsNightMid, CelGlowThreshold = 0f },
                new SkyThreshold { Colors = weatherDef.skyColorsNightEdge, CelGlowThreshold = 0.1f },
                new SkyThreshold { Colors = weatherDef.skyColorsDusk, CelGlowThreshold = 0.6f },
                new SkyThreshold { Colors = weatherDef.skyColorsDay, CelGlowThreshold = 1f }
            };

            int lowerIndex = 0;
            int upperIndex = 0;
            for (int i = 0; i < skyTargets.Length; i++)
            {
                upperIndex = i;
                if (virtualGlow + 0.001f < skyTargets[i].CelGlowThreshold)
                {
                    break;
                }

                lowerIndex = i;
            }

            SkyThreshold lower = skyTargets[lowerIndex];
            SkyThreshold upper = skyTargets[upperIndex];
            float span = upper.CelGlowThreshold - lower.CelGlowThreshold;
            float lerpFactor = span != 0f ? (virtualGlow - lower.CelGlowThreshold) / span : 1f;

            SkyTarget result = new SkyTarget
            {
                glow = Mathf.Min(virtualGlow, weatherDef.maxGlow),
                colors = SkyColorSet.Lerp(lower.Colors, upper.Colors, lerpFactor)
            };

            if (IsDaytime(virtualGlow))
            {
                result.lightsourceShineIntensity = 1f;
                result.lightsourceShineSize = 1f;
            }
            else
            {
                result.lightsourceShineIntensity = 0.7f;
                result.lightsourceShineSize = 0.5f;
            }

            return result;
        }

        private static GenCelestial.LightInfo GetLightSourceInfo(float virtualDayPercent, float virtualGlow, GenCelestial.LightType type)
        {
            float dayPercent = virtualDayPercent;
            bool daySide;
            float intensity;
            switch (type)
            {
                case GenCelestial.LightType.Shadow:
                    daySide = IsDaytime(virtualGlow);
                    intensity = CurShadowStrength(virtualGlow);
                    break;
                case GenCelestial.LightType.LightingSun:
                    daySide = true;
                    intensity = Mathf.Clamp01((virtualGlow - 0.6f + 0.2f) / 0.15f);
                    break;
                case GenCelestial.LightType.LightingMoon:
                    daySide = false;
                    intensity = Mathf.Clamp01((0f - (virtualGlow - 0.6f - 0.2f)) / 0.15f);
                    break;
                default:
                    daySide = true;
                    intensity = 0f;
                    break;
            }

            float t;
            float baseHeight;
            float radius;
            if (daySide)
            {
                t = dayPercent;
                baseHeight = -1.5f;
                radius = 15f;
            }
            else
            {
                t = dayPercent > 0.5f
                    ? Mathf.InverseLerp(0.5f, 1f, dayPercent) * 0.5f
                    : 0.5f + Mathf.InverseLerp(0f, 0.5f, dayPercent) * 0.5f;
                baseHeight = -0.9f;
                radius = 15f;
            }

            float x = Mathf.LerpUnclamped(-radius, radius, t);
            float y = baseHeight - 2.5f * (x * x / 100f);

            return new GenCelestial.LightInfo
            {
                vector = new Vector2(x, y),
                intensity = intensity
            };
        }

        private static float CurShadowStrength(float glow)
        {
            return Mathf.Clamp01(Mathf.Abs(glow - 0.6f) / 0.15f);
        }

        private static bool IsDaytime(float glow)
        {
            return glow > 0.6f;
        }
    }
}
