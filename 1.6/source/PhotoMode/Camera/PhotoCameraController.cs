using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public class PhotoCameraController
    {
        private const float EdgeMarginCells = 2f;
        private const float BaseKeySpeed = 18f;
        private const float SmoothAccelTime = 0.35f;
        private const float ZoomLerpTightness = 0.4f;
        private const float ScrollWheelZoomRate = 0.35f;
        private const float ZoomSpeed = 2.6f;
        private const float ZoomScaleFromAltDenominator = 35f;

        public const float MinRollDegrees = -45f;
        public const float MaxRollDegrees = 45f;

        private Vector3 pos;
        private Vector3 keyVelocity;
        private float size;
        private float desiredSize;

        private Vector3 entryPos;
        private float entrySize;

        private float zoomMin;
        private float zoomMax;

        private Vector2 dragAccum;

        public Vector3 CurrentPos => pos;
        public float CurrentSize => size;
        public float ZoomMin => zoomMin;
        public float ZoomMax => zoomMax;

        public void Begin(Vector3 rootPos, float rootSize, float zoomMin, float zoomMax, PhotoModeCameraState state)
        {
            entryPos = rootPos;
            pos = rootPos;
            keyVelocity = Vector3.zero;
            size = rootSize;
            desiredSize = rootSize;
            entrySize = rootSize;
            dragAccum = Vector2.zero;

            this.zoomMin = zoomMin;
            this.zoomMax = zoomMax;

            state.Position = pos;
            state.Zoom = size;
        }

        public void ResetToEntry(PhotoModeCameraState state)
        {
            pos = entryPos;
            size = entrySize;
            desiredSize = entrySize;
            keyVelocity = Vector3.zero;

            state.Position = pos;
            state.Zoom = size;
            state.RollDegrees = 0f;
        }

        public void SetZoom(float value)
        {
            desiredSize = Mathf.Clamp(value, zoomMin, zoomMax);
        }

        public static float ClampRoll(float degrees)
        {
            return Mathf.Clamp(degrees, MinRollDegrees, MaxRollDegrees);
        }

        public void HandleOnGUI()
        {
            if (Find.WindowStack.GetWindowAt(UI.MousePositionOnUIInverted) != null)
            {
                return;
            }

            if (!CameraMotionAllowed())
            {
                return;
            }

            if (Event.current.type == EventType.ScrollWheel)
            {
                float wheelDelta = Event.current.delta.y * ScrollWheelZoomRate * ZoomSpeed * size / ZoomScaleFromAltDenominator;
                desiredSize = Mathf.Clamp(desiredSize + wheelDelta, zoomMin, zoomMax);
                Event.current.Use();
                return;
            }

            if (!UnityGUIBugsFixer.MouseDrag(2))
            {
                return;
            }

            Vector2 delta = UnityGUIBugsFixer.CurrentEventDelta;
            if (Event.current.type == EventType.MouseDrag)
            {
                Event.current.Use();
            }

            if (delta == Vector2.zero)
            {
                return;
            }

            Vector2 currentUI = UI.MousePositionOnUI;
            Vector2 previousUI = currentUI - new Vector2(delta.x, -delta.y);

            Vector3 worldCurrent = UI.UIToMapPosition(currentUI);
            Vector3 worldPrevious = UI.UIToMapPosition(previousUI);

            dragAccum += new Vector2(worldPrevious.x - worldCurrent.x, worldPrevious.z - worldCurrent.z) * Prefs.MapDragSensitivity;
        }

        public void HandleUpdate(Map map, PhotoModeCameraState state)
        {
            Vector3 inputDir = Vector3.zero;
            bool motionAllowed = CameraMotionAllowed();

            if (motionAllowed)
            {
                Vector2 keyDolly = Vector2.zero;
                if (KeyBindingDefOf.MapDolly_Left.IsDown)
                {
                    keyDolly.x -= 1f;
                }
                if (KeyBindingDefOf.MapDolly_Right.IsDown)
                {
                    keyDolly.x += 1f;
                }
                if (KeyBindingDefOf.MapDolly_Up.IsDown)
                {
                    keyDolly.y += 1f;
                }
                if (KeyBindingDefOf.MapDolly_Down.IsDown)
                {
                    keyDolly.y -= 1f;
                }

                if (keyDolly != Vector2.zero)
                {
                    Transform camTransform = Find.Camera.transform;
                    Vector3 right = camTransform.right;
                    Vector3 up = camTransform.up;
                    right.y = 0f;
                    up.y = 0f;
                    inputDir = (right.normalized * keyDolly.x + up.normalized * keyDolly.y).normalized;
                }

                if (dragAccum != Vector2.zero)
                {
                    pos += new Vector3(dragAccum.x, 0f, dragAccum.y);
                }
            }

            dragAccum = Vector2.zero;

            float speed = Mathf.Max(0.05f, state.MovementSpeed);
            Vector3 targetVelocity = inputDir * BaseKeySpeed * speed;

            if (state.SmoothMovement)
            {
                float accel = BaseKeySpeed * speed / SmoothAccelTime;
                keyVelocity = Vector3.MoveTowards(keyVelocity, targetVelocity, accel * Time.deltaTime);
            }
            else
            {
                keyVelocity = targetVelocity;
            }

            pos += keyVelocity * Time.deltaTime;

            pos.x = Mathf.Clamp(pos.x, EdgeMarginCells, map.Size.x - EdgeMarginCells);
            pos.z = Mathf.Clamp(pos.z, EdgeMarginCells, map.Size.z - EdgeMarginCells);

            size = Mathf.Abs(desiredSize - size) < 0.001f ? desiredSize : Mathf.Lerp(size, desiredSize, ZoomLerpTightness);

            state.Position = pos;
            state.Zoom = size;
        }

        private static bool CameraMotionAllowed()
        {
            if (Find.WindowStack.WindowsPreventCameraMotion)
            {
                return false;
            }

            if (WorldRendererUtility.WorldSelected)
            {
                return false;
            }

            return Verse.Current.Game.PlayerHasControl;
        }
    }
}
