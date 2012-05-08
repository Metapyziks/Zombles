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
            Subdivide( city.RootDistrict, rand );
            city.UpdateVertexBuffer();
            return city;
        }

        private void Subdivide( District district, Random rand )
        {
            bool horz = rand.Next( district.Width * district.Width + district.Height * district.Height )
                >= district.Width * district.Width;
            int min = 4;
            bool fit = false;

            while ( min * 2 < ( horz ? district.Height : district.Width ) - 1
                && !( fit = WillFit( ( horz ? district.Width : min ) - 4,
                    ( horz ? min : district.Height ) - 4, true ) ) )
                ++min;

            if ( !fit )
            {
                horz = !horz;

                min = 4;
                fit = false;

                while ( min * 2 < ( horz ? district.Height : district.Width ) - 1
                    && !( fit = WillFit( ( horz ? district.Width : min ) - 4,
                        ( horz ? min : district.Height ) - 4, true ) ) )
                    ++min;
            }

            if ( fit )
            {
                district.Split( horz, rand.Next( min, ( horz ? district.Height : district.Width ) - min ) );
                Subdivide( district.ChildA, rand );
                Subdivide( district.ChildB, rand );
            }
            else
            {
                BlockGenerator gen = GetRandomBlockGenerator( district.Width - 4, district.Height - 4, rand );
                district.SetBlock( gen.Generate( district.X, district.Y, district.Width, district.Height,
                    2, 2, 2, 2, rand ) );
            }
        }
    }
}
