using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Geometry;

namespace Zombles.Entities
{
    public class NearbyEntityEnumerator : IEnumerator<Entity>
    {
        private District _curDistrict;
        private IEnumerator<Entity> _entEnumerator;
        private float _range2;

        private float _hWidth;
        private float _hHeight;

        public World City { get; private set; }
        public Vector2 Center { get; private set; }
        public float Range { get; private set; }

        public NearbyEntityEnumerator(World city, Vector2 center, float range)
        {
            City = city;
            Center = center;
            Range = range;

            _curDistrict = null;
            _entEnumerator = null;
            _range2 = range * range;

            _hWidth = City.Width / 2.0f;
            _hHeight = City.Height / 2.0f;
        }

        public Entity Current
        {
            get { return _entEnumerator.Current; }
        }

        public void Dispose()
        {
            _curDistrict = null;
            _entEnumerator = null;
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            while (true) {
                if (_curDistrict == null)
                    _curDistrict = City.RootDistrict;
                else if (_curDistrict.IsLeaf) {
                    while (_entEnumerator.MoveNext())
                        if (InRange(_entEnumerator.Current))
                            return true;

                    do {
                        while (!_curDistrict.IsRoot && _curDistrict == _curDistrict.Parent.ChildB)
                            _curDistrict = _curDistrict.Parent;

                        if (_curDistrict.IsRoot)
                            return false;

                        _curDistrict = _curDistrict.Parent.ChildB;
                    }
                    while (!InRange(_curDistrict));
                }

                while (_curDistrict.IsBranch) {
                    if (InRange(_curDistrict.ChildA))
                        _curDistrict = _curDistrict.ChildA;
                    else
                        _curDistrict = _curDistrict.ChildB;
                }

                _entEnumerator = _curDistrict.Block.GetEnumerator();
            }
        }

        private bool InRange(District district)
        {
            float xdiff = 0.0f, ydiff = 0.0f;
            if (Center.X < district.Bounds.Left)
                xdiff = Math.Min(district.Bounds.Left - Center.X, Center.X - district.Bounds.Right + City.Width);
            else if (Center.X > district.Bounds.Right)
                xdiff = Math.Min(Center.X - district.Bounds.Right, district.Bounds.Left - Center.X + City.Width);

            if (Center.Y < district.Bounds.Top)
                ydiff = Math.Min(district.Bounds.Top - Center.Y, Center.Y - district.Bounds.Bottom + City.Height);
            else if (Center.Y > district.Bounds.Bottom)
                ydiff = Math.Min(Center.Y - district.Bounds.Bottom, district.Bounds.Top - Center.Y + City.Height);

            return xdiff * xdiff + ydiff * ydiff <= _range2;
        }

        private bool InRange(Entity ent)
        {
            return City.Difference(Center, ent.Position2D).LengthSquared <= _range2;
        }

        public void Reset()
        {
            _curDistrict = null;
            _entEnumerator = null;
        }
    }
}
