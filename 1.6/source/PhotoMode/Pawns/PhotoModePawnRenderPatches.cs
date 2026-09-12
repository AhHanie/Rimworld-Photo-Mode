using HarmonyLib;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.DynamicDrawPhaseAt))]
    [HarmonyPatchCategory(PhotoModePatchController.RuntimeCategory)]
    internal static class Patch_PawnRenderer_DynamicDrawPhaseAt
    {
        private static bool Prefix(PawnRenderer __instance, DrawPhase phase, ref Vector3 drawLoc, ref Rot4? rotOverride)
        {
            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return true;
            }

            if (PhotoSceneRenderer.IsRenderingSceneProxy)
            {
                return true;
            }

            Pawn pawn = __instance.pawn;
            if (pawn == null || !pawn.Spawned)
            {
                return true;
            }

            if (!PhotoModeManager.Current.State.PawnOverrides.TryGetValue(pawn, out PawnPhotoOverride photoOverride))
            {
                return true;
            }

            if (photoOverride.Facing.HasValue)
            {
                rotOverride = photoOverride.Facing;
            }

            drawLoc += photoOverride.Offset;

            if (phase == DrawPhase.EnsureInitialized || photoOverride.Pose != PhotoPawnPose.LayingFaceUp)
            {
                return true;
            }

            if (!PhotoPawnPoseSupport.CanRenderLayingFaceUp(pawn))
            {
                return true;
            }

            Rot4 poseFacing = photoOverride.Facing ?? rotOverride ?? Rot4.South;
            PhotoPawnPoseRenderer.DrawLayingFaceUp(__instance, phase, drawLoc, poseFacing);
            return false;
        }
    }
}
