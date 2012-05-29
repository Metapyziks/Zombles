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
        private District myCurDistrict;
        private IEnumerator<Entity> myEntEnumerator;
        private float myRange2;

        public readonly City City;
        public readonly Vector2 Center;
        public readonly float Range;

        public NearbyEntityEnumerator( City city, Vector2 center, float range )
        {
            City = city;
            Center = center;
            Range = range;

            myCurDistrict = null;
            myEntEnumerator = null;
            myRange2 = range * range;
        }

        public Entity Current
        {
            get { return myEntEnumerator.Current; }
        }

        public void Dispose()
        {
            myCurDistrict = null;
            myEntEnumerator = null;
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            while( true )
            {
                if ( myCurDistrict == null )
                    myCurDistrict = City.RootDistrict;
                else if ( myCurDistrict.IsLeaf )
                {
                    while ( myEntEnumerator.MoveNext() )
                        if ( InRange( myEntEnumerator.Current ) )
                            return true;

                    do
                    {
                        while ( !myCurDistrict.IsRoot && myCurDistrict == myCurDistrict.Parent.ChildB )
                            myCurDistrict = myCurDistrict.Parent;

                        if ( myCurDistrict.IsRoot )
                            return false;

                        myCurDistrict = myCurDistrict.Parent.ChildB;
                    }
                    while ( !InRange( myCurDistrict ) );
                }

                while ( myCurDistrict.IsBranch )
                {
                    if ( InRange( myCurDistrict.ChildA ) )
                        myCurDistrict = myCurDistrict.ChildA;
                    else
                        myCurDistrict = myCurDistrict.ChildB;
                }

                myEntEnumerator = myCurDistrict.Block.GetEnumerator();
            }
        }

        private bool InRange( District district )
        {
            float xdiff = 0.0f, ydiff = 0.0f;
            if ( Center.X < district.Bounds.Left )
                xdiff = district.Bounds.Left - Center.X;
            else if ( Center.X > district.Bounds.Right )
                xdiff = district.Bounds.Right - Center.X;
            if ( Center.Y < district.Bounds.Top )
                ydiff = district.Bounds.Top - Center.Y;
            else if ( Center.Y > district.Bounds.Bottom )
                ydiff = district.Bounds.Bottom - Center.Y;

            return xdiff * xdiff + ydiff * ydiff <= myRange2;
        }

        private bool InRange( Entity ent )
        {
            float xdiff = Center.X - ent.Position.X;
            float ydiff = Center.Y - ent.Position.Z;

            return xdiff * xdiff + ydiff * ydiff <= myRange2;
        }

        public void Reset()
        {
            myCurDistrict = null;
            myEntEnumerator = null;
        }
    }
}
