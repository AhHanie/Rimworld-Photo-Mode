using System;
using Verse;

namespace Photo_Mode
{
    [Serializable]
    public class PhotoPresetFile
    {
        public int SchemaVersion = PhotoPresetService.CurrentSchemaVersion;
        public string Id = string.Empty;
        public string DisplayName = string.Empty;

        public bool UseRealTime = true;
        public float TimeHours;

        public bool UseRealWeather = true;
        public string VisualWeatherDefName = string.Empty;
        public float OverlayIntensity = 1f;
        public bool OverlayParticlesEnabled = true;

        public float Exposure;
        public float Brightness;
        public float Contrast;
        public float Saturation;
        public float Gamma = 1f;
        public float Temperature;
        public float Tint;

        public bool VignetteEnabled;
        public float VignetteIntensity;
        public float VignetteRadius = 1f;
        public float VignetteSoftness = 0.5f;

        public bool SharpenEnabled;
        public float SharpenStrength;

        public bool BloomEnabled;
        public float BloomIntensity = 1f;
        public float BloomThreshold = 0.75f;
        public float BloomSpread = 0.5f;

        public string LutId = string.Empty;
        public float LutIntensity = 0.05f;

        public bool GrainEnabled;
        public float GrainIntensity = 0.3f;
        public float GrainSize = 1.5f;

        public bool ChromaticAberrationEnabled;
        public float ChromaticAberrationIntensity = 0.3f;

        public bool TiltShiftEnabled;
        public float TiltShiftFocusPosition = 0.5f;
        public float TiltShiftFocusWidth = 0.25f;
        public float TiltShiftStrength = 0.5f;
        public float TiltShiftFalloff = 0.25f;

        public bool EdgeBlurEnabled;
        public float EdgeBlurStrength = 0.4f;

        public float CameraMovementSpeed = 1f;
        public bool CameraSmoothMovement = true;
    }

    public class PhotoPresetInfo : IRenameable
    {
        public readonly string Id;
        public readonly string FilePath;
        private string displayName;

        public PhotoPresetInfo(string id, string displayName, string filePath)
        {
            Id = id;
            this.displayName = displayName;
            FilePath = filePath;
        }

        public string RenamableLabel
        {
            get => displayName;
            set => displayName = value;
        }

        public string BaseLabel => displayName;
        public string InspectLabel => displayName;
    }

    public enum PhotoPresetOperationStatus
    {
        Success,
        NotFound,
        InvalidName,
        InvalidFile,
        WeatherDefMissing,
        LutMissing
    }

    public struct PhotoPresetOperationResult
    {
        public PhotoPresetOperationStatus Status;
        public PhotoPresetInfo Preset;

        public bool Success => Status == PhotoPresetOperationStatus.Success || Status == PhotoPresetOperationStatus.WeatherDefMissing || Status == PhotoPresetOperationStatus.LutMissing;

        public static PhotoPresetOperationResult Ok(PhotoPresetInfo preset, PhotoPresetOperationStatus status = PhotoPresetOperationStatus.Success)
        {
            return new PhotoPresetOperationResult { Status = status, Preset = preset };
        }

        public static PhotoPresetOperationResult Fail(PhotoPresetOperationStatus status)
        {
            return new PhotoPresetOperationResult { Status = status, Preset = null };
        }
    }
}
