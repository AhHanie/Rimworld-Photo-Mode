using LudeonTK;

namespace Photo_Mode
{
    internal static class PhotoModeCaptureDebugActions
    {
        [DebugAction("Photo Mode", "Capture screen resolution PNG", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void CaptureScreenResolution()
        {
            PhotoModeManager manager = PhotoModeManager.Current;
            if (manager == null || !manager.State.Active)
            {
                Logger.Warning("Debug capture requested while Photo Mode is inactive.");
                return;
            }

            manager.CaptureService.TryRequestScreenResolutionCapture(OnCaptureComplete);
        }

        [DebugAction("Photo Mode", "Open screenshot folder", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void OpenScreenshotFolder()
        {
            PhotoOutputService.OpenOutputFolder();
        }

        private static void OnCaptureComplete(CaptureResult result)
        {
            if (result.Success)
            {
                Logger.Message("Debug capture succeeded: " + result.Width + "x" + result.Height + ", " + result.PngBytes.Length + " bytes.");
            }

            PhotoOutputService.HandleCaptureResult(result);
        }
    }
}
