using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry
{
    public class City
    {
        public District RootDistrict { get; private set; }

        public City( int width, int height )
        {
            RootDistrict = new District( 0, 0, width, height );
        }
    }
}
