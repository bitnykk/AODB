namespace AODB.Common.Structs
{
    public struct Vertex
    {
        public Vector3 Position;

        public Vector3 Normal;

        public Color Color;

        public Vector2 UVs;
    }

    public struct VertexDescription
    {
        public int Unk1;
        public int Unk2;
        public int Unk3;
        public int NumVertices;
    }
}
