using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MineGame.World {
    public class Generator {
        public int[,] Tiles { get; protected set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int PixelSize { get; set; }

        public Generator(int width, int height, int pixelSize) {
            Width = width;
            Height = height;
            PixelSize = pixelSize;
            Tiles = new int[width, height];
        }

        public static int GetMap(Random rng, Point _, float value) {
            var reg = Game1.Instance.tileRegister;
            int id = reg.GetIdByName("wall");

            const int bands = 2;
            const float bandWidth = 0.15f;

            for (int i = 0; i < bands; i++) {
                float start = i / (float)bands;
                float end = start + bandWidth;
                if (value >= start && value < end) {
                    id = reg.GetIdByName(rng.Next() % 2 == 0 ? "floor1" : "floor2");
                    break;
                }
            }

            return id;
        }

        public void Generate() {
            Random seedRng = new();
            var seed = seedRng.Next();

            Random rng = new(seed);

            float[,] noise = GenerateNoiseMap(Width, Height, seed);

            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    float value = noise[x, y];
                    Tiles[x, y] = GetMap(rng, new(x, y), value);
                }
            }
        }

        private static float[,] GenerateNoiseMap(int width, int height, int seed) {
            float[,] noiseMap = new float[width, height];
            Random rng = new(seed);

            // Perlin noise parameters
            float scale = 10f;
            int octaves = 4;
            float persistence = 0.5f;
            float lacunarity = 2f;

            Vector2[] octaveOffsets = new Vector2[octaves];
            for (int i = 0; i < octaves; i++) {
                float offsetX = rng.Next(-100000, 100000);
                float offsetY = rng.Next(-100000, 100000);
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0f;

                    for (int i = 0; i < octaves; i++) {
                        float sampleX = (x / scale * frequency) + octaveOffsets[i].X;
                        float sampleY = (y / scale * frequency) + octaveOffsets[i].Y;

                        float perlinValue = PerlinNoise(sampleX, sampleY);
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    noiseMap[x, y] = noiseHeight;
                }
            }

            // Normalize to 0-1 range
            return NormalizeNoiseMap(noiseMap, width, height);
        }

        public static float[,] FillSmallGaps(float[,] noise, int width, int height, float threshold = 0.5f, int maxGap = 2) {
            float[,] result = (float[,])noise.Clone();

            // Horizontal pass
            for (int y = 0; y < height; y++) {
                int gapStart = -1;
                for (int x = 0; x < width; x++) {
                    if (noise[x, y] < threshold) {
                        if (gapStart == -1)
                            gapStart = x;
                    } else {
                        if (gapStart != -1) {
                            int gapLength = x - gapStart;
                            if (gapLength <= maxGap) {
                                for (int gx = gapStart; gx < x; gx++) {
                                    result[gx, y] = threshold; // fill the gap
                                }
                            }
                            gapStart = -1;
                        }
                    }
                }
            }

            // Vertical pass
            for (int x = 0; x < width; x++) {
                int gapStart = -1;
                for (int y = 0; y < height; y++) {
                    if (noise[x, y] < threshold) {
                        if (gapStart == -1)
                            gapStart = y;
                    } else {
                        if (gapStart != -1) {
                            int gapLength = y - gapStart;
                            if (gapLength <= maxGap) {
                                for (int gy = gapStart; gy < y; gy++) {
                                    result[x, gy] = threshold;
                                }
                            }
                            gapStart = -1;
                        }
                    }
                }
            }

            return result;
        }


        private static float PerlinNoise(float x, float y) {
            // Simplified Perlin-like noise using sine waves
            return (float)(Math.Sin(x * 0.1) * Math.Cos(y * 0.1) +
                           Math.Sin(x * 0.3 + 2) * Math.Cos(y * 0.3 + 2) * 0.5 +
                           Math.Sin(x * 0.7 + 5) * Math.Cos(y * 0.7 + 5) * 0.25) / 1.75f;
        }

        private static float[,] NormalizeNoiseMap(float[,] noiseMap, int width, int height) {
            float min = float.MaxValue;
            float max = float.MinValue;

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (noiseMap[x, y] < min) min = noiseMap[x, y];
                    if (noiseMap[x, y] > max) max = noiseMap[x, y];
                }
            }

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    noiseMap[x, y] = (noiseMap[x, y] - min) / (max - min);
                }
            }

            return noiseMap;
        }

        public int[,] GetTiles() {
            return Tiles;
        }

        public int GetTile(int x, int y) {
            if (x >= 0 && x < Width && y >= 0 && y < Height) {
                return Tiles[x, y];
            }
            return -1;
        }
    }
}
