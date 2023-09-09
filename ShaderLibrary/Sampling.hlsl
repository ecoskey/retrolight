#ifndef RETROLIGHT_SAMPLERS_INCLUDED
#define RETROLIGHT_SAMPLERS_INCLUDED

#include "Packages/net.cosc.retrolight/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"

#define POINT_SAMPLER sampler_PointClamp
#define BILINEAR_SAMPLER sampler_BilinearClamp

SAMPLER(sampler_BilinearClamp);

//#if defined(SHADER_STAGE_FRAGMENT)
float4 SampleTex2DSprite(TEXTURE2D(tex), float2 uv, float4 texRes) {
    //box filter size in texel units
    float2 boxSize = clamp(fwidth(uv) * texRes.xy, 1e-5, 1);
    //scale uv by texture size to get texel coordinate
    float2 tx = uv * texRes.xy - 0.5 * boxSize;
    //compute offset for pixel-sized box filter
    float2 txOffset = smoothstep(1 - boxSize, 1, frac(tx));
    //compute bilinear sample uv coordinates
    float2 newUV = (floor(tx) + 0.5 + txOffset) * texRes.zw;
    return SAMPLE_TEXTURE2D_GRAD(tex, BILINEAR_SAMPLER, newUV, ddx(uv), ddy(uv));
}
//#endif

float4 SampleTex2DSprite_Explicit(TEXTURE2D(tex), float2 uv, float2 texelSize, float4 ddxy_uv) {
    //box filter size in texel units
    float2 boxSize = clamp((abs(ddxy_uv.xy) + abs(ddxy_uv.zw))  * texelSize, 1e-5, 1);
    //scale uv by texture size to get texel coordinate
    float2 tx = uv * texelSize - 0.5 * boxSize;
    //compute offset for pixel-sized box filter
    float2 txOffset = smoothstep(1 - boxSize, 1, frac(tx));
    //compute bilinear sample uv coordinates
    float2 newUV = (floor(tx) + 0.5 + txOffset) * texelSize;
    return SAMPLE_TEXTURE2D_GRAD(tex, BILINEAR_SAMPLER, newUV, ddxy_uv.xy, ddxy_uv.zw);
}

#endif