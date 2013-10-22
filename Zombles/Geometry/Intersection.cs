using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Zombles.Geometry
{
    public class Intersection
    {
        private Dictionary<Intersection, Vector2> _edges;

        public Vector2 Position { get; private set; }
        public int ID { get; private set; }

        public float X { get { return Position.X; } }
        public float Y { get { return Position.Y; } }

        public IEnumerable<KeyValuePair<Intersection, Vector2>> Edges
        {
            get { return _edges; }
        }

        public Intersection(Vector2 pos, int id)
        {
            Position = pos;
            ID = id;

            _edges = new Dictionary<Intersection, Vector2>();
        }

        public void Connect(Intersection other, Vector2 diff)
        {
            if (!_edges.ContainsKey(other)) {
                _edges.Add(other, diff);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Intersection && ((Intersection) obj).Position.Equals(Position);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        public override string ToString()
        {
            return Position.ToString();
        }
    }
}
