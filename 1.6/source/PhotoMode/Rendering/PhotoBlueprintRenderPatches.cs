using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    [HarmonyPatch(typeof(Blueprint), nameof(Blueprint.Graphic), MethodType.Getter)]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_Blueprint_Graphic
    {
        private static bool Prefix(Blueprint __instance, ref Graphic __result)
        {
            if (!PhotoBlueprintRenderService.IsBlueprintPreviewActive(__instance))
            {
                return true;
            }

            if (!PhotoBlueprintRenderService.TryResolveFinalGraphic(__instance, out Graphic graphic))
            {
                return true;
            }

            __result = graphic;
            return false;
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.Print))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_Thing_Print_TerrainBlueprintPreview
    {
        private static bool Prefix(Thing __instance, SectionLayer layer)
        {
            if (!(__instance is Blueprint blueprint) || !PhotoBlueprintRenderService.IsBlueprintPreviewActive(blueprint))
            {
                return true;
            }

            if (!(blueprint.EntityToBuild() is TerrainDef terrainDef))
            {
                return true;
            }

            return !PhotoBlueprintRenderService.TryPrintTerrainPreview(blueprint, terrainDef, layer);
        }
    }

    internal static class PhotoBlueprintRenderService
    {
        private class ProxyCacheEntry
        {
            public ThingDef TargetDef;
            public ThingDef Stuff;
            public ThingStyleDef Style;
            public Graphic Graphic;
        }

        private static readonly Dictionary<Blueprint, ProxyCacheEntry> ProxyCache = new Dictionary<Blueprint, ProxyCacheEntry>();

        public static bool IsBlueprintPreviewActive(Blueprint blueprint)
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return false;
            }

            if (blueprint == null || !blueprint.Spawned)
            {
                return false;
            }

            PhotoModeManager manager = PhotoModeManager.Current;
            if (!manager.State.Overlay.ShowBlueprintsAsConstructed)
            {
                return false;
            }

            return blueprint.Map == manager.State.TargetMap;
        }

        public static bool TryResolveFinalGraphic(Blueprint blueprint, out Graphic graphic)
        {
            if (blueprint is Blueprint_Install installBlueprint)
            {
                Thing thingToInstall = installBlueprint.ThingToInstall;
                if (thingToInstall != null)
                {
                    graphic = thingToInstall.Graphic;
                    return graphic != null;
                }

                graphic = null;
                return false;
            }

            if (!(blueprint.EntityToBuild() is ThingDef targetDef) || targetDef.graphicData == null)
            {
                graphic = null;
                return false;
            }

            graphic = ResolveThingDefProxyGraphic(blueprint, targetDef);
            return graphic != null;
        }

        private static Graphic ResolveThingDefProxyGraphic(Blueprint blueprint, ThingDef targetDef)
        {
            ThingDef stuff = blueprint.EntityToBuildStuff();
            ThingStyleDef style = blueprint.EntityToBuildStyle();

            if (ProxyCache.TryGetValue(blueprint, out ProxyCacheEntry entry)
                && entry.TargetDef == targetDef
                && entry.Stuff == stuff
                && entry.Style == style)
            {
                return entry.Graphic;
            }

            Thing proxy = ThingMaker.MakeThing(targetDef, stuff);
            if (style != null && targetDef.CanBeStyled())
            {
                proxy.SetStyleDef(style);
            }

            Graphic graphic = proxy.Graphic;

            ProxyCache[blueprint] = new ProxyCacheEntry
            {
                TargetDef = targetDef,
                Stuff = stuff,
                Style = style,
                Graphic = graphic
            };

            return graphic;
        }

        public static bool TryPrintTerrainPreview(Blueprint blueprint, TerrainDef terrainDef, SectionLayer layer)
        {
            Graphic terrainGraphic = terrainDef.graphic;
            if (terrainGraphic == null || terrainGraphic == BaseContent.BadGraphic)
            {
                return false;
            }

            Material material = terrainGraphic.MatSingle;
            if (material == null)
            {
                return false;
            }

            Vector3 center = blueprint.Position.ToVector3Shifted();
            center.y = AltitudeLayer.Terrain.AltitudeFor();

            Printer_Plane.PrintPlane(layer, center, terrainGraphic.drawSize, material);
            return true;
        }

        public static void InvalidateBlueprintMeshes(Map map)
        {
            if (map == null || map.Disposed)
            {
                return;
            }

            List<Thing> blueprints = map.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint);
            for (int i = 0; i < blueprints.Count; i++)
            {
                Thing thing = blueprints[i];
                if (thing.Spawned)
                {
                    thing.DirtyMapMesh(map);
                }
            }
        }

        public static void ClearProxyCache()
        {
            ProxyCache.Clear();
        }
    }
}
