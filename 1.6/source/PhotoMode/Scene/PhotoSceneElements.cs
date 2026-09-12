using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public enum PhotoSceneMode
    {
        Pawns,
        Props,
        Atmosphere
    }

    public enum PhotoSceneTool
    {
        Paint,
        Erase
    }

    public enum PhotoAtmosphereKind
    {
        Blood,
        Dirt,
        Ash,
        Rubble,
        Fire
    }

    public enum PhotoDecalKind
    {
        Blood,
        Dirt,
        Ash,
        Rubble
    }

    public abstract class PhotoSceneElement
    {
        public int Id;
        public Vector3 Position;
    }

    public class PhotoPawnElement : PhotoSceneElement
    {
        public Pawn Source;
        public Rot4 Facing;
        public PhotoPawnPose Pose;
    }

    public class PhotoPropElement : PhotoSceneElement
    {
        public ThingDef Def;
        public float RotationDegrees;
        public float Scale = 1f;
        public int VariantSeed;
    }

    public class PhotoDecalElement : PhotoSceneElement
    {
        public PhotoDecalKind Kind;
        public float Opacity = 1f;
        public float Scale = 1f;
        public int Seed;
    }

    public class PhotoFireElement : PhotoSceneElement
    {
        public float Scale = 1f;
        public int Seed;
    }
}
