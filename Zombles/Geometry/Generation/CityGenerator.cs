using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation
{
    public class CityGenerator
    {
        private Dictionary<BlockGenerator, double> myBlockTypes;
        private int myMaxLongSide;
        private int myMaxShortSide;

        public CityGenerator()
        {
            myBlockTypes = new Dictionary<BlockGenerator, double>();
            myMaxLongSide = 0;
            myMaxShortSide = 0;
        }

        public void AddBlockGenerator( BlockGenerator gen, double frequency = 1.0 )
        {
            myBlockTypes.Add( gen, frequency );
            if ( gen.MaxLongSide > myMaxLongSide )
                myMaxLongSide = gen.MaxLongSide;
            if ( gen.MaxShortSide > myMaxShortSide )
                myMaxShortSide = gen.MaxShortSide;
        }

        protected bool WillFit( int width, int height, bool acceptLarger = false )
        {
            foreach ( BlockGenerator gen in myBlockTypes.Keys )
                if ( myBlockTypes[ gen ] > 0.0 && gen.WillFit( width, height, acceptLarger ) )
                    return true;

            return false;
        }

        protected BlockGenerator GetRandomBlockGenerator( int width, int height, Random rand )
        {
            BlockGenerator cur = null;
            double freq = double.MaxValue;
            foreach ( BlockGenerator gen in myBlockTypes.Keys )
            {
                if ( gen.WillFit( width, height ) )
                {
                    double val = myBlockTypes[ gen ];
                    if ( val <= 0.0 )
                        val = double.MaxValue / 2.0;
                    else
                        val = rand.NextDouble() / val;

                    if ( val < freq )
                    {
                        cur = gen;
                        freq = val;
                    }
                }
            }

            return cur;
        }

        protected BlockGenerator[] GetRandomBlockGenerators( int width, int height, Random rand )
        {
            List<BlockGenerator> gens = new List<BlockGenerator>();
            foreach ( BlockGenerator gen in myBlockTypes.Keys )
                if ( gen.WillFit( width, height ) )
                    gens.Add( gen );

            return gens.OrderBy( x => myBlockTypes[ x ] <= 0.0
                ? double.MaxValue : rand.NextDouble() / myBlockTypes[ x ] ).ToArray();
        }

        public City Generate( int width, int height, int seed = 0 )
        {
            Random rand = ( seed == 0 ? new Random() : new Random( seed ) );
            return Generate( width, height, rand );
        }

        public City Generate( int width, int height, Random rand )
        {
            City city = new City( width, height );
            Subdivide( city.RootDistrict, 0, 1, 1, 1, 1, rand );
            city.UpdateVertexBuffer();
            return city;
        }

        private void Subdivide( District district, int depth,
            int borderLeft, int borderTop,
            int borderRight, int borderBottom, Random rand )
        {
            int nextBorder = depth < 4 ? 3 : depth < 6 ? 2 : 1;

            int minHorz = 0;
            bool fitHorz = false;
            int minVert = 0;
            bool fitVert = false;

            while ( ( minHorz + nextBorder ) * 2 + borderTop + borderBottom <= district.Height &&
                !( fitHorz = WillFit( district.Width - borderLeft - borderRight, minHorz, true ) ) )
                ++minHorz;

            while ( ( minVert + nextBorder ) * 2 + borderLeft + borderRight <= district.Width &&
                !( fitVert = WillFit( minVert, district.Height - borderTop - borderBottom, true ) ) )
                ++minVert;

            bool horz = fitHorz && ( !fitVert || rand.Next( district.Width * district.Width + district.Height * district.Height )
                >= district.Width * district.Width );

            if ( horz )
            {
                int min = borderTop + nextBorder + minHorz;
                int max = district.Height - borderBottom - minHorz - nextBorder;
                int mid = ( min + max ) / 2;
                district.Split( true, rand.Next( ( min + mid ) / 2, ( mid + max ) / 2 ) );
                Subdivide( district.ChildA, depth + 1, borderLeft, borderTop, borderRight, nextBorder, rand );
                Subdivide( district.ChildB, depth + 1, borderLeft, nextBorder, borderRight, borderBottom, rand );
            }
            else if( !horz && fitVert )
            {
                int min = minVert + borderLeft + nextBorder;
                int max = district.Width - borderRight - minVert - nextBorder;
                int mid = ( min + max ) / 2;
                district.Split( false, rand.Next( ( min + mid ) / 2, ( mid + max ) / 2 ) );
                Subdivide( district.ChildA, depth + 1, borderLeft, borderTop, nextBorder, borderBottom, rand );
                Subdivide( district.ChildB, depth + 1, nextBorder, borderTop, borderRight, borderBottom, rand );
            }
            else
            {
                BlockGenerator gen = GetRandomBlockGenerator( district.Width - borderLeft - borderRight,
                    district.Height - borderTop - borderBottom, rand );
                district.SetBlock( gen.Generate( district.X, district.Y, district.Width, district.Height,
                    borderLeft, borderTop, borderRight, borderBottom, rand ) );
            }
        }
    }
}
