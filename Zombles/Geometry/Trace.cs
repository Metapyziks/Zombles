using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Entities;

namespace Zombles.Geometry
{
    public class Trace
    {
        public Vector2 Start { get; set; }
        public Vector2 End { get; set; }

        public bool HitEntities { get; set; }
        public bool HitGeometry { get; set; }

        public Predicate<Entity> HitEntityPredicate { get; set; }

        public Vector2 Vector
        {
            get { return End - Start; }
            set
            {
                End = Start + value;
            }
        }

        public Vector2 Normal
        {
            get
            {
                return Vector / Length;
            }
            set
            {
                Vector = value * Length;
            }
        }

        public float Length
        {
            get
            {
                return Vector.Length;
            }
            set
            {
                Vector = Normal * value;
            }
        }

        public TraceResult GetResult()
        {
            return null;
        }
    }

    public class TraceResult
    {
        public Vector2 Start;
        public Vector2 End;

        public float Length;

        public bool HitEntity;
        public bool HitWorld;

        public Face Face;
        public Entity Entity;
    }
}
