using System;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public partial class PhotoModePanel
    {
        private static bool DrawSectionHeader(Listing_Standard listing, bool collapsed, string labelKey)
        {
            Rect headerRect = listing.GetRect(SectionHeaderHeight);
            Widgets.DrawOptionBackground(headerRect, false);

            if (Widgets.ButtonInvisible(headerRect))
            {
                collapsed = !collapsed;
                ModSettings.Save();
            }

            string arrow = collapsed ? "> " : "v ";
            Widgets.Label(headerRect.ContractedBy(4f, 0f), arrow + labelKey.Translate());

            return collapsed;
        }

        private static void DrawGradeControl(Listing_Standard listing, string labelKey, float min, float max, float defaultValue, Func<float> getValue, Action<float> setValue, string tooltip = null)
        {
            float value = getValue();

            Rect labelRowRect = listing.GetRect(PlaceholderHeight);
            Rect labelRect = labelRowRect.LeftPart(0.72f);
            Rect resetRect = labelRowRect.RightPart(0.26f);

            Widgets.Label(labelRect, labelKey.Translate(value.ToString("0.00")));
            if (!string.IsNullOrEmpty(tooltip))
            {
                TooltipHandler.TipRegion(labelRect, tooltip);
            }

            if (Widgets.ButtonText(resetRect, "PhotoMode.Image.Reset".Translate()))
            {
                setValue(defaultValue);
                value = defaultValue;
            }

            Rect sliderRect = listing.GetRect(PlaceholderHeight);
            float newValue = Widgets.HorizontalSlider(sliderRect, value, min, max);
            if (!Mathf.Approximately(newValue, value))
            {
                setValue(newValue);
            }

            listing.Gap(2f);
        }
    }
}
