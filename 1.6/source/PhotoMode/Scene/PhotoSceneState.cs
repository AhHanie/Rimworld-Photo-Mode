using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public class PhotoSceneState
    {
        public const int MaxElements = 250;
        public const int MaxPawnClones = 64;
        public const int MaxUndoDepth = 30;
        public const int InvalidElementId = 0;

        public const float MinBrushRadius = 0f;
        public const float MaxBrushRadius = 6f;
        public const float MinDensity = 1f;
        public const float MaxDensity = 8f;
        public const float MinPropScale = 0.25f;
        public const float MaxPropScale = 3f;
        public const float MinDecalScale = 0.25f;
        public const float MaxDecalScale = 3f;
        public const float MinFireScale = 0.5f;
        public const float MaxFireScale = 2.5f;

        public readonly List<PhotoSceneElement> Elements = new List<PhotoSceneElement>();

        public PhotoSceneMode Mode = PhotoSceneMode.Pawns;
        public PhotoSceneTool Tool = PhotoSceneTool.Paint;

        public ThingDef SelectedPropDef;
        public PhotoAtmosphereKind SelectedAtmosphere = PhotoAtmosphereKind.Blood;

        public float BrushRadius = 1f;
        public float Density = 1f;
        public bool RandomFacing = true;
        public Rot4 FixedFacing = Rot4.South;
        public PhotoPawnPose PawnPose = PhotoPawnPose.Standing;

        public float PropScale = 1f;
        public bool PropRandomRotation = true;
        public float PropFixedRotationDegrees;
        public float DecalOpacity = 1f;
        public float DecalScale = 1f;
        public float FireScale = 1f;

        public string CapWarningKey;

        public int SelectedElementId = InvalidElementId;

        private int nextElementId = 1;

        private enum SceneEditKind
        {
            AddRemove,
            Move
        }

        private class SceneEditCommand
        {
            public SceneEditKind Kind;
            public List<PhotoSceneElement> Added;
            public List<PhotoSceneElement> Removed;
            public PhotoSceneElement MovedElement;
            public Vector3 PreviousPosition;
        }

        private readonly List<SceneEditCommand> undoStack = new List<SceneEditCommand>();

        public int AllocateId()
        {
            return nextElementId++;
        }

        public PhotoSceneElement GetElementById(int id)
        {
            if (id == InvalidElementId)
            {
                return null;
            }

            for (int i = 0; i < Elements.Count; i++)
            {
                if (Elements[i].Id == id)
                {
                    return Elements[i];
                }
            }

            return null;
        }

        public void ValidateSelection()
        {
            if (SelectedElementId != InvalidElementId && GetElementById(SelectedElementId) == null)
            {
                SelectedElementId = InvalidElementId;
            }
        }

        public int PawnCloneCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Elements.Count; i++)
                {
                    if (Elements[i] is PhotoPawnElement)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public bool CanUndo => undoStack.Count > 0;

        public void PushUndo(List<PhotoSceneElement> added, List<PhotoSceneElement> removed)
        {
            if ((added == null || added.Count == 0) && (removed == null || removed.Count == 0))
            {
                return;
            }

            undoStack.Add(new SceneEditCommand { Kind = SceneEditKind.AddRemove, Added = added, Removed = removed });
            if (undoStack.Count > MaxUndoDepth)
            {
                undoStack.RemoveAt(0);
            }
        }

        public void PushMoveUndo(PhotoSceneElement element, Vector3 previousPosition)
        {
            if (element == null || element.Position == previousPosition)
            {
                return;
            }

            undoStack.Add(new SceneEditCommand { Kind = SceneEditKind.Move, MovedElement = element, PreviousPosition = previousPosition });
            if (undoStack.Count > MaxUndoDepth)
            {
                undoStack.RemoveAt(0);
            }
        }

        public void Undo()
        {
            if (undoStack.Count == 0)
            {
                return;
            }

            SceneEditCommand command = undoStack[undoStack.Count - 1];
            undoStack.RemoveAt(undoStack.Count - 1);

            if (command.Kind == SceneEditKind.Move)
            {
                command.MovedElement.Position = command.PreviousPosition;
                ValidateSelection();
                return;
            }

            if (command.Added != null)
            {
                for (int i = 0; i < command.Added.Count; i++)
                {
                    Elements.Remove(command.Added[i]);
                }
            }

            if (command.Removed != null)
            {
                Elements.AddRange(command.Removed);
            }

            ValidateSelection();
        }

        public void ClearAll()
        {
            if (Elements.Count == 0)
            {
                return;
            }

            List<PhotoSceneElement> removed = new List<PhotoSceneElement>(Elements);
            Elements.Clear();
            SelectedElementId = InvalidElementId;
            PushUndo(null, removed);
        }

        public void Reset()
        {
            Elements.Clear();
            undoStack.Clear();
            nextElementId = 1;
            SelectedElementId = InvalidElementId;

            Mode = PhotoSceneMode.Pawns;
            Tool = PhotoSceneTool.Paint;
            SelectedPropDef = null;
            SelectedAtmosphere = PhotoAtmosphereKind.Blood;

            BrushRadius = 1f;
            Density = 1f;
            RandomFacing = true;
            FixedFacing = Rot4.South;
            PawnPose = PhotoPawnPose.Standing;

            PropScale = 1f;
            PropRandomRotation = true;
            PropFixedRotationDegrees = 0f;
            DecalOpacity = 1f;
            DecalScale = 1f;
            FireScale = 1f;

            CapWarningKey = null;
        }
    }
}
