using RimWorld;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public class CameraSession
    {
        public const float PhotoZoomMin = 6f;
        public const float PhotoZoomMax = 100f;

        private static bool warnedNonstandardConfigThisSession;

        private Map map;
        private Vector3 rootPos;
        private float rootSize;
        private TimeSpeed entryTimeSpeed;
        private bool screenshotModeWasActive;

        private CameraMapConfig zoomConfig;
        private FloatRange originalSizeRange;
        private bool zoomRangeWidened;

        public Vector3 EntryRootPos => rootPos;
        public float EntryRootSize => rootSize;
        public float ZoomMin { get; private set; }
        public float ZoomMax { get; private set; }

        public void Capture(Map targetMap)
        {
            map = targetMap;

            RememberedCameraPos remembered = targetMap.rememberedCameraPos;
            rootPos = remembered.rootPos;
            rootSize = remembered.rootSize;

            entryTimeSpeed = Find.TickManager.CurTimeSpeed;
            screenshotModeWasActive = Find.UIRoot.screenshotMode.Active;

            CaptureZoomRange();
        }

        private void CaptureZoomRange()
        {
            zoomConfig = Find.CameraDriver != null ? Find.CameraDriver.config : null;
            zoomRangeWidened = false;

            if (zoomConfig == null)
            {
                ZoomMin = PhotoZoomMin;
                ZoomMax = PhotoZoomMax;
                return;
            }

            originalSizeRange = zoomConfig.sizeRange;

            if (zoomConfig.GetType() != typeof(CameraMapConfig_Normal) && !warnedNonstandardConfigThisSession)
            {
                warnedNonstandardConfigThisSession = true;
                Logger.Warning("Nonstandard CameraMapConfig detected (" + zoomConfig.GetType().Name + "); extending its zoom range.");
                Messages.Message("PhotoMode.NonstandardCameraConfigWarning".Translate(), MessageTypeDefOf.CautionInput, historical: false);
            }

            if (!ModSettings.ExtendedZoomEnabled)
            {
                ZoomMin = originalSizeRange.min;
                ZoomMax = originalSizeRange.max;
                return;
            }

            float widenedMin = Mathf.Min(PhotoZoomMin, originalSizeRange.min);
            float widenedMax = Mathf.Max(PhotoZoomMax, originalSizeRange.max);

            if (widenedMin < originalSizeRange.min || widenedMax > originalSizeRange.max)
            {
                zoomConfig.sizeRange = new FloatRange(widenedMin, widenedMax);
                zoomRangeWidened = true;
            }

            ZoomMin = widenedMin;
            ZoomMax = widenedMax;
        }

        public void Restore()
        {
            if (map == null)
            {
                return;
            }

            RememberedCameraPos remembered = map.rememberedCameraPos;
            remembered.rootPos = rootPos;
            remembered.rootSize = rootSize;

            if (Find.CurrentMap == map && Find.CameraDriver != null)
            {
                Find.CameraDriver.SetRootPosAndSize(rootPos, rootSize);
            }

            RestoreZoomRange();

            if (Find.TickManager != null)
            {
                Find.TickManager.CurTimeSpeed = entryTimeSpeed;
            }

            if (Find.UIRoot != null)
            {
                Find.UIRoot.screenshotMode.Active = screenshotModeWasActive;
            }

            map = null;
        }

        private void RestoreZoomRange()
        {
            if (zoomRangeWidened && zoomConfig != null)
            {
                if (Find.CameraDriver != null && Find.CameraDriver.config == zoomConfig)
                {
                    zoomConfig.sizeRange = originalSizeRange;
                }
                else
                {
                    Logger.Warning("CameraDriver.config changed during Photo Mode session; skipped restoring its original zoom range.");
                }
            }

            zoomConfig = null;
            zoomRangeWidened = false;
        }
    }
}
