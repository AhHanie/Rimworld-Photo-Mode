using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public partial class PhotoModePanel
    {
        private string customWidthBuffer;
        private string customHeightBuffer;

        private void DrawCaptureSection(Listing_Standard listing, ref bool collapsed)
        {
            collapsed = DrawSectionHeader(listing, collapsed, "PhotoMode.Section.Capture");

            if (!collapsed)
            {
                CaptureSettings capture = manager.State.Capture;

                DrawResolutionModeSelector(listing, capture);

                listing.Gap(4f);

                if (capture.ResolutionMode == CaptureResolutionMode.Fixed)
                {
                    DrawFixedResolutionPresets(listing, capture);
                    listing.Gap(4f);
                }
                else if (capture.ResolutionMode == CaptureResolutionMode.Custom)
                {
                    DrawCustomResolutionFields(listing, capture);
                    listing.Gap(4f);
                }

                CaptureValidation validation = manager.ResolveRequestedCaptureDimensions();
                DrawResolutionPreview(listing, validation);

                listing.Gap(4f);

                bool captureInProgress = manager.CaptureService.CaptureInProgress;
                GUI.enabled = !captureInProgress && validation.IsValid;

                Rect captureButtonRect = listing.GetRect(PlaceholderHeight + 6f);
                string captureLabel = captureInProgress
                    ? "PhotoMode.Capture.Button.Busy".Translate()
                    : "PhotoMode.Capture.Button".Translate(PhotoModeKeyBindingDefOf.PhotoMode_Capture.MainKeyLabel);
                if (Widgets.ButtonText(captureButtonRect, captureLabel))
                {
                    manager.RequestCapture();
                }

                GUI.enabled = true;

                if (capture.Status != CaptureRequestStatus.None)
                {
                    Rect statusRect = listing.GetRect(PlaceholderHeight);
                    GUI.color = capture.Status == CaptureRequestStatus.Success ? Color.green : Color.red;
                    Widgets.Label(statusRect, capture.StatusMessage);
                    GUI.color = Color.white;
                }

                listing.Gap(4f);

                Rect openFolderRect = listing.GetRect(PlaceholderHeight);
                if (Widgets.ButtonText(openFolderRect, "PhotoMode.Capture.OpenFolder".Translate()))
                {
                    PhotoOutputService.OpenOutputFolder();
                }
            }

            listing.Gap(4f);
        }

        private static void DrawResolutionModeSelector(Listing_Standard listing, CaptureSettings capture)
        {
            Rect rowRect = listing.GetRect(PlaceholderHeight);
            float columnWidth = rowRect.width / 3f;

            DrawResolutionModeButton(new Rect(rowRect.x, rowRect.y, columnWidth, rowRect.height), capture, CaptureResolutionMode.Screen, "PhotoMode.Capture.Mode.Screen".Translate());
            DrawResolutionModeButton(new Rect(rowRect.x + columnWidth, rowRect.y, columnWidth, rowRect.height), capture, CaptureResolutionMode.Fixed, "PhotoMode.Capture.Mode.Fixed".Translate());
            DrawResolutionModeButton(new Rect(rowRect.x + columnWidth * 2f, rowRect.y, columnWidth, rowRect.height), capture, CaptureResolutionMode.Custom, "PhotoMode.Capture.Mode.Custom".Translate());
        }

        private static void DrawResolutionModeButton(Rect rect, CaptureSettings capture, CaptureResolutionMode mode, string label)
        {
            rect = rect.ContractedBy(2f, 0f);
            bool active = capture.ResolutionMode == mode;
            Widgets.DrawOptionBackground(rect, active);
            if (Widgets.ButtonInvisible(rect))
            {
                capture.ResolutionMode = mode;
            }

            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, label);
            Text.Anchor = anchor;
        }

        private static readonly CaptureFixedResolution[] FixedResolutionOptions =
        {
            CaptureFixedResolution.FullHD,
            CaptureFixedResolution.QuadHD,
            CaptureFixedResolution.UltraHD
        };

        private static void DrawFixedResolutionPresets(Listing_Standard listing, CaptureSettings capture)
        {
            Rect rowRect = listing.GetRect(PlaceholderHeight);
            float columnWidth = rowRect.width / FixedResolutionOptions.Length;

            for (int i = 0; i < FixedResolutionOptions.Length; i++)
            {
                CaptureFixedResolution option = FixedResolutionOptions[i];
                option.GetDimensions(out int width, out int height);
                Rect buttonRect = new Rect(rowRect.x + columnWidth * i, rowRect.y, columnWidth, rowRect.height).ContractedBy(2f, 0f);

                bool active = capture.FixedResolution == option;
                Widgets.DrawOptionBackground(buttonRect, active);
                if (Widgets.ButtonInvisible(buttonRect))
                {
                    capture.FixedResolution = option;
                }

                TextAnchor anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(buttonRect, width + "x" + height);
                Text.Anchor = anchor;
            }
        }

        private void DrawCustomResolutionFields(Listing_Standard listing, CaptureSettings capture)
        {
            Rect rowRect = listing.GetRect(PlaceholderHeight);
            Rect widthRect = rowRect.LeftHalf().ContractedBy(2f, 0f);
            Rect heightRect = rowRect.RightHalf().ContractedBy(2f, 0f);

            Widgets.TextFieldNumeric(widthRect, ref capture.Width, ref customWidthBuffer, 1f, SystemInfo.maxTextureSize);
            Widgets.TextFieldNumeric(heightRect, ref capture.Height, ref customHeightBuffer, 1f, SystemInfo.maxTextureSize);
        }

        private static void DrawResolutionPreview(Listing_Standard listing, CaptureValidation validation)
        {
            Rect resolutionRect = listing.GetRect(PlaceholderHeight);
            GUI.color = Color.gray;
            Widgets.Label(resolutionRect, "PhotoMode.Capture.Resolution".Translate(validation.Width, validation.Height));
            GUI.color = Color.white;

            float aspect = validation.Height > 0 ? (float)validation.Width / validation.Height : 0f;
            double megapixels = validation.Width * (double)validation.Height / 1_000_000d;

            Rect detailRect = listing.GetRect(PlaceholderHeight);
            GUI.color = Color.gray;
            Widgets.Label(detailRect, "PhotoMode.Capture.AspectAndPixels".Translate(aspect.ToString("0.00"), megapixels.ToString("0.0")));
            GUI.color = Color.white;

            if (!validation.IsValid)
            {
                Rect invalidRect = listing.GetRect(PlaceholderHeight);
                GUI.color = Color.red;
                Widgets.Label(invalidRect, ("PhotoMode.Capture.Invalid." + validation.Status).Translate());
                GUI.color = Color.white;
            }
        }
    }
}
