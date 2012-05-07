using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zombles.Graphics;

namespace Zombles.Geometry
{
    public class City
    {
        public District RootDistrict { get; private set; }

        public City( int width, int height )
        {
            RootDistrict = new District( 0, 0, width, height );
        }

        public void Render( GeometryShader shader, bool baseOnly = false )
        {
            RootDistrict.Render( shader, baseOnly );
        }
    }
}
