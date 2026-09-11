using RimWorld;
using Verse;

namespace Photo_Mode
{
    [DefOf]
    public static class PhotoModeKeyBindingDefOf
    {
        public static KeyBindingDef PhotoMode_ToggleActive;
        public static KeyBindingDef PhotoMode_HoldPreviewBypass;
        public static KeyBindingDef PhotoMode_Capture;
        public static KeyBindingDef PhotoMode_TogglePanel;

        static PhotoModeKeyBindingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PhotoModeKeyBindingDefOf));
        }
    }
}
