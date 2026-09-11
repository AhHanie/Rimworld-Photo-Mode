using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public partial class PhotoModePanel
    {
        private void DrawEffectsSection(Listing_Standard listing, ref bool collapsed)
        {
            collapsed = DrawSectionHeader(listing, collapsed, "PhotoMode.Section.Effects");

            if (!collapsed)
            {
                PhotoImageSettings image = manager.State.Image;

                bool vignetteEnabled = image.VignetteEnabled;
                listing.CheckboxLabeled("PhotoMode.Effects.VignetteEnabled".Translate(), ref vignetteEnabled);
                image.VignetteEnabled = vignetteEnabled;

                listing.Gap(4f);

                DrawGradeControl(listing, "PhotoMode.Effects.VignetteIntensity", 0f, 1f, 0f, () => image.VignetteIntensity, v => image.VignetteIntensity = v);
                DrawGradeControl(listing, "PhotoMode.Effects.VignetteRadius", 0f, 2f, 1f, () => image.VignetteRadius, v => image.VignetteRadius = v);
                DrawGradeControl(listing, "PhotoMode.Effects.VignetteSoftness", 0f, 1.5f, 0.5f, () => image.VignetteSoftness, v => image.VignetteSoftness = v);

                listing.Gap(4f);

                Rect resetRect = listing.GetRect(PlaceholderHeight);
                if (Widgets.ButtonText(resetRect, "PhotoMode.Effects.ResetVignette".Translate()))
                {
                    image.VignetteEnabled = false;
                    image.VignetteIntensity = 0f;
                    image.VignetteRadius = 1f;
                    image.VignetteSoftness = 0.5f;
                }

                listing.Gap(8f);

                bool sharpenEnabled = image.SharpenEnabled;
                listing.CheckboxLabeled("PhotoMode.Effects.SharpenEnabled".Translate(), ref sharpenEnabled);
                image.SharpenEnabled = sharpenEnabled;

                listing.Gap(4f);

                DrawGradeControl(listing, "PhotoMode.Effects.SharpenStrength", 0f, 1f, 0f, () => image.SharpenStrength, v => image.SharpenStrength = v);

                listing.Gap(4f);

                Rect resetSharpenRect = listing.GetRect(PlaceholderHeight);
                if (Widgets.ButtonText(resetSharpenRect, "PhotoMode.Effects.ResetSharpen".Translate()))
                {
                    image.SharpenEnabled = false;
                    image.SharpenStrength = 0f;
                }

                listing.Gap(8f);

                bool bloomEnabled = image.BloomEnabled;
                listing.CheckboxLabeled("PhotoMode.Effects.BloomEnabled".Translate(), ref bloomEnabled);
                image.BloomEnabled = bloomEnabled;

                listing.Gap(4f);

                DrawGradeControl(listing, "PhotoMode.Effects.BloomIntensity", 0f, 10f, 1f, () => image.BloomIntensity, v => image.BloomIntensity = v);
                DrawGradeControl(listing, "PhotoMode.Effects.BloomThreshold", 0f, 2f, 0.75f, () => image.BloomThreshold, v => image.BloomThreshold = v);
                DrawGradeControl(listing, "PhotoMode.Effects.BloomSpread", 0f, 1f, 0.5f, () => image.BloomSpread, v => image.BloomSpread = v);

                listing.Gap(4f);

                Rect resetBloomRect = listing.GetRect(PlaceholderHeight);
                if (Widgets.ButtonText(resetBloomRect, "PhotoMode.Effects.ResetBloom".Translate()))
                {
                    image.BloomEnabled = false;
                    image.BloomIntensity = 1f;
                    image.BloomThreshold = 0.75f;
                    image.BloomSpread = 0.5f;
                }

                listing.Gap(8f);

                DrawLutControls(listing, image);

                listing.Gap(8f);

                bool grainEnabled = image.GrainEnabled;
                listing.CheckboxLabeled("PhotoMode.Effects.GrainEnabled".Translate(), ref grainEnabled);
                image.GrainEnabled = grainEnabled;

                listing.Gap(4f);

                DrawGradeControl(listing, "PhotoMode.Effects.GrainIntensity", 0f, 1f, 0.3f, () => image.GrainIntensity, v => image.GrainIntensity = v);
                DrawGradeControl(listing, "PhotoMode.Effects.GrainSize", 0.5f, 4f, 1.5f, () => image.GrainSize, v => image.GrainSize = v);

                listing.Gap(4f);

                Rect resetGrainRect = listing.GetRect(PlaceholderHeight);
                if (Widgets.ButtonText(resetGrainRect, "PhotoMode.Effects.ResetGrain".Translate()))
                {
                    image.GrainEnabled = false;
                    image.GrainIntensity = 0.3f;
                    image.GrainSize = 1.5f;
                }

                listing.Gap(8f);

                bool chromaticAberrationEnabled = image.ChromaticAberrationEnabled;
                listing.CheckboxLabeled("PhotoMode.Effects.ChromaticAberrationEnabled".Translate(), ref chromaticAberrationEnabled);
                image.ChromaticAberrationEnabled = chromaticAberrationEnabled;

                listing.Gap(4f);

                DrawGradeControl(listing, "PhotoMode.Effects.ChromaticAberrationIntensity", 0f, 1f, 0.3f, () => image.ChromaticAberrationIntensity, v => image.ChromaticAberrationIntensity = v);

                listing.Gap(4f);

                Rect resetChromaticAberrationRect = listing.GetRect(PlaceholderHeight);
                if (Widgets.ButtonText(resetChromaticAberrationRect, "PhotoMode.Effects.ResetChromaticAberration".Translate()))
                {
                    image.ChromaticAberrationEnabled = false;
                    image.ChromaticAberrationIntensity = 0.3f;
                }

                listing.Gap(8f);

                bool tiltShiftEnabled = image.TiltShiftEnabled;
                listing.CheckboxLabeled("PhotoMode.Effects.TiltShiftEnabled".Translate(), ref tiltShiftEnabled);
                image.TiltShiftEnabled = tiltShiftEnabled;

                listing.Gap(4f);

                DrawGradeControl(listing, "PhotoMode.Effects.TiltShiftFocusPosition", 0f, 1f, 0.5f, () => image.TiltShiftFocusPosition, v => image.TiltShiftFocusPosition = v);
                DrawGradeControl(listing, "PhotoMode.Effects.TiltShiftFocusWidth", 0f, 1f, 0.25f, () => image.TiltShiftFocusWidth, v => image.TiltShiftFocusWidth = v);
                DrawGradeControl(listing, "PhotoMode.Effects.TiltShiftStrength", 0f, 1f, 0.5f, () => image.TiltShiftStrength, v => image.TiltShiftStrength = v);
                DrawGradeControl(listing, "PhotoMode.Effects.TiltShiftFalloff", 0.01f, 1f, 0.25f, () => image.TiltShiftFalloff, v => image.TiltShiftFalloff = v);

                listing.Gap(4f);

                Rect resetTiltShiftRect = listing.GetRect(PlaceholderHeight);
                if (Widgets.ButtonText(resetTiltShiftRect, "PhotoMode.Effects.ResetTiltShift".Translate()))
                {
                    image.TiltShiftEnabled = false;
                    image.TiltShiftFocusPosition = 0.5f;
                    image.TiltShiftFocusWidth = 0.25f;
                    image.TiltShiftStrength = 0.5f;
                    image.TiltShiftFalloff = 0.25f;
                }

                listing.Gap(8f);

                bool edgeBlurEnabled = image.EdgeBlurEnabled;
                listing.CheckboxLabeled("PhotoMode.Effects.EdgeBlurEnabled".Translate(), ref edgeBlurEnabled);
                image.EdgeBlurEnabled = edgeBlurEnabled;

                listing.Gap(4f);

                DrawGradeControl(listing, "PhotoMode.Effects.EdgeBlurStrength", 0f, 1f, 0.4f, () => image.EdgeBlurStrength, v => image.EdgeBlurStrength = v);

                listing.Gap(4f);

                Rect resetEdgeBlurRect = listing.GetRect(PlaceholderHeight);
                if (Widgets.ButtonText(resetEdgeBlurRect, "PhotoMode.Effects.ResetEdgeBlur".Translate()))
                {
                    image.EdgeBlurEnabled = false;
                    image.EdgeBlurStrength = 0.4f;
                }
            }

            listing.Gap(4f);
        }

        private void DrawLutControls(Listing_Standard listing, PhotoImageSettings image)
        {
            string currentLutLabel;
            if (string.IsNullOrEmpty(image.LutId))
            {
                currentLutLabel = "PhotoMode.Effects.LutNone".Translate();
            }
            else if (PhotoLutLibrary.TryFind(image.LutId, out PhotoLutOption option))
            {
                currentLutLabel = option.IsCustom ? "PhotoMode.Effects.LutCustomLabel".Translate(option.DisplayName).ToString() : option.DisplayName;
            }
            else
            {
                currentLutLabel = image.LutId;
            }

            Rect lutLabelRect = listing.GetRect(PlaceholderHeight);
            Widgets.Label(lutLabelRect, "PhotoMode.Effects.Lut".Translate(currentLutLabel));

            Rect lutDropdownRect = listing.GetRect(PlaceholderHeight);
            if (Widgets.ButtonText(lutDropdownRect, currentLutLabel))
            {
                OpenLutFloatMenu(image);
            }

            listing.Gap(4f);

            if (!string.IsNullOrEmpty(image.LutId))
            {
                DrawGradeControl(listing, "PhotoMode.Effects.LutIntensity", 0f, 1f, 0.05f, () => image.LutIntensity, v => image.LutIntensity = v);
            }

            if (image.LutStatus == PhotoLutStatus.Missing || image.LutStatus == PhotoLutStatus.Invalid)
            {
                Rect lutStatusRect = listing.GetRect(PlaceholderHeight);
                GUI.color = Color.red;
                Widgets.Label(lutStatusRect, image.LutStatusMessage);
                GUI.color = Color.white;

                listing.Gap(4f);
            }

            Rect openLutFolderRect = listing.GetRect(PlaceholderHeight);
            if (Widgets.ButtonText(openLutFolderRect, "PhotoMode.Effects.OpenLutFolder".Translate()))
            {
                PhotoLutLibrary.OpenCustomFolder();
            }

            listing.Gap(4f);

            Rect resetLutRect = listing.GetRect(PlaceholderHeight);
            if (Widgets.ButtonText(resetLutRect, "PhotoMode.Effects.ResetLut".Translate()))
            {
                image.LutId = string.Empty;
                image.LutIntensity = 0.05f;
            }
        }

        private static void OpenLutFloatMenu(PhotoImageSettings image)
        {
            List<PhotoLutOption> lutOptions = PhotoLutLibrary.ListAll();
            List<FloatMenuOption> menuOptions = new List<FloatMenuOption>(lutOptions.Count + 1)
            {
                new FloatMenuOption("PhotoMode.Effects.LutNone".Translate(), () => image.LutId = string.Empty)
            };

            for (int i = 0; i < lutOptions.Count; i++)
            {
                PhotoLutOption option = lutOptions[i];
                string label = option.IsCustom ? "PhotoMode.Effects.LutCustomLabel".Translate(option.DisplayName).ToString() : option.DisplayName;
                menuOptions.Add(new FloatMenuOption(label, () => image.LutId = option.Id));
            }

            Find.WindowStack.Add(new FloatMenu(menuOptions));
        }
    }
}
