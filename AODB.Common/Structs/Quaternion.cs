using System;

namespace AODB.Common.Structs
{
    public struct Quaternion
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }


        public double Magnitude
        {
            get { return Math.Sqrt((X * X) + (Y * Y) + (Z * Z) + (W * W)); }
        }

        public static Quaternion Identity => new Quaternion(0, 0, 0, 1f);


        public Quaternion(double x, double y, double z, double w)
        {
            X = (float)x;
            Y = (float)y;
            Z = (float)z;
            W = (float)w;
        }

        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static Quaternion FromAxisAngleRad(Vector3 axis, float angle)
        {
            Quaternion result = new Quaternion();

            float sin = (float)Math.Sin(angle / 2.0f);
            result.X = axis.X * sin;
            result.Y = axis.Y * sin;
            result.Z = axis.Z * sin;
            float cos = (float)Math.Cos(angle / 2.0f);

            result.W = cos;

            return Quaternion.Normalize(result);
        }

        public static Quaternion Normalize(Quaternion q1)
        {
            double mag = q1.Magnitude;

            return new Quaternion(q1.X / mag, q1.Y / mag, q1.Z / mag, q1.W / mag);
        }

        public Quaternion Normalize()
        {
            return Normalize(this);
        }

        public static Vector3 operator *(Quaternion rotation, Vector3 point)
        {
            float num = rotation.X * 2f;
            float num2 = rotation.Y * 2f;
            float num3 = rotation.Z * 2f;
            float num4 = rotation.X * num;
            float num5 = rotation.Y * num2;
            float num6 = rotation.Z * num3;
            float num7 = rotation.X * num2;
            float num8 = rotation.X * num3;
            float num9 = rotation.Y * num3;
            float num10 = rotation.W * num;
            float num11 = rotation.W * num2;
            float num12 = rotation.W * num3;
            return new Vector3
            {
                X = (1f - (num5 + num6)) * point.X + (num7 - num12) * point.Y + (num8 + num11) * point.Z,
                Y = (num7 + num12) * point.X + (1f - (num4 + num6)) * point.Y + (num9 - num10) * point.Z,
                Z = (num8 - num11) * point.X + (num9 + num10) * point.Y + (1f - (num4 + num5)) * point.Z
            };
        }

        public static Quaternion operator *(Quaternion lhs, Quaternion rhs)
        {
            return new Quaternion(lhs.W * rhs.X + lhs.X * rhs.W + lhs.Y * rhs.Z - lhs.Z * rhs.Y, lhs.W * rhs.Y + lhs.Y * rhs.W + lhs.Z * rhs.X - lhs.X * rhs.Z, lhs.W * rhs.Z + lhs.Z * rhs.W + lhs.X * rhs.Y - lhs.Y * rhs.X, lhs.W * rhs.W - lhs.X * rhs.X - lhs.Y * rhs.Y - lhs.Z * rhs.Z);
        }

        public static implicit operator Quaternion(Assimp.Quaternion q) => new Quaternion(q.X, q.Y, q.Z, q.W);
        public static implicit operator Assimp.Quaternion(Quaternion q) => new Assimp.Quaternion(q.W, q.X, q.Y, q.Z);
    }
}
