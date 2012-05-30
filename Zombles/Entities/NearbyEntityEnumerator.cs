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

        private float myHWidth;
        private float myHHeight;

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

            myHWidth = City.Width / 2.0f;
            myHHeight = City.Height / 2.0f;
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
                xdiff = Math.Min( district.Bounds.Left - Center.X, Center.X - district.Bounds.Right + City.Width );
            else if ( Center.X > district.Bounds.Right )
                xdiff = Math.Min( Center.X - district.Bounds.Right, district.Bounds.Left - Center.X + City.Width );

            if ( Center.Y < district.Bounds.Top )
                ydiff = Math.Min( district.Bounds.Top - Center.Y, Center.Y - district.Bounds.Bottom + City.Height );
            else if ( Center.Y > district.Bounds.Bottom )
                ydiff = Math.Min( Center.Y - district.Bounds.Bottom, district.Bounds.Top - Center.Y + City.Height );

            return xdiff * xdiff + ydiff * ydiff <= myRange2;
        }

        private bool InRange( Entity ent )
        {
            float xdiff = ent.Position.X - Center.X;
            float ydiff = ent.Position.Z - Center.Y;

            if ( xdiff >= myHWidth )
                xdiff -= City.Width;
            else if ( xdiff < -myHWidth )
                xdiff += City.Width;

            if ( ydiff >= myHHeight )
                ydiff -= City.Height;
            else if ( ydiff < -myHHeight )
                ydiff += City.Height;

            return xdiff * xdiff + ydiff * ydiff <= myRange2;
        }

        public void Reset()
        {
            myCurDistrict = null;
            myEntEnumerator = null;
        }
    }
}
