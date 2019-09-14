using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Numerics;

// Sharp GLTF
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;

namespace sh4modeltool
{
    using VERTEX = SharpGLTF.Geometry.VertexTypes.VertexPosition;

    class Program
    {
        static void Main(string[] args)
        {

            // Open a filestream with the user selected file
            FileStream file = new FileStream(args[0], FileMode.Open);

            // Create a binary reader that will be used to read the file
            BinaryReader reader = new BinaryReader(file);

            // Read data from model

            Chunks.Model model = new Chunks.Model();

            model.magic = reader.ReadUInt32();

            model.gameVersion = reader.ReadUInt32();

            model.scalePointer = reader.ReadUInt32();

            model.numParts = reader.ReadUInt32();

            reader.ReadBytes(0x10);

            model.numSubParts = reader.ReadUInt32();

            model.pointerToFirstMesh = reader.ReadUInt32();

            reader.ReadUInt32();

            model.pointerToEndOfLastMesh = reader.ReadUInt32();

            model.numTextures = reader.ReadUInt32();

            // Skip past unknowns as we don't need them to dump a model, but we need to reverse them if we want to import a model
            reader.ReadBytes(0x4c);

            model.modelScales = new Chunks.Model.ModelScale[model.numParts].Select(v => new Chunks.Model.ModelScale()).ToArray();

            // Read model scales
            foreach (var scale in model.modelScales)
            {

                for(var i=0;i < scale.unknown1.Length;i++)
                {
                    scale.unknown1[i] = reader.ReadSingle();
                }
                for (var i = 0; i < scale.unknown2.Length; i++)
                {
                    scale.unknown2[i] = reader.ReadSingle();
                }
                for (var i = 0; i < scale.unknown3.Length; i++)
                {
                    scale.unknown3[i] = reader.ReadSingle();
                }
                for (var i = 0; i < scale.unknown4.Length; i++)
                {
                    scale.unknown4[i] = reader.ReadSingle();
                }
            }

            model.part.unknownMagic = reader.ReadUInt32();

            model.part.unknownChunk = reader.ReadBytes(0x3c);

            model.part.unknownFloatChunks = new Chunks.Model.Part.UnknownFloatChunk[8].Select(p => new Chunks.Model.Part.UnknownFloatChunk()).ToArray();

            foreach (var chunk in model.part.unknownFloatChunks)
            {
                for(var i =0;i < 3;i++)
                {
                    chunk.unknown[i] = reader.ReadSingle();
                }
                chunk.unknown2 = reader.ReadUInt32();
            }

            // Check for an unknown section
            int check = reader.ReadInt32();
            if(check != 0)
            {
                reader.ReadInt16();
                int count = reader.ReadInt16();

                reader.ReadBytes(0x8);

                if (count % 2 == 1)
                {
                    // Read whatever this is
                    // TODO: Add this to the structure
                    reader.ReadBytes(0xC * count);

                    // Skip padding
                    reader.ReadUInt32();
                }
                else
                {
                    // Read whatever this is
                    // TODO: Add this to the structure
                    reader.ReadBytes(0xC * count);

                    // No padding this time because it's an even number of whatever this is
                }

            }
            else
            {
                reader.BaseStream.Position = reader.BaseStream.Position - 4;
                model.part.unknownChunk2 = reader.ReadBytes(0x10);
            }


            model.subParts = new Chunks.Model.SubPart[model.numSubParts].Select(p => new Chunks.Model.SubPart()).ToArray();

            // Now for the fun part :)
            for (var i=0;i<model.numSubParts;i++)
            {
                model.subParts[i].subPartSize = reader.ReadUInt32();

                model.subParts[i].unknownChunk = reader.ReadBytes(0x4c);

                model.subParts[i].colorChunk = new Chunks.Model.SubPart.ColorChunk[4].Select(v => new Chunks.Model.SubPart.ColorChunk()).ToArray();


                foreach (var chunk in model.subParts[i].colorChunk)
                {
                    chunk.red = reader.ReadSingle();
                    chunk.green = reader.ReadSingle();
                    chunk.blue = reader.ReadSingle();
                    chunk.alpha = reader.ReadSingle();
                }

                model.part.unknownChunk2 = reader.ReadBytes(0x6c);

                model.subParts[i].triStripCount = reader.ReadUInt32();
                model.subParts[i].vertexCount = reader.ReadUInt32();
                model.subParts[i].unknown = reader.ReadUInt32();

                model.subParts[i].vertices = new Chunks.Model.SubPart.Vertex[model.subParts[i].vertexCount].Select(v => new Chunks.Model.SubPart.Vertex()).ToArray();
                model.subParts[i].triStrips = new Chunks.Model.SubPart.TriangleStripFace[model.subParts[i].triStripCount].Select(v => new Chunks.Model.SubPart.TriangleStripFace()).ToArray();

                reader.ReadBytes(0x10);

                // Read vertices
                foreach (var vert in model.subParts[i].vertices)
                {
                    vert.x = reader.ReadSingle();
                    vert.y = reader.ReadSingle();
                    vert.z = reader.ReadSingle();

                    reader.ReadBytes(0xc);

                    vert.texCoordU = reader.ReadSingle();
                    vert.texCoordV = reader.ReadSingle();

                    reader.ReadBytes(0x20);
                }

                int count = 0;
                foreach(var triStrip in model.subParts[i].triStrips)
                {
                    if(count == 0)
                    {
                        // Check for padding
                        long origBaseStreamPos = reader.BaseStream.Position;

                        reader.ReadBytes(Convert.ToInt32(model.subParts[i].triStripCount) * 0x2);

                        // We skip to where the end of the triangle strips should be and read 0x4 bytes
                        // This will always be 0 if the triangle strip has truly ended
                        // If it is not 0, then we need to go to the beginning of the triangle strips and seek 0x2 bytes forward and read as normal
                        // This accounts for padding that seems to crop up every now and then, for whatever reason
                        if(reader.ReadUInt32() != 0)
                        {
                            reader.BaseStream.Position = origBaseStreamPos + 0x2;
                        }
                        else
                        {
                            reader.BaseStream.Position = origBaseStreamPos;
                        }
                    }
                    triStrip.index = reader.ReadInt16();

                    count++;
                }

                model.subParts[i].padding = reader.ReadUInt32();

                model.subParts[i].FFArray = reader.ReadBytes(0x4c);


            }

            // Write obj file for each part

            int partsWritten = 0;

            foreach(var part in model.subParts)
            {

                StreamWriter writer = new StreamWriter(new FileStream(partsWritten + ".obj", FileMode.Create));

                writer.Write("# Silent Hill 4 Model\n");
                writer.Write("# Ripped by sh4modeltool, 2019 Hunter Stanton\n");
                foreach (var vert in part.vertices)
                {
                    writer.Write("v " + vert.x + " " + -vert.y + " " + vert.z + "\n");
                }


                for (var i = 0; i < part.triStripCount; i++)
                {
                    if(i == part.triStripCount - 2)
                    {
                        break;
                    }
                    if (i % 2 == 1)
                    {
                        var a = part.triStrips[i].index + 1;
                        var b = part.triStrips[i + 1].index + 1;
                        var c = part.triStrips[i + 2].index + 1;

                        // Remove triangles that are degenerate
                        if (a == b || a == c || b == c)
                            continue;

                        // Write a face command for a triangle
                        writer.Write("f " + c + " " + b + " " + a + "\n");
                    }
                    else
                    {
                        var a = part.triStrips[i].index + 1;
                        var b = part.triStrips[i + 1].index + 1;
                        var c = part.triStrips[i + 2].index + 1;

                        // Remove triangles that are degenerate
                        if (a == b || a == c || b == c)
                            continue;

                        // Write a face command for a triangle
                        writer.Write("f " + a + " " + b + " " + c + "\n");
                    }
                }

                writer.Close();
                partsWritten++;
            }

        }
    }
}
