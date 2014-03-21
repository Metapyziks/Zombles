using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

using OpenTK;

using Zombles.Geometry;
using ResourceLibrary;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Zombles
{
    public static class Tools
    {
        public static readonly Vector3 Up = new Vector3(0f, 1f, 0f);

        public static readonly Random Random = new Random(0xb6ba069);

        [ResourceTypeRegistration]
        public static void RegisterResourceTypes()
        {
            Archive.Register<JObject>(ResourceFormat.Compressed, SaveJObject, LoadJObject,
                ".json");
        }

        public static void SaveJObject(Stream stream, JObject resource)
        {
            var writer = new JsonTextWriter(new StreamWriter(stream));
            resource.WriteTo(writer);
            writer.Flush();
        }

        public static JObject LoadJObject(Stream stream)
        {
            var reader = new JsonTextReader(new StreamReader(stream));
            return JObject.Load(reader);
        }

        public static bool DoesExtend(this Type self, Type type)
        {
            return self.BaseType == type || (self.BaseType != null && self.BaseType.DoesExtend(type));
        }

        public static byte[] ReadBytes(this Stream self, int count)
        {
            byte[] data = new byte[count];
            for (int i = 0; i < count; ++i) {
                int bt = self.ReadByte();
                if (bt == -1)
                    throw new EndOfStreamException();

                data[i] = (byte) bt;
            }

            return data;
        }

        #region Clamps
        public static Byte Clamp(Byte value, Byte min, Byte max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static UInt16 Clamp(UInt16 value, UInt16 min, UInt16 max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static UInt32 Clamp(UInt32 value, UInt32 min, UInt32 max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static UInt64 Clamp(UInt64 value, UInt64 min, UInt64 max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static SByte Clamp(SByte value, SByte min, SByte max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static Int16 Clamp(Int16 value, Int16 min, Int16 max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static Int32 Clamp(Int32 value, Int32 min, Int32 max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static Int64 Clamp(Int64 value, Int64 min, Int64 max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static Single Clamp(Single value, Single min, Single max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static Double Clamp(Double value, Double min, Double max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }
        #endregion Clamps

        #region MinMax
        public static int Min(params int[] values)
        {
            int min = values[0];
            foreach (int val in values)
                if (val < min)
                    min = val;

            return min;
        }

        public static double Min(params double[] values)
        {
            double min = values[0];
            foreach (double val in values)
                if (val < min)
                    min = val;

            return min;
        }

        public static int Max(params int[] values)
        {
            int max = values[0];
            foreach (int val in values)
                if (val > max)
                    max = val;

            return max;
        }

        public static double Max(params double[] values)
        {
            double max = values[0];
            foreach (double val in values)
                if (val > max)
                    max = val;

            return max;
        }
        #endregion MinMax

        public static int FloorDiv(int numer, int denom)
        {
            return (numer / denom) - (numer < 0 && (numer % denom) != 0 ? 1 : 0);
        }

        public static String ApplyWordWrap(this String text, float charWidth, float wrapWidth)
        {
            if (wrapWidth <= 0.0f)
                return text;

            String newText = "";
            int charsPerLine = (int) (wrapWidth / charWidth);
            int x = 0, i = 0;
            while (i < text.Length) {
                String word = "";
                while (i < text.Length && !char.IsWhiteSpace(text[i]))
                    word += text[i++];

                if (x + word.Length > charsPerLine) {
                    if (x == 0) {
                        newText += word.Substring(0, charsPerLine) + "\n" + word.Substring(charsPerLine);
                        x = word.Length - charsPerLine;
                    } else {
                        newText += "\n" + word;
                        x = word.Length;
                    }
                } else {
                    newText += word;
                    x += word.Length;
                }

                if (i < text.Length) {
                    newText += text[i];
                    x++;

                    if (text[i++] == '\n')
                        x = 0;
                }
            }

            return newText;
        }

        public static int QuickLog2(int value)
        {
            int i = 0;
            while ((value >>= 1) != 0)
                ++i;

            return i;
        }

        public static float WrapAngle(float ang)
        {
            return ang - (float) Math.Floor(ang / (MathHelper.TwoPi) + 0.5f) * MathHelper.TwoPi;
        }

        public static float WrapAngle(float ang, float basis)
        {
            return WrapAngle(ang - basis) + basis;
        }

        public static float AngleDif(float angA, float angB)
        {
            return WrapAngle(angA - angB);
        }

        public static int GetIndex(this Geometry.Face face)
        {
            switch (face) {
                case Geometry.Face.West:
                    return 0;
                case Geometry.Face.North:
                    return 1;
                case Geometry.Face.East:
                    return 2;
                case Geometry.Face.South:
                    return 3;
                default:
                    return -1;
            }
        }

        public static Geometry.Face GetOpposite(this Geometry.Face face)
        {
            switch (face) {
                case Geometry.Face.West:
                    return Geometry.Face.East;
                case Geometry.Face.North:
                    return Geometry.Face.South;
                case Geometry.Face.East:
                    return Geometry.Face.West;
                case Geometry.Face.South:
                    return Geometry.Face.North;
                default:
                    return Geometry.Face.None;
            }
        }

        public static Vector2 GetNormal(this Geometry.Face face)
        {
            return new Vector2(face.GetNormalX(), face.GetNormalY());
        }

        public static int GetNormalX(this Geometry.Face face)
        {
            switch (face) {
                case Geometry.Face.West:
                    return -1;
                case Geometry.Face.East:
                    return 1;
                default:
                    return 0;
            }
        }

        public static int GetNormalY(this Geometry.Face face)
        {
            switch (face) {
                case Geometry.Face.North:
                    return -1;
                case Geometry.Face.South:
                    return 1;
                default:
                    return 0;
            }
        }

        public static float NextSingle(this Random rand)
        {
            return (float) rand.NextDouble();
        }

        public static float NextSingle(this Random rand, float min, float max)
        {
            return (float) (rand.NextDouble() * (max - min) + min);
        }

        public static double NextDouble(this Random rand, double min, double max)
        {
            return rand.NextDouble() * (max - min) + min;
        }

        public static ResourceLocator NextTexture(this Random rand, ResourceLocator prefix, int max)
        {
            return prefix[rand.Next(max).ToString("X").ToLower()];
        }

        public static ResourceLocator NextTexture(this Random rand, ResourceLocator prefix, int min, int max)
        {
            return prefix[rand.Next(min, max).ToString("X").ToLower()];
        }

        // I wish I could think of a better way of doing this
        public static Face NextFace(this Random rand, Face mask = Face.All)
        {
            if (mask == Face.None)
                return Face.None;

            List<Face> valid = new List<Face>();
            if ((mask & Face.West) != 0)
                valid.Add(Face.West);
            if ((mask & Face.North) != 0)
                valid.Add(Face.North);
            if ((mask & Face.East) != 0)
                valid.Add(Face.East);
            if ((mask & Face.South) != 0)
                valid.Add(Face.South);

            return valid[rand.Next(valid.Count)];
        }

        public static bool HasAttribute<T>(this Type type, bool inherit)
            where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).Length > 0;
        }

        public static T GetAttribute<T>(this Type type, bool inherit)
            where T : Attribute
        {
            return (T) type.GetCustomAttributes(typeof(T), inherit)[0];
        }

        public static T[] GetAttributes<T>(this Type type, bool inherit)
            where T : Attribute
        {
            return (T[]) type.GetCustomAttributes(typeof(T), inherit);
        }
    }
}
