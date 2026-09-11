using RimWorld;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    internal static class PhotoPawnPoseRenderer
    {
        internal static void DrawLayingFaceUp(PawnRenderer renderer, DrawPhase phase, Vector3 drawLoc, Rot4 facing)
        {
            Pawn pawn = renderer.pawn;

            Vector3 bodyPos = drawLoc;
            bodyPos.y = AltitudeLayer.LayingPawn.AltitudeFor();

            float angle = facing.AsAngle;

            PawnDrawParms parms = new PawnDrawParms
            {
                pawn = pawn,
                matrix = Matrix4x4.TRS(bodyPos + pawn.ageTracker.CurLifeStage.bodyDrawOffset, Quaternion.AngleAxis(angle, Vector3.up), Vector3.one),
                facing = facing,
                rotDrawMode = renderer.CurRotDrawMode,
                posture = PawnPosture.LayingOnGroundFaceUp,
                flags = PawnRenderFlags.Clothes | PawnRenderFlags.Headgear,
                tint = Color.white,
                bed = null,
                coveredInFoam = false,
                dead = false,
                crawling = false,
                swimming = false,
                carriedThing = null,
                statueColor = null
            };

            switch (phase)
            {
                case DrawPhase.ParallelPreDraw:
                    renderer.renderTree.ParallelPreDraw(parms);
                    break;
                case DrawPhase.Draw:
                    renderer.renderTree.Draw(parms);
                    break;
            }
        }
    }
}
