namespace AODB.Common.Structs
{
    public struct Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static readonly Vector3 Zero = new Vector3(0, 0, 0);
        public static readonly Vector3 Forward = new Vector3(0, 0, 1);
        public static readonly Vector3 Right = new Vector3(1, 0, 0);
        public static readonly Vector3 Up = new Vector3(0, 1, 0);

        public Vector3(double x, double y, double z)
        {
            X = (float)x;
            Y = (float)y;
            Z = (float)z;
        }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
