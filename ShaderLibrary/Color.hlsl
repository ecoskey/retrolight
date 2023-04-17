#ifndef RETROLIGHT_COLOR_INCLUDED
#define RETROLIGHT_COLOR_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

struct SplitToningSettings {
    float3 shadows;
    float3 highlights;
    float balance;
};

struct ChannelMixerSettings {
    float3 red;
    float3 green;
    float3 blue;
};

struct SmhSettings {
    float3 shadows;
    float3 midtones;
    float3 highlights;
    float shadowsStart;
    float shadowsEnd;
    float highlightsStart;
    float highlightsEnd;
};

struct PostProcessingSettings {
    float exposure;
    float3 filter;
    float hueShift;
    float saturation;
    SplitToningSettings splitToning;
};

float3 TonemapReinhard(float3 color) {
    color = min(color, 60);
    return color / (color + 1);
}

float3 TonemapNeutral(float3 color) {
    color = min(color, 60);
    return NeutralTonemap(color);
}

float3 TonemapACES(float3 color) {
    color = min(color, 60);
    return AcesTonemap(unity_to_ACES(color));
}

float3 PostExposure(float3 color, const float exposure) {
    color = LinearToLogC(color);
    color = (color - ACEScc_MIDGRAY) * exposure + ACEScc_MIDGRAY;
    return max(LogCToLinear(color), 0);
}

float3 ColorFilter(const float3 color, const float3 filter) {
    return color * filter;
}

float3 HueShift(float3 color, float hueShift) {
    color = RgbToHsv(color);
    const float hue = color.x + hueShift;
    color.x = RotateHue(hue, 0, 1);
    return HsvToRgb(color);
}

float3 Saturation(float3 color, float saturation) {
    const float luminance = Luminance(color);
    return (color - luminance) * saturation + luminance;
}

float3 Contrast(float3 color, float contrast) {
    color = LinearToLogC(color);
    color = (color - ACEScc_MIDGRAY) * contrast + ACEScc_MIDGRAY;
    color = LogCToLinear(color);
    return max(color, 0);
}

float3 WhiteBalance(float3 color, float3 coeffs) {
    color = LinearToLMS(color);
    color *= coeffs;
    return LMSToLinear(color);
}

float3 SplitToning(float3 color, SplitToningSettings splitToning) {
    color = PositivePow(color, 1.0 / 2.2);
    const float t = saturate(Luminance(saturate(color)) + splitToning.balance);
    const float3 shadows = lerp(0.5, splitToning.shadows, 1.0 - t);
    const float3 highlights = lerp(0.5, splitToning.highlights, t);
    color = SoftLight(color, shadows);
    color = SoftLight(color, highlights);
    return PositivePow(color, 2.2);
}

float3 ChannelMixer(float3 color, ChannelMixerSettings channelMixer) {
    const float3x3 mix = float3x3(channelMixer.red, channelMixer.green, channelMixer.blue);
    color = mul(mix, color);
    return max(color, 0);
}

float3 ShadowsMidtonesHighlights(float3 color, SmhSettings smh) {
    const float luminance = Luminance(color);
    const float shadowsWeight = 1.0 - smoothstep(smh.shadowsStart, smh.shadowsEnd, luminance);
    const float highlightsWeight = smoothstep(smh.highlightsEnd, smh.highlightsEnd, luminance);
    const float midtonesWeight = 1.0 - shadowsWeight - highlightsWeight;
    return
        color * smh.shadows * shadowsWeight +
        color * smh.midtones * midtonesWeight +
        color * smh.highlights * highlightsWeight;
}

#endif