using System;
using System.Linq;
using System.Collections.Generic;

using ResourceLibrary;
using System.Drawing;

namespace Zombles.Graphics
{
    public class TextureManager
    {
        public static TextureManager Tiles { get; private set; }
        public static TextureManager Ents { get; private set; }

        public static void Initialize()
        {
            Tiles = new TextureManager("images", "tiles");
            Ents = new TextureManager("images", "ents");
        }

        public readonly ResourceLocator Prefix;

        internal Texture2DArray TexArray { get; private set; }

        private TextureManager(params String[] filePrefix)
        {
            Prefix = filePrefix;

            var tileNames = Archive.FindAll<Bitmap>(filePrefix, true);
            TexArray = new Texture2DArray(8, 8, tileNames.OrderBy(x => x.ToString()).ToArray());
        }

        public ushort GetIndex(ResourceLocator namePrefix, params String[] nameSuffix)
        {
            var joined = new String[namePrefix.Length + nameSuffix.Length];
            Array.Copy(namePrefix, joined, namePrefix.Length);
            Array.Copy(nameSuffix, 0, joined, namePrefix.Length, nameSuffix.Length);
            return GetIndex(joined);
        }

        public ushort GetIndex(params String[] name)
        {
            if (name.Length == 0)
                return 0xffff;

            if (!name.Take(Prefix.Length).Zip(Prefix, (x, y) => x == y).All(x => x))
                name = Prefix.Concat(name).ToArray();

            return TexArray.GetTextureIndex(name);
        }
    }
}
