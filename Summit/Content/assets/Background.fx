#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Parameters (uniforms) matching the original shader’s externs:
float time;
float spin_time;
float4 colour1;
float4 colour2;
float4 colour3;
float contrast;
float spin_amount;
float2 screenSize;    // (screen width, height) – analogous to love_ScreenSize

// We assume the sprite’s texture and sampler are bound to register(s0).
Texture2D SpriteTexture;
SamplerState SpriteSampler;

// Pixel shader: MonoGame will supply the vertex color and texture UV via COLOR0 and TEXCOORD0.
// We compute a “screen_coords” in pixels by multiplying UV by screenSize.
float4 MainPS(float4 inColor : COLOR0, float2 texCoord : TEXCOORD0) : SV_Target
{
    // Compute screen coordinates (in pixels) assuming a full-screen sprite
    float2 screen_coords = texCoord * screenSize;

// ----- Replicate the Balatro effect logic -----
float pixel_size = length(screenSize) / 700.0;
float2 uv = (floor(screen_coords / pixel_size) * pixel_size - 0.5 * screenSize) / length(screenSize) - float2(0.12, 0.0);
float uv_len = length(uv);

// Apply a time-varying central swirl
float speed = (spin_time * 0.5 * 0.2) + 302.2;
float new_angle = atan2(uv.y, uv.x) + speed - 0.5 * 20.0 * (spin_amount * uv_len + (1.0 - spin_amount));
float2 mid = (screenSize / length(screenSize)) / 2.0;
uv = float2(uv_len * cos(new_angle) + mid.x, uv_len * sin(new_angle) + mid.y) - mid;

// Paint-like iterative warp
uv *= 30.0;
speed = time * 2.0;
float2 uv2 = float2(uv.x + uv.y, uv.x + uv.y);
[unroll(5)]
for (int i = 0; i < 5; i++)
{
    uv2 += sin(max(uv.x, uv.y)) + uv;
    uv += 0.5 * float2(cos(5.1123314 + 0.353 * uv2.y + speed * 0.131121),
                      sin(uv2.x - 0.113 * speed));
    uv -= (cos(uv.x + uv.y) - sin(uv.x * 0.711 - uv.y));
}

// Contrast modulation and color mixing (range 0–2)
float contrast_mod = (0.25 * contrast + 0.5 * spin_amount + 1.2);
float paint_res = min(2.0, max(0.0, length(uv) * 0.035 * contrast_mod));
float c1p = max(0.0, 1.0 - contrast_mod * abs(1.0 - paint_res));
float c2p = max(0.0, 1.0 - contrast_mod * abs(paint_res));
float c3p = 1.0 - min(1.0, c1p + c2p);

float4 result = (0.3 / contrast) * colour1
    + (1.0 - 0.3 / contrast) * (colour1 * c1p + colour2 * c2p + float4(c3p * colour3.rgb, c3p * colour1.a));

// Multiply by the sprite’s vertex color (tint)
return result * inColor;
}

technique BasicEffect
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
