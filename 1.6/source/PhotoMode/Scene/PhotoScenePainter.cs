using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public static class PhotoScenePainter
    {
        private const float MinDragSpacing = 0.75f;

        private static Vector3 lastPaintPos;

        public static void BeginStroke(PhotoModeManager manager, Map map, Vector3 worldPos)
        {
            ApplyToolAt(manager, map, worldPos);
            lastPaintPos = worldPos;
        }

        public static void ContinueStroke(PhotoModeManager manager, Map map, Vector3 worldPos)
        {
            if ((worldPos - lastPaintPos).sqrMagnitude < MinDragSpacing * MinDragSpacing)
            {
                return;
            }

            ApplyToolAt(manager, map, worldPos);
            lastPaintPos = worldPos;
        }

        public static void EndStroke()
        {
        }

        public static void ResetInputState()
        {
            lastPaintPos = Vector3.zero;
        }

        private static void ApplyToolAt(PhotoModeManager manager, Map map, Vector3 worldPos)
        {
            PhotoSceneState scene = manager.State.SceneDressing;
            if (scene.Tool == PhotoSceneTool.Erase)
            {
                EraseAt(manager, worldPos);
            }
            else
            {
                PaintAt(manager, map, worldPos);
            }
        }

        public static void PaintAt(PhotoModeManager manager, Map map, Vector3 worldPos)
        {
            PhotoSceneState scene = manager.State.SceneDressing;

            IntVec3 cell = worldPos.ToIntVec3();
            if (!cell.InBounds(map))
            {
                return;
            }

            scene.CapWarningKey = null;

            int remainingCap = PhotoSceneState.MaxElements - scene.Elements.Count;
            if (remainingCap <= 0)
            {
                scene.CapWarningKey = "PhotoMode.SceneDressing.CapReached";
                return;
            }

            int requestedCount = Mathf.Clamp(Mathf.RoundToInt(scene.Density), 1, 8);
            List<PhotoSceneElement> added = new List<PhotoSceneElement>();
            int pawnCloneCountSoFar = scene.PawnCloneCount;

            Rand.PushState();
            try
            {
                for (int i = 0; i < requestedCount; i++)
                {
                    if (added.Count >= remainingCap)
                    {
                        scene.CapWarningKey = "PhotoMode.SceneDressing.CapReached";
                        break;
                    }

                    Vector3 offset = i == 0 ? Vector3.zero : RandomOffsetInBrush(scene.BrushRadius);
                    Vector3 pos = worldPos + offset;

                    PhotoSceneElement element = CreateElement(manager, scene, pos, pawnCloneCountSoFar);
                    if (element == null)
                    {
                        if (scene.Mode == PhotoSceneMode.Pawns)
                        {
                            scene.CapWarningKey = scene.CapWarningKey ?? "PhotoMode.SceneDressing.NoSource";
                        }
                        break;
                    }

                    if (element is PhotoPawnElement)
                    {
                        pawnCloneCountSoFar++;
                    }

                    added.Add(element);
                }
            }
            finally
            {
                Rand.PopState();
            }

            if (added.Count == 0)
            {
                return;
            }

            scene.Elements.AddRange(added);
            scene.PushUndo(added, null);
        }

        private static PhotoSceneElement CreateElement(PhotoModeManager manager, PhotoSceneState scene, Vector3 pos, int pawnCloneCountSoFar)
        {
            int id = scene.AllocateId();

            switch (scene.Mode)
            {
                case PhotoSceneMode.Pawns:
                {
                    if (pawnCloneCountSoFar >= PhotoSceneState.MaxPawnClones)
                    {
                        scene.CapWarningKey = "PhotoMode.SceneDressing.PawnCapReached";
                        return null;
                    }

                    Pawn source = ResolveCloneSource(manager);
                    if (source == null)
                    {
                        return null;
                    }

                    Rot4 facing = scene.RandomFacing ? RandomRot4() : scene.FixedFacing;
                    return new PhotoPawnElement
                    {
                        Id = id,
                        Position = pos,
                        Source = source,
                        Facing = facing,
                        Pose = PhotoPawnPoseSupport.IsSupported(source, scene.PawnPose) ? scene.PawnPose : PhotoPawnPose.Standing
                    };
                }
                case PhotoSceneMode.Props:
                {
                    if (scene.SelectedPropDef == null)
                    {
                        return null;
                    }

                    return new PhotoPropElement
                    {
                        Id = id,
                        Position = pos,
                        Def = scene.SelectedPropDef,
                        RotationDegrees = scene.PropRandomRotation ? Rand.Range(0f, 360f) : scene.PropFixedRotationDegrees,
                        VariantSeed = id * 7919,
                        Scale = scene.PropScale
                    };
                }
                default:
                {
                    if (scene.SelectedAtmosphere == PhotoAtmosphereKind.Fire)
                    {
                        return new PhotoFireElement
                        {
                            Id = id,
                            Position = pos,
                            Scale = scene.FireScale,
                            Seed = id * 7919
                        };
                    }

                    return new PhotoDecalElement
                    {
                        Id = id,
                        Position = pos,
                        Kind = ToDecalKind(scene.SelectedAtmosphere),
                        Opacity = scene.DecalOpacity,
                        Scale = scene.DecalScale,
                        Seed = id * 7919
                    };
                }
            }
        }

        private static PhotoDecalKind ToDecalKind(PhotoAtmosphereKind kind)
        {
            switch (kind)
            {
                case PhotoAtmosphereKind.Dirt:
                    return PhotoDecalKind.Dirt;
                case PhotoAtmosphereKind.Ash:
                    return PhotoDecalKind.Ash;
                case PhotoAtmosphereKind.Rubble:
                    return PhotoDecalKind.Rubble;
                default:
                    return PhotoDecalKind.Blood;
            }
        }

        private static Pawn ResolveCloneSource(PhotoModeManager manager)
        {
            List<Pawn> selected = manager.State.SelectedPawns;
            if (selected.Count == 0)
            {
                return null;
            }

            Pawn candidate = selected[selected.Count - 1];
            if (candidate == null || candidate.Destroyed || !candidate.Spawned || candidate.Map != manager.State.TargetMap)
            {
                return null;
            }

            return candidate;
        }

        private static Vector3 RandomOffsetInBrush(float radius)
        {
            if (radius <= 0f)
            {
                return Vector3.zero;
            }

            Vector2 point = Random.insideUnitCircle * radius;
            return new Vector3(point.x, 0f, point.y);
        }

        private static Rot4 RandomRot4()
        {
            return new Rot4(Rand.RangeInclusive(0, 3));
        }

        public static void EraseAt(PhotoModeManager manager, Vector3 worldPos)
        {
            PhotoSceneState scene = manager.State.SceneDressing;
            float radius = Mathf.Max(scene.BrushRadius, 0.5f);
            float radiusSq = radius * radius;

            List<PhotoSceneElement> removed = null;
            for (int i = scene.Elements.Count - 1; i >= 0; i--)
            {
                PhotoSceneElement element = scene.Elements[i];
                Vector3 delta = element.Position - worldPos;
                delta.y = 0f;
                if (delta.sqrMagnitude > radiusSq)
                {
                    continue;
                }

                if (removed == null)
                {
                    removed = new List<PhotoSceneElement>();
                }

                removed.Add(element);
                scene.Elements.RemoveAt(i);
            }

            if (removed != null)
            {
                scene.PushUndo(null, removed);
                scene.ValidateSelection();
            }
        }
    }
}
