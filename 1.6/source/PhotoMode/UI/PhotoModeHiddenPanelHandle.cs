using UnityEngine;
using Verse;

namespace Photo_Mode
{
    internal static class PhotoModeHiddenPanelHandle
    {
        private const float HandleWidth = 48f;
        private const float HandleHeight = 60f;

        public static void Draw(PhotoModeManager manager)
        {
            bool onRight = ModSettings.PanelSide == PhotoModePanelSide.Right;
            float x = onRight ? UI.screenWidth - HandleWidth : 0f;
            float y = (UI.screenHeight - HandleHeight) / 2f;
            Rect handleRect = new Rect(x, y, HandleWidth, HandleHeight);

            if (Widgets.ButtonText(handleRect, string.Empty))
            {
                manager.ShowPanel();
            }

            Text.Font = GameFont.Tiny;
            TextAnchor anchor = Text.Anchor;
            bool wordWrap = Text.WordWrap;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            Widgets.Label(handleRect, "PhotoMode.ShowPanel".Translate());
            Text.WordWrap = wordWrap;
            Text.Anchor = anchor;
            Text.Font = GameFont.Small;
        }
    }
}
