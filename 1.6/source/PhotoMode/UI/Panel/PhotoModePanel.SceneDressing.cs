using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public partial class PhotoModePanel
    {
        private void DrawSceneDressingSection(Listing_Standard listing, ref bool collapsed)
        {
            collapsed = DrawSectionHeader(listing, collapsed, "PhotoMode.Section.SceneDressing");

            if (!collapsed)
            {
                PhotoSceneState scene = manager.State.SceneDressing;

                GUI.color = Color.gray;
                listing.Label("PhotoMode.SceneDressing.Hint".Translate());
                GUI.color = Color.white;

                listing.Gap(4f);

                bool toolActive = manager.State.SceneDressingToolActive;
                bool newToolActive = toolActive;
                listing.CheckboxLabeled("PhotoMode.SceneDressing.BrushActive".Translate(), ref newToolActive);
                if (newToolActive != toolActive)
                {
                    manager.SetSceneDressingToolActive(newToolActive);
                }

                GUI.color = Color.gray;
                listing.Label("PhotoMode.SceneDressing.BrushActiveHint".Translate());
                GUI.color = Color.white;

                listing.Gap(4f);

                DrawModeTabs(listing, scene);

                listing.Gap(4f);

                DrawToolTabs(listing, scene);

                listing.Gap(4f);

                DrawGradeControl(listing, "PhotoMode.SceneDressing.BrushRadius", PhotoSceneState.MinBrushRadius, PhotoSceneState.MaxBrushRadius, 1f,
                    () => scene.BrushRadius, v => manager.SetSceneBrushRadius(v));

                if (scene.Tool == PhotoSceneTool.Paint)
                {
                    DrawGradeControl(listing, "PhotoMode.SceneDressing.Density", PhotoSceneState.MinDensity, PhotoSceneState.MaxDensity, 1f,
                        () => scene.Density, v => manager.SetSceneDensity(v));
                }

                listing.Gap(4f);

                switch (scene.Mode)
                {
                    case PhotoSceneMode.Pawns:
                        DrawPawnsMode(listing, scene);
                        break;
                    case PhotoSceneMode.Props:
                        DrawPropsMode(listing, scene);
                        break;
                    default:
                        DrawAtmosphereMode(listing, scene);
                        break;
                }

                listing.Gap(6f);

                if (!string.IsNullOrEmpty(scene.CapWarningKey))
                {
                    GUI.color = Color.yellow;
                    listing.Label(scene.CapWarningKey.Translate());
                    GUI.color = Color.white;
                }

                GUI.color = Color.gray;
                listing.Label("PhotoMode.SceneDressing.Count".Translate(scene.Elements.Count, PhotoSceneState.MaxElements));
                GUI.color = Color.white;

                PhotoSceneElement selectedElement = scene.GetElementById(scene.SelectedElementId);
                if (selectedElement != null)
                {
                    GUI.color = Color.gray;
                    listing.Label("PhotoMode.SceneDressing.Selected".Translate(DescribeElement(selectedElement)));
                    GUI.color = Color.white;
                }

                listing.Gap(2f);

                Rect actionsRect = listing.GetRect(PlaceholderHeight);
                Rect undoRect = actionsRect.LeftHalf().ContractedBy(2f, 0f);
                Rect clearRect = actionsRect.RightHalf().ContractedBy(2f, 0f);

                GUI.enabled = scene.CanUndo;
                if (Widgets.ButtonText(undoRect, "PhotoMode.SceneDressing.Undo".Translate()))
                {
                    manager.UndoSceneEdit();
                }

                GUI.enabled = scene.Elements.Count > 0;
                if (Widgets.ButtonText(clearRect, "PhotoMode.SceneDressing.ClearLayer".Translate()))
                {
                    manager.ClearScene();
                }

                GUI.enabled = true;

                listing.Gap(2f);

                GUI.color = Color.gray;
                listing.Label("PhotoMode.SceneDressing.NonDestructiveHint".Translate());
                GUI.color = Color.white;
            }

            listing.Gap(4f);
        }

        private static string DescribeElement(PhotoSceneElement element)
        {
            switch (element)
            {
                case PhotoPawnElement pawnElement:
                    return pawnElement.Source != null ? pawnElement.Source.LabelCap.ToString() : "PhotoMode.SceneDressing.Mode.Pawns".Translate().ToString();
                case PhotoPropElement propElement:
                    return propElement.Def != null ? propElement.Def.LabelCap.ToString() : "PhotoMode.SceneDressing.Mode.Props".Translate().ToString();
                case PhotoDecalElement decalElement:
                    return decalElement.Kind.ToString();
                case PhotoFireElement _:
                    return "PhotoMode.SceneDressing.Atmosphere.Fire".Translate().ToString();
                default:
                    return element.Id.ToString();
            }
        }

        private void DrawModeTabs(Listing_Standard listing, PhotoSceneState scene)
        {
            Rect rowRect = listing.GetRect(PlaceholderHeight);
            float columnWidth = rowRect.width / 3f;

            DrawTabButton(new Rect(rowRect.x, rowRect.y, columnWidth, rowRect.height), "PhotoMode.SceneDressing.Mode.Pawns".Translate(), scene.Mode == PhotoSceneMode.Pawns, () => manager.SetSceneMode(PhotoSceneMode.Pawns));
            DrawTabButton(new Rect(rowRect.x + columnWidth, rowRect.y, columnWidth, rowRect.height), "PhotoMode.SceneDressing.Mode.Props".Translate(), scene.Mode == PhotoSceneMode.Props, () => manager.SetSceneMode(PhotoSceneMode.Props));
            DrawTabButton(new Rect(rowRect.x + columnWidth * 2f, rowRect.y, columnWidth, rowRect.height), "PhotoMode.SceneDressing.Mode.Atmosphere".Translate(), scene.Mode == PhotoSceneMode.Atmosphere, () => manager.SetSceneMode(PhotoSceneMode.Atmosphere));
        }

        private void DrawToolTabs(Listing_Standard listing, PhotoSceneState scene)
        {
            Rect rowRect = listing.GetRect(PlaceholderHeight);
            Rect paintRect = rowRect.LeftHalf().ContractedBy(2f, 0f);
            Rect eraseRect = rowRect.RightHalf().ContractedBy(2f, 0f);

            DrawTabButton(paintRect, "PhotoMode.SceneDressing.Tool.Paint".Translate(), scene.Tool == PhotoSceneTool.Paint, () => manager.SetSceneTool(PhotoSceneTool.Paint));
            DrawTabButton(eraseRect, "PhotoMode.SceneDressing.Tool.Erase".Translate(), scene.Tool == PhotoSceneTool.Erase, () => manager.SetSceneTool(PhotoSceneTool.Erase));
        }

        private static void DrawTabButton(Rect rect, string label, bool active, System.Action onClick)
        {
            rect = rect.ContractedBy(2f, 0f);
            Widgets.DrawOptionBackground(rect, active);
            if (Widgets.ButtonText(rect, label))
            {
                onClick();
            }
        }

        private void DrawPawnsMode(Listing_Standard listing, PhotoSceneState scene)
        {
            List<Pawn> selected = manager.State.SelectedPawns;
            Pawn source = selected.Count > 0 ? selected[selected.Count - 1] : null;

            if (source == null)
            {
                GUI.color = Color.gray;
                listing.Label("PhotoMode.SceneDressing.NoSource".Translate());
                GUI.color = Color.white;
            }
            else
            {
                listing.Label("PhotoMode.SceneDressing.CloneSource".Translate(source.LabelCap));
            }

            listing.Gap(2f);

            bool randomFacing = scene.RandomFacing;
            listing.CheckboxLabeled("PhotoMode.SceneDressing.RandomFacing".Translate(), ref randomFacing);
            if (randomFacing != scene.RandomFacing)
            {
                manager.SetSceneRandomFacing(randomFacing);
            }

            if (!scene.RandomFacing)
            {
                Rect facingRect = listing.GetRect(PlaceholderHeight);
                float facingWidth = facingRect.width / 4f;
                DrawTabButton(new Rect(facingRect.x, facingRect.y, facingWidth, facingRect.height), "PhotoMode.Pawns.Facing.North".Translate(), scene.FixedFacing == Rot4.North, () => manager.SetSceneFixedFacing(Rot4.North));
                DrawTabButton(new Rect(facingRect.x + facingWidth, facingRect.y, facingWidth, facingRect.height), "PhotoMode.Pawns.Facing.East".Translate(), scene.FixedFacing == Rot4.East, () => manager.SetSceneFixedFacing(Rot4.East));
                DrawTabButton(new Rect(facingRect.x + facingWidth * 2f, facingRect.y, facingWidth, facingRect.height), "PhotoMode.Pawns.Facing.South".Translate(), scene.FixedFacing == Rot4.South, () => manager.SetSceneFixedFacing(Rot4.South));
                DrawTabButton(new Rect(facingRect.x + facingWidth * 3f, facingRect.y, facingWidth, facingRect.height), "PhotoMode.Pawns.Facing.West".Translate(), scene.FixedFacing == Rot4.West, () => manager.SetSceneFixedFacing(Rot4.West));
            }

            listing.Gap(4f);

            bool layingSupported = source != null && PhotoPawnPoseSupport.IsSupported(source, PhotoPawnPose.LayingFaceUp);
            Rect poseRect = listing.GetRect(PlaceholderHeight);
            Rect standingRect = poseRect.LeftHalf().ContractedBy(2f, 0f);
            Rect layingRect = poseRect.RightHalf().ContractedBy(2f, 0f);

            Widgets.DrawOptionBackground(standingRect, scene.PawnPose == PhotoPawnPose.Standing);
            if (Widgets.ButtonText(standingRect, "PhotoMode.Pawns.Pose.Standing".Translate()))
            {
                manager.SetScenePawnPose(PhotoPawnPose.Standing);
            }

            GUI.enabled = layingSupported;
            Widgets.DrawOptionBackground(layingRect, scene.PawnPose == PhotoPawnPose.LayingFaceUp);
            if (Widgets.ButtonText(layingRect, "PhotoMode.Pawns.Pose.Laying".Translate()))
            {
                manager.SetScenePawnPose(PhotoPawnPose.LayingFaceUp);
            }
            GUI.enabled = true;

            GUI.color = Color.gray;
            listing.Label("PhotoMode.SceneDressing.PawnCloneCount".Translate(scene.PawnCloneCount, PhotoSceneState.MaxPawnClones));
            GUI.color = Color.white;
        }

        private void DrawPropsMode(Listing_Standard listing, PhotoSceneState scene)
        {
            string currentLabel = scene.SelectedPropDef != null ? scene.SelectedPropDef.LabelCap.ToString() : "PhotoMode.SceneDressing.NoPropSelected".Translate().ToString();

            Rect pickerRect = listing.GetRect(PlaceholderHeight);
            if (Widgets.ButtonText(pickerRect, currentLabel))
            {
                OpenPropFloatMenu();
            }

            listing.Gap(4f);

            DrawGradeControl(listing, "PhotoMode.SceneDressing.PropScale", PhotoSceneState.MinPropScale, PhotoSceneState.MaxPropScale, 1f,
                () => scene.PropScale, v => manager.SetScenePropScale(v));

            bool randomRotation = scene.PropRandomRotation;
            listing.CheckboxLabeled("PhotoMode.SceneDressing.RandomRotation".Translate(), ref randomRotation);
            if (randomRotation != scene.PropRandomRotation)
            {
                manager.SetScenePropRandomRotation(randomRotation);
            }

            if (!scene.PropRandomRotation)
            {
                DrawGradeControl(listing, "PhotoMode.SceneDressing.PropRotation", 0f, 360f, 0f,
                    () => scene.PropFixedRotationDegrees, v => manager.SetScenePropFixedRotation(v));
            }
        }

        private void OpenPropFloatMenu()
        {
            List<ThingDef> supported = PhotoSceneCapabilityRegistry.GetSupportedProps();
            List<FloatMenuOption> options = new List<FloatMenuOption>(supported.Count);
            for (int i = 0; i < supported.Count; i++)
            {
                ThingDef def = supported[i];
                options.Add(new FloatMenuOption(def.LabelCap, () => manager.SetScenePropDef(def)));
            }

            if (options.Count == 0)
            {
                options.Add(new FloatMenuOption("PhotoMode.SceneDressing.NoPropSelected".Translate(), null));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void DrawAtmosphereMode(Listing_Standard listing, PhotoSceneState scene)
        {
            Rect rowRect = listing.GetRect(PlaceholderHeight);
            float columnWidth = rowRect.width / 5f;

            DrawTabButton(new Rect(rowRect.x, rowRect.y, columnWidth, rowRect.height), "PhotoMode.SceneDressing.Atmosphere.Blood".Translate(), scene.SelectedAtmosphere == PhotoAtmosphereKind.Blood, () => manager.SetSceneAtmosphereKind(PhotoAtmosphereKind.Blood));
            DrawTabButton(new Rect(rowRect.x + columnWidth, rowRect.y, columnWidth, rowRect.height), "PhotoMode.SceneDressing.Atmosphere.Dirt".Translate(), scene.SelectedAtmosphere == PhotoAtmosphereKind.Dirt, () => manager.SetSceneAtmosphereKind(PhotoAtmosphereKind.Dirt));
            DrawTabButton(new Rect(rowRect.x + columnWidth * 2f, rowRect.y, columnWidth, rowRect.height), "PhotoMode.SceneDressing.Atmosphere.Ash".Translate(), scene.SelectedAtmosphere == PhotoAtmosphereKind.Ash, () => manager.SetSceneAtmosphereKind(PhotoAtmosphereKind.Ash));
            DrawTabButton(new Rect(rowRect.x + columnWidth * 3f, rowRect.y, columnWidth, rowRect.height), "PhotoMode.SceneDressing.Atmosphere.Rubble".Translate(), scene.SelectedAtmosphere == PhotoAtmosphereKind.Rubble, () => manager.SetSceneAtmosphereKind(PhotoAtmosphereKind.Rubble));
            DrawTabButton(new Rect(rowRect.x + columnWidth * 4f, rowRect.y, columnWidth, rowRect.height), "PhotoMode.SceneDressing.Atmosphere.Fire".Translate(), scene.SelectedAtmosphere == PhotoAtmosphereKind.Fire, () => manager.SetSceneAtmosphereKind(PhotoAtmosphereKind.Fire));

            listing.Gap(4f);

            if (scene.SelectedAtmosphere == PhotoAtmosphereKind.Fire)
            {
                DrawGradeControl(listing, "PhotoMode.SceneDressing.FireScale", PhotoSceneState.MinFireScale, PhotoSceneState.MaxFireScale, 1f,
                    () => scene.FireScale, v => manager.SetSceneFireScale(v));
            }
            else
            {
                DrawGradeControl(listing, "PhotoMode.SceneDressing.DecalOpacity", 0f, 1f, 1f,
                    () => scene.DecalOpacity, v => manager.SetSceneDecalOpacity(v));
                DrawGradeControl(listing, "PhotoMode.SceneDressing.DecalScale", PhotoSceneState.MinDecalScale, PhotoSceneState.MaxDecalScale, 1f,
                    () => scene.DecalScale, v => manager.SetSceneDecalScale(v));
            }
        }
    }
}
