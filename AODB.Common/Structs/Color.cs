namespace AODB.Common.Structs
{
    public class Color
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public static implicit operator Assimp.Color4D(Color c) => new Assimp.Color4D(c.R, c.G, c.B, 1f);
    }
}
