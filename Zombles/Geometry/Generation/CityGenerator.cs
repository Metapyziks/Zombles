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

        public void AddBlockGenerator( BlockGenerator gen, double frequency )
        {
            myBlockTypes.Add( gen, frequency );
            if ( gen.MaxLongSide > myMaxLongSide )
                myMaxLongSide = gen.MaxLongSide;
            if ( gen.MaxShortSide > myMaxShortSide )
                myMaxShortSide = gen.MaxShortSide;
        }

        protected bool WillFit( int width, int height )
        {
            foreach ( BlockGenerator gen in myBlockTypes.Keys )
                if ( gen.WillFit( width, height ) )
                    return true;

            return false;
        }

        protected BlockGenerator GetRandomBlockGenerator( int width, int height, Random rand )
        {
            BlockGenerator cur = null;
            double freq = double.MaxValue;
            foreach ( BlockGenerator gen in myBlockTypes.Keys )
            {
                if ( gen.WillFit( width, height ) && myBlockTypes[ gen ] > 0.0f )
                {
                    double val = rand.NextDouble() / myBlockTypes[ gen ];
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
                if ( gen.WillFit( width, height ) && myBlockTypes[ gen ] > 0.0f )
                    gens.Add( gen );

            return gens.OrderBy( x => rand.NextDouble() / myBlockTypes[ x ] ).ToArray();
        }

        public City Generate( int width, int height, int seed = 0 )
        {
            Random rand = ( seed == 0 ? new Random() : new Random( seed ) );
            return Generate( width, height, rand );
        }

        public City Generate( int width, int height, Random rand )
        {
            City city = new City( width, height );
            Subdivide( city.RootDistrict, rand );
            return city;
        }

        private void Subdivide( District district, Random rand )
        {
            bool horz = rand.Next( district.Width + district.Height ) >= district.Width;
            int min = 4;
            bool fit = false;

            while ( min <= district.Width / 2
                && !( fit = WillFit( horz ? district.Width : min, horz ? min : district.Height ) ) )
                ++min;

            if ( fit )
            {
                district.Split( horz, rand.Next( min, district.Width - min ) );
                Subdivide( district.ChildA, rand );
                Subdivide( district.ChildB, rand );
            }
            else
            {
                BlockGenerator gen = GetRandomBlockGenerator( district.Width, district.Height, rand );
                district.SetBlock( gen.Generate( district.X, district.Y, district.Width, district.Height, rand ) );
            }
        }
    }
}
