using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public class PhotoMapInteractionController
    {
        private const float MinHitRadius = 0.35f;
        private const float PawnCloneHitRadius = 0.5f;

        private enum DragKind
        {
            None,
            Pawn,
            SceneElement,
            BrushStroke
        }

        private DragKind dragKind = DragKind.None;
        private Pawn draggedPawn;
        private int draggedElementId = PhotoSceneState.InvalidElementId;
        private Vector3 dragAnchor;
        private Vector3 dragStartValue;
        private bool dragMoved;

        public void HandleInput(PhotoModeManager manager)
        {
            Map map = manager.State.TargetMap;
            if (map == null)
            {
                return;
            }

            if (Event.current.rawType == EventType.MouseUp && Event.current.button == 0)
            {
                EndActiveGesture(manager);
            }

            if (dragKind == DragKind.Pawn || dragKind == DragKind.SceneElement)
            {
                ContinueObjectDrag(manager);
                return;
            }

            bool windowBlocksInput = Find.WindowStack.GetWindowAt(UI.MousePositionOnUIInverted) != null;

            if (dragKind == DragKind.BrushStroke)
            {
                if (!windowBlocksInput)
                {
                    ContinueBrushStroke(manager, map);
                }
                return;
            }

            if (windowBlocksInput)
            {
                return;
            }

            if (Event.current.type != EventType.MouseDown || Event.current.button != 0)
            {
                return;
            }

            Event.current.Use();
            BeginGesture(manager, map);
        }

        public void ResetInputState()
        {
            dragKind = DragKind.None;
            draggedPawn = null;
            draggedElementId = PhotoSceneState.InvalidElementId;
            dragMoved = false;
            PhotoScenePainter.ResetInputState();
        }

        private void BeginGesture(PhotoModeManager manager, Map map)
        {
            Vector3 mouseWorld = UI.MouseMapPosition();

            Pawn hitPawn = PhotoPawnSelector.TryFindPawnAtPhotoDrawPosition(map, manager.State.PawnOverrides, mouseWorld);
            if (hitPawn != null)
            {
                manager.ClearSelectedSceneElement();

                if (RimWorld.Selector.ShiftIsHeld)
                {
                    List<Pawn> selected = manager.State.SelectedPawns;
                    if (!selected.Remove(hitPawn))
                    {
                        selected.Add(hitPawn);
                    }
                    return;
                }

                List<Pawn> soleSelection = manager.State.SelectedPawns;
                if (soleSelection.Count != 1 || soleSelection[0] != hitPawn)
                {
                    soleSelection.Clear();
                    soleSelection.Add(hitPawn);
                }

                dragKind = DragKind.Pawn;
                draggedPawn = hitPawn;
                dragAnchor = mouseWorld;
                dragStartValue = manager.State.PawnOverrides.TryGetValue(hitPawn, out PawnPhotoOverride photoOverride) ? photoOverride.Offset : Vector3.zero;
                dragMoved = false;
                return;
            }

            if (manager.State.SceneDressingToolActive)
            {
                PhotoScenePainter.BeginStroke(manager, map, mouseWorld);
                dragKind = DragKind.BrushStroke;
                return;
            }

            PhotoSceneElement hitElement = TryFindSceneElementAt(manager.State.SceneDressing, map, mouseWorld);
            if (hitElement != null)
            {
                manager.SelectSceneElement(hitElement.Id);

                dragKind = DragKind.SceneElement;
                draggedElementId = hitElement.Id;
                dragAnchor = mouseWorld;
                dragStartValue = hitElement.Position;
                dragMoved = false;
                return;
            }

            if (!RimWorld.Selector.ShiftIsHeld)
            {
                manager.State.SelectedPawns.Clear();
            }

            manager.ClearSelectedSceneElement();
        }

        private void ContinueObjectDrag(PhotoModeManager manager)
        {
            EventType type = Event.current.type;
            if (type == EventType.MouseDrag || type == EventType.MouseDown)
            {
                Event.current.Use();
            }

            Vector3 mouseWorld = UI.MouseMapPosition();
            Vector3 delta = mouseWorld - dragAnchor;
            delta.y = 0f;

            if (delta.sqrMagnitude > 0f)
            {
                dragMoved = true;
            }

            if (dragKind == DragKind.Pawn)
            {
                manager.SetPawnOffset(draggedPawn, dragStartValue + delta);
            }
            else if (dragKind == DragKind.SceneElement)
            {
                manager.SetSceneElementPosition(draggedElementId, dragStartValue + delta);
            }
        }

        private void ContinueBrushStroke(PhotoModeManager manager, Map map)
        {
            if (Event.current.type != EventType.MouseDrag || Event.current.button != 0)
            {
                return;
            }

            Event.current.Use();
            PhotoScenePainter.ContinueStroke(manager, map, UI.MouseMapPosition());
        }

        private void EndActiveGesture(PhotoModeManager manager)
        {
            switch (dragKind)
            {
                case DragKind.Pawn:
                    dragKind = DragKind.None;
                    draggedPawn = null;
                    break;
                case DragKind.SceneElement:
                    if (dragMoved)
                    {
                        manager.CommitSceneElementMove(draggedElementId, dragStartValue);
                    }

                    dragKind = DragKind.None;
                    draggedElementId = PhotoSceneState.InvalidElementId;
                    break;
                case DragKind.BrushStroke:
                    PhotoScenePainter.EndStroke();
                    dragKind = DragKind.None;
                    break;
            }
        }

        private static PhotoSceneElement TryFindSceneElementAt(PhotoSceneState scene, Map map, Vector3 worldPos)
        {
            List<PhotoSceneElement> elements = scene.Elements;
            CellRect viewRect = Find.CameraDriver.CurrentViewRect.ExpandedBy(4);

            for (int i = elements.Count - 1; i >= 0; i--)
            {
                PhotoSceneElement element = elements[i];
                IntVec3 cell = element.Position.ToIntVec3();

                if (!cell.InBounds(map) || !viewRect.Contains(cell) || cell.Fogged(map))
                {
                    continue;
                }

                float radius = GetHitRadius(element);
                Vector3 delta = element.Position - worldPos;
                delta.y = 0f;
                if (delta.sqrMagnitude <= radius * radius)
                {
                    return element;
                }
            }

            return null;
        }

        private static float GetHitRadius(PhotoSceneElement element)
        {
            if (element is PhotoPawnElement)
            {
                return PawnCloneHitRadius;
            }

            if (element is PhotoPropElement propElement)
            {
                Graphic graphic = propElement.Def?.graphic;
                Vector2 drawSize = graphic != null ? graphic.drawSize : Vector2.one;
                float scale = Mathf.Clamp(propElement.Scale, PhotoSceneState.MinPropScale, PhotoSceneState.MaxPropScale);
                float radius = Mathf.Max(drawSize.x, drawSize.y) * scale * 0.5f;
                return Mathf.Max(radius, MinHitRadius);
            }

            if (element is PhotoDecalElement decalElement)
            {
                Graphic_Cluster cluster = PhotoSceneRenderer.GetDecalDef(decalElement.Kind)?.graphic as Graphic_Cluster;
                Vector2 baseSize = cluster != null ? cluster.drawSize : Vector2.one;
                float scale = Mathf.Clamp(decalElement.Scale, PhotoSceneState.MinDecalScale, PhotoSceneState.MaxDecalScale);
                float maxDim = Mathf.Max(baseSize.x, baseSize.y) * (1f + PhotoSceneRenderer.ClusterSizeVariance) * scale;
                float radius = maxDim * 0.5f + PhotoSceneRenderer.ClusterPositionVariance;
                return Mathf.Max(radius, MinHitRadius);
            }

            if (element is PhotoFireElement fireElement)
            {
                float scale = PhotoSceneRenderer.ComputeFireVisualScale(fireElement.Scale);
                return Mathf.Max(scale * 0.5f, MinHitRadius);
            }

            return MinHitRadius;
        }
    }
}
