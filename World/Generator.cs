using Microsoft.Xna.Framework;
using MineGameB.Misc;
using MineGameB.Scenes;
using MineGameB.World.Tiles;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MineGameB.World;

public class Generator {
    public int[,,] Tiles { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Depth { get; set; }
    public int PixelSize { get; set; }

    private static int[] permutation;

    public static event Action<float, string> OnProgressUpdate;
    public static event Action<float, string> OnProgressPhaseUpdate;

    public Generator(int width, int height, int pixelSize, int depth = 10) {
        Width = width;
        Height = height;
        Depth = depth;
        PixelSize = pixelSize;
        Tiles = new int[width, height, depth];
    }

    public float _scale = 50f;
    public int _octaves = 4;
    public float _persistence = .5f;
    public float _lacunarity = 2f;
    public int _bands = 2;
    public float _bandWidth = .15f;
    public int? Seed = null;
    static Random seedRng = new();

    public int GetSeed() {
        if (Seed == null) {
            throw new InvalidOperationException("Seed is not set.");
        }
        return (int)Seed;
    }

    public void SetSeed(int seed) {
        Seed = seed;
    }

    public int RandomSeed() {
        Seed = seedRng.Next();
        return (int)Seed;
    }

    private static int GetFloorTile(Random rng, string floor1, string floor2) {
        return GameScene.TileRegister.GetIdByName(rng.Next() % 2 == 0 ? floor1 : floor2);
    }

    private static bool ShouldPlaceWall(float value, int bands, float bandWidth) {
        for (int i = 0; i < bands; i++) {
            float start = i / (float)bands;
            float end = start + bandWidth;
            if (value >= start && value < end) {
                return false;
            }
        }
        return true;
    }

    public float Smoothstep(float edge0, float edge1, float x) {
        x = Math.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
        return x * x * (3f - 2f * x);
    }

    public Generator GenerateNoiseBasedTerrain(
        Func<float, int, int, Random, int> tileSelector,
        float scale = float.NaN,
        int octaves = int.MinValue,
        float persistence = float.NaN,
        float lacunarity = float.NaN,
        bool useEdgeFalloff = true,
        float falloffStart = 0f,
        float falloffEnd = 0.2f) {

        // Apply defaults
        scale = float.IsNaN(scale) ? _scale : scale;
        octaves = octaves == int.MinValue ? _octaves : octaves;
        persistence = float.IsNaN(persistence) ? _persistence : persistence;
        lacunarity = float.IsNaN(lacunarity) ? _lacunarity : lacunarity;

        var rng = new Random((int)Seed);
        InitializePermutation(rng);

        float[,] noise = GenerateNoiseMap(Width, Height, rng, scale, octaves, persistence, lacunarity);

        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                float value = noise[x, y];

                // Apply edge falloff if enabled
                if (useEdgeFalloff) {
                    float edgeDistX = Math.Min(x, Width - 1 - x) / (Width * 0.5f);
                    float edgeDistY = Math.Min(y, Height - 1 - y) / (Height * 0.5f);
                    float edgeDist = Math.Min(edgeDistX, edgeDistY);
                    float falloff = Smoothstep(falloffStart, falloffEnd, edgeDist);
                    value = falloff > 0 ? value / falloff : value;
                }

                // Call the selector function to determine tile ID
                int tileId = tileSelector(value, x, y, rng);

                // Determine which layer to place on (could be part of selector logic)
                Tiles[x, y, 0] = tileId;
            }
        }

        return this;
    }

    // Helper method for weighted random selection
    public int GetWeightedRandomVariant(Random rng, int[] priorities) {
        int totalWeight = 0;
        for (int i = 0; i < priorities.Length; i++) {
            totalWeight += priorities[i];
        }

        int rand = rng.Next(0, totalWeight);
        int cumulative = 0;

        for (int i = 0; i < priorities.Length; i++) {
            cumulative += priorities[i];
            if (rand < cumulative)
                return i;
        }

        return 0;
    }

    public static void ReportProgress(float progress, string message) {
        OnProgressUpdate?.Invoke(progress, message);
    }
    public static void ReportPhaseProgress(float progress, string message) {
        OnProgressPhaseUpdate?.Invoke(progress, message);
    }


    public async Task<int[,,]> GenerateWallAnd2FloorVariant(
        string wallName,
        string floor1,
        string floor2,
        float scale = float.NaN,
        int octaves = int.MinValue,
        float persistence = float.NaN,
        float lacunarity = float.NaN) {

        return await Task.Run(() => {
            var progress = OnProgressUpdate;

            ReportProgress(0.0f, "Initializing terrain generation...");

            scale = float.IsNaN(scale) ? _scale : scale;
            octaves = octaves == int.MinValue ? _octaves : octaves;
            persistence = float.IsNaN(persistence) ? _persistence : persistence;
            lacunarity = float.IsNaN(lacunarity) ? _lacunarity : lacunarity;
            int bands = _bands;
            float bandWidth = _bandWidth;

            ReportProgress(0.1f, "Parameters configured");

            // You'll need to modify GenerateNoiseBasedTerrainMultiLayer to accept progress
            return GenerateNoiseBasedTerrainMultiLayerWithProgress(
                (value, x, y, layer, rng) => {
                    if (layer == 0) {
                        return rng.Next(2) == 0
                            ? GameScene.TileRegister.GetIdByName(floor1)
                            : GameScene.TileRegister.GetIdByName(floor2);
                    } else if (layer == 1) {
                        if (ShouldPlaceWall(value, bands, bandWidth)) {
                            return GameScene.TileRegister.GetIdByName(wallName);
                        }
                        return 0;
                    }
                    return 0;
                },
                scale: scale,
                octaves: octaves,
                persistence: persistence,
                lacunarity: lacunarity,
                useEdgeFalloff: true,
                falloffStart: 0f,
                falloffEnd: 0.2f
            ).Tiles;
        });
    }

    public Generator GenerateNoiseBasedTerrainMultiLayerWithProgress(
        Func<float, int, int, int, Random, int> layeredTileSelector,
        float scale = float.NaN,
        int octaves = int.MinValue,
        float persistence = float.NaN,
        float lacunarity = float.NaN,
        bool useEdgeFalloff = true,
        float falloffStart = 0f,
        float falloffEnd = 0.2f) {

        scale = float.IsNaN(scale) ? _scale : scale;
        octaves = octaves == int.MinValue ? _octaves : octaves;
        persistence = float.IsNaN(persistence) ? _persistence : persistence;
        lacunarity = float.IsNaN(lacunarity) ? _lacunarity : lacunarity;

        ReportProgress(0.15f, "Initializing RNG and permutation table...");
        var rng = new Random((int)Seed);
        InitializePermutation(rng);

        ReportProgress(0.2f, "Generating noise map...");
        float[,] noise = GenerateNoiseMap(Width, Height, rng, scale, octaves, persistence, lacunarity);

        ReportProgress(0.4f, "Processing terrain tiles...");

        int totalTiles = Width * Height;
        int processedTiles = 0;

        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                float value = noise[x, y];

                if (useEdgeFalloff) {
                    float edgeDistX = Math.Min(x, Width - 1 - x) / (Width * 0.5f);
                    float edgeDistY = Math.Min(y, Height - 1 - y) / (Height * 0.5f);
                    float edgeDist = Math.Min(edgeDistX, edgeDistY);
                    float falloff = Smoothstep(falloffStart, falloffEnd, edgeDist);
                    value = falloff > 0 ? value / falloff : value;
                }

                // Generate each layer
                for (int layer = 0; layer < Depth; layer++) {
                    int tileId = layeredTileSelector(value, x, y, layer, rng);
                    Tiles[x, y, layer] = tileId;
                }

                processedTiles++;
                // Report progress every 5% of tiles
                if (processedTiles % (totalTiles / 20) == 0) {
                    var tyconConvert = TyconNumberConvertor.Convert;
                    float progressValue = 0.4f + (processedTiles / (float)totalTiles) * 0.55f;
                    ReportProgress(progressValue, $"Placing tiles... {tyconConvert(processedTiles)}/{tyconConvert(totalTiles)}");
                }
            }
        }

        ReportProgress(0.95f, "Finalizing terrain...");
        ReportProgress(1.0f, "Terrain generation complete!");

        return this;
    }

    public async Task<int[,,]> GenerateTopograficMapAsync() {
        return await Task.Run(() => {
            var rng = new Random(GetSeed());
            InitializePermutation(rng); // Initialize permutation array FIRST

            var TileRegister = GameScene.TileRegister;
            int[,,] tiles = new int[Width, Height, Depth];
            float scaleFactor = 1f;

            // Step 1: Generate elevation noise (0-50%)
            ReportProgress(0.0f, "Initializing elevation map...");
            var elevationNoise = GenerateNoiseMap(
                Width, Height, rng,
                scale: 240f * scaleFactor,
                octaves: 6,
                persistence: 0.5f * scaleFactor,
                lacunarity: 2.0f * scaleFactor);

            // Step 2: Process tiles (50-100%)
            ReportProgress(0.5f, "Placing tiles...");

            int totalTiles = Width * Height;
            int processedTiles = 0;

            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    float elevation = elevationNoise[x, y];

                    // Border detection
                    float distX = Math.Min(x, Width - 1 - x);
                    float distY = Math.Min(y, Height - 1 - y);
                    float dist = Math.Min(distX, distY);
                    float borderNoise = elevation * 3f;
                    bool isBorder = dist < (20f + borderNoise);

                    if (isBorder) {
                        tiles[x, y, 0] = TileRegister.GetIdByName("water");
                        tiles[x, y, 1] = TileRegister.GetIdByName("ultraRock");
                        goto _continue;
                    }

                    // Ground layer based on elevation
                    int groundId;
                    if (elevation < 0.3f) {
                        // Water (lowest)
                        groundId = TileRegister.GetIdByName("water");
                    } else if (elevation < 0.38f) {
                        // Beach/Sand
                        int v = GetWeightedRandomVariant(rng, [10, 10, 1, 1]);
                        groundId = TileRegister.GetIdByName("sandVar1") + v;
                    } else if (elevation < 0.55f) {
                        // Plains/Grass (low-mid elevation)
                        int v = GetWeightedRandomVariant(rng, [1, 1, 1, 40, 40, 5]);
                        groundId = TileRegister.GetIdByName("grassVar1") + v;
                    } else if (elevation < 0.70f) {
                        // Forest (mid-high elevation)
                        int v = GetWeightedRandomVariant(rng, [5, 5, 3]);
                        groundId = TileRegister.GetIdByName("forestVar1") + v;
                    } else if (elevation < 0.80f) {
                        // Mountain floor zone
                        groundId = TileRegister.GetIdByName("mountain_floor");
                    } else if (elevation < 0.90f) {
                        // High mountain floor zone
                        groundId = TileRegister.GetIdByName("highMountain_floor");
                    } else {
                        // Ultra high mountain floor zone
                        groundId = TileRegister.GetIdByName("ultraHighMountain_floor");
                    }

                    tiles[x, y, 0] = groundId;

                    // Mountain layer
                    int mountainId = 0;
                    if (elevation >= 0.75f && elevation < 0.85f) {
                        mountainId = TileRegister.GetIdByName("mountain");
                    } else if (elevation >= 0.85f && elevation < 0.93f) {
                        mountainId = TileRegister.GetIdByName("highMountain");
                    } else if (elevation >= 0.93f) {
                        mountainId = TileRegister.GetIdByName("ultraHighMountain");
                    }

                    tiles[x, y, 1] = mountainId;

                    _continue:
                    // Update progress smoothly
                    processedTiles++;
                    if (processedTiles % (totalTiles / 20) == 0) {
                        var tyconConvert = TyconNumberConvertor.Convert;
                        float progressValue = 0.5f + (processedTiles / (float)totalTiles) * 0.5f;
                        ReportProgress(progressValue, $"Placing tiles... {tyconConvert(processedTiles)}/{tyconConvert(totalTiles)}");
                    }
                }
            }

            // Final update
            ReportProgress(1.0f, "Terrain generation complete");

            return tiles;
        });
    }

    private static void InitializePermutation(Random rng) {
        permutation = new int[512];
        int[] p = new int[256];

        for (int i = 0; i < 256; i++) {
            p[i] = i;
        }

        for (int i = 255; i > 0; i--) {
            int j = rng.Next(i + 1);
            (p[j], p[i]) = (p[i], p[j]);
        }

        for (int i = 0; i < 512; i++) {
            permutation[i] = p[i % 256];
        }
    }

    public float[,] GenerateNoiseMap(int width, int height, Random rng,
        float scale, int octaves, float persistence, float lacunarity) {

        float[,] noiseMap = new float[width, height];

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

        return NormalizeNoiseMap(noiseMap, width, height);
    }

    private static float PerlinNoise(float x, float y) {
        int xi = (int)Math.Floor(x) & 255;
        int yi = (int)Math.Floor(y) & 255;

        float xf = x - (float)Math.Floor(x);
        float yf = y - (float)Math.Floor(y);

        float u = Fade(xf);
        float v = Fade(yf);

        int aa = permutation[permutation[xi] + yi];
        int ab = permutation[permutation[xi] + yi + 1];
        int ba = permutation[permutation[xi + 1] + yi];
        int bb = permutation[permutation[xi + 1] + yi + 1];

        float x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
        float x2 = Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);

        return Lerp(x1, x2, v);
    }

    private static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);

    private static float Lerp(float a, float b, float t) => MathHelper.Lerp(a, b, t);

    private static float Grad(int hash, float x, float y) {
        int h = hash & 7;
        float u = h < 4 ? x : y;
        float v = h < 4 ? y : x;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    public static float[,] FillSmallGaps(float[,] noise, int width, int height, float threshold = 0.5f, int maxGap = 2) {
        float[,] result = (float[,])noise.Clone();

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
                                result[gx, y] = threshold;
                            }
                        }
                        gapStart = -1;
                    }
                }
            }
        }

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

    private static float[,] NormalizeNoiseMap(float[,] noiseMap, int width, int height) {
        float min = float.MaxValue;
        float max = float.MinValue;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (noiseMap[x, y] < min)
                    min = noiseMap[x, y];
                if (noiseMap[x, y] > max)
                    max = noiseMap[x, y];
            }
        }

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                noiseMap[x, y] = (noiseMap[x, y] - min) / (max - min);
            }
        }

        return noiseMap;
    }

    public int[,,] GetTiles() {
        return Tiles;
    }

    public int GetTile(int x, int y, int layer = 0) {
        if (x >= 0 && x < Width && y >= 0 && y < Height && layer >= 0 && layer < Depth) {
            return Tiles[x, y, layer];
        }
        return -1;
    }

    public void FlatGenerate(int tileId, int layer = 0) {
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                Tiles[x, y, layer] = tileId;
            }
        }
    }

    public void FuncGenerate(Func<int, int, int> f, int layer = 0) {
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                Tiles[x, y, layer] = f(x, y);
            }
        }
    }
}
