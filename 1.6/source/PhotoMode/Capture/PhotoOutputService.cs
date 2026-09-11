using System;
using System.Globalization;
using System.IO;
using RimWorld;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public static class PhotoOutputService
    {
        public static string OutputFolderPath => Path.Combine(GenFilePaths.ScreenshotFolderPath, "PhotoMode");

        public static string HandleCaptureResult(CaptureResult result)
        {
            if (!result.Success)
            {
                Logger.Warning("PhotoOutputService: capture failed (" + result.FailureReason + "); nothing written.");
                Messages.Message("PhotoMode.CaptureFailed".Translate(result.FailureReason), MessageTypeDefOf.RejectInput, historical: false);
                return null;
            }

            Directory.CreateDirectory(OutputFolderPath);
            string path = BuildUniqueFilePath();
            File.WriteAllBytes(path, result.PngBytes);

            Logger.Message("PhotoOutputService: saved " + path);

            if (ModSettings.NotifyOnCapture)
            {
                Messages.Message("PhotoMode.CaptureSaved".Translate(Path.GetFileName(path)), MessageTypeDefOf.TaskCompletion, historical: false);
            }

            return path;
        }

        private static string BuildUniqueFilePath()
        {
            string baseName = "PhotoMode_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
            string path = Path.Combine(OutputFolderPath, baseName + ".png");

            int suffix = 1;
            while (File.Exists(path))
            {
                path = Path.Combine(OutputFolderPath, baseName + "_" + suffix.ToString(CultureInfo.InvariantCulture) + ".png");
                suffix++;
            }

            return path;
        }

        public static void OpenOutputFolder()
        {
            Directory.CreateDirectory(OutputFolderPath);
            string fullPath = Path.GetFullPath(OutputFolderPath);

            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                Application.OpenURL(fullPath);
                return;
            }

            Find.WindowStack.Add(new Dialog_MessageBox(fullPath));
        }
    }
}
