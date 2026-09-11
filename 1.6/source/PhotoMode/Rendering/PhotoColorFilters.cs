namespace Photo_Mode
{
    public enum PhotoColorFilter
    {
        Vanilla,
        Warm,
        Cold,
        Cinematic,
        HighContrast,
        Desaturated,
        BlackAndWhite,
        Sepia
    }

    public static class PhotoColorFilters
    {
        public static void Apply(PhotoImageSettings image, PhotoColorFilter filter)
        {
            switch (filter)
            {
                case PhotoColorFilter.Vanilla:
                    image.Exposure = 0f;
                    image.Brightness = 0f;
                    image.Contrast = 0f;
                    image.Gamma = 1f;
                    image.Saturation = 0f;
                    image.Temperature = 0f;
                    image.Tint = 0f;
                    image.VignetteEnabled = false;
                    image.VignetteIntensity = 0f;
                    image.VignetteRadius = 1f;
                    image.VignetteSoftness = 0.5f;
                    return;
                case PhotoColorFilter.Warm:
                    image.Contrast = 0.05f;
                    image.Saturation = 0.1f;
                    image.Temperature = 0.35f;
                    image.Tint = 0.05f;
                    image.VignetteEnabled = false;
                    return;
                case PhotoColorFilter.Cold:
                    image.Contrast = 0.05f;
                    image.Saturation = -0.05f;
                    image.Temperature = -0.35f;
                    image.Tint = -0.05f;
                    image.VignetteEnabled = false;
                    return;
                case PhotoColorFilter.Cinematic:
                    image.Contrast = 0.25f;
                    image.Saturation = -0.15f;
                    image.Temperature = 0.05f;
                    image.VignetteEnabled = true;
                    image.VignetteIntensity = 0.35f;
                    image.VignetteRadius = 0.85f;
                    image.VignetteSoftness = 0.6f;
                    return;
                case PhotoColorFilter.HighContrast:
                    image.Contrast = 0.5f;
                    image.Gamma = 0.9f;
                    image.Saturation = 0.1f;
                    image.VignetteEnabled = false;
                    return;
                case PhotoColorFilter.Desaturated:
                    image.Saturation = -0.6f;
                    image.Contrast = 0.05f;
                    image.VignetteEnabled = false;
                    return;
                case PhotoColorFilter.BlackAndWhite:
                    image.Saturation = -1f;
                    image.Contrast = 0.15f;
                    image.VignetteEnabled = false;
                    return;
                case PhotoColorFilter.Sepia:
                    image.Saturation = -0.6f;
                    image.Contrast = 0.05f;
                    image.Temperature = 0.45f;
                    image.Tint = 0.15f;
                    image.VignetteEnabled = true;
                    image.VignetteIntensity = 0.2f;
                    image.VignetteRadius = 0.9f;
                    image.VignetteSoftness = 0.7f;
                    return;
            }
        }
    }
}
