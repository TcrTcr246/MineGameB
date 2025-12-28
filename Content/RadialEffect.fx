sampler2D TileMask : register(s0);
float TileSize;
float Softness;

float4 MainPS(float2 uv : TEXCOORD0) : COLOR0
{
    // Single-pass optimized blur with 5 samples instead of original approach
    float center = tex2D(TileMask, uv).r;
    
    // Sample 4 cardinal directions
    float blur = tex2D(TileMask, uv + float2(-TileSize, 0)).r
               + tex2D(TileMask, uv + float2(TileSize, 0)).r
               + tex2D(TileMask, uv + float2(0, -TileSize)).r
               + tex2D(TileMask, uv + float2(0, TileSize)).r;
    
    // Average with center
    blur = (blur + center) / 5.0;
    
    float light = saturate(blur * Softness);
    return float4(0, 0, 0, 1 - light);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}