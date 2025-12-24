#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Global constants (converted to HLSL float3)
float3 centerGlowColor = float3(0.27, 0.52, 0.42);

// GLSL fract() -> HLSL frac(); mix() -> lerp(); etc.
float rand(float2 co, float time)
{
    return frac(sin(dot(co, float2(12.9898, 78.233))) * 50000.5453123 + time);
}

float pseudoNoise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

float smoothNoise(float2 p, float timeOffset)
{
    float n1 = pseudoNoise(p);
    float n2 = pseudoNoise(p + float2(0.3, 0.5) + timeOffset);
    float n3 = pseudoNoise(p + float2(0.6, 0.2) - timeOffset);
    return (n1 + n2 + n3) / 3.0;
}

float2x2 rotationMatrix(float angle)
{
    float c = cos(angle);
    float s = sin(angle);
    return float2x2(c, -s, s, c);
}

// Shader parameters (to be set from C#)
float iTime;
float2 iResolution;
float RingSpacing;
float RotationSpeed;
float SpiralFactor;

// SpriteBatch texture and sampler
Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

// Main effect function (converted from GLSL calculate_color)
float3 calculate_color(float2 fragCoord)
{
    float pixelScale = 1920.0;
    float2 originalUv = (fragCoord - 0.5 * iResolution) / iResolution.y;

    float dist = length(originalUv);
    float spiralFactor = dist * SpiralFactor;
    float timeOffset = iTime * 0.5;

    float rotationSpeed = RotationSpeed;
    float rotationAngle = iTime * rotationSpeed + spiralFactor;
    float2x2 rotMatrix = rotationMatrix(rotationAngle);
    originalUv = mul(rotMatrix, originalUv);

    float liquidSpeed = 0.5;
    float liquidStrength = 0.1;

    float2 waveOffset = float2(
        sin(iTime * liquidSpeed + originalUv.y * 10.0 + smoothNoise(originalUv * 3.0, timeOffset) * 3.0),
        cos(iTime * liquidSpeed + originalUv.x * 10.0 + smoothNoise(originalUv * 3.0, timeOffset) * 3.0)
    ) * liquidStrength;

    float2 flowCoords = originalUv * 4.0 + waveOffset;
    float noiseX = smoothNoise(flowCoords, timeOffset * 0.3);
    float noiseY = smoothNoise(flowCoords + 50.0, timeOffset * 0.3);
    float2 flow = float2(noiseX, noiseY) * 2.0 - 1.0;

    float2 smudgedUv = originalUv + flow * 0.02;
    float2 uv = smudgedUv;
    float2 pixUv = floor(uv * pixelScale) / pixelScale;

    float r = length(pixUv);
    float a = atan2(pixUv.y, pixUv.x); // GLSL atan(y,x) -> HLSL atan2(y,x)

    float warble = 0.01 * sin(a * 10.0 + iTime * 0.5 + pseudoNoise(uv * 10.0) * 2.0);
    float distortedR = max(r + warble, 0.01);

    float s = RingSpacing;
    float spiral = fmod(log(distortedR) - iTime * 0.4, s) - 0.5 * s;
    float finalSpiral = spiral;

    float spiralMask = abs(finalSpiral * 4.0);
    float3 blendColor = float3(0.18, 0.37, 0.29);
    float3 colPattern = float3(0.23, 0.46, 0.36);
    float3 col = lerp(blendColor, colPattern, spiralMask);

    float glow = smoothstep(0.5, 0.0, length(originalUv) + 0.2);
    float centralLiquidMotion = 0.05 * sin(iTime * 1.5 + length(originalUv) * 20.0);
    float3 liquidEffect = lerp(float3(1.0, 1.0, 1.0), centerGlowColor, centralLiquidMotion);
    col = lerp(col, liquidEffect, glow);

    float flowSmudge = smoothstep(0.5, 0.3, cos(dot(flow, pixUv * 100.0)) * sin(length(flow) * 10.0));
    col = lerp(col, float3(0.13, 0.3, 0.35), flowSmudge * 0.3);

    float scanline = sin(fragCoord.y * 3.14159 / iResolution.y * pixelScale);
    col *= (0.95 + 0.05 * scanline);

    return col;
}

// The Pixel Shader: called for each pixel by MonoGame
float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Reconstruct pixel coordinates
    float2 fragCoord = input.TextureCoordinates * iResolution;
    float3 color = calculate_color(fragCoord);
    // Apply the sprite's vertex color (usually white for full effect)
    return float4(color, 1.0) * input.Color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
