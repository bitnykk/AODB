namespace AODB.Common.Structs
{
    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public static Vector2 Zero => new Vector2(0, 0);

        public Vector2(double x, double y)
        {
            X = (float)x;
            Y = (float)y;
        }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}
