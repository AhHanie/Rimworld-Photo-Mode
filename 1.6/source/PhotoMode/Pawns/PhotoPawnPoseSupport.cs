using RimWorld;
using Verse;

namespace Photo_Mode
{
    public static class PhotoPawnPoseSupport
    {
        public static bool IsSupported(Pawn pawn, PhotoPawnPose pose)
        {
            if (pose == PhotoPawnPose.Standing)
            {
                return pawn != null;
            }

            return pose == PhotoPawnPose.LayingFaceUp && CanRenderLayingFaceUp(pawn);
        }

        public static bool CanRenderLayingFaceUp(Pawn pawn)
        {
            if (pawn == null || !pawn.Spawned || pawn.Dead)
            {
                return false;
            }

            if (!pawn.RaceProps.Humanlike)
            {
                return false;
            }

            if (pawn.CarriedBy != null || pawn.carryTracker?.CarriedThing != null)
            {
                return false;
            }

            if (pawn.CurrentBed() != null)
            {
                return false;
            }

            if (pawn.Drawer.renderer.CurAnimation != null)
            {
                return false;
            }

            return true;
        }
    }
}
