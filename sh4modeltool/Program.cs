using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Assimp;

namespace sh4modeltool
{
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

            model.unknownChunk.unknownMagic = reader.ReadUInt32();

            model.unknownChunk.unknownChunk = reader.ReadBytes(0x3c);

            model.unknownChunk.unknownFloatChunks = new Chunks.Model.UnknownChunk.UnknownFloatChunk[8].Select(p => new Chunks.Model.UnknownChunk.UnknownFloatChunk()).ToArray();

            foreach (var chunk in model.unknownChunk.unknownFloatChunks)
            {
                for(var i =0;i < 3;i++)
                {
                    chunk.unknown[i] = reader.ReadSingle();
                }
                chunk.unknown2 = reader.ReadUInt32();
            }

            model.unknownChunk.unknownChunk2 = reader.ReadBytes(0x10);

            model.parts = new Chunks.Model.Part[model.numSubParts].Select(p => new Chunks.Model.Part()).ToArray();

            // Now for the fun part :)
            for (var i=0;i<model.numSubParts;i++)
            {
                model.parts[i].partSize = reader.ReadUInt32();

                model.parts[i].unknownChunk = reader.ReadBytes(0x4c);

                model.parts[i].unknownFloatChunk = new Chunks.Model.Part.UnknownFloatChunk[4].Select(v => new Chunks.Model.Part.UnknownFloatChunk()).ToArray();


                foreach (var chunk in model.parts[i].unknownFloatChunk)
                {
                    for(var chunkCount=0;chunkCount < chunk.unknown.Length;chunkCount++)
                    {
                        chunk.unknown[chunkCount] = reader.ReadSingle();
                    }
                }

                model.unknownChunk.unknownChunk2 = reader.ReadBytes(0x6c);

                model.parts[i].triStripCount = reader.ReadUInt32();
                model.parts[i].vertexCount = reader.ReadUInt32();
                model.parts[i].unknown = reader.ReadUInt32();

                model.parts[i].vertices = new Chunks.Model.Part.Vertex[model.parts[i].vertexCount].Select(v => new Chunks.Model.Part.Vertex()).ToArray();
                model.parts[i].triStrips = new Chunks.Model.Part.TriangleStripFace[model.parts[i].triStripCount].Select(v => new Chunks.Model.Part.TriangleStripFace()).ToArray();

                reader.ReadBytes(0x10);

                // Read vertices
                foreach (var vert in model.parts[i].vertices)
                {
                    vert.x = reader.ReadSingle();
                    vert.y = reader.ReadSingle();
                    vert.z = reader.ReadSingle();

                    reader.ReadBytes(0xc);

                    vert.texCoordU = reader.ReadSingle();
                    vert.texCoordV = reader.ReadSingle();

                    reader.ReadBytes(0x20);
                }

                foreach(var triStrip in model.parts[i].triStrips)
                {
                    triStrip.index = reader.ReadInt16();
                }

                model.parts[i].padding = reader.ReadUInt32();

                model.parts[i].FFArray = reader.ReadBytes(0x4c);


            }

            // Write obj file for each part

            int partsWritten = 0;

            foreach(var part in model.parts)
            {
                StreamWriter writer = new StreamWriter(new FileStream(partsWritten+".obj", FileMode.Create));

                writer.Write("# Silent Hill 4 Model\n");
                writer.Write("# Ripped by sh4modeltool, 2019 Hunter Stanton\n");
                foreach (var vert in part.vertices)
                {
                    writer.Write("v "+vert.x+" "+-vert.y+" "+vert.z+"\n");
                }

                for (var i = 0; i < part.triStripCount; i++)
                {
                    if (i%2 == 1)
                    {
                        if (i != part.triStripCount - 2)
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
                    }
                    else
                    {
                        if (i != part.triStripCount - 1)
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
                }

                writer.Close();
                partsWritten++;
            }

        }
    }
}
