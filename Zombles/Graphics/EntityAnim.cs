using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using OpenTK;
using ResourceLibrary;

namespace Zombles.Graphics
{
    public class EntityAnim
    {
        private static Dictionary<String, EntityAnim> stFound = new Dictionary<string, EntityAnim>();

        public static EntityAnim GetAnim(params String[] nameLocator)
        {
            var name = String.Join("/", nameLocator);

            if (!stFound.ContainsKey(name)) {
                stFound.Add(name, new EntityAnim(Archive.Get<JObject>(nameLocator)));
            }

            return stFound[name];
        }

        public readonly double Frequency;

        public readonly Vector2 Size;

        public readonly int FrameCount;
        public readonly bool IsDirectional;

        public readonly ushort[,] FrameIndices;

        private EntityAnim(JObject obj)
        {
            FrameCount = (int) obj["frame count"];
            Frequency = (double) obj["frequency"];

            var size = obj["size"];
            Size = new Vector2(
                (int) size["width"] / 8.0f,
                (int) size["height"] / 8.0f
            );

            IsDirectional = (bool) obj["directional"];

            var framePrefix = obj["frame prefix"].Select(x => (String) x).ToArray();
            
            if (IsDirectional) {
                var frameNames = new string[4, FrameCount];
                FrameIndices = new ushort[4, FrameCount];

                var dirs = (JArray) obj["frames"];

                for (int i = 0; i < 4; ++i) {
                    var frames = (JArray) dirs[i];
                    for (int f = 0; f < FrameCount; ++f) {
                        frameNames[i, f] = (String) frames[f];
                        FrameIndices[i, f] = TextureManager.Ents.GetIndex(framePrefix, frameNames[i, f]);
                    }
                }
            } else {
                var frameNames = new string[FrameCount];
                FrameIndices = new ushort[1, FrameCount];

                var frames = (JArray) obj["frames"];
                for (int f = 0; f < FrameCount; ++f) {
                    frameNames[f] = (String) frames[f];
                    FrameIndices[0, f] = TextureManager.Ents.GetIndex(framePrefix, frameNames[f]);
                }
            }
        }
    }
}
