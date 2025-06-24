using AODB.Common.Structs;
using Assimp;
using System;
using System.Collections.Generic;
using Quaternion = AODB.Common.Structs.Quaternion;
using AQuaternion = Assimp.Quaternion;

namespace AODB.Encoding
{
    public static class MathExtensions
    {
        public static Vector3 ToAODB(this Assimp.Vector3D v) => new Vector3(v.X, v.Y, v.Z);
        public static Assimp.Vector3D ToAssimp(this Vector3 v) => new Assimp.Vector3D(v.X, v.Y, v.Z);
        public static Quaternion ToAODB(this AQuaternion q) => new Quaternion(q.X, q.Y, q.Z, q.W);
        public static AQuaternion ToAssimp(this Quaternion q) => new AQuaternion(q.W, q.X, q.Y, q.Z);
        public static Assimp.Color4D ToAssimp(this Color c) => new Assimp.Color4D(c.R, c.G, c.B, 1f);

        public static Assimp.Vector3D ToAssimpScaleFix(this Vector3 v) => new Assimp.Vector3D(-v.X, v.Y, v.Z)*100;

        public static Assimp.Matrix4x4 ToAssimpMatrix(this Matrix m)
        {
            return new Assimp.Matrix4x4(
                m.values[0, 0], m.values[0, 1], m.values[0, 2], m.values[0, 3],
                m.values[1, 0], m.values[1, 1], m.values[1, 2], m.values[1, 3],
                m.values[2, 0], m.values[2, 1], m.values[2, 2], m.values[2, 3],
                m.values[3, 0], m.values[3, 1], m.values[3, 2], m.values[3, 3]
            );
        }

        public static Matrix FromAssimpMatrix(this Assimp.Matrix4x4 assimpMatrix)
        {
            var m = new Matrix();
            m.values = new float[4, 4];

            m.values[0, 0] = assimpMatrix.A1;
            m.values[0, 1] = assimpMatrix.A2;
            m.values[0, 2] = assimpMatrix.A3;
            m.values[0, 3] = assimpMatrix.A4;

            m.values[1, 0] = assimpMatrix.B1;
            m.values[1, 1] = assimpMatrix.B2;
            m.values[1, 2] = assimpMatrix.B3;
            m.values[1, 3] = assimpMatrix.B4;

            m.values[2, 0] = assimpMatrix.C1;
            m.values[2, 1] = assimpMatrix.C2;
            m.values[2, 2] = assimpMatrix.C3;
            m.values[2, 3] = assimpMatrix.C4;

            m.values[3, 0] = assimpMatrix.D1;
            m.values[3, 1] = assimpMatrix.D2;
            m.values[3, 2] = assimpMatrix.D3;
            m.values[3, 3] = assimpMatrix.D4;

            return m;
        }

        public static Vector3D GetEulerAnglesFromQuaternion(this AQuaternion q)
        {
            // Normalize quaternion just in case
            float norm = (float)Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W);
            float x = q.X / norm;
            float y = q.Y / norm;
            float z = q.Z / norm;
            float w = q.W / norm;

            // Rotation order: Y (yaw), X (pitch), Z (roll) — YXZ

            // Calculate Euler angles
            float sinY = 2f * (w * y - x * z);
            float angleY = (float)Math.Asin(Clamp(sinY, -1f, 1f)); // yaw (Y)

            float angleX = (float)Math.Atan2(2f * (w * x + y * z), 1f - 2f * (x * x + y * y)); // pitch (X)
            float angleZ = (float)Math.Atan2(2f * (w * z + x * y), 1f - 2f * (y * y + z * z)); // roll (Z)

            // Convert to degrees
            float degX = (float)(angleX * (180f / Math.PI));
            float degY = (float)(angleY * (180f / Math.PI));
            float degZ = (float)(angleZ * (180f / Math.PI));

            return new Vector3D(degX, degY, degZ); // (Pitch, Yaw, Roll)
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static bool TryGetKey<K, V>(this IDictionary<K, V> instance, V value, out K key)
        {
            foreach (var entry in instance)
            {
                if (entry.Value.Equals(value))
                {
                    key = entry.Key;
                    return true;
                }
            }
            key = default(K);
            return false;
        }
    }
}
