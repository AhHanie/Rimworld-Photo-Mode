using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    internal static class PhotoSceneRenderer
    {
        internal static bool IsRenderingSceneProxy;

        internal const float ClusterPositionVariance = 0.45f;
        internal const float ClusterSizeVariance = 0.2f;
        private const int ClusterScatterCount = 3;
        private const int FireTicksPerFrame = 15;

        private static readonly Dictionary<PhotoDecalKind, string> DecalDefNames = new Dictionary<PhotoDecalKind, string>
        {
            { PhotoDecalKind.Blood, "Filth_Blood" },
            { PhotoDecalKind.Dirt, "Filth_Dirt" },
            { PhotoDecalKind.Ash, "Filth_Ash" },
            { PhotoDecalKind.Rubble, "Filth_RubbleRock" }
        };

        private static readonly Dictionary<PhotoDecalKind, ThingDef> decalDefCache = new Dictionary<PhotoDecalKind, ThingDef>();
        private static ThingDef fireDefCache;
        private static readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        internal static void RenderAll(Map map)
        {
            PhotoModeManager manager = PhotoModeManager.Current;
            if (manager == null)
            {
                return;
            }

            PhotoSceneState scene = manager.State.SceneDressing;
            List<PhotoSceneElement> elements = scene.Elements;
            if (elements.Count == 0)
            {
                return;
            }

            CellRect viewRect = Find.CameraDriver.CurrentViewRect.ExpandedBy(4);

            for (int i = 0; i < elements.Count; i++)
            {
                PhotoSceneElement element = elements[i];
                IntVec3 cell = element.Position.ToIntVec3();

                if (!cell.InBounds(map) || !viewRect.Contains(cell) || cell.Fogged(map))
                {
                    continue;
                }

                if (element is PhotoPawnElement pawnElement)
                {
                    DrawPawnClone(map, pawnElement);
                }
                else if (element is PhotoPropElement propElement)
                {
                    DrawProp(propElement);
                }
                else if (element is PhotoDecalElement decalElement)
                {
                    DrawDecal(decalElement);
                }
                else if (element is PhotoFireElement fireElement)
                {
                    DrawFire(fireElement);
                }
            }
        }

        private static void DrawPawnClone(Map map, PhotoPawnElement element)
        {
            Pawn source = element.Source;
            if (source == null || source.Destroyed || !source.Spawned || source.Map != map)
            {
                return;
            }

            PawnRenderer renderer = source.Drawer.renderer;
            if (renderer == null)
            {
                return;
            }

            Vector3 drawLoc = element.Position;
            drawLoc.y = AltitudeLayer.Pawn.AltitudeFor();

            IsRenderingSceneProxy = true;
            try
            {
                renderer.DynamicDrawPhaseAt(DrawPhase.EnsureInitialized, drawLoc);

                if (element.Pose == PhotoPawnPose.LayingFaceUp && PhotoPawnPoseSupport.CanRenderLayingFaceUp(source))
                {
                    Vector3 layingLoc = drawLoc;
                    layingLoc.y = AltitudeLayer.LayingPawn.AltitudeFor();
                    PhotoPawnPoseRenderer.DrawLayingFaceUp(renderer, DrawPhase.ParallelPreDraw, layingLoc, element.Facing);
                    PhotoPawnPoseRenderer.DrawLayingFaceUp(renderer, DrawPhase.Draw, layingLoc, element.Facing);
                }
                else
                {
                    renderer.DynamicDrawPhaseAt(DrawPhase.ParallelPreDraw, drawLoc, element.Facing);
                    renderer.DynamicDrawPhaseAt(DrawPhase.Draw, drawLoc, element.Facing);
                }
            }
            finally
            {
                IsRenderingSceneProxy = false;
            }
        }

        private static void DrawProp(PhotoPropElement element)
        {
            ThingDef def = element.Def;
            if (def == null || def.graphicData == null)
            {
                return;
            }

            Graphic graphic = def.graphic;
            if (graphic == null || graphic == BaseContent.BadGraphic)
            {
                return;
            }

            if (graphic is Graphic_Random randomGraphic && randomGraphic.SubGraphicsCount > 0)
            {
                graphic = randomGraphic.SubGraphicAtIndex(element.VariantSeed);
            }

            Material material = graphic.MatAt(Rot4.North, null);
            if (material == null)
            {
                return;
            }

            float scale = Mathf.Clamp(element.Scale, PhotoSceneState.MinPropScale, PhotoSceneState.MaxPropScale);
            Vector2 size = graphic.drawSize * scale;
            Mesh mesh = MeshPool.GridPlane(size);

            Vector3 pos = element.Position;
            pos.y = def.Altitude;

            Quaternion rotation = Quaternion.Euler(0f, element.RotationDegrees, 0f);
            Graphics.DrawMesh(mesh, pos, rotation, material, 0);
        }

        private static void DrawDecal(PhotoDecalElement element)
        {
            ThingDef def = GetDecalDef(element.Kind);
            Graphic_Cluster cluster = def?.graphic as Graphic_Cluster;
            if (cluster == null)
            {
                return;
            }

            Graphic[] subGraphics = cluster.subGraphics;
            if (subGraphics == null || subGraphics.Length == 0)
            {
                return;
            }

            Vector2 baseSize = cluster.drawSize;
            float sizeFactorMin = 1f - ClusterSizeVariance;
            float sizeFactorMax = 1f + ClusterSizeVariance;
            float scale = Mathf.Clamp(element.Scale, PhotoSceneState.MinDecalScale, PhotoSceneState.MaxDecalScale);
            float opacity = Mathf.Clamp01(element.Opacity);

            Rand.PushState(element.Seed);
            try
            {
                for (int i = 0; i < ClusterScatterCount; i++)
                {
                    Graphic sub = subGraphics[Rand.Range(0, subGraphics.Length)];
                    Material material = sub.MatSingle;
                    if (material == null)
                    {
                        continue;
                    }

                    Vector3 jitter = new Vector3(Rand.Range(-ClusterPositionVariance, ClusterPositionVariance), 0f, Rand.Range(-ClusterPositionVariance, ClusterPositionVariance));
                    Vector2 size = new Vector2(
                        Rand.Range(baseSize.x * sizeFactorMin, baseSize.x * sizeFactorMax),
                        Rand.Range(baseSize.y * sizeFactorMin, baseSize.y * sizeFactorMax)) * scale;
                    float rotationDegrees = Rand.Range(0f, 360f);

                    Vector3 pos = element.Position + jitter;
                    pos.y = AltitudeLayer.Filth.AltitudeFor();

                    Mesh mesh = MeshPool.GridPlane(size);
                    Quaternion rotation = Quaternion.Euler(0f, rotationDegrees, 0f);

                    if (opacity < 1f)
                    {
                        Color color = material.GetColor(ShaderPropertyIDs.Color);
                        color.a *= opacity;
                        propertyBlock.Clear();
                        propertyBlock.SetColor(ShaderPropertyIDs.Color, color);
                        Graphics.DrawMesh(mesh, pos, rotation, material, 0, null, 0, propertyBlock);
                    }
                    else
                    {
                        Graphics.DrawMesh(mesh, pos, rotation, material, 0);
                    }
                }
            }
            finally
            {
                Rand.PopState();
            }
        }

        private static void DrawFire(PhotoFireElement element)
        {
            ThingDef fireDef = GetFireDef();
            Graphic_Flicker flicker = fireDef?.graphic as Graphic_Flicker;
            if (flicker == null)
            {
                return;
            }

            Graphic[] subGraphics = flicker.subGraphics;
            if (subGraphics == null || subGraphics.Length == 0)
            {
                return;
            }

            int tickPhase = Find.TickManager.TicksGame + element.Seed;
            int frameStep = tickPhase / FireTicksPerFrame;
            int frameIndex = Mathf.Abs(frameStep ^ (element.Seed * 391)) % subGraphics.Length;

            Material material = subGraphics[frameIndex].MatSingle;
            if (material == null)
            {
                return;
            }

            float scale = ComputeFireVisualScale(element.Scale);

            Vector3 pos = element.Position;
            pos.y = AltitudeLayer.PawnState.AltitudeFor();

            Matrix4x4 matrix = default;
            matrix.SetTRS(pos, Quaternion.identity, new Vector3(scale, 1f, scale));

            Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
        }

        internal static float ComputeFireVisualScale(float userScale)
        {
            float clamped = Mathf.Clamp(userScale, PhotoSceneState.MinFireScale, PhotoSceneState.MaxFireScale);
            float vanillaDefaultFireSizeFactor = Mathf.Min(1f / 1.2f, 1.2f);
            return vanillaDefaultFireSizeFactor * clamped;
        }

        internal static ThingDef GetDecalDef(PhotoDecalKind kind)
        {
            if (decalDefCache.TryGetValue(kind, out ThingDef cached))
            {
                return cached;
            }

            string defName = DecalDefNames[kind];
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            decalDefCache[kind] = def;
            return def;
        }

        private static ThingDef GetFireDef()
        {
            if (fireDefCache != null)
            {
                return fireDefCache;
            }

            fireDefCache = DefDatabase<ThingDef>.GetNamedSilentFail("Fire");
            return fireDefCache;
        }
    }
}
