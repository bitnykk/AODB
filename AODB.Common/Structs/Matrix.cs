using System;

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

        public Matrix Transpose()
        {
            var result = new Matrix();
            result.values = new float[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result.values[i, j] = values[j, i];
                }
            }
            return result;
        }
    }
}