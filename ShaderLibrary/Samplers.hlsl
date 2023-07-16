#ifndef RETROLIGHT_SAMPLERS_INCLUDED
#define RETROLIGHT_SAMPLERS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"

#define POINT_SAMPLER sampler_PointClamp
#define BILINEAR_SAMPLER sampler_BilinearClamp

SAMPLER(sampler_BilinearClamp);

#endif