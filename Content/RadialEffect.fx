sampler2D TileMask : register(s0);

float TileSize; // 1 / textureWidth
float Softness; // intensity

float4 MainPS(float2 uv : TEXCOORD0) : COLOR0
{
    // fixed 3x3 blur (1 tile radius)
    float light = 0.0;

    light += tex2D(TileMask, uv + float2(-TileSize, -TileSize)).r;
    light += tex2D(TileMask, uv + float2(0, -TileSize)).r;
    light += tex2D(TileMask, uv + float2(TileSize, -TileSize)).r;

    light += tex2D(TileMask, uv + float2(-TileSize, 0)).r;
    light += tex2D(TileMask, uv).r;
    light += tex2D(TileMask, uv + float2(TileSize, 0)).r;

    light += tex2D(TileMask, uv + float2(-TileSize, TileSize)).r;
    light += tex2D(TileMask, uv + float2(0, TileSize)).r;
    light += tex2D(TileMask, uv + float2(TileSize, TileSize)).r;

    light /= 9.0;
    light = saturate(light * Softness);

    return float4(0, 0, 0, 1 - light);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}
