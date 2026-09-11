using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public static class PhotoPresetService
    {
        public const int CurrentSchemaVersion = 1;
        private const string FileExtension = ".json";

        public static string PresetsFolderPath => Path.Combine(GenFilePaths.ConfigFolderPath, "PhotoModePresets");

        public static List<PhotoPresetInfo> ListPresets()
        {
            List<PhotoPresetInfo> presets = new List<PhotoPresetInfo>();

            if (!Directory.Exists(PresetsFolderPath))
            {
                return presets;
            }

            string[] files = Directory.GetFiles(PresetsFolderPath, "*" + FileExtension);
            for (int i = 0; i < files.Length; i++)
            {
                PhotoPresetFile data = ReadFile(files[i]);
                if (data == null)
                {
                    continue;
                }

                presets.Add(new PhotoPresetInfo(data.Id, data.DisplayName, files[i]));
            }

            presets.Sort((a, b) => string.Compare(a.RenamableLabel, b.RenamableLabel, StringComparison.OrdinalIgnoreCase));
            return presets;
        }

        public static PhotoPresetOperationResult SaveNew(string displayName, PhotoModeState state)
        {
            string trimmedName = (displayName ?? string.Empty).Trim();
            if (trimmedName.Length == 0)
            {
                return PhotoPresetOperationResult.Fail(PhotoPresetOperationStatus.InvalidName);
            }

            string id = Guid.NewGuid().ToString("N");
            PhotoPresetFile data = BuildFileFromState(id, trimmedName, state);

            Directory.CreateDirectory(PresetsFolderPath);
            File.WriteAllText(GetPath(id), JsonUtility.ToJson(data, prettyPrint: true));

            return PhotoPresetOperationResult.Ok(new PhotoPresetInfo(id, trimmedName, GetPath(id)));
        }

        public static PhotoPresetOperationResult Rename(string id, string newDisplayName)
        {
            string trimmedName = (newDisplayName ?? string.Empty).Trim();
            if (trimmedName.Length == 0)
            {
                return PhotoPresetOperationResult.Fail(PhotoPresetOperationStatus.InvalidName);
            }

            string path = GetPath(id);
            PhotoPresetFile data = ReadFile(path);
            if (data == null)
            {
                return PhotoPresetOperationResult.Fail(PhotoPresetOperationStatus.NotFound);
            }

            data.DisplayName = trimmedName;
            File.WriteAllText(path, JsonUtility.ToJson(data, prettyPrint: true));

            return PhotoPresetOperationResult.Ok(new PhotoPresetInfo(id, trimmedName, path));
        }

        public static PhotoPresetOperationResult Duplicate(string id)
        {
            PhotoPresetFile data = ReadFile(GetPath(id));
            if (data == null)
            {
                return PhotoPresetOperationResult.Fail(PhotoPresetOperationStatus.NotFound);
            }

            string newId = Guid.NewGuid().ToString("N");
            data.Id = newId;
            data.DisplayName = "PhotoMode.Presets.CopyName".Translate(data.DisplayName);

            Directory.CreateDirectory(PresetsFolderPath);
            File.WriteAllText(GetPath(newId), JsonUtility.ToJson(data, prettyPrint: true));

            return PhotoPresetOperationResult.Ok(new PhotoPresetInfo(newId, data.DisplayName, GetPath(newId)));
        }

        public static bool Delete(string id)
        {
            string path = GetPath(id);
            if (!File.Exists(path))
            {
                return false;
            }

            File.Delete(path);
            return true;
        }

        public static PhotoPresetOperationResult Load(string id, PhotoModeState state)
        {
            string path = GetPath(id);
            PhotoPresetFile data = ReadFile(path);
            if (data == null)
            {
                return PhotoPresetOperationResult.Fail(PhotoPresetOperationStatus.NotFound);
            }

            PhotoEnvironmentState environment = state.Environment;
            environment.UseRealTime = data.UseRealTime;
            environment.TimeHours = Mathf.Repeat(data.TimeHours, 24f);
            environment.OverlayIntensity = Mathf.Clamp01(data.OverlayIntensity);
            environment.OverlayParticlesEnabled = data.OverlayParticlesEnabled;

            PhotoPresetOperationStatus status = PhotoPresetOperationStatus.Success;
            if (data.UseRealWeather || string.IsNullOrEmpty(data.VisualWeatherDefName))
            {
                environment.UseRealWeather = true;
                environment.VisualWeather = null;
            }
            else
            {
                WeatherDef weatherDef = DefDatabase<WeatherDef>.GetNamedSilentFail(data.VisualWeatherDefName);
                if (weatherDef != null)
                {
                    environment.UseRealWeather = false;
                    environment.VisualWeather = weatherDef;
                }
                else
                {
                    environment.UseRealWeather = true;
                    environment.VisualWeather = null;
                    status = PhotoPresetOperationStatus.WeatherDefMissing;
                }
            }

            PhotoImageSettings image = state.Image;
            image.Exposure = data.Exposure;
            image.Brightness = data.Brightness;
            image.Contrast = data.Contrast;
            image.Saturation = data.Saturation;
            image.Gamma = data.Gamma;
            image.Temperature = data.Temperature;
            image.Tint = data.Tint;

            image.VignetteEnabled = data.VignetteEnabled;
            image.VignetteIntensity = data.VignetteIntensity;
            image.VignetteRadius = data.VignetteRadius;
            image.VignetteSoftness = data.VignetteSoftness;

            image.SharpenEnabled = data.SharpenEnabled;
            image.SharpenStrength = data.SharpenStrength;

            image.BloomEnabled = data.BloomEnabled;
            image.BloomIntensity = data.BloomIntensity;
            image.BloomThreshold = data.BloomThreshold;
            image.BloomSpread = data.BloomSpread;

            if (string.IsNullOrEmpty(data.LutId) || PhotoLutLibrary.Exists(data.LutId))
            {
                image.LutId = data.LutId ?? string.Empty;
                image.LutIntensity = Mathf.Clamp01(data.LutIntensity);
            }
            else
            {
                image.LutId = string.Empty;
                image.LutIntensity = 0.05f;
                if (status == PhotoPresetOperationStatus.Success)
                {
                    status = PhotoPresetOperationStatus.LutMissing;
                }
            }

            image.GrainEnabled = data.GrainEnabled;
            image.GrainIntensity = data.GrainIntensity;
            image.GrainSize = data.GrainSize;

            image.ChromaticAberrationEnabled = data.ChromaticAberrationEnabled;
            image.ChromaticAberrationIntensity = data.ChromaticAberrationIntensity;

            image.TiltShiftEnabled = data.TiltShiftEnabled;
            image.TiltShiftFocusPosition = Mathf.Clamp01(data.TiltShiftFocusPosition);
            image.TiltShiftFocusWidth = Mathf.Clamp01(data.TiltShiftFocusWidth);
            image.TiltShiftStrength = Mathf.Clamp01(data.TiltShiftStrength);
            image.TiltShiftFalloff = Mathf.Clamp(data.TiltShiftFalloff, 0.01f, 1f);

            image.EdgeBlurEnabled = data.EdgeBlurEnabled;
            image.EdgeBlurStrength = Mathf.Clamp01(data.EdgeBlurStrength);

            state.Camera.MovementSpeed = data.CameraMovementSpeed;
            state.Camera.SmoothMovement = data.CameraSmoothMovement;

            return PhotoPresetOperationResult.Ok(new PhotoPresetInfo(data.Id, data.DisplayName, path), status);
        }

        private static PhotoPresetFile BuildFileFromState(string id, string displayName, PhotoModeState state)
        {
            PhotoEnvironmentState environment = state.Environment;
            PhotoImageSettings image = state.Image;

            return new PhotoPresetFile
            {
                SchemaVersion = CurrentSchemaVersion,
                Id = id,
                DisplayName = displayName,

                UseRealTime = environment.UseRealTime,
                TimeHours = environment.TimeHours,

                UseRealWeather = environment.UseRealWeather,
                VisualWeatherDefName = environment.VisualWeather?.defName ?? string.Empty,
                OverlayIntensity = environment.OverlayIntensity,
                OverlayParticlesEnabled = environment.OverlayParticlesEnabled,

                Exposure = image.Exposure,
                Brightness = image.Brightness,
                Contrast = image.Contrast,
                Saturation = image.Saturation,
                Gamma = image.Gamma,
                Temperature = image.Temperature,
                Tint = image.Tint,

                VignetteEnabled = image.VignetteEnabled,
                VignetteIntensity = image.VignetteIntensity,
                VignetteRadius = image.VignetteRadius,
                VignetteSoftness = image.VignetteSoftness,

                SharpenEnabled = image.SharpenEnabled,
                SharpenStrength = image.SharpenStrength,

                BloomEnabled = image.BloomEnabled,
                BloomIntensity = image.BloomIntensity,
                BloomThreshold = image.BloomThreshold,
                BloomSpread = image.BloomSpread,

                LutId = image.LutId,
                LutIntensity = image.LutIntensity,

                GrainEnabled = image.GrainEnabled,
                GrainIntensity = image.GrainIntensity,
                GrainSize = image.GrainSize,

                ChromaticAberrationEnabled = image.ChromaticAberrationEnabled,
                ChromaticAberrationIntensity = image.ChromaticAberrationIntensity,

                TiltShiftEnabled = image.TiltShiftEnabled,
                TiltShiftFocusPosition = image.TiltShiftFocusPosition,
                TiltShiftFocusWidth = image.TiltShiftFocusWidth,
                TiltShiftStrength = image.TiltShiftStrength,
                TiltShiftFalloff = image.TiltShiftFalloff,

                EdgeBlurEnabled = image.EdgeBlurEnabled,
                EdgeBlurStrength = image.EdgeBlurStrength,

                CameraMovementSpeed = state.Camera.MovementSpeed,
                CameraSmoothMovement = state.Camera.SmoothMovement
            };
        }

        private static string GetPath(string id)
        {
            return Path.Combine(PresetsFolderPath, id + FileExtension);
        }

        private static PhotoPresetFile ReadFile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            string text = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            string trimmed = text.Trim();
            if (!trimmed.StartsWith("{", StringComparison.Ordinal) || !trimmed.EndsWith("}", StringComparison.Ordinal))
            {
                return null;
            }

            PhotoPresetFile data = JsonUtility.FromJson<PhotoPresetFile>(trimmed);
            if (data == null || data.SchemaVersion <= 0 || string.IsNullOrEmpty(data.Id))
            {
                return null;
            }

            return data;
        }
    }
}
