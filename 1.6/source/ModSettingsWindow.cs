using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public static class ModSettingsWindow
    {
        private static Vector2 scrollPosition;
        private static float contentHeight = 300f;

        public static void Draw(Rect parent)
        {
            bool needsScrollbar = contentHeight > parent.height;
            float viewWidth = parent.width - (needsScrollbar ? 16f : 0f);
            Rect viewRect = new Rect(0f, 0f, viewWidth, contentHeight);

            Widgets.BeginScrollView(parent, ref scrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(new Rect(0f, 0f, viewWidth, 999999f));

            listing.Label("PhotoMode.PanelSide".Translate());

            Rect sideRow = listing.GetRect(30f);
            Rect leftRect = sideRow.LeftHalf().ContractedBy(2f);
            Rect rightRect = sideRow.RightHalf().ContractedBy(2f);

            if (Widgets.RadioButtonLabeled(leftRect, "PhotoMode.PanelSide.Left".Translate(), ModSettings.PanelSide == PhotoModePanelSide.Left))
            {
                ModSettings.PanelSide = PhotoModePanelSide.Left;
            }

            if (Widgets.RadioButtonLabeled(rightRect, "PhotoMode.PanelSide.Right".Translate(), ModSettings.PanelSide == PhotoModePanelSide.Right))
            {
                ModSettings.PanelSide = PhotoModePanelSide.Right;
            }

            listing.Gap(12f);

            bool extendedZoomEnabled = ModSettings.ExtendedZoomEnabled;
            listing.CheckboxLabeled("PhotoMode.ExtendedZoomEnabled".Translate(), ref extendedZoomEnabled, "PhotoMode.ExtendedZoomEnabled.Tooltip".Translate());
            ModSettings.ExtendedZoomEnabled = extendedZoomEnabled;

            listing.Gap(12f);

            bool notifyOnCapture = ModSettings.NotifyOnCapture;
            listing.CheckboxLabeled("PhotoMode.NotifyOnCapture".Translate(), ref notifyOnCapture, "PhotoMode.NotifyOnCapture.Tooltip".Translate());
            ModSettings.NotifyOnCapture = notifyOnCapture;

            listing.Gap(12f);

            bool hideHandleWhenPanelHidden = ModSettings.HideHandleWhenPanelHidden;
            listing.CheckboxLabeled("PhotoMode.HideHandleWhenPanelHidden".Translate(), ref hideHandleWhenPanelHidden, "PhotoMode.HideHandleWhenPanelHidden.Tooltip".Translate());
            ModSettings.HideHandleWhenPanelHidden = hideHandleWhenPanelHidden;

            listing.Gap(12f);

            bool showMainButton = ModSettings.ShowMainButton;
            listing.CheckboxLabeled("PhotoMode.ShowMainButton".Translate(), ref showMainButton, "PhotoMode.ShowMainButton.Tooltip".Translate());
            ModSettings.ShowMainButton = showMainButton;

            listing.Gap(12f);

            DrawPatchLifetimeSetting(listing);

            listing.Gap(12f);

            DrawControlsSection(listing);

            contentHeight = listing.CurHeight;
            listing.End();

            Widgets.EndScrollView();
        }

        private static void DrawControlsSection(Listing_Standard listing)
        {
            listing.Label("PhotoMode.Section.Controls".Translate());

            DrawControlRow(listing, PhotoModeKeyBindingDefOf.PhotoMode_ToggleActive);
            DrawControlRow(listing, PhotoModeKeyBindingDefOf.PhotoMode_TogglePanel);
            DrawControlRow(listing, PhotoModeKeyBindingDefOf.PhotoMode_Capture);
            DrawControlRow(listing, PhotoModeKeyBindingDefOf.PhotoMode_HoldPreviewBypass);

            GUI.color = Color.gray;
            listing.Label("PhotoMode.Controls.Hint".Translate("KeyboardConfig".Translate()));
            GUI.color = Color.white;
        }

        private static void DrawControlRow(Listing_Standard listing, KeyBindingDef keyDef)
        {
            listing.Label(keyDef.LabelCap + ": " + FormatBoundKeys(keyDef));
        }

        private static string FormatBoundKeys(KeyBindingDef keyDef)
        {
            KeyPrefsData keyPrefsData = KeyPrefs.KeyPrefsData;
            KeyCode keyCodeA = keyPrefsData.GetBoundKeyCode(keyDef, KeyPrefs.BindingSlot.A);
            KeyCode keyCodeB = keyPrefsData.GetBoundKeyCode(keyDef, KeyPrefs.BindingSlot.B);

            if (keyCodeA == KeyCode.None && keyCodeB == KeyCode.None)
            {
                return "PhotoMode.Controls.NotBound".Translate();
            }

            if (keyCodeA != KeyCode.None && keyCodeB != KeyCode.None)
            {
                return keyCodeA.ToStringReadable() + " / " + keyCodeB.ToStringReadable();
            }

            return (keyCodeA != KeyCode.None ? keyCodeA : keyCodeB).ToStringReadable();
        }

        private static void DrawPatchLifetimeSetting(Listing_Standard listing)
        {
            listing.Label("PhotoMode.PatchLifetime".Translate());

            bool locked = PhotoModePatchController.IsSettingLocked();
            PhotoModePatchLifetime displayed = PhotoModePatchController.DisplayedLifetime;

            Rect dynamicRect = listing.GetRect(30f);
            if (Widgets.RadioButtonLabeled(dynamicRect, "PhotoMode.PatchLifetime.Dynamic".Translate(), displayed == PhotoModePatchLifetime.Dynamic, locked) && !locked)
            {
                PhotoModePatchController.RequestLifetimeChange(PhotoModePatchLifetime.Dynamic);
            }
            TooltipHandler.TipRegion(dynamicRect, "PhotoMode.PatchLifetime.Dynamic.Tooltip".Translate());

            Rect alwaysLoadedRect = listing.GetRect(30f);
            if (Widgets.RadioButtonLabeled(alwaysLoadedRect, "PhotoMode.PatchLifetime.AlwaysLoaded".Translate(), displayed == PhotoModePatchLifetime.AlwaysLoaded, locked) && !locked)
            {
                PhotoModePatchController.RequestLifetimeChange(PhotoModePatchLifetime.AlwaysLoaded);
            }
            TooltipHandler.TipRegion(alwaysLoadedRect, "PhotoMode.PatchLifetime.AlwaysLoaded.Tooltip".Translate());

            if (locked)
            {
                listing.Gap(2f);
                GUI.color = Color.gray;
                listing.Label("PhotoMode.PatchLifetime.Locked".Translate());
                GUI.color = Color.white;
            }
        }
    }
}
