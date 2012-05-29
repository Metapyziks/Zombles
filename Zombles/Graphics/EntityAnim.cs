using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ResourceLib;

namespace Zombles.Graphics
{
    public class EntityAnim
    {
        private static Dictionary<String, EntityAnim> stFound;

        private static void FindAll()
        {
            stFound = new Dictionary<string, EntityAnim>();

            foreach ( InfoObject obj in Info.GetAll( "anim" ) )
                stFound.Add( obj.Name, new EntityAnim( obj ) );
        }

        public static EntityAnim GetAnim( String name )
        {
            if ( stFound == null )
                FindAll();

            return stFound[ name ];
        }

        public readonly String Name;
        public readonly double Frequency;

        public readonly int FrameCount;
        public readonly bool IsDirectional;

        public readonly String[,] FrameNames;
        public readonly ushort[,] FrameIndices;

        private EntityAnim( InfoObject obj )
        {
            Name = obj.Name;

            FrameCount = (int) obj[ "frame count" ].AsInteger();
            Frequency = obj[ "frequency" ].AsDouble();

            IsDirectional = obj[ "directional" ].AsBoolean();

            if ( IsDirectional )
            {
                FrameNames = new string[ 4, FrameCount ];
                FrameIndices = new ushort[ 4, FrameCount ];

                InfoValue[] dirs = obj[ "frames" ].AsArray();

                for ( int i = 0; i < 4; ++i )
                {
                    InfoValue[] frames = dirs[ i ].AsArray();
                    for ( int f = 0; f < FrameCount; ++f )
                    {
                        FrameNames[ i, f ] = frames[ f ].AsString();
                        FrameIndices[ i, f ] = TextureManager.Ents.GetIndex( FrameNames[ i, f ] );
                    }
                }
            }
            else
            {
                FrameNames = new string[ 1, FrameCount ];
                FrameIndices = new ushort[ 1, FrameCount ];

                InfoValue[] frames = obj[ "frames" ].AsArray();
                for ( int f = 0; f < FrameCount; ++f )
                {
                    FrameNames[ 0, f ] = frames[ f ].AsString();
                    FrameIndices[ 0, f ] = TextureManager.Ents.GetIndex( FrameNames[ 0, f ] );
                }
            }
        }
    }
}
