using System;
using System.Linq;
using System.Collections.Generic;

using ResourceLibrary;

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

        public readonly String[] Prefix;

        internal Texture2DArray TexArray { get; private set; }

        private static void DiscoverImages(IEnumerable<String> locator, List<String[]> tileNames)
        {
            var locatorArr = locator.ToArray();
            foreach (var name in Archive.GetAllNames<ScriptFile>(locator)) {
                tileNames.Add(locator.Concat(new String[] { name }).ToArray());
            }

            foreach (var name in Archive.GetAllNames<ScriptFile>(locator)) {
                DiscoverImages(locator.Concat(new String[] { name }), tileNames);
            }
        }

        private TextureManager(params String[] filePrefix)
        {
            Prefix = filePrefix;
            
            var tileNames = new List<String[]>();
            DiscoverImages(filePrefix, tileNames);

            tileNames.Sort();

            TexArray = new Texture2DArray(8, 8, tileNames.ToArray());
        }

        public ushort GetIndex(String[] namePrefix, params String[] nameSuffix)
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
