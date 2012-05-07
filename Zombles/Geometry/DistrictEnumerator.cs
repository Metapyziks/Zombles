using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry
{
    public class DistrictEnumerator : IEnumerator<Block>
    {
        private District myRootDistrict;
        private District myCurDistrict;

        public Block Current
        {
            get { return myCurDistrict.Block; }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public DistrictEnumerator( District root )
        {
            myRootDistrict = root;
            myCurDistrict = null;
        }

        public void Dispose()
        {
            myCurDistrict = null;
            myRootDistrict = null;
        }

        public bool MoveNext()
        {
            if ( myCurDistrict == null )
                myCurDistrict = myRootDistrict;
            else if ( myCurDistrict.IsLeaf )
            {
                while ( !myCurDistrict.IsRoot && myCurDistrict == myCurDistrict.Parent.ChildB )
                    myCurDistrict = myCurDistrict.Parent;

                if ( myCurDistrict.IsRoot )
                    return false;

                myCurDistrict = myCurDistrict.Parent.ChildB;
            }

            while ( myCurDistrict.IsBranch )
                myCurDistrict = myCurDistrict.ChildA;

            return true;
        }

        public void Reset()
        {
            myCurDistrict = null;
        }
    }
}
