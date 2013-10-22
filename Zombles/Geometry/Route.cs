using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Zombles.Geometry
{
    public abstract class Route : IEnumerable<Vector2>
    {
        private class MacroRoute : Route
        {
            public MacroRoute(City city, Vector2 origin, Vector2 target)
                : base(city, origin, target) { }

            public override IEnumerator<Vector2> GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        private class MicroRoute : Route
        {
            public MicroRoute(City city, Vector2 origin, Vector2 target)
                : base(city, origin, target) { }
        }

        public static Route Find(City city, Vector2 origin, Vector2 dest)
        {
            return new MacroRoute(city, origin, dest);
        }

        protected City City { get; private set; }
        public Vector2 Origin { get; private set; }
        public Vector2 Target { get; private set; }

        protected Route(City city, Vector2 origin, Vector2 target)
        {
            City = city;
            Origin = origin;
            Target = target;
        }

        public abstract IEnumerator<Vector2> GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
