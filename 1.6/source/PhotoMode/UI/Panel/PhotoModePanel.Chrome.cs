using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public partial class PhotoModePanel
    {
        private void DrawShowDevToolbarToggle(Rect rect)
        {
            bool showDevToolbar = manager.State.ShowDevToolbar;
            Widgets.CheckboxLabeled(rect, "PhotoMode.ShowDevToolbar".Translate(), ref showDevToolbar);
            manager.State.ShowDevToolbar = showDevToolbar;
        }

        private void DrawPreviewBypassButton(Rect rect)
        {
            bool held = Mouse.IsOver(rect) && Input.GetMouseButton(0);
            manager.State.PreviewBypassMouseHeld = held;

            Widgets.DrawOptionBackground(rect, held);
            Widgets.DrawHighlightIfMouseover(rect);

            string keyLabel = PhotoModeKeyBindingDefOf.PhotoMode_HoldPreviewBypass.MainKeyLabel;
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, "PhotoMode.HoldPreviewBypass".Translate(keyLabel));
            Text.Anchor = anchor;
        }

        private void DrawHeader(Rect rect)
        {
            Rect titleRect = rect.LeftPart(0.6f);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "PhotoMode.PanelTitle".Translate());
            Text.Font = GameFont.Small;

            Rect hideButtonRect = rect.RightPart(0.4f);
            if (Widgets.ButtonText(hideButtonRect, "PhotoMode.HidePanel".Translate()))
            {
                manager.HidePanel();
            }
        }
    }
}
