using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public enum PhotoModeTransition
    {
        None,
        Entering,
        Exiting
    }

    public class PhotoModeManager
    {
        public static PhotoModeManager Current { get; private set; }

        public readonly PhotoModeState State = new PhotoModeState();
        public readonly RenderOverrideScope RenderScope = new RenderOverrideScope();
        public readonly PhotoCameraController CameraController = new PhotoCameraController();
        public readonly PhotoCaptureService CaptureService = new PhotoCaptureService();

        private readonly CameraSession cameraSession = new CameraSession();
        private readonly PhotoTimeOfDayRenderParticipant timeOfDayParticipant = new PhotoTimeOfDayRenderParticipant();
        private readonly PhotoWeatherOverlayRenderParticipant weatherOverlayParticipant = new PhotoWeatherOverlayRenderParticipant();

        public PhotoModePanel Panel { get; private set; }

        public CameraSession CameraSession => cameraSession;

        public PhotoModeTransition Transition { get; private set; } = PhotoModeTransition.None;
        public bool IsTransitioning => Transition != PhotoModeTransition.None;

        public PhotoModeManager()
        {
            RenderScope.RegisterParticipant(timeOfDayParticipant);
            RenderScope.RegisterParticipant(weatherOverlayParticipant);
        }

        internal static void SetCurrent(PhotoModeManager manager)
        {
            Current = manager;
        }

        public static bool IsRenderingThisCurrentMap
        {
            get
            {
                PhotoModeManager current = Current;
                if (current == null || !current.State.Active)
                {
                    return false;
                }

                Map targetMap = current.State.TargetMap;
                return targetMap != null && !targetMap.Disposed && targetMap == Find.CurrentMap;
            }
        }

        public bool CanEnter()
        {
            if (State.Active)
            {
                return false;
            }

            if (Verse.Current.ProgramState != ProgramState.Playing)
            {
                return false;
            }

            if (Find.CurrentMap == null || Find.CameraDriver == null)
            {
                return false;
            }

            if (LongEventHandler.ShouldWaitForEvent)
            {
                return false;
            }

            return true;
        }

        public void RequestEnter()
        {
            if (IsTransitioning || !CanEnter())
            {
                return;
            }

            if (PhotoModePatchController.CurrentLifetime == PhotoModePatchLifetime.AlwaysLoaded)
            {
                Enter();
                return;
            }

            Transition = PhotoModeTransition.Entering;
            LongEventHandler.QueueLongEvent(RunDynamicEnter, "PhotoMode.Transition.Preparing", doAsynchronously: false, HandleTransitionException);
        }

        private void RunDynamicEnter()
        {
            try
            {
                PhotoModePatchController.EnsureCategoryApplied();
                Enter();
            }
            finally
            {
                Transition = PhotoModeTransition.None;
            }
        }

        private void HandleTransitionException(Exception ex)
        {
            Logger.Warning("Photo Mode transition failed: " + ex);
            Transition = PhotoModeTransition.None;
        }

        public void Enter()
        {
            if (!CanEnter())
            {
                return;
            }

            Map map = Find.CurrentMap;
            cameraSession.Capture(map);
            CameraController.Begin(cameraSession.EntryRootPos, cameraSession.EntryRootSize, cameraSession.ZoomMin, cameraSession.ZoomMax, State.Camera);

            State.Active = true;
            State.TargetMap = map;

            PhotoPostProcess.EnsureAttached(Find.Camera);

            Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
            EnableCleanScreenshot();

            OpenPanel();

            Logger.Message("Entered Photo Mode.");
        }

        public void Exit(string reason)
        {
            if (!State.Active)
            {
                return;
            }

            ClosePanelIfOpen();
            ExitCore(reason);
            PhotoModePatchController.MarkCategoryRemovalPendingIfDynamic();
            Transition = PhotoModeTransition.None;
        }

        public void RequestExit(string reason)
        {
            if (!State.Active || IsTransitioning)
            {
                return;
            }

            if (PhotoModePatchController.CurrentLifetime == PhotoModePatchLifetime.AlwaysLoaded)
            {
                Exit(reason);
                return;
            }

            ClosePanelIfOpen();
            Transition = PhotoModeTransition.Exiting;
            LongEventHandler.QueueLongEvent(() => RunDynamicExit(reason), "PhotoMode.Transition.Restoring", doAsynchronously: false, HandleTransitionException);
        }

        private void RunDynamicExit(string reason)
        {
            try
            {
                ExitCore(reason);
            }
            finally
            {
                PhotoModePatchController.RemoveCategoryIfDynamic();
                Transition = PhotoModeTransition.None;
            }
        }

        private void ExitCore(string reason)
        {
            try
            {
                RestoreBlueprintPreview();
                CaptureService.CancelIfActive(reason);
                RenderScope.ForceCloseIfActive();
                PhotoPostProcess.Detach();
                weatherOverlayParticipant.ClearMaterialCache();
                cameraSession.Restore();
            }
            finally
            {
                State.Reset();
            }

            Logger.Message("Exited Photo Mode (" + reason + ").");
        }

        public void Reset()
        {
            ClosePanelIfOpen();
            try
            {
                RestoreBlueprintPreview();
                CaptureService.CancelIfActive("Reset");
                RenderScope.ForceCloseIfActive();
                PhotoPostProcess.Detach();
                weatherOverlayParticipant.ClearMaterialCache();
                cameraSession.Restore();
            }
            finally
            {
                State.Reset();
            }
        }

        public void Dispose()
        {
            ClosePanelIfOpen();
            try
            {
                RestoreBlueprintPreview();
                CaptureService.CancelIfActive("Dispose");
                RenderScope.ForceCloseIfActive();
                PhotoPostProcess.Detach();
                weatherOverlayParticipant.ClearMaterialCache();
                cameraSession.Restore();
            }
            finally
            {
                State.Reset();
            }
        }

        private void RestoreBlueprintPreview()
        {
            bool wasEnabled = State.Overlay.ShowBlueprintsAsConstructed;
            State.Overlay.ShowBlueprintsAsConstructed = false;
            PhotoBlueprintRenderService.ClearProxyCache();

            if (wasEnabled)
            {
                PhotoBlueprintRenderService.InvalidateBlueprintMeshes(State.TargetMap);
            }
        }

        public void SetShowBlueprintsAsConstructed(bool enabled)
        {
            if (!State.Active || State.Overlay.ShowBlueprintsAsConstructed == enabled)
            {
                return;
            }

            State.Overlay.ShowBlueprintsAsConstructed = enabled;
            PhotoBlueprintRenderService.InvalidateBlueprintMeshes(State.TargetMap);
        }

        public void EnableCleanScreenshot()
        {
            if (!State.Active)
            {
                return;
            }

            PhotoOverlayOptions overlay = State.Overlay;
            overlay.HideNameplates = true;
            overlay.HideColonistBar = true;
            overlay.HideSelectionBrackets = true;
            overlay.HideDraftedIndicators = true;
            overlay.HideDesignationOverlays = true;
            overlay.HideZones = true;
            overlay.HideRoomOverlays = true;
            overlay.HideInteractionBubbles = true;
            overlay.HideMotes = true;
            overlay.HideTargetingIndicators = true;
            overlay.HideCursor = true;
            overlay.HideForbiddenDesignator = true;

            SetVanillaScreenshotBaseline(true);
        }

        public void SetVanillaScreenshotBaseline(bool active)
        {
            if (!State.Active)
            {
                return;
            }

            State.Overlay.UseVanillaScreenshotBaseline = active;
            Find.UIRoot.screenshotMode.Active = active;
        }

        public void ResetCamera()
        {
            if (!State.Active)
            {
                return;
            }

            CameraController.ResetToEntry(State.Camera);
        }

        public void SetZoom(float value)
        {
            if (!State.Active)
            {
                return;
            }

            CameraController.SetZoom(value);
        }

        public void SetCameraRoll(float degrees)
        {
            if (!State.Active)
            {
                return;
            }

            float clamped = PhotoCameraController.ClampRoll(degrees);
            State.Camera.RollDegrees = clamped;
            cameraSession.ApplyRoll(clamped);
        }

        public void SetVisualTimeHours(float hours)
        {
            if (!State.Active)
            {
                return;
            }

            State.Environment.UseRealTime = false;
            State.Environment.TimeHours = Mathf.Repeat(hours, 24f);
        }

        public void UseCurrentTime()
        {
            if (!State.Active)
            {
                return;
            }

            State.Environment.UseRealTime = true;
        }

        public void SetVisualWeather(WeatherDef weatherDef)
        {
            if (!State.Active || weatherDef == null)
            {
                return;
            }

            State.Environment.UseRealWeather = false;
            State.Environment.VisualWeather = weatherDef;
        }

        public void UseCurrentWeather()
        {
            if (!State.Active)
            {
                return;
            }

            State.Environment.UseRealWeather = true;
        }

        public WeatherOverlaySupportStatus GetVisualWeatherOverlaySupportStatus()
        {
            if (!State.Active)
            {
                return WeatherOverlaySupportStatus.UsingRealWeather;
            }

            PhotoEnvironmentState environment = State.Environment;
            if (environment.UseRealWeather || environment.VisualWeather == null)
            {
                return WeatherOverlaySupportStatus.UsingRealWeather;
            }

            return PhotoWeatherOverlaySupport.Classify(environment.VisualWeather);
        }

        public void ValidateEnvironmentState()
        {
            if (!State.Active)
            {
                return;
            }

            PhotoEnvironmentState environment = State.Environment;
            if (environment.VisualWeather != null && !DefDatabase<WeatherDef>.AllDefsListForReading.Contains(environment.VisualWeather))
            {
                environment.VisualWeather = null;
                environment.UseRealWeather = true;
            }
        }

        public List<PhotoPresetInfo> ListPresets()
        {
            return PhotoPresetService.ListPresets();
        }

        public PhotoPresetOperationResult SavePreset(string displayName)
        {
            if (!State.Active)
            {
                return PhotoPresetOperationResult.Fail(PhotoPresetOperationStatus.NotFound);
            }

            return PhotoPresetService.SaveNew(displayName, State);
        }

        public PhotoPresetOperationResult RenamePreset(string id, string newDisplayName)
        {
            return PhotoPresetService.Rename(id, newDisplayName);
        }

        public PhotoPresetOperationResult DuplicatePreset(string id)
        {
            return PhotoPresetService.Duplicate(id);
        }

        public bool DeletePreset(string id)
        {
            return PhotoPresetService.Delete(id);
        }

        public PhotoPresetOperationResult LoadPreset(string id)
        {
            if (!State.Active)
            {
                return PhotoPresetOperationResult.Fail(PhotoPresetOperationStatus.NotFound);
            }

            return PhotoPresetService.Load(id, State);
        }

        public void ResetVisualPreset()
        {
            if (!State.Active)
            {
                return;
            }

            State.Image.Reset();

            UseCurrentTime();
            UseCurrentWeather();
            State.Environment.OverlayIntensity = 1f;
            State.Environment.OverlayParticlesEnabled = true;

            State.Camera.MovementSpeed = 1f;
            State.Camera.SmoothMovement = true;
        }

        public void SelectNextPawn()
        {
            if (!State.Active)
            {
                return;
            }

            PhotoPawnSelector.SelectNext(State.TargetMap, State.SelectedPawns);
        }

        public void SelectPreviousPawn()
        {
            if (!State.Active)
            {
                return;
            }

            PhotoPawnSelector.SelectPrevious(State.TargetMap, State.SelectedPawns);
        }

        public void DeselectPawn(Pawn pawn)
        {
            if (!State.Active)
            {
                return;
            }

            State.SelectedPawns.Remove(pawn);
        }

        public const float MaxPawnOffsetPerAxis = 2f;

        public void SetSelectedPawnsFacing(Rot4 facing)
        {
            if (!State.Active)
            {
                return;
            }

            List<Pawn> selected = State.SelectedPawns;
            for (int i = 0; i < selected.Count; i++)
            {
                GetOrCreateOverride(selected[i]).Facing = facing;
            }
        }

        public void SetSelectedPawnsOffset(Vector3 offset)
        {
            if (!State.Active)
            {
                return;
            }

            offset.x = Mathf.Clamp(offset.x, -MaxPawnOffsetPerAxis, MaxPawnOffsetPerAxis);
            offset.y = 0f;
            offset.z = Mathf.Clamp(offset.z, -MaxPawnOffsetPerAxis, MaxPawnOffsetPerAxis);

            List<Pawn> selected = State.SelectedPawns;
            for (int i = 0; i < selected.Count; i++)
            {
                GetOrCreateOverride(selected[i]).Offset = offset;
            }
        }

        public void ResetSelectedPawnPositions()
        {
            if (!State.Active)
            {
                return;
            }

            List<Pawn> selected = State.SelectedPawns;
            for (int i = 0; i < selected.Count; i++)
            {
                Pawn pawn = selected[i];
                if (!State.PawnOverrides.TryGetValue(pawn, out PawnPhotoOverride photoOverride))
                {
                    continue;
                }

                photoOverride.Offset = Vector3.zero;
                RemoveOverrideIfDefault(pawn, photoOverride);
            }
        }

        public void SetSelectedPawnsPose(PhotoPawnPose pose)
        {
            if (!State.Active)
            {
                return;
            }

            List<Pawn> selected = State.SelectedPawns;
            for (int i = 0; i < selected.Count; i++)
            {
                Pawn pawn = selected[i];
                if (!PhotoPawnPoseSupport.IsSupported(pawn, pose))
                {
                    continue;
                }

                if (pose == PhotoPawnPose.Standing)
                {
                    if (State.PawnOverrides.TryGetValue(pawn, out PawnPhotoOverride existing))
                    {
                        existing.Pose = PhotoPawnPose.Standing;
                        RemoveOverrideIfDefault(pawn, existing);
                    }
                    continue;
                }

                GetOrCreateOverride(pawn).Pose = pose;
            }
        }

        private void RemoveOverrideIfDefault(Pawn pawn, PawnPhotoOverride photoOverride)
        {
            if (photoOverride.IsDefault)
            {
                State.PawnOverrides.Remove(pawn);
            }
        }

        public void RotateSelectedPawns(RotationDirection direction)
        {
            if (!State.Active)
            {
                return;
            }

            List<Pawn> selected = State.SelectedPawns;
            for (int i = 0; i < selected.Count; i++)
            {
                Pawn pawn = selected[i];
                PawnPhotoOverride photoOverride = GetOrCreateOverride(pawn);
                Rot4 fallback = photoOverride.Pose == PhotoPawnPose.LayingFaceUp ? Rot4.South : pawn.Rotation;
                Rot4 current = photoOverride.Facing ?? fallback;
                photoOverride.Facing = current.Rotated(direction);
            }
        }

        private PawnPhotoOverride GetOrCreateOverride(Pawn pawn)
        {
            if (!State.PawnOverrides.TryGetValue(pawn, out PawnPhotoOverride photoOverride))
            {
                photoOverride = new PawnPhotoOverride();
                State.PawnOverrides[pawn] = photoOverride;
            }

            return photoOverride;
        }

        public void ResetSelectedPawnOverrides()
        {
            if (!State.Active)
            {
                return;
            }

            List<Pawn> selected = State.SelectedPawns;
            for (int i = 0; i < selected.Count; i++)
            {
                State.PawnOverrides.Remove(selected[i]);
            }
        }

        public void ResetAllPawnOverrides()
        {
            if (!State.Active)
            {
                return;
            }

            State.PawnOverrides.Clear();
        }

        public void ValidatePawnState()
        {
            if (!State.Active)
            {
                return;
            }

            PhotoPawnSelector.ValidateSelection(State.SelectedPawns);
            PhotoPawnSelector.ValidateOverrides(State.PawnOverrides);
        }

        public CaptureValidation ResolveRequestedCaptureDimensions()
        {
            int width;
            int height;
            switch (State.Capture.ResolutionMode)
            {
                case CaptureResolutionMode.Fixed:
                    State.Capture.FixedResolution.GetDimensions(out width, out height);
                    break;
                case CaptureResolutionMode.Custom:
                    width = State.Capture.Width;
                    height = State.Capture.Height;
                    break;
                default:
                    width = Screen.width;
                    height = Screen.height;
                    break;
            }

            return PhotoCaptureService.Validate(width, height);
        }

        public void RequestCapture()
        {
            if (!State.Active || CaptureService.CaptureInProgress)
            {
                return;
            }

            CaptureValidation validation = ResolveRequestedCaptureDimensions();
            State.Capture.Status = CaptureRequestStatus.None;
            State.Capture.StatusMessage = null;

            if (!validation.IsValid)
            {
                State.Capture.Status = CaptureRequestStatus.Failed;
                State.Capture.StatusMessage = ("PhotoMode.Capture.Invalid." + validation.Status).Translate();
                return;
            }

            CaptureService.TryRequestCapture(validation.Width, validation.Height, OnCaptureComplete);
        }

        private void OnCaptureComplete(CaptureResult result)
        {
            string savedPath = PhotoOutputService.HandleCaptureResult(result);

            if (result.Success)
            {
                State.Capture.Status = CaptureRequestStatus.Success;
                State.Capture.StatusMessage = "PhotoMode.Capture.Status.Success".Translate(Path.GetFileName(savedPath));
            }
            else
            {
                State.Capture.Status = CaptureRequestStatus.Failed;
                State.Capture.StatusMessage = "PhotoMode.CaptureFailed".Translate(result.FailureReason);
            }
        }

        public void RequestExitFromPanel()
        {
            if (!State.Active || IsTransitioning)
            {
                return;
            }

            Panel = null;
            RequestExit("PanelClosed");
        }

        public void HidePanel()
        {
            if (!State.Active)
            {
                return;
            }

            ClosePanelIfOpen();
            State.PanelVisible = false;
        }

        public void ShowPanel()
        {
            if (!State.Active || Panel != null)
            {
                return;
            }

            OpenPanel();
        }

        private void OpenPanel()
        {
            if (Panel != null)
            {
                return;
            }

            Panel = new PhotoModePanel(this);
            Find.WindowStack.Add(Panel);
            State.PanelVisible = true;
        }

        private void ClosePanelIfOpen()
        {
            if (Panel == null)
            {
                return;
            }

            PhotoModePanel panel = Panel;
            Panel = null;

            panel.SuppressExitOnClose = true;
            if (Find.WindowStack != null)
            {
                Find.WindowStack.TryRemove(panel, doCloseSound: false);
            }
        }
    }
}
