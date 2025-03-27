//
// Perlin noise generator for Unity
// Keijiro Takahashi, 2013, 2015
// https://github.com/keijiro/PerlinNoise
//
// Based on the original implementation by Ken Perlin
// http://mrl.nyu.edu/~perlin/noise/
//

using UnityEngine;

namespace Rector
{
    public static class Noise
    {
        public static Vector3 CurlNoise(float x, float y, float z)
        {
            const float e = 0.01f;
            const float divisor = 1.0f / (255 * e);

            var a = ValueNoiseVector(x, y, z);
            var dadx = (ValueNoiseVector(x + e, y, z) - a) * divisor;
            var dady = (ValueNoiseVector(x, y + e, z) - a) * divisor;
            var dadz = (ValueNoiseVector(x, y, z + e) - a) * divisor;

            return new Vector3(dady.z - dadz.y, dadz.x - dadx.z, dadx.y - dady.x);
        }


        public static float ValueNoise(float x)
        {
            var i = Mathf.FloorToInt(x) & 0xff;
            x -= Mathf.Floor(x);
            var u = Fade(x);
            return Lerp(u, Perm[i], Perm[i + 1]);
        }

        public static float ValueNoise(float x, float y)
        {
            var i = Mathf.FloorToInt(x) & 0xff;
            var j = Mathf.FloorToInt(y) & 0xff;
            x -= Mathf.Floor(x);
            y -= Mathf.Floor(y);
            var u = Fade(x);
            var v = Fade(y);
            var a = (Perm[i] + j) & 0xff;
            var b = (Perm[i + 1] + j) & 0xff;
            return Lerp(v, Lerp(u, Perm[a], Perm[a + 1]), Lerp(u, Perm[b], Perm[b + 1]));
        }

        public static float ValueNoise(Vector2 coord)
        {
            return ValueNoise(coord.x, coord.y);
        }

        public static Vector3 ValueNoiseVector(float x, float y, float z)
        {
            return new Vector3(ValueNoise(x, y, z), ValueNoise(y + 100, z + 200, x), ValueNoise(z + 200, x + 50, y));
        }

        public static float ValueNoise(float x, float y, float z)
        {
            var i = Mathf.FloorToInt(x) & 0xff;
            var j = Mathf.FloorToInt(y) & 0xff;
            var k = Mathf.FloorToInt(z) & 0xff;
            x -= Mathf.Floor(x);
            y -= Mathf.Floor(y);
            z -= Mathf.Floor(z);
            var u = Fade(x);
            var v = Fade(y);
            var w = Fade(z);
            var a = (Perm[i] + j) & 0xff;
            var b = (Perm[i + 1] + j) & 0xff;
            var aa = (Perm[a] + k) & 0xff;
            var ba = (Perm[b] + k) & 0xff;
            var ab = (Perm[a + 1] + k) & 0xff;
            var bb = (Perm[b + 1] + k) & 0xff;
            return Lerp(w,
                Lerp(v,
                    Lerp(u, Perm[aa], Perm[ba]),
                    Lerp(u, Perm[ab], Perm[bb])),
                Lerp(v,
                    Lerp(u, Perm[aa + 1], Perm[ba + 1]),
                    Lerp(u, Perm[ab + 1], Perm[bb + 1])));
        }

        public static float ValueNoise(Vector3 coord)
        {
            return ValueNoise(coord.x, coord.y, coord.z);
        }

        public static float PerlinNoise(float x)
        {
            var i = Mathf.FloorToInt(x) & 0xff;
            x -= Mathf.Floor(x);
            var u = Fade(x);
            return Lerp(u, Grad(Perm[i], x), Grad(Perm[i + 1], x - 1)) * 2;
        }

        public static float PerlinNoise(float x, float y)
        {
            var i = Mathf.FloorToInt(x) & 0xff;
            var j = Mathf.FloorToInt(y) & 0xff;
            x -= Mathf.Floor(x);
            y -= Mathf.Floor(y);
            var u = Fade(x);
            var v = Fade(y);
            var a = (Perm[i] + j) & 0xff;
            var b = (Perm[i + 1] + j) & 0xff;
            return Lerp(v, Lerp(u, Grad(Perm[a], x, y), Grad(Perm[b], x - 1, y)),
                Lerp(u, Grad(Perm[a + 1], x, y - 1), Grad(Perm[b + 1], x - 1, y - 1)));
        }

        public static float PerlinNoise(Vector2 coord)
        {
            return PerlinNoise(coord.x, coord.y);
        }

        public static float PerlinNoise(float x, float y, float z)
        {
            var i = Mathf.FloorToInt(x) & 0xff;
            var j = Mathf.FloorToInt(y) & 0xff;
            var k = Mathf.FloorToInt(z) & 0xff;
            x -= Mathf.Floor(x);
            y -= Mathf.Floor(y);
            z -= Mathf.Floor(z);
            var u = Fade(x);
            var v = Fade(y);
            var w = Fade(z);
            var a = (Perm[i] + j) & 0xff;
            var b = (Perm[i + 1] + j) & 0xff;
            var aa = (Perm[a] + k) & 0xff;
            var ba = (Perm[b] + k) & 0xff;
            var ab = (Perm[a + 1] + k) & 0xff;
            var bb = (Perm[b + 1] + k) & 0xff;
            return Lerp(w, Lerp(v, Lerp(u, Grad(Perm[aa], x, y, z), Grad(Perm[ba], x - 1, y, z)),
                    Lerp(u, Grad(Perm[ab], x, y - 1, z), Grad(Perm[bb], x - 1, y - 1, z))),
                Lerp(v, Lerp(u, Grad(Perm[aa + 1], x, y, z - 1), Grad(Perm[ba + 1], x - 1, y, z - 1)),
                    Lerp(u, Grad(Perm[ab + 1], x, y - 1, z - 1), Grad(Perm[bb + 1], x - 1, y - 1, z - 1))));
        }

        public static float PerlinNoise(Vector3 coord)
        {
            return PerlinNoise(coord.x, coord.y, coord.z);
        }

        public static float Fbm(float x, int octave)
        {
            var f = 0.0f;
            var w = 0.5f;
            for (var i = 0; i < octave; i++)
            {
                f += w * PerlinNoise(x);
                x *= 2.0f;
                w *= 0.5f;
            }
            return f;
        }

        public static float Fbm(Vector2 coord, int octave)
        {
            var f = 0.0f;
            var w = 0.5f;
            for (var i = 0; i < octave; i++)
            {
                f += w * PerlinNoise(coord);
                coord *= 2.0f;
                w *= 0.5f;
            }
            return f;
        }

        public static float Fbm(float x, float y, int octave)
        {
            return Fbm(new Vector2(x, y), octave);
        }

        public static float Fbm(Vector3 coord, int octave)
        {
            var f = 0.0f;
            var w = 0.5f;
            for (var i = 0; i < octave; i++)
            {
                f += w * PerlinNoise(coord);
                coord *= 2.0f;
                w *= 0.5f;
            }
            return f;
        }

        public static float Fbm(float x, float y, float z, int octave)
        {
            return Fbm(new Vector3(x, y, z), octave);
        }

        static float Fade(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        static float Lerp(float t, float a, float b)
        {
            return a + t * (b - a);
        }

        static float Grad(int hash, float x)
        {
            return (hash & 1) == 0 ? x : -x;
        }

        static float Grad(int hash, float x, float y)
        {
            return ((hash & 1) == 0 ? x : -x) + ((hash & 2) == 0 ? y : -y);
        }

        static float Grad(int hash, float x, float y, float z)
        {
            var h = hash & 15;
            var u = h < 8 ? x : y;
            var v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        static readonly int[] Perm =
        {
            151, 160, 137, 91, 90, 15,
            131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23,
            190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33,
            88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166,
            77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244,
            102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
            135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123,
            5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42,
            223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
            129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228,
            251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107,
            49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254,
            138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
            151
        };
    }
}
