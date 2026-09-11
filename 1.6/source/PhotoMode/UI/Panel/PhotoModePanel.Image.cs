using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public partial class PhotoModePanel
    {
        private void DrawImageSection(Listing_Standard listing, ref bool collapsed)
        {
            collapsed = DrawSectionHeader(listing, collapsed, "PhotoMode.Section.Image");

            if (!collapsed)
            {
                PhotoImageSettings image = manager.State.Image;

                DrawFilterButtons(listing, image);

                listing.Gap(8f);

                DrawGradeControl(listing, "PhotoMode.Image.Exposure", -3f, 3f, 0f, () => image.Exposure, v => image.Exposure = v);
                DrawGradeControl(listing, "PhotoMode.Image.Brightness", -1f, 1f, 0f, () => image.Brightness, v => image.Brightness = v);
                DrawGradeControl(listing, "PhotoMode.Image.Contrast", -1f, 1f, 0f, () => image.Contrast, v => image.Contrast = v);
                DrawGradeControl(listing, "PhotoMode.Image.Gamma", 0.25f, 3f, 1f, () => image.Gamma, v => image.Gamma = v, "PhotoMode.Image.Gamma.Tooltip".Translate());
                DrawGradeControl(listing, "PhotoMode.Image.Saturation", -1f, 1f, 0f, () => image.Saturation, v => image.Saturation = v);
                DrawGradeControl(listing, "PhotoMode.Image.Temperature", -1f, 1f, 0f, () => image.Temperature, v => image.Temperature = v);
                DrawGradeControl(listing, "PhotoMode.Image.Tint", -1f, 1f, 0f, () => image.Tint, v => image.Tint = v);

                listing.Gap(4f);

                Rect resetAllRect = listing.GetRect(PlaceholderHeight);
                if (Widgets.ButtonText(resetAllRect, "PhotoMode.Image.ResetAll".Translate()))
                {
                    image.Reset();
                }
            }

            listing.Gap(4f);
        }

        private static readonly PhotoColorFilter[] ColorFilters =
        {
            PhotoColorFilter.Vanilla,
            PhotoColorFilter.Warm,
            PhotoColorFilter.Cold,
            PhotoColorFilter.Cinematic,
            PhotoColorFilter.HighContrast,
            PhotoColorFilter.Desaturated,
            PhotoColorFilter.BlackAndWhite,
            PhotoColorFilter.Sepia
        };

        private static string FilterLabelKey(PhotoColorFilter filter)
        {
            return "PhotoMode.Image.Filter." + filter;
        }

        private static void DrawFilterButtons(Listing_Standard listing, PhotoImageSettings image)
        {
            Rect filtersLabelRect = listing.GetRect(PlaceholderHeight);
            Widgets.Label(filtersLabelRect, "PhotoMode.Image.Filters".Translate());

            const int columns = 2;
            int rows = Mathf.CeilToInt(ColorFilters.Length / (float)columns);

            for (int row = 0; row < rows; row++)
            {
                Rect rowRect = listing.GetRect(PlaceholderHeight);
                float columnWidth = rowRect.width / columns;

                for (int col = 0; col < columns; col++)
                {
                    int index = row * columns + col;
                    if (index >= ColorFilters.Length)
                    {
                        continue;
                    }

                    PhotoColorFilter filter = ColorFilters[index];
                    Rect buttonRect = new Rect(rowRect.x + columnWidth * col, rowRect.y, columnWidth, rowRect.height).ContractedBy(2f, 0f);
                    if (Widgets.ButtonText(buttonRect, FilterLabelKey(filter).Translate()))
                    {
                        PhotoColorFilters.Apply(image, filter);
                    }
                }

                listing.Gap(2f);
            }
        }
    }
}
