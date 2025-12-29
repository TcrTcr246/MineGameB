using Microsoft.Xna.Framework;
using MineGameB.Scenes;
using MineGameB.World.Tiles;
using System;

namespace MineGameB.World;
public class Generator {
    public int[,,] Tiles { get; protected set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Depth { get; set; }
    public int PixelSize { get; set; }

    private static int[] permutation;

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

    // Multi-layer version
    public Generator GenerateNoiseBasedTerrainMultiLayer(
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

        var rng = new Random((int)Seed);
        InitializePermutation(rng);

        float[,] noise = GenerateNoiseMap(Width, Height, rng, scale, octaves, persistence, lacunarity);

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

    public Generator GenerateWallAnd2FloorVariant(string wallName, string floor1, string floor2,
    float scale = float.NaN, int octaves = int.MinValue, float persistence = float.NaN, float lacunarity = float.NaN) {
        scale = float.IsNaN(scale) ? _scale : scale;
        octaves = octaves == int.MinValue ? _octaves : octaves;
        persistence = float.IsNaN(persistence) ? _persistence : persistence;
        lacunarity = float.IsNaN(lacunarity) ? _lacunarity : lacunarity;

        int bands = _bands;
        float bandWidth = _bandWidth;

        return GenerateNoiseBasedTerrainMultiLayer(
            (value, x, y, layer, rng) => {
                if (layer == 0) {
                    // Floor layer - randomly choose between floor1 and floor2
                    return rng.Next(2) == 0
                        ? GameScene.TileRegister.GetIdByName(floor1)
                        : GameScene.TileRegister.GetIdByName(floor2);
                } else if (layer == 1) {
                    // Wall layer - place wall based on noise bands
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
        );
    }

    public int[,,] GenerateTopograficMap() {
        var rng = new Random(GetSeed());
        var TileRegister = GameScene.TileRegister;

        // Initialize the 3D array with proper dimensions
        int[,,] tiles = new int[Width, Height, Depth];

        // Generate two different noise maps
        var elevationNoise = GenerateNoiseMap(
            Width, Height, rng,
            scale: 240f, octaves: 6, persistence: 0.5f, lacunarity: 2.0f);
        var moistureNoise = GenerateNoiseMap(
            Width, Height, new Random(rng.Next()),
            scale: 180f, octaves: 4, persistence: 0.4f, lacunarity: 2.5f);

        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                float elevation = elevationNoise[x, y];
                float moisture = moistureNoise[x, y];

                // Apply edge falloff to elevation
                float edgeDistX = Math.Min(x, Width - 1 - x) / (Width * 0.5f);
                float edgeDistY = Math.Min(y, Height - 1 - y) / (Height * 0.5f);
                float edgeDist = Math.Min(edgeDistX, edgeDistY);
                float falloff = Smoothstep(0f, 0.15f, edgeDist);
                elevation = falloff > 0 ? elevation / falloff : elevation;

                int baseTileId = 0;
                int topTileId = 0;

                // Water
                if (elevation < 0.3f) {
                    baseTileId = TileRegister.GetIdByName("water");
                }
                // Beach/Sand
                else if (elevation < 0.38f) {
                    int[] priorities = { 10, 10, 1, 1 };
                    int variant = GetWeightedRandomVariant(rng, priorities);
                    baseTileId = TileRegister.GetIdByName("sandVar1") + variant;
                }
                // Land biomes based on elevation and moisture
                else if (elevation < 0.75f) {
                    // Desert (low moisture)
                    if (moisture < 0.3f) {
                        int[] priorities = { 5, 5 };
                        int variant = GetWeightedRandomVariant(rng, priorities);
                        baseTileId = TileRegister.GetIdByName("sandVar1") + variant;
                    }
                    // Forest (high moisture) - increased priority
                    else if (moisture > 0.5f) {
                        int[] priorities = { 5, 5, 3 };
                        int variant = GetWeightedRandomVariant(rng, priorities);
                        baseTileId = TileRegister.GetIdByName("forestVar1") + variant;
                    }
                    // Grassland (medium moisture) - lower priority
                    else {
                        int[] priorities = { 7, 7, 7, 1 };
                        int variant = GetWeightedRandomVariant(rng, priorities);
                        baseTileId = TileRegister.GetIdByName("grassVar1") + variant;
                    }
                }
                // Mountains - with underlying land
                else if (elevation < 0.9f) {
                    // Base layer: determine land type based on moisture
                    if (moisture > 0.5f) {
                        int[] priorities = { 5, 5, 3 };
                        int variant = GetWeightedRandomVariant(rng, priorities);
                        baseTileId = TileRegister.GetIdByName("forestVar1") + variant;
                    } else {
                        int[] priorities = { 7, 7, 7, 1 };
                        int variant = GetWeightedRandomVariant(rng, priorities);
                        baseTileId = TileRegister.GetIdByName("grassVar1") + variant;
                    }
                    // Top layer: mountain
                    topTileId = TileRegister.GetIdByName("mountain");
                }
                // High mountains - with mountain underneath
                else {
                    // Base layer: determine land type based on moisture
                    if (moisture > 0.5f) {
                        int[] priorities = { 5, 5, 3 };
                        int variant = GetWeightedRandomVariant(rng, priorities);
                        baseTileId = TileRegister.GetIdByName("forestVar1") + variant;
                    } else {
                        int[] priorities = { 7, 7, 7, 1 };
                        int variant = GetWeightedRandomVariant(rng, priorities);
                        baseTileId = TileRegister.GetIdByName("grassVar1") + variant;
                    }
                    // Layer 1: mountain
                    tiles[x, y, 1] = TileRegister.GetIdByName("mountain");
                    // Layer 2: high mountain
                    topTileId = TileRegister.GetIdByName("highMountain");
                    tiles[x, y, 2] = topTileId;
                }

                tiles[x, y, 0] = baseTileId;
                if (topTileId != 0 && elevation < 0.9f) {
                    tiles[x, y, 1] = topTileId;
                }
            }
        }
        return tiles;
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
