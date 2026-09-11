using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public partial class PhotoModePanel
    {
        private void DrawOverlaySection(Listing_Standard listing, ref bool collapsed)
        {
            collapsed = DrawSectionHeader(listing, collapsed, "PhotoMode.Section.Overlay");

            if (!collapsed)
            {
                PhotoOverlayOptions overlay = manager.State.Overlay;

                bool cleanBaseline = overlay.UseVanillaScreenshotBaseline;
                listing.CheckboxLabeled("PhotoMode.Overlay.CleanBaseline".Translate(), ref cleanBaseline);
                if (cleanBaseline != overlay.UseVanillaScreenshotBaseline)
                {
                    manager.SetVanillaScreenshotBaseline(cleanBaseline);
                }

                listing.Gap(4f);

                listing.CheckboxLabeled("PhotoMode.Overlay.HideNameplates".Translate(), ref overlay.HideNameplates);
                listing.CheckboxLabeled("PhotoMode.Overlay.HideColonistBar".Translate(), ref overlay.HideColonistBar);
                listing.CheckboxLabeled("PhotoMode.Overlay.HideSelectionBrackets".Translate(), ref overlay.HideSelectionBrackets);
                listing.CheckboxLabeled("PhotoMode.Overlay.HideDesignationOverlays".Translate(), ref overlay.HideDesignationOverlays);
                listing.CheckboxLabeled("PhotoMode.Overlay.HideTargetingIndicators".Translate(), ref overlay.HideTargetingIndicators);
                listing.CheckboxLabeled("PhotoMode.Overlay.HideInteractionBubbles".Translate(), ref overlay.HideInteractionBubbles);
                listing.CheckboxLabeled("PhotoMode.Overlay.HideMotes".Translate(), ref overlay.HideMotes);
                listing.CheckboxLabeled("PhotoMode.Overlay.HideForbiddenDesignator".Translate(), ref overlay.HideForbiddenDesignator);

                listing.Gap(4f);

                Rect cleanScreenshotRect = listing.GetRect(PlaceholderHeight);
                if (Widgets.ButtonText(cleanScreenshotRect, "PhotoMode.CleanScreenshot".Translate()))
                {
                    manager.EnableCleanScreenshot();
                }
            }

            listing.Gap(4f);
        }
    }
}
