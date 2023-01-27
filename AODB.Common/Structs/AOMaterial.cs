using System.Collections.Generic;

namespace AODB.Common.Structs
{
    public class AOMaterial
    {
        public string MaterialName;
        public string TextureName;
        public uint Texture;
        public uint TextureType;
        public Dictionary<int, int> Variables;
    }
}
