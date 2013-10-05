using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry
{
    public class DistrictEnumerator : IEnumerator<Block>
    {
        private District _rootDistrict;
        private District _curDistrict;

        public Block Current
        {
            get { return _curDistrict.Block; }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public DistrictEnumerator(District root)
        {
            _rootDistrict = root;
            _curDistrict = null;
        }

        public void Dispose()
        {
            _curDistrict = null;
            _rootDistrict = null;
        }

        public bool MoveNext()
        {
            if (_curDistrict == null) {
                _curDistrict = _rootDistrict;
            } else if (_curDistrict.IsLeaf) {
                while (!_curDistrict.IsRoot && _curDistrict == _curDistrict.Parent.ChildB)
                    _curDistrict = _curDistrict.Parent;

                if (_curDistrict.IsRoot)
                    return false;

                _curDistrict = _curDistrict.Parent.ChildB;
            }

            while (_curDistrict.IsBranch)
                _curDistrict = _curDistrict.ChildA;

            return true;
        }

        public void Reset()
        {
            _curDistrict = null;
        }
    }
}
