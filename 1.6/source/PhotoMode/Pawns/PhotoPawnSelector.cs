using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public static class PhotoPawnSelector
    {
        public static void HandleMapClicks(Map map, List<Pawn> selected)
        {
            if (Event.current.type != EventType.MouseDown || Event.current.button != 0)
            {
                return;
            }

            if (Find.WindowStack.GetWindowAt(UI.MousePositionOnUIInverted) != null)
            {
                return;
            }

            Event.current.Use();

            Pawn clicked = PawnUnderMouse(map);
            bool additive = RimWorld.Selector.ShiftIsHeld;

            if (clicked == null)
            {
                if (!additive)
                {
                    selected.Clear();
                }
                return;
            }

            if (additive)
            {
                if (!selected.Remove(clicked))
                {
                    selected.Add(clicked);
                }
                return;
            }

            selected.Clear();
            selected.Add(clicked);
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

        public static bool IsPawnUnderMouse(Map map)
        {
            return PawnUnderMouse(map) != null;
        }

        private static Pawn PawnUnderMouse(Map map)
        {
            TargetingParameters targetingParameters = new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = false,
                canTargetItems = false,
                canTargetPlants = false,
                canTargetAnimals = true,
                canTargetHumans = true,
                canTargetMechs = true,
                canTargetSubhumans = true,
                canTargetEntities = true,
                mustBeSelectable = false,
                mapObjectTargetsMustBeAutoAttackable = false
            };

            List<Thing> things = GenUI.ThingsUnderMouse(UI.MouseMapPosition(), 1f, targetingParameters);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is Pawn pawn && pawn.Map == map && pawn.Spawned)
                {
                    return pawn;
                }
            }

            return null;
        }
    }
}
