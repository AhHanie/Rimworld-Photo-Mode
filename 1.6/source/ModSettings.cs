using Verse;

namespace Photo_Mode
{
    public enum PhotoModePanelSide
    {
        Left,
        Right
    }

    public class ModSettings : Verse.ModSettings
    {
        private static ModSettings instance;

        public static PhotoModePanelSide PanelSide = PhotoModePanelSide.Right;
        public static bool ExtendedZoomEnabled = true;
        public static bool NotifyOnCapture = true;
        public static bool HideHandleWhenPanelHidden;
        public static bool ShowMainButton = true;

        public static bool CameraSectionCollapsed;
        public static bool OverlaySectionCollapsed;
        public static bool SceneSectionCollapsed;
        public static bool PawnsSectionCollapsed;
        public static bool PresetsSectionCollapsed;
        public static bool ImageSectionCollapsed;
        public static bool EffectsSectionCollapsed;
        public static bool CaptureSectionCollapsed;

        public static PhotoModePatchLifetime PatchLifetime = PhotoModePatchLifetime.Dynamic;

        public ModSettings()
        {
            instance = this;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref PanelSide, "panelSide", PhotoModePanelSide.Right);
            Scribe_Values.Look(ref ExtendedZoomEnabled, "extendedZoomEnabled", true);
            Scribe_Values.Look(ref NotifyOnCapture, "notifyOnCapture", true);
            Scribe_Values.Look(ref HideHandleWhenPanelHidden, "hideHandleWhenPanelHidden");
            Scribe_Values.Look(ref ShowMainButton, "showMainButton", true);
            Scribe_Values.Look(ref CameraSectionCollapsed, "cameraSectionCollapsed");
            Scribe_Values.Look(ref OverlaySectionCollapsed, "overlaySectionCollapsed");
            Scribe_Values.Look(ref SceneSectionCollapsed, "sceneSectionCollapsed");
            Scribe_Values.Look(ref PawnsSectionCollapsed, "pawnsSectionCollapsed");
            Scribe_Values.Look(ref PresetsSectionCollapsed, "presetsSectionCollapsed");
            Scribe_Values.Look(ref ImageSectionCollapsed, "imageSectionCollapsed");
            Scribe_Values.Look(ref EffectsSectionCollapsed, "effectsSectionCollapsed");
            Scribe_Values.Look(ref CaptureSectionCollapsed, "captureSectionCollapsed");
            Scribe_Values.Look(ref PatchLifetime, "patchLifetime", PhotoModePatchLifetime.Dynamic);
        }

        public static void Save()
        {
            instance?.Write();
        }
    }
}
