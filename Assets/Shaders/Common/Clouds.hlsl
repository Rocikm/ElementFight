#ifndef CLOUDS_INCLUDED
#define CLOUDS_INCLUDED

#include "Noise.hlsl"

float2 _Wind;
float2 _CloudOffset;
float _CloudDensity;
float _CloudScale;
float _CloudAmount;
float _CloudRigidness;
float _CloudGap;
float _CloudSharpness;

float2 WorldToCloudUV(float2 worldXZ)
{
    return worldXZ * _CloudScale;
}

float GetCloudMask(float2 uv, float quality)
{
    // float2 speed = -_Wind.xy * _Time.x;
    float cloudLayer1 = fbm(uv + _CloudOffset / 4, 2 * quality);
    float cloudLayer2 = fbm(uv * 4 - _CloudOffset / 2, 3 * quality);
    float cloudMask = fbm(uv * _CloudGap + _CloudOffset, 2 * quality);

    cloudMask = smoothstep(0, 2 - _CloudAmount * 2, cloudMask);
    float clouds = smoothstep(0, _CloudDensity * 2, cloudLayer1) * 5;
    clouds += cloudLayer2 * _CloudRigidness * 5;
    clouds *= 1 - cloudMask;
    clouds = smoothstep(1, 0, clouds);
    // clouds *= cloudMask;

    clouds += smoothstep(0, 2.01 - _CloudSharpness * (1 - cloudLayer1) * 2, clouds);

    return clouds;
}

float3 GetCloudNormal(float2 uv, float strength)
{
    float stepSize = 0.5;
    float quality = 6;
    float cloudLayer1 = fbm(uv + _CloudOffset / 4, 3);
    float hU = fbm(uv * _CloudGap + _CloudOffset + float2(0, stepSize), quality);
    float hD = fbm(uv * _CloudGap + _CloudOffset + float2(0, -stepSize), quality);
    float hL = fbm(uv * _CloudGap + _CloudOffset + float2(-stepSize, 0), quality);
    float hR = fbm(uv * _CloudGap + _CloudOffset + float2(stepSize, 0), quality);
    float dx = hR - hL;
    float dy = hU - hD;
    // float3 normal = normalize(float3(dx, -1 + strength * cloudLayer1, dy));
    float3 normal = normalize(float3(dx, -exp(-1 - strength * cloudLayer1), dy));
    return normal;
}

float GetCloudShadowMask(float2 uv, float3 V, float3 L)
{
    float offset = smoothstep(-0.9, 0.9, V.y - L.y);
    offset = (offset - 0.5) * 0.4 + 0.2;
    float cloudShadow = fbm(uv * _CloudGap + L.xz * offset + _CloudOffset, 3);
    // float cloudShadow2 = fbm(uv + L.xz * offset * 0.5 + _CloudOffset, 4);
    // cloudShadow = 1 - smoothstep(0.1, 0.8, cloudShadow + cloudShadow2 * 0.1);
    cloudShadow = 1 - smoothstep(0.1, 0.8, cloudShadow);

    return cloudShadow;
}

#endif