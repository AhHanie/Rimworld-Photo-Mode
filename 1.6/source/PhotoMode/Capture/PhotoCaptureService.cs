using System;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public struct CaptureResult
    {
        public readonly bool Success;
        public readonly byte[] PngBytes;
        public readonly int Width;
        public readonly int Height;
        public readonly string FailureReason;

        private CaptureResult(bool success, byte[] pngBytes, int width, int height, string failureReason)
        {
            Success = success;
            PngBytes = pngBytes;
            Width = width;
            Height = height;
            FailureReason = failureReason;
        }

        public static CaptureResult Succeeded(byte[] pngBytes, int width, int height)
        {
            return new CaptureResult(true, pngBytes, width, height, null);
        }

        public static CaptureResult Failed(string reason)
        {
            return new CaptureResult(false, null, 0, 0, reason);
        }
    }

    public enum CaptureValidationStatus
    {
        Valid,
        InvalidDimensions,
        ExceedsMaxTextureSize,
        ExceedsMaxPixelCount,
        UnsupportedRenderTarget
    }

    public struct CaptureValidation
    {
        public readonly CaptureValidationStatus Status;
        public readonly int Width;
        public readonly int Height;

        public bool IsValid => Status == CaptureValidationStatus.Valid;

        public CaptureValidation(CaptureValidationStatus status, int width, int height)
        {
            Status = status;
            Width = width;
            Height = height;
        }
    }

    public class PhotoCaptureService
    {
        public const int MaxCapturePixels = 32_000_000;

        private Camera pendingCamera;
        private RenderTexture pendingTexture;
        private RenderTexture previousTargetTexture;
        private Rect previousRect;
        private bool previousPanelVisible;
        private bool previousShowCompositionGuides;
        private bool previousCursorVisible;
        private Action<CaptureResult> pendingCallback;

        public bool CaptureInProgress { get; private set; }

        public static CaptureValidation Validate(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                return new CaptureValidation(CaptureValidationStatus.InvalidDimensions, width, height);
            }

            if (width > SystemInfo.maxTextureSize || height > SystemInfo.maxTextureSize)
            {
                return new CaptureValidation(CaptureValidationStatus.ExceedsMaxTextureSize, width, height);
            }

            if ((long)width * height > MaxCapturePixels)
            {
                return new CaptureValidation(CaptureValidationStatus.ExceedsMaxPixelCount, width, height);
            }

            if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32))
            {
                return new CaptureValidation(CaptureValidationStatus.UnsupportedRenderTarget, width, height);
            }

            return new CaptureValidation(CaptureValidationStatus.Valid, width, height);
        }

        public bool TryGetCaptureAspect(out float aspect)
        {
            if (!CaptureInProgress || pendingTexture == null)
            {
                aspect = 0f;
                return false;
            }

            aspect = (float)pendingTexture.width / pendingTexture.height;
            return true;
        }

        public bool TryRequestScreenResolutionCapture(Action<CaptureResult> onComplete)
        {
            return TryRequestCapture(Screen.width, Screen.height, onComplete);
        }

        public bool TryRequestCapture(int width, int height, Action<CaptureResult> onComplete)
        {
            if (CaptureInProgress)
            {
                onComplete?.Invoke(CaptureResult.Failed("CaptureInProgress"));
                return false;
            }

            PhotoModeManager manager = PhotoModeManager.Current;
            if (manager == null || !manager.State.Active)
            {
                onComplete?.Invoke(CaptureResult.Failed("PhotoModeInactive"));
                return false;
            }

            Map map = manager.State.TargetMap;
            if (map == null || map.Disposed || map != Find.CurrentMap)
            {
                onComplete?.Invoke(CaptureResult.Failed("NoActiveMap"));
                return false;
            }

            Camera camera = Find.Camera;
            if (camera == null)
            {
                onComplete?.Invoke(CaptureResult.Failed("NoCamera"));
                return false;
            }

            CaptureValidation validation = Validate(width, height);
            if (!validation.IsValid)
            {
                onComplete?.Invoke(CaptureResult.Failed(validation.Status.ToString()));
                return false;
            }

            RenderTexture texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            if (!texture.Create())
            {
                UnityEngine.Object.Destroy(texture);
                onComplete?.Invoke(CaptureResult.Failed("RenderTextureCreateFailed"));
                return false;
            }

            pendingCallback = onComplete;
            pendingCamera = camera;
            pendingTexture = texture;
            previousTargetTexture = camera.targetTexture;
            previousRect = camera.rect;
            previousPanelVisible = manager.State.PanelVisible;
            previousShowCompositionGuides = manager.State.Camera.ShowCompositionGuides;
            previousCursorVisible = Cursor.visible;

            manager.HidePanel();
            manager.State.Camera.ShowCompositionGuides = false;
            Cursor.visible = false;

            camera.targetTexture = texture;
            camera.rect = new Rect(0f, 0f, 1f, 1f);

            CaptureInProgress = true;
            return true;
        }

        public void HandleCameraRenderComplete(Camera camera)
        {
            if (!CaptureInProgress || camera != pendingCamera)
            {
                return;
            }

            CaptureResult result = CaptureResult.Failed("ReadbackFailed");
            Texture2D readback = null;
            try
            {
                readback = pendingTexture.CreateTexture2D(TextureFormat.RGB24, mipChain: false);
                result = CaptureResult.Succeeded(readback.EncodeToPNG(), pendingTexture.width, pendingTexture.height);
            }
            finally
            {
                if (readback != null)
                {
                    UnityEngine.Object.Destroy(readback);
                }

                CompleteActive(result, restorePhotoModeUi: true);
            }
        }

        public void CancelIfActive(string reason)
        {
            if (!CaptureInProgress)
            {
                return;
            }

            Logger.Warning("PhotoCaptureService: cancelling active capture (" + reason + ").");
            CompleteActive(CaptureResult.Failed(reason), restorePhotoModeUi: false);
        }

        private void CompleteActive(CaptureResult result, bool restorePhotoModeUi)
        {
            Camera camera = pendingCamera;
            if (camera != null)
            {
                camera.targetTexture = previousTargetTexture;
                camera.rect = previousRect;
            }

            if (pendingTexture != null)
            {
                pendingTexture.Release();
                UnityEngine.Object.Destroy(pendingTexture);
            }

            Cursor.visible = previousCursorVisible;

            if (restorePhotoModeUi)
            {
                PhotoModeManager manager = PhotoModeManager.Current;
                if (manager != null && manager.State.Active)
                {
                    manager.State.Camera.ShowCompositionGuides = previousShowCompositionGuides;
                    if (previousPanelVisible)
                    {
                        manager.ShowPanel();
                    }
                }
            }

            pendingCamera = null;
            pendingTexture = null;
            previousTargetTexture = null;
            CaptureInProgress = false;

            Action<CaptureResult> callback = pendingCallback;
            pendingCallback = null;
            callback?.Invoke(result);
        }
    }
}
