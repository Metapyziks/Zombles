using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zombles.Geometry.Generation
{
    public abstract class RoomGenerator : StructureGenerator
    {
        private Dictionary<Face, List<bool>> _entries;

        public RoomGenerator()
        {
            _entries = new Dictionary<Face, List<bool>>();

            foreach (var face in new[] { Face.North, Face.East, Face.South, Face.West }) {
                _entries.Add(face, new List<bool>());
            }
        }

        public void ClearEntries()
        {
            foreach (var face in new[] { Face.North, Face.East, Face.South, Face.West }) {
                _entries[face].Clear();
            }
        }

        public void AddEntry(Face face, int offset)
        {
            AddEntry(face, offset, 1);
        }

        public void AddEntry(Face face, int offset, int size)
        {
            var list = _entries[face];
            while (list.Count < offset + size) list.Add(false);

            for (int i = 0; i < size; ++ i) list[offset + i] = true;
        }

        public bool IsEntry(Face face, int offset)
        {
            var list = _entries[face];
            return list.Count > offset && list[offset];
        }
    }
}
