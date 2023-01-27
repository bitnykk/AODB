namespace AODB.Common.Structs
{
    public struct Matrix
    {
        public float[,] values;

        public static Matrix Empty => new Matrix()
        {
            values = new float[4, 4]
            {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },   
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 },
            }
        };
    }
}