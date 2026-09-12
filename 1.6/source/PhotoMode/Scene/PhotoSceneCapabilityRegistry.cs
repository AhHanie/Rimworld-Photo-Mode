using System.Collections.Generic;
using Verse;

namespace Photo_Mode
{
    public enum PhotoScenePropSupport
    {
        Supported,
        NoGraphic,
        UnsupportedCategory,
        UnsupportedGraphicClass
    }

    public static class PhotoSceneCapabilityRegistry
    {
        private static List<ThingDef> cachedSupportedProps;

        public static PhotoScenePropSupport Classify(ThingDef def)
        {
            if (def == null || def.graphicData == null)
            {
                return PhotoScenePropSupport.NoGraphic;
            }

            if (def.category != ThingCategory.Item && def.category != ThingCategory.Building && def.category != ThingCategory.Plant)
            {
                return PhotoScenePropSupport.UnsupportedCategory;
            }

            if (def.IsBlueprint || def.IsFrame)
            {
                return PhotoScenePropSupport.UnsupportedCategory;
            }

            Graphic graphic = def.graphic;
            if (graphic == null || graphic == BaseContent.BadGraphic)
            {
                return PhotoScenePropSupport.NoGraphic;
            }

            if (graphic is Graphic_Single || graphic is Graphic_Random)
            {
                return PhotoScenePropSupport.Supported;
            }

            return PhotoScenePropSupport.UnsupportedGraphicClass;
        }

        public static bool IsSupported(ThingDef def)
        {
            return Classify(def) == PhotoScenePropSupport.Supported;
        }

        public static List<ThingDef> GetSupportedProps()
        {
            if (cachedSupportedProps != null)
            {
                return cachedSupportedProps;
            }

            List<ThingDef> result = new List<ThingDef>();
            List<ThingDef> allDefs = DefDatabase<ThingDef>.AllDefsListForReading;
            for (int i = 0; i < allDefs.Count; i++)
            {
                ThingDef def = allDefs[i];
                if (IsSupported(def))
                {
                    result.Add(def);
                }
            }

            result.Sort((a, b) => string.Compare(a.LabelCap.ToString(), b.LabelCap.ToString(), System.StringComparison.OrdinalIgnoreCase));
            cachedSupportedProps = result;
            return cachedSupportedProps;
        }
    }
}
