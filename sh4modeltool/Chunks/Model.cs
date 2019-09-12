using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sh4modeltool.Chunks
{
    class Model
    {
        // If a chunk starts with this, it is a model chunk
        public uint magic = 0xFFFF0003;

        // Game/engine version
        // Always 4 for SH4
        public uint gameVersion = 0x04;

        // Points to the model scaling(???)
        public uint scalePointer;

        // Number of parts in a model
        public uint numParts;

        public uint unknown1;
        public uint unknown2;
        public uint unknown3;
        public uint unknown4;

        // The number of sub parts in a model
        public uint numSubParts;

        // Pointer to the first 3D mesh in the model chunk
        public uint pointerToFirstMesh;

        public uint unknown6;

        // Pointer to the end of the last 3D mesh in the model chunk
        public uint pointerToEndOfLastMesh;

        // Number of textures in the texture chunk that the model uses
        public uint numTextures;

        public uint unknown7;

        public uint unknown8;

        public uint unknown9;

        public uint unknown10;
        public uint unknown11;
        public uint unknown12;
        public uint unknown13;

        public uint unknown14;
        public uint unknown15;
        public uint unknown16;
        public uint unknown17;

        public uint unknown18;
        public uint unknown19;
        public uint unknown20;
        public uint unknown21;

        public uint unknown22;
        public uint unknown23;
        public uint unknown24;
        public uint unknown25;

        public ModelScale[] modelScales = new ModelScale[1];

        public UnknownChunk unknownChunk = new UnknownChunk();

        // The good stuff
        public Part[] parts;


        public class UnknownChunk
        {
            // Always seems to be 0xFF, but could be something else in some models
            public uint unknownMagic = 0xFF;

            public byte[] unknownChunk = new byte[0x3c];

            public UnknownFloatChunk[] unknownFloatChunks = new UnknownFloatChunk[8];

            public byte[] unknownChunk2 = new byte[0x10];

            public class UnknownFloatChunk
            {
                public float[] unknown = new float[3];
                public uint unknown2;
            }
        }

        public class Part
        {

            public uint partSize;

            public byte[] unknownChunk = new byte[0x4c];

            public UnknownFloatChunk[] unknownFloatChunk = new UnknownFloatChunk[4];

            public byte[] unknownChunk2 = new byte[0x6c];

            public uint triStripCount;
            public uint vertexCount;

            // Seems to always be 0x40, not sure what it represents
            public uint unknown;

            public Vertex[] vertices;
            public TriangleStripFace[] triStrips;

            // Always seems to be 4 bytes of padding after tristrips
            public uint padding;

            // Always seems to be 17 0xFFs following the padding after triangle strips
            public byte[] FFArray;

            public class UnknownFloatChunk
            {
                public float[] unknown = new float[4];
            }

            public class Vertex
            {
                public float x;
                public float y;
                public float z;

                public float unknown1;
                public float unknown2;
                public float unknown3;

                public float texCoordU;
                public float texCoordV;

                public float unknown4;
                public float unknown5;
                public float unknown6;
                public float unknown7;
                public float unknown8;
                public float unknown9;
                public float unknown10;
                public float unknown11;
            }

            public class TriangleStripFace
            {
                // Not sure what good names for these would be tbh
                public short index;
            }
        }

        public class ModelScale
        {
            public float[] unknown1 = new float[4];
            public float[] unknown2 = new float[4];
            public float[] unknown3 = new float[4];
            public float[] unknown4 = new float[4];
        }
    }
}
