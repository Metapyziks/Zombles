using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using OpenTKTK.Utils;

using ResourceLibrary;

namespace Zombles.Graphics
{
    public class EntityModel
    {
        private class Vertex
        {
            public const int Size = 9;

            public Vector3 Position;
            public Vector2 TexCoord;

            public Vertex(JObject obj)
            {
                Position = new Vector3 {
                    X = (int) obj["x"] / 8f,
                    Y = (int) obj["y"] / 8f,
                    Z = (int) obj["z"] / 8f
                };

                TexCoord = new Vector2 {
                    X = (int) obj["u"] / 8f,
                    Y = (int) obj["v"] / 8f
                };
            }
            
            public void GetVertices(float[] verts, ref int i)
            {
                verts[i++] = Position.X;
                verts[i++] = Position.Y;
                verts[i++] = Position.Z;
                verts[i++] = TexCoord.X;
                verts[i++] = TexCoord.Y;
            }
        }

        private class Face
        {
            public ResourceLocator Texture;
            public Vertex[] Vertices;

            public Face(JObject obj, Dictionary<String, ResourceLocator> skin)
            {
                Texture = skin[(String) obj["texture"]];
                Vertices = ((JArray) obj["verts"])
                    .Select(x => new Vertex((JObject) x))
                    .ToArray();
            }

            public void GetVertices(float[] verts, ref int i)
            {
                int texIndex = TextureManager.Ents.GetIndex(Texture);

                var normal = Vector3.Cross(
                    Vertices[2].Position - Vertices[0].Position,
                    Vertices[3].Position - Vertices[1].Position)
                    .Normalized();

                foreach (var vert in Vertices) {
                    vert.GetVertices(verts, ref i);
                    verts[i++] = texIndex;
                    verts[i++] = normal.X;
                    verts[i++] = normal.Y;
                    verts[i++] = normal.Z;
                }
            }
        }

        private static VertexBuffer _sVB;
        private static bool _sVBInvalid;
        
        private static Dictionary<String, EntityModel> _sFound =
            new Dictionary<string, EntityModel>();

        public static VertexBuffer VertexBuffer
        {
            get
            {
                if (_sVB == null) {
                    _sVB = new VertexBuffer(Vertex.Size, BufferUsageHint.StaticDraw);
                    _sVBInvalid = true;
                }
                
                if (_sVBInvalid) {
                    float[] data = new float[_sFound.Values.Sum(x => x.TotalSize * Vertex.Size)];
                    int i = 0;
                    foreach (var mdl in _sFound.Values) {
                        mdl.GetVertices(data, ref i);
                    }
                    _sVB.SetData(data);
                    _sVBInvalid = false;
                }

                return _sVB;
            }
        }

        public static EntityModel Get(params String[] nameLocator)
        {
            var name = String.Join("/", nameLocator);

            if (!_sFound.ContainsKey(name)) {
                _sFound.Add(name, new EntityModel(Archive.Get<JObject>(nameLocator)));
                _sVBInvalid = true;
            }

            return _sFound[name];
        }

        private int _vertOffset;
        private Face[,] _faces;

        public int Skins { get; private set; }
        public int Faces { get; private set; }

        public int TotalSize { get; private set; }
        public int SingleSize { get; private set; }

        private EntityModel(JObject obj)
        {
            var skins = ((JArray) obj["skins"])
                .Cast<JObject>()
                .Select(x => x.Properties()
                    .ToDictionary(
                        y => y.Name,
                        y => (ResourceLocator) (String) y.Value))
                .ToArray();

            Skins = skins.Length;

            var faces = ((JArray) obj["faces"])
                .Cast<JObject>()
                .ToArray();

            Faces = faces.Length;

            _faces = new Face[Skins, Faces];

            for (int s = 0; s < Skins; ++s) {
                for (int f = 0; f < Faces; ++f) {
                    _faces[s, f] = new Face(faces[f], skins[s]);
                }
            }

            SingleSize = Faces * 4;
            TotalSize = SingleSize * Skins;
        }

        private void GetVertices(float[] verts, ref int i)
        {
            _vertOffset = i / Vertex.Size;

            foreach (var face in _faces) {
                face.GetVertices(verts, ref i);
            }
        }

        public void Render(int skin)
        {
            VertexBuffer.Render(_vertOffset + skin * SingleSize, SingleSize);
        }
    }
}
