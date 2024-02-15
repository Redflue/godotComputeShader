using Godot;
using System;

namespace ShaderTools {
    public partial class BytePacker : GodotObject
    {
        byte[] bytes;
        int offset;
        int bufferUsedSize;
        int bufferMaxSize;
        bool packed;
        public bool Packed {
            get {return packed;}
        }

        public BytePacker(int bufferSize) {
            bufferMaxSize = bufferSize;
            offset = 0;
            bufferUsedSize = 0;
            bytes = new byte[bufferMaxSize];
            packed = false;
        }

        public void PackUp() {
            packed = true;
        }

        public byte[] GetBytes() {
            if (packed) 
                return bytes;
            else
                throw new Exception("Trying to access an unpacked buffer");
        }

        public void AppendFloat() {
            
        }
    }
}
