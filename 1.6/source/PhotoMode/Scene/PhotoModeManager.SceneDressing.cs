using System.Collections.Generic;
using Verse;

namespace Photo_Mode
{
    public partial class PhotoModeManager
    {
        public void SetSceneDressingToolActive(bool active)
        {
            if (!State.Active)
            {
                return;
            }

            MapInteraction.ResetInputState();
            State.SceneDressingToolActive = active;
        }

        public void SetSceneMode(PhotoSceneMode mode)
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.Mode = mode;
            State.SceneDressing.CapWarningKey = null;
        }

        public void SetSceneTool(PhotoSceneTool tool)
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.Tool = tool;
        }

        public void SetScenePropDef(ThingDef def)
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.SelectedPropDef = def;
        }

        public void SetSceneAtmosphereKind(PhotoAtmosphereKind kind)
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.SelectedAtmosphere = kind;
        }

        public void SetSceneBrushRadius(float radius)
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.BrushRadius = UnityEngine.Mathf.Clamp(radius, PhotoSceneState.MinBrushRadius, PhotoSceneState.MaxBrushRadius);
        }

        public void SetSceneDensity(float density)
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.Density = UnityEngine.Mathf.Clamp(density, PhotoSceneState.MinDensity, PhotoSceneState.MaxDensity);
        }

        public void SetSceneRandomFacing(bool randomFacing)
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.RandomFacing = randomFacing;
        }

        public void SetSceneFixedFacing(Rot4 facing)
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.FixedFacing = facing;
        }

        public void SetScenePawnPose(PhotoPawnPose pose)
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.PawnPose = pose;
        }

        public void SetScenePropScale(float scale)
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.PropScale = UnityEngine.Mathf.Clamp(scale, PhotoSceneState.MinPropScale, PhotoSceneState.MaxPropScale);
        }

        public void SetScenePropRandomRotation(bool randomRotation)
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.PropRandomRotation = randomRotation;
        }

        public void SetScenePropFixedRotation(float degrees)
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.PropFixedRotationDegrees = UnityEngine.Mathf.Repeat(degrees, 360f);
        }

        public void SetSceneDecalOpacity(float opacity)
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.DecalOpacity = UnityEngine.Mathf.Clamp01(opacity);
        }

        public void SetSceneDecalScale(float scale)
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.DecalScale = UnityEngine.Mathf.Clamp(scale, PhotoSceneState.MinDecalScale, PhotoSceneState.MaxDecalScale);
        }

        public void SetSceneFireScale(float scale)
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.FireScale = UnityEngine.Mathf.Clamp(scale, PhotoSceneState.MinFireScale, PhotoSceneState.MaxFireScale);
        }

        public void SelectSceneElement(int elementId)
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.SelectedElementId = elementId;
        }

        public void ClearSelectedSceneElement()
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.SelectedElementId = PhotoSceneState.InvalidElementId;
        }

        public void SetSceneElementPosition(int elementId, UnityEngine.Vector3 position)
        {
            if (!State.Active)
            {
                return;
            }

            PhotoSceneElement element = State.SceneDressing.GetElementById(elementId);
            if (element == null)
            {
                return;
            }

            Map map = State.TargetMap;
            if (map != null)
            {
                position.x = UnityEngine.Mathf.Clamp(position.x, 0f, map.Size.x - 0.01f);
                position.z = UnityEngine.Mathf.Clamp(position.z, 0f, map.Size.z - 0.01f);
            }

            position.y = 0f;
            element.Position = position;
        }

        public void CommitSceneElementMove(int elementId, UnityEngine.Vector3 previousPosition)
        {
            if (!State.Active)
            {
                return;
            }

            PhotoSceneElement element = State.SceneDressing.GetElementById(elementId);
            if (element == null)
            {
                return;
            }

            State.SceneDressing.PushMoveUndo(element, previousPosition);
        }

        public void UndoSceneEdit()
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.Undo();
        }

        public void ClearScene()
        {
            if (!State.Active)
            {
                return;
            }

            State.SceneDressing.ClearAll();
        }

        public void ValidateSceneState()
        {
            if (!State.Active)
            {
                return;
            }

            List<PhotoSceneElement> elements = State.SceneDressing.Elements;
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                if (elements[i] is PhotoPawnElement pawnElement)
                {
                    Pawn source = pawnElement.Source;
                    if (source == null || source.Destroyed || !source.Spawned || source.Map != State.TargetMap)
                    {
                        elements.RemoveAt(i);
                    }
                }
                else if (elements[i] is PhotoPropElement propElement)
                {
                    if (propElement.Def == null)
                    {
                        elements.RemoveAt(i);
                    }
                }
            }

            State.SceneDressing.ValidateSelection();
        }
    }
}
