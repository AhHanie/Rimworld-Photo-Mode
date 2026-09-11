using HarmonyLib;
using RimWorld;
using Verse;

namespace Photo_Mode
{
    [HarmonyPatch(typeof(ThingOverlays), nameof(ThingOverlays.ThingOverlaysOnGUI))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_ThingOverlays_ThingOverlaysOnGUI
    {
        private static bool Prefix()
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            return !PhotoModeManager.Current.State.Overlay.HideNameplates;
        }
    }

    [HarmonyPatch(typeof(ColonistBar), nameof(ColonistBar.ColonistBarOnGUI))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_ColonistBar_ColonistBarOnGUI
    {
        private static bool Prefix()
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            return !PhotoModeManager.Current.State.Overlay.HideColonistBar;
        }
    }

    internal static class PhotoModeOverlaySupplement
    {
        public static void DrawColonistBarIfSuppressedByBaseline()
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return;
            }

            if (PhotoModeManager.Current.State.Overlay.HideColonistBar)
            {
                return;
            }

            if (!Find.UIRoot.screenshotMode.FiltersCurrentEvent)
            {
                return;
            }

            Find.ColonistBar.ColonistBarOnGUI();
        }
    }

    [HarmonyPatch(typeof(SelectionDrawer), nameof(SelectionDrawer.DrawSelectionOverlays))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_SelectionDrawer_DrawSelectionOverlays
    {
        private static bool forcedScreenshotModeOff;

        private static bool Prefix()
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            if (PhotoModeManager.Current.State.Overlay.HideSelectionBrackets)
            {
                return false;
            }

            ScreenshotModeHandler screenshotMode = Find.UIRoot.screenshotMode;
            if (screenshotMode.Active)
            {
                screenshotMode.Active = false;
                forcedScreenshotModeOff = true;
            }

            return true;
        }

        private static void Postfix()
        {
            if (!forcedScreenshotModeOff)
            {
                return;
            }

            forcedScreenshotModeOff = false;
            Find.UIRoot.screenshotMode.Active = true;
        }
    }

    [HarmonyPatch(typeof(DesignatorManager), nameof(DesignatorManager.DesignationManagerOnGUI))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_DesignatorManager_DesignationManagerOnGUI
    {
        private static bool Prefix()
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            return !PhotoModeManager.Current.State.Overlay.HideDesignationOverlays;
        }
    }

    [HarmonyPatch(typeof(DesignationManager), nameof(DesignationManager.DrawDesignations))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_DesignationManager_DrawDesignations
    {
        private static bool Prefix()
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            return !PhotoModeManager.Current.State.Overlay.HideDesignationOverlays;
        }
    }

    [HarmonyPatch(typeof(Targeter), nameof(Targeter.TargeterOnGUI))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_Targeter_TargeterOnGUI
    {
        private static bool Prefix()
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            return !PhotoModeManager.Current.State.Overlay.HideTargetingIndicators;
        }
    }

    [HarmonyPatch(typeof(UIRoot), nameof(UIRoot.HideMotes), MethodType.Getter)]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_UIRoot_HideMotes
    {
        private static void Postfix(ref bool __result)
        {
            if (__result || !PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return;
            }

            if (PhotoModeManager.Current.State.Overlay.HideMotes)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(MoteText), nameof(MoteText.DrawGUIOverlay))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_MoteText_DrawGUIOverlay
    {
        private static bool Prefix()
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            return !PhotoModeManager.Current.State.Overlay.HideMotes;
        }
    }

    [HarmonyPatch(typeof(MoteBubble), "DrawAt")]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_MoteBubble_DrawAt
    {
        private static bool Prefix()
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            return !PhotoModeManager.Current.State.Overlay.HideInteractionBubbles;
        }
    }

    [HarmonyPatch(typeof(OverlayDrawer), "RenderForbiddenOverlay")]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_OverlayDrawer_RenderForbiddenOverlay
    {
        private static bool Prefix()
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            return !PhotoModeManager.Current.State.Overlay.HideForbiddenDesignator;
        }
    }

    [HarmonyPatch(typeof(OverlayDrawer), "RenderForbiddenBigOverlay")]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_OverlayDrawer_RenderForbiddenBigOverlay
    {
        private static bool Prefix()
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            return !PhotoModeManager.Current.State.Overlay.HideForbiddenDesignator;
        }
    }

    [HarmonyPatch(typeof(OverlayDrawer), "RenderForbiddenRefuelOverlay")]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_OverlayDrawer_RenderForbiddenRefuelOverlay
    {
        private static bool Prefix()
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            return !PhotoModeManager.Current.State.Overlay.HideForbiddenDesignator;
        }
    }

    [HarmonyPatch(typeof(OverlayDrawer), "RenderForbiddenAtomizerOverlay")]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_OverlayDrawer_RenderForbiddenAtomizerOverlay
    {
        private static bool Prefix()
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            return !PhotoModeManager.Current.State.Overlay.HideForbiddenDesignator;
        }
    }
}
