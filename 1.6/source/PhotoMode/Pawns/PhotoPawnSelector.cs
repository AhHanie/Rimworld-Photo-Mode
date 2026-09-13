using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public static class PhotoPawnSelector
    {
        private const float HitRadius = 0.5f;

        public static Pawn TryFindPawnAtPhotoDrawPosition(Map map, Dictionary<Pawn, PawnPhotoOverride> overrides, Vector3 worldPos)
        {
            if (map == null)
            {
                return null;
            }

            float radiusSq = HitRadius * HitRadius;

            Pawn best = null;
            float bestDistSq = float.MaxValue;

            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (!IsValid(pawn) || pawn.Map != map)
                {
                    continue;
                }

                Vector3 drawPos = pawn.DrawPos;
                if (overrides.TryGetValue(pawn, out PawnPhotoOverride photoOverride))
                {
                    drawPos += photoOverride.Offset;
                }

                Vector3 delta = drawPos - worldPos;
                delta.y = 0f;
                float distSq = delta.sqrMagnitude;
                if (distSq <= radiusSq && distSq < bestDistSq)
                {
                    best = pawn;
                    bestDistSq = distSq;
                }
            }

            return best;
        }

        public static void SelectNext(Map map, List<Pawn> selected)
        {
            Cycle(map, selected, 1);
        }

        public static void SelectPrevious(Map map, List<Pawn> selected)
        {
            Cycle(map, selected, -1);
        }

        public static void ValidateSelection(List<Pawn> selected)
        {
            for (int i = selected.Count - 1; i >= 0; i--)
            {
                if (!IsValid(selected[i]))
                {
                    selected.RemoveAt(i);
                }
            }
        }

        public static void ValidateOverrides(Dictionary<Pawn, PawnPhotoOverride> overrides)
        {
            List<Pawn> stale = null;
            foreach (Pawn pawn in overrides.Keys)
            {
                if (IsValid(pawn))
                {
                    continue;
                }

                if (stale == null)
                {
                    stale = new List<Pawn>();
                }

                stale.Add(pawn);
            }

            if (stale == null)
            {
                return;
            }

            for (int i = 0; i < stale.Count; i++)
            {
                overrides.Remove(stale[i]);
            }
        }

        private static bool IsValid(Pawn pawn)
        {
            return pawn != null && !pawn.Destroyed && pawn.Spawned;
        }

        private static void Cycle(Map map, List<Pawn> selected, int direction)
        {
            if (map == null)
            {
                return;
            }

            List<Pawn> ordered = OrderedSpawnedPawns(map);
            if (ordered.Count == 0)
            {
                selected.Clear();
                return;
            }

            Pawn primary = selected.Count > 0 ? selected[selected.Count - 1] : null;
            int index = primary != null ? ordered.IndexOf(primary) : -1;
            int nextIndex = index < 0 ? 0 : (index + direction + ordered.Count) % ordered.Count;

            selected.Clear();
            selected.Add(ordered[nextIndex]);
        }

        private static List<Pawn> OrderedSpawnedPawns(Map map)
        {
            List<Pawn> pawns = new List<Pawn>(map.mapPawns.AllPawnsSpawned);
            pawns.Sort((a, b) => a.thingIDNumber.CompareTo(b.thingIDNumber));
            return pawns;
        }
    }
}
