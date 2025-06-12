using AODB.Common.Structs;
using System.Collections.Generic;

namespace AODB.Encoding
{
    public static class MathExtensions
    {
        internal static Vector3 ToAODb(this Assimp.Vector3D v) => new Vector3(v.X, v.Y, v.Z);
        internal static Assimp.Vector3D ToAssimp(this Vector3 v) => new Assimp.Vector3D(v.X, v.Y, v.Z);
        internal static Quaternion ToAODb(this Assimp.Quaternion q) => new Quaternion(q.X, q.Y, q.Z, q.W);
        internal static Assimp.Quaternion ToAssimp(this Quaternion q) => new Assimp.Quaternion(q.W, q.X, q.Y, q.Z);
        internal static Assimp.Color4D ToAssimp(this Color c) => new Assimp.Color4D(c.R, c.G, c.B, 1f);

        internal static Assimp.Vector3D ToAssimpScaleFix(this Vector3 v) => new Assimp.Vector3D(-v.X, v.Y, v.Z)*100;


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
