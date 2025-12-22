sampler2D TileMask;
float TileSize;
float Softness;

float4 MainPS(float2 uv : TEXCOORD0) : COLOR0
{
    // horizontal pass
    float h = tex2D(TileMask, uv + float2(-TileSize, 0)).r
            + tex2D(TileMask, uv).r
            + tex2D(TileMask, uv + float2(TileSize, 0)).r;
    h /= 3.0;

    // vertical pass
    float v = tex2D(TileMask, uv + float2(0, -TileSize)).r
            + h
            + tex2D(TileMask, uv + float2(0, TileSize)).r;
    v /= 3.0;

    float light = saturate(v * Softness);
    return float4(0, 0, 0, 1 - light);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}
