using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public partial class PhotoModePanel
    {
        private void DrawCameraSection(Listing_Standard listing, ref bool collapsed)
        {
            collapsed = DrawSectionHeader(listing, collapsed, "PhotoMode.Section.Camera");

            if (!collapsed)
            {
                PhotoModeCameraState camera = manager.State.Camera;

                Rect speedLabelRect = listing.GetRect(PlaceholderHeight);
                Widgets.Label(speedLabelRect, "PhotoMode.Camera.Speed".Translate(camera.MovementSpeed.ToString("0.0")));

                Rect speedSliderRect = listing.GetRect(PlaceholderHeight);
                camera.MovementSpeed = Widgets.HorizontalSlider(speedSliderRect, camera.MovementSpeed, 0.2f, 5f, roundTo: 0.1f);

                bool smoothMovement = camera.SmoothMovement;
                listing.CheckboxLabeled("PhotoMode.Camera.SmoothMovement".Translate(), ref smoothMovement);
                camera.SmoothMovement = smoothMovement;

                listing.Gap(4f);

                Rect zoomLabelRect = listing.GetRect(PlaceholderHeight);
                Widgets.Label(zoomLabelRect, "PhotoMode.Camera.Zoom".Translate(camera.Zoom.ToString("0.0")));

                Rect zoomSliderRect = listing.GetRect(PlaceholderHeight);
                float zoomValue = Widgets.HorizontalSlider(zoomSliderRect, camera.Zoom, manager.CameraController.ZoomMin, manager.CameraController.ZoomMax);
                if (!Mathf.Approximately(zoomValue, camera.Zoom))
                {
                    manager.SetZoom(zoomValue);
                }

                listing.Gap(4f);

                Rect rollLabelRect = listing.GetRect(PlaceholderHeight);
                Widgets.Label(rollLabelRect, "PhotoMode.Camera.Roll".Translate(camera.RollDegrees.ToString("0")));
                TooltipHandler.TipRegion(rollLabelRect, "PhotoMode.Camera.Roll.Tooltip".Translate());

                Rect rollSliderRect = listing.GetRect(PlaceholderHeight);
                float rollValue = Widgets.HorizontalSlider(rollSliderRect, camera.RollDegrees, PhotoCameraController.MinRollDegrees, PhotoCameraController.MaxRollDegrees, roundTo: 1f);
                if (!Mathf.Approximately(rollValue, camera.RollDegrees))
                {
                    manager.SetCameraRoll(rollValue);
                }

                Rect resetRect = listing.GetRect(PlaceholderHeight);
                if (Widgets.ButtonText(resetRect, "PhotoMode.Camera.Reset".Translate()))
                {
                    manager.ResetCamera();
                }
            }

            listing.Gap(4f);
        }
    }
}
