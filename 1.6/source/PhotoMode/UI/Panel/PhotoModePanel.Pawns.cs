using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public partial class PhotoModePanel
    {
        private void DrawPawnsSection(Listing_Standard listing, ref bool collapsed)
        {
            collapsed = DrawSectionHeader(listing, collapsed, "PhotoMode.Section.Pawns");

            if (!collapsed)
            {
                GUI.color = Color.gray;
                listing.Label("PhotoMode.Pawns.ClickHint".Translate());
                GUI.color = Color.white;

                listing.Gap(2f);

                Rect navRect = listing.GetRect(PlaceholderHeight);
                Rect prevRect = navRect.LeftHalf().ContractedBy(2f, 0f);
                Rect nextRect = navRect.RightHalf().ContractedBy(2f, 0f);
                if (Widgets.ButtonText(prevRect, "PhotoMode.Pawns.Previous".Translate()))
                {
                    manager.SelectPreviousPawn();
                }
                if (Widgets.ButtonText(nextRect, "PhotoMode.Pawns.Next".Translate()))
                {
                    manager.SelectNextPawn();
                }

                listing.Gap(4f);

                List<Pawn> selected = manager.State.SelectedPawns;
                if (selected.Count == 0)
                {
                    Rect noneRect = listing.GetRect(PlaceholderHeight);
                    GUI.color = Color.gray;
                    Widgets.Label(noneRect, "PhotoMode.Pawns.NoneSelected".Translate());
                    GUI.color = Color.white;
                }
                else
                {
                    for (int i = selected.Count - 1; i >= 0; i--)
                    {
                        Pawn pawn = selected[i];
                        Rect rowRect = listing.GetRect(PlaceholderHeight);
                        Rect labelRect = rowRect.LeftPart(0.76f);
                        Rect removeRect = rowRect.RightPart(0.22f);

                        Widgets.Label(labelRect, pawn.LabelCap);
                        if (Widgets.ButtonText(removeRect, "PhotoMode.Pawns.Deselect".Translate()))
                        {
                            manager.DeselectPawn(pawn);
                        }
                    }
                }

                listing.Gap(4f);

                DrawFacingControls(listing, selected);

                listing.Gap(4f);

                DrawPoseControls(listing, selected);

                listing.Gap(4f);

                DrawPositionControls(listing, selected);

                listing.Gap(4f);

                Rect resetSelectedRect = listing.GetRect(PlaceholderHeight);
                if (Widgets.ButtonText(resetSelectedRect, "PhotoMode.Pawns.ResetSelected".Translate()))
                {
                    manager.ResetSelectedPawnOverrides();
                }

                Rect resetAllRect = listing.GetRect(PlaceholderHeight);
                if (Widgets.ButtonText(resetAllRect, "PhotoMode.Pawns.ResetAll".Translate()))
                {
                    manager.ResetAllPawnOverrides();
                }
            }

            listing.Gap(4f);
        }

        private void DrawFacingControls(Listing_Standard listing, List<Pawn> selected)
        {
            Rect facingLabelRect = listing.GetRect(PlaceholderHeight);
            Widgets.Label(facingLabelRect, "PhotoMode.Pawns.Facing".Translate());

            bool hasSelection = selected.Count > 0;
            GUI.enabled = hasSelection;

            Rect directionsRect = listing.GetRect(PlaceholderHeight);
            float directionWidth = directionsRect.width / 4f;

            Rect northRect = new Rect(directionsRect.x, directionsRect.y, directionWidth, directionsRect.height).ContractedBy(2f, 0f);
            if (Widgets.ButtonText(northRect, "PhotoMode.Pawns.Facing.North".Translate()))
            {
                manager.SetSelectedPawnsFacing(Rot4.North);
            }

            Rect eastRect = new Rect(directionsRect.x + directionWidth, directionsRect.y, directionWidth, directionsRect.height).ContractedBy(2f, 0f);
            if (Widgets.ButtonText(eastRect, "PhotoMode.Pawns.Facing.East".Translate()))
            {
                manager.SetSelectedPawnsFacing(Rot4.East);
            }

            Rect southRect = new Rect(directionsRect.x + directionWidth * 2f, directionsRect.y, directionWidth, directionsRect.height).ContractedBy(2f, 0f);
            if (Widgets.ButtonText(southRect, "PhotoMode.Pawns.Facing.South".Translate()))
            {
                manager.SetSelectedPawnsFacing(Rot4.South);
            }

            Rect westRect = new Rect(directionsRect.x + directionWidth * 3f, directionsRect.y, directionWidth, directionsRect.height).ContractedBy(2f, 0f);
            if (Widgets.ButtonText(westRect, "PhotoMode.Pawns.Facing.West".Translate()))
            {
                manager.SetSelectedPawnsFacing(Rot4.West);
            }

            listing.Gap(4f);

            Rect rotateRect = listing.GetRect(PlaceholderHeight);
            Rect rotateCcwRect = rotateRect.LeftHalf().ContractedBy(2f, 0f);
            Rect rotateCwRect = rotateRect.RightHalf().ContractedBy(2f, 0f);
            if (Widgets.ButtonText(rotateCcwRect, "PhotoMode.Pawns.RotateCounterclockwise".Translate()))
            {
                manager.RotateSelectedPawns(RotationDirection.Counterclockwise);
            }
            if (Widgets.ButtonText(rotateCwRect, "PhotoMode.Pawns.RotateClockwise".Translate()))
            {
                manager.RotateSelectedPawns(RotationDirection.Clockwise);
            }

            GUI.enabled = true;
        }

        private void DrawPoseControls(Listing_Standard listing, List<Pawn> selected)
        {
            Rect poseLabelRect = listing.GetRect(PlaceholderHeight);
            Widgets.Label(poseLabelRect, "PhotoMode.Pawns.Pose".Translate());

            bool hasSelection = selected.Count > 0;
            Pawn primary = hasSelection ? selected[selected.Count - 1] : null;

            PhotoPawnPose currentPose = PhotoPawnPose.Standing;
            if (primary != null && manager.State.PawnOverrides.TryGetValue(primary, out PawnPhotoOverride photoOverride))
            {
                currentPose = photoOverride.Pose;
            }

            bool layingSupported = primary != null && PhotoPawnPoseSupport.IsSupported(primary, PhotoPawnPose.LayingFaceUp);

            Rect rowRect = listing.GetRect(PlaceholderHeight);
            Rect standingRect = rowRect.LeftHalf().ContractedBy(2f, 0f);
            Rect layingRect = rowRect.RightHalf().ContractedBy(2f, 0f);

            GUI.enabled = hasSelection;
            Widgets.DrawOptionBackground(standingRect, currentPose == PhotoPawnPose.Standing);
            if (Widgets.ButtonText(standingRect, "PhotoMode.Pawns.Pose.Standing".Translate()))
            {
                manager.SetSelectedPawnsPose(PhotoPawnPose.Standing);
            }

            GUI.enabled = hasSelection && layingSupported;
            Widgets.DrawOptionBackground(layingRect, currentPose == PhotoPawnPose.LayingFaceUp);
            if (Widgets.ButtonText(layingRect, "PhotoMode.Pawns.Pose.Laying".Translate()))
            {
                manager.SetSelectedPawnsPose(PhotoPawnPose.LayingFaceUp);
            }

            GUI.enabled = true;

            if (hasSelection && !layingSupported)
            {
                Rect hintRect = listing.GetRect(PlaceholderHeight);
                GUI.color = Color.gray;
                Widgets.Label(hintRect, "PhotoMode.Pawns.Pose.Unsupported".Translate());
                GUI.color = Color.white;
            }
        }

        private void DrawPositionControls(Listing_Standard listing, List<Pawn> selected)
        {
            Rect positionLabelRect = listing.GetRect(PlaceholderHeight);
            Widgets.Label(positionLabelRect, "PhotoMode.Pawns.Position".Translate());

            bool hasSelection = selected.Count > 0;
            GUI.enabled = hasSelection;

            Pawn primary = hasSelection ? selected[selected.Count - 1] : null;
            Vector3 currentOffset = Vector3.zero;
            if (primary != null && manager.State.PawnOverrides.TryGetValue(primary, out PawnPhotoOverride photoOverride))
            {
                currentOffset = photoOverride.Offset;
            }

            float max = PhotoModeManager.MaxPawnOffsetPerAxis;

            DrawGradeControl(listing, "PhotoMode.Pawns.OffsetX", -max, max, 0f, () => currentOffset.x,
                v => manager.SetSelectedPawnsOffset(new Vector3(v, 0f, currentOffset.z)));
            DrawGradeControl(listing, "PhotoMode.Pawns.OffsetZ", -max, max, 0f, () => currentOffset.z,
                v => manager.SetSelectedPawnsOffset(new Vector3(currentOffset.x, 0f, v)));

            listing.Gap(4f);

            Rect resetPositionRect = listing.GetRect(PlaceholderHeight);
            if (Widgets.ButtonText(resetPositionRect, "PhotoMode.Pawns.ResetPosition".Translate()))
            {
                manager.ResetSelectedPawnPositions();
            }

            GUI.enabled = true;
        }
    }
}
