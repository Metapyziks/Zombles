using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation
{
    public class CityGenerator
    {
        public City Generate( int width, int height, int seed = 0 )
        {
            Random rand = ( seed == 0 ? new Random() : new Random( seed ) );
            return Generate( width, height, rand );
        }

        public City Generate( int width, int height, Random rand )
        {
            City city = new City( width, height );
            Subdivide( city.RootDistrict, 0, 3, 3, 3, 3, rand );
            city.UpdateVertexBuffer();
            return city;
        }

        private void Subdivide( District district, int depth,
            int borderLeft, int borderTop,
            int borderRight, int borderBottom, Random rand,
            BlockGenerator target = null )
        {
            int nextBorder = depth < 4 ? 3 : depth < 6 ? 2 : 1;

            int width = district.Width - borderLeft - borderRight;
            int height = district.Height - borderTop - borderBottom;

            if( target == null )
                target = BlockGenerator.GetRandom( width, height, rand, true );

            if ( target.WillFit( width, height, false ) )
            {
                district.SetBlock( target.Generate( district.X, district.Y, district.Width, district.Height,
                    borderLeft, borderTop, borderRight, borderBottom, rand ) );
            }
            else
            {
                int minHL = target.MinLongSide;
                int maxHL = target.MaxLongSide;
                double fitHL = BlockGenerator.FitnessScore( width, height - maxHL - nextBorder * 2 );

                int minHS = target.MinShortSide;
                int maxHS = target.MaxShortSide;
                double fitHS = BlockGenerator.FitnessScore( width, height - maxHS - nextBorder * 2 );

                int minH, maxH;
                double fitH;

                if ( fitHL > fitHS || ( fitHL == fitHS && rand.NextDouble() < 0.5 ) )
                {
                    minH = minHL; maxH = maxHL; fitH = fitHL;
                }
                else
                {
                    minH = minHS; maxH = maxHS; fitH = fitHS;
                }

                int minVL = target.MinLongSide;
                int maxVL = target.MaxLongSide;
                double fitVL = BlockGenerator.FitnessScore( width - maxVL - nextBorder * 2, height );

                int minVS = target.MinShortSide;
                int maxVS = target.MaxShortSide;
                double fitVS = BlockGenerator.FitnessScore( width - maxVS - nextBorder * 2, height );

                int minV, maxV;
                double fitV;

                if ( fitVL > fitVS || ( fitVL == fitVS && rand.NextDouble() < 0.5 ) )
                {
                    minV = minVL; maxV = maxVL; fitV = fitVL;
                }
                else
                {
                    minV = minVS; maxV = maxVS; fitV = fitVS;
                }

                if ( fitH != fitV || fitH != 0.0 )
                {
                    bool splitSide = rand.NextDouble() < 0.5;

                    if ( fitH > fitV || ( fitH == fitV && rand.NextDouble() < 0.5 ) )
                    {
                        if( splitSide )
                            district.Split( true, borderTop + nextBorder + rand.Next( minH, maxH ) );
                        else
                            district.Split( true, district.Height - borderBottom - nextBorder - rand.Next( minH, maxH ) );

                        Subdivide( district.ChildA, depth + 1,
                            borderLeft, borderTop, borderRight, nextBorder,
                            rand, splitSide ? target : null );
                        Subdivide( district.ChildB, depth + 1,
                            borderLeft, nextBorder, borderRight, borderBottom,
                            rand, splitSide ? null : target );
                    }
                    else
                    {
                        if ( splitSide )
                            district.Split( false, borderLeft + nextBorder + rand.Next( minV, maxV ) );
                        else
                            district.Split( false, district.Width - borderRight - nextBorder - rand.Next( minV, maxV ) );

                        Subdivide( district.ChildA, depth + 1,
                            borderLeft, borderTop, nextBorder, borderBottom,
                            rand, splitSide ? target : null );
                        Subdivide( district.ChildB, depth + 1,
                            nextBorder, borderTop, borderRight, borderBottom,
                            rand, splitSide ? null : target );
                    }
                }
                else
                {
                    target = BlockGenerator.GetRandom( width, height, rand );
                    district.SetBlock( target.Generate( district.X, district.Y, district.Width, district.Height,
                        borderLeft, borderTop, borderRight, borderBottom, rand ) );
                }
            }
        }
    }
}
