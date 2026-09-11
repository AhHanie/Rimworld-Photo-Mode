using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public class PhotoModeState
    {
        public bool Active;
        public Map TargetMap;
        public bool PanelVisible;
        public bool PreviewBypassed;
        public bool PreviewBypassMouseHeld;
        public bool ShowDevToolbar;

        public readonly PhotoModeCameraState Camera = new PhotoModeCameraState();
        public readonly PhotoEnvironmentState Environment = new PhotoEnvironmentState();
        public readonly PhotoImageSettings Image = new PhotoImageSettings();
        public readonly PhotoOverlayOptions Overlay = new PhotoOverlayOptions();
        public readonly CaptureSettings Capture = new CaptureSettings();

        public readonly Dictionary<Pawn, PawnPhotoOverride> PawnOverrides = new Dictionary<Pawn, PawnPhotoOverride>();
        public readonly List<Pawn> SelectedPawns = new List<Pawn>();

        public void Reset()
        {
            Active = false;
            TargetMap = null;
            PanelVisible = false;
            PreviewBypassed = false;
            PreviewBypassMouseHeld = false;
            ShowDevToolbar = false;

            Camera.Reset();
            Environment.Reset();
            Image.Reset();
            Overlay.Reset();
            Capture.Reset();

            PawnOverrides.Clear();
            SelectedPawns.Clear();
        }
    }

    public class PhotoModeCameraState
    {
        public Vector3 Position;
        public float Zoom;
        public float MovementSpeed = 1f;
        public bool SmoothMovement = true;
        public bool ShowCompositionGuides;

        public void Reset()
        {
            Position = Vector3.zero;
            Zoom = 0f;
            MovementSpeed = 1f;
            SmoothMovement = true;
            ShowCompositionGuides = false;
        }
    }

    public class PhotoEnvironmentState
    {
        public bool UseRealTime = true;
        public float TimeHours;
        public bool UseRealWeather = true;
        public WeatherDef VisualWeather;
        public float OverlayIntensity = 1f;
        public bool OverlayParticlesEnabled = true;

        public void Reset()
        {
            UseRealTime = true;
            TimeHours = 0f;
            UseRealWeather = true;
            VisualWeather = null;
            OverlayIntensity = 1f;
            OverlayParticlesEnabled = true;
        }
    }

    public class PhotoImageSettings
    {
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
        public PhotoLutStatus LutStatus = PhotoLutStatus.None;
        public string LutStatusMessage;

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

        public void Reset()
        {
            Exposure = 0f;
            Brightness = 0f;
            Contrast = 0f;
            Saturation = 0f;
            Gamma = 1f;
            Temperature = 0f;
            Tint = 0f;

            VignetteEnabled = false;
            VignetteIntensity = 0f;
            VignetteRadius = 1f;
            VignetteSoftness = 0.5f;

            SharpenEnabled = false;
            SharpenStrength = 0f;

            BloomEnabled = false;
            BloomIntensity = 1f;
            BloomThreshold = 0.75f;
            BloomSpread = 0.5f;

            LutId = string.Empty;
            LutIntensity = 0.05f;
            LutStatus = PhotoLutStatus.None;
            LutStatusMessage = null;

            GrainEnabled = false;
            GrainIntensity = 0.3f;
            GrainSize = 1.5f;

            ChromaticAberrationEnabled = false;
            ChromaticAberrationIntensity = 0.3f;

            TiltShiftEnabled = false;
            TiltShiftFocusPosition = 0.5f;
            TiltShiftFocusWidth = 0.25f;
            TiltShiftStrength = 0.5f;
            TiltShiftFalloff = 0.25f;

            EdgeBlurEnabled = false;
            EdgeBlurStrength = 0.4f;
        }
    }

    public class PhotoOverlayOptions
    {
        public bool UseVanillaScreenshotBaseline = true;
        public bool HideNameplates = true;
        public bool HideColonistBar = true;
        public bool HideSelectionBrackets = true;
        public bool HideDraftedIndicators = true;
        public bool HideDesignationOverlays = true;
        public bool HideZones = true;
        public bool HideRoomOverlays = true;
        public bool HideInteractionBubbles = true;
        public bool HideMotes = true;
        public bool HideTargetingIndicators = true;
        public bool HideCursor = true;
        public bool HideForbiddenDesignator = true;

        public void Reset()
        {
            UseVanillaScreenshotBaseline = true;
            HideNameplates = true;
            HideColonistBar = true;
            HideSelectionBrackets = true;
            HideDraftedIndicators = true;
            HideDesignationOverlays = true;
            HideZones = true;
            HideRoomOverlays = true;
            HideInteractionBubbles = true;
            HideMotes = true;
            HideTargetingIndicators = true;
            HideCursor = true;
            HideForbiddenDesignator = true;
        }
    }

    public enum CaptureResolutionMode
    {
        Screen,
        Fixed,
        Custom
    }

    public enum CaptureFixedResolution
    {
        FullHD,
        QuadHD,
        UltraHD
    }

    public static class CaptureFixedResolutionExtensions
    {
        public static void GetDimensions(this CaptureFixedResolution resolution, out int width, out int height)
        {
            switch (resolution)
            {
                case CaptureFixedResolution.QuadHD:
                    width = 2560;
                    height = 1440;
                    return;
                case CaptureFixedResolution.UltraHD:
                    width = 3840;
                    height = 2160;
                    return;
                default:
                    width = 1920;
                    height = 1080;
                    return;
            }
        }
    }

    public enum CaptureRequestStatus
    {
        None,
        Success,
        Failed
    }

    public class CaptureSettings
    {
        public CaptureResolutionMode ResolutionMode = CaptureResolutionMode.Screen;
        public CaptureFixedResolution FixedResolution = CaptureFixedResolution.FullHD;
        public int Width = 1920;
        public int Height = 1080;
        public string OutputFolderOverride;

        public CaptureRequestStatus Status = CaptureRequestStatus.None;
        public string StatusMessage;

        public void Reset()
        {
            ResolutionMode = CaptureResolutionMode.Screen;
            FixedResolution = CaptureFixedResolution.FullHD;
            Width = 1920;
            Height = 1080;
            OutputFolderOverride = null;

            Status = CaptureRequestStatus.None;
            StatusMessage = null;
        }
    }

    public enum PhotoPawnPose
    {
        Standing,
        LayingFaceUp
    }

    public class PawnPhotoOverride
    {
        public Rot4? Facing;
        public Vector3 Offset;
        public PhotoPawnPose Pose = PhotoPawnPose.Standing;

        public bool IsDefault => !Facing.HasValue && Offset == Vector3.zero && Pose == PhotoPawnPose.Standing;
    }
}
