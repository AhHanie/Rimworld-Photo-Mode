using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public partial class PhotoModePanel
    {
        private static readonly string[] TimePresetLabelKeys =
        {
            "PhotoMode.Scene.Preset.Dawn",
            "PhotoMode.Scene.Preset.Morning",
            "PhotoMode.Scene.Preset.Noon",
            "PhotoMode.Scene.Preset.Afternoon",
            "PhotoMode.Scene.Preset.GoldenHour",
            "PhotoMode.Scene.Preset.Sunset",
            "PhotoMode.Scene.Preset.Dusk",
            "PhotoMode.Scene.Preset.Midnight"
        };

        private static readonly float[] TimePresetHours = { 6f, 9f, 12f, 15f, 17.5f, 19f, 20f, 0f };

        private void DrawSceneSection(Listing_Standard listing, ref bool collapsed)
        {
            collapsed = DrawSectionHeader(listing, collapsed, "PhotoMode.Section.Scene");

            if (!collapsed)
            {
                PhotoEnvironmentState environment = manager.State.Environment;

                Rect timeLabelRect = listing.GetRect(PlaceholderHeight);
                Widgets.Label(timeLabelRect, "PhotoMode.Scene.Time".Translate(environment.TimeHours.ToString("0.0")));

                Rect timeSliderRect = listing.GetRect(PlaceholderHeight);
                float sliderValue = Widgets.HorizontalSlider(timeSliderRect, environment.TimeHours, 0f, 24f, roundTo: 0.5f);
                if (!Mathf.Approximately(sliderValue, environment.TimeHours))
                {
                    manager.SetVisualTimeHours(sliderValue);
                }

                listing.Gap(4f);

                Rect useCurrentRect = listing.GetRect(PlaceholderHeight);
                if (Widgets.ButtonText(useCurrentRect, "PhotoMode.Scene.UseCurrentTime".Translate()))
                {
                    manager.UseCurrentTime();
                }

                Rect statusRect = listing.GetRect(PlaceholderHeight);
                GUI.color = Color.gray;
                Widgets.Label(statusRect, environment.UseRealTime ? "PhotoMode.Scene.UsingRealTime".Translate() : "PhotoMode.Scene.UsingVisualTime".Translate());
                GUI.color = Color.white;

                listing.Gap(4f);

                DrawTimePresets(listing);

                listing.Gap(8f);

                DrawWeatherControls(listing, environment);
            }

            listing.Gap(4f);
        }

        private void DrawWeatherControls(Listing_Standard listing, PhotoEnvironmentState environment)
        {
            WeatherDef currentWeatherDef = environment.UseRealWeather ? manager.State.TargetMap?.weatherManager.curWeather : environment.VisualWeather;
            string currentLabel = currentWeatherDef != null ? currentWeatherDef.LabelCap.ToString() : string.Empty;

            Rect weatherLabelRect = listing.GetRect(PlaceholderHeight);
            Widgets.Label(weatherLabelRect, "PhotoMode.Scene.Weather".Translate(currentLabel));

            Rect weatherDropdownRect = listing.GetRect(PlaceholderHeight);
            if (Widgets.ButtonText(weatherDropdownRect, currentLabel))
            {
                OpenWeatherFloatMenu();
            }

            listing.Gap(4f);

            Rect useCurrentWeatherRect = listing.GetRect(PlaceholderHeight);
            if (Widgets.ButtonText(useCurrentWeatherRect, "PhotoMode.Scene.UseCurrentWeather".Translate()))
            {
                manager.UseCurrentWeather();
            }

            Rect weatherStatusRect = listing.GetRect(PlaceholderHeight);
            GUI.color = Color.gray;
            Widgets.Label(weatherStatusRect, environment.UseRealWeather ? "PhotoMode.Scene.UsingRealWeather".Translate() : "PhotoMode.Scene.UsingVisualWeather".Translate());
            GUI.color = Color.white;

            listing.Gap(4f);

            GUI.enabled = !environment.UseRealWeather;

            bool particlesEnabled = environment.OverlayParticlesEnabled;
            listing.CheckboxLabeled("PhotoMode.Scene.Weather.Particles".Translate(), ref particlesEnabled);
            environment.OverlayParticlesEnabled = particlesEnabled;

            DrawGradeControl(listing, "PhotoMode.Scene.Weather.Intensity", 0f, 1f, 1f, () => environment.OverlayIntensity, v => environment.OverlayIntensity = v);

            GUI.enabled = true;

            WeatherOverlaySupportStatus overlayStatus = manager.GetVisualWeatherOverlaySupportStatus();
            if (overlayStatus != WeatherOverlaySupportStatus.UsingRealWeather)
            {
                Rect overlayStatusRect = listing.GetRect(PlaceholderHeight);
                GUI.color = Color.gray;
                Widgets.Label(overlayStatusRect, WeatherOverlayStatusLabel(overlayStatus));
                GUI.color = Color.white;
            }
        }

        private static string WeatherOverlayStatusLabel(WeatherOverlaySupportStatus status)
        {
            switch (status)
            {
                case WeatherOverlaySupportStatus.NoOverlay:
                    return "PhotoMode.Scene.Weather.Status.NoOverlay".Translate();
                case WeatherOverlaySupportStatus.FullySupported:
                    return "PhotoMode.Scene.Weather.Status.FullySupported".Translate();
                case WeatherOverlaySupportStatus.PartiallySupported:
                    return "PhotoMode.Scene.Weather.Status.PartiallySupported".Translate();
                case WeatherOverlaySupportStatus.Unsupported:
                    return "PhotoMode.Scene.Weather.Status.Unsupported".Translate();
                default:
                    return string.Empty;
            }
        }

        private void OpenWeatherFloatMenu()
        {
            List<WeatherDef> weatherDefs = DefDatabase<WeatherDef>.AllDefsListForReading;
            List<FloatMenuOption> options = new List<FloatMenuOption>(weatherDefs.Count + 1)
            {
                new FloatMenuOption("PhotoMode.Scene.UseCurrentWeather".Translate(), manager.UseCurrentWeather)
            };

            for (int i = 0; i < weatherDefs.Count; i++)
            {
                WeatherDef weatherDef = weatherDefs[i];
                options.Add(new FloatMenuOption(weatherDef.LabelCap, () => manager.SetVisualWeather(weatherDef)));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void DrawTimePresets(Listing_Standard listing)
        {
            const int columns = 2;
            int rows = Mathf.CeilToInt(TimePresetLabelKeys.Length / (float)columns);

            for (int row = 0; row < rows; row++)
            {
                Rect rowRect = listing.GetRect(PlaceholderHeight);
                float columnWidth = rowRect.width / columns;

                for (int col = 0; col < columns; col++)
                {
                    int index = row * columns + col;
                    if (index >= TimePresetLabelKeys.Length)
                    {
                        continue;
                    }

                    Rect buttonRect = new Rect(rowRect.x + columnWidth * col, rowRect.y, columnWidth, rowRect.height).ContractedBy(2f, 0f);
                    if (Widgets.ButtonText(buttonRect, TimePresetLabelKeys[index].Translate()))
                    {
                        manager.SetVisualTimeHours(TimePresetHours[index]);
                    }
                }

                listing.Gap(2f);
            }
        }
    }
}
