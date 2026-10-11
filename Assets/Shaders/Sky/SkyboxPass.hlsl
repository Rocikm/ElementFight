#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../Common/Noise.hlsl"
#include "../Common/Clouds.hlsl"

struct Attributes
{
    float4 vertex : POSITION;
    float3 uv : TEXCOORD0;
};

struct Varyings
{
    float3 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float4 vertex : SV_POSITION;
};

TEXTURE2D(_NightTex);
TEXTURE2D(_MoonTex);
SAMPLER(sampler_NightTex);
SAMPLER(sampler_MoonTex);

float3 _Ground;
float3 _Rayleigh;
float3 _Mie;
float3 _MieColor;
float3 _Ozone;
float _MieScatter;
float _Density;
float _SunSize;
float _HorizonFactor;
float _HorizonFactor2;
int _Scale;
float _StarVisibility;
float3 _MoonDir;
float3 _MoonColor;
float _MoonSize;

float3 _CloudColor;
float _SilverLining;
float _CloudHorizon;
float _CloudShadowing;
float _CloudShadowingIntensity;
float _LightScattering;
float3 _ShadowColor;
float _CloudHeight;
float _CloudNormalStrength;
float _CloudParallaxStrength;
float _CloudParallaxAmount;
float3 _CameraPosition;

Varyings vert (Attributes i)
{
    Varyings o;
    VertexPositionInputs positionInputs = GetVertexPositionInputs(i.vertex);
    o.vertex = positionInputs.positionCS;
    o.positionWS = positionInputs.positionWS;
    o.uv = i.uv;
    return o;
}

float2 DirToRectilinear(float3 uv)
{
    float x = atan2(uv.x, uv.z) / (PI * 2) + 0.5;
    float y = uv.y * 0.5 + 0.5;
    return float2(x,y);
}

// Source: https://github.com/mrdooz/kumi/blob/master/effects/luminance.hlsl
// Approximates luminance from an RGB value
float CalcLuminance(float3 color)
{
    return dot(color, float3(0.299f, 0.587f, 0.114f));
}

float MieScattering(float lightDotView, float scattering)
{
    float result = 1.0f - scattering * scattering;
    result /= (4.0f * PI * pow(1.0f + scattering * scattering - (2.0f * scattering) * lightDotView, 1.5f));
    return result;
}

float RayleighPhase(float cosTheta)
{
    return (3.0 / (16.0 * PI)) * (1.0 + cosTheta * cosTheta);
}

float3 ComputeRayleigh(
    float3 viewDir,      // normalized view direction
    float3 sunDir,       // normalized sun direction
    float3 upDir,        // normalized up vector
    float3 betaRayleigh,  // Rayleigh scattering coefficients
    float opticalDepth
)
{
    // Phase function
    float cosTheta = dot(viewDir, sunDir);
    float phase = (3.0 / (16.0 * PI)) * (1.0 + cosTheta * cosTheta);

    // Approximate sun optical depth based on sun elevation
    float mu_s = dot(sunDir, upDir);
    mu_s = saturate(mu_s * 0.6 + 0.18); // clamp to [0,1]
    
    // Avoid divide by zero
    float epsilon = 0.001;

    // Darken sky opposite to the sun
    float pathFactor = 1.0 / (mu_s + epsilon);
    pathFactor = lerp(pathFactor * 1.5, pathFactor, cosTheta * 0.5 + 0.5);
    
    // Sun transmittance along long path (approximate)
    float3 sunTransmittance = exp(-betaRayleigh * pathFactor / _Scale / (mu_s + epsilon + (1 / opticalDepth)));

    // Scattered light along view
    float3 scatteredLight = betaRayleigh * phase * sunTransmittance * opticalDepth;

    return scatteredLight;
}

float3 SunTransmittance(float3 viewDir, float3 sunDir, float3 betaRayleigh) {
    float3 upDir = float3(0,1,0);

    float mu_s = dot(sunDir, upDir);
    mu_s = saturate(mu_s + 0.05); // clamp to [0,1]

    float mu_v = dot(viewDir, upDir);
    mu_v = saturate(mu_v + 0.05);
    
    float epsilon = 0.001;
    
    float3 sunTransmittance = exp(-betaRayleigh / _Scale / (mu_s + epsilon));
    return sunTransmittance;
}

float3 AtmosphericTransmittance(float3 viewDir, float3 betaRayleigh)
{
    float mu = max(dot(viewDir, float3(0,1,0)), 0.05);

    return exp(-betaRayleigh * _Scale / mu);
}


float4 frag (Varyings i) : SV_Target
{
    // Input data
    float2 rectUV = DirToRectilinear(i.uv);
    float3 V = normalize(i.uv);
    float3 viewDirWS = -GetWorldSpaceNormalizeViewDir(i.positionWS);
    float3 upDir = float3(0,1,0);
    float3 camPosWS = _WorldSpaceCameraPos;
    Light mainLight = GetMainLight();
    float3 sunColor = mainLight.color;
    float sunHeight = saturate(mainLight.direction.y);
    float3 L = mainLight.direction;
    
    float LdotV = dot(L,V);
    float LdotV_WS = dot(L,viewDirWS);

    // Optical depth
    float mu_v = dot(V, upDir);
    mu_v = saturate(mu_v);
    float opticalDepth = 1 / (mu_v + _HorizonFactor * exp(-_HorizonFactor2 * mu_v)) * _Density / 10;

    // Sky
    float3 mieAngle = lerp(_MieColor, 0.9, smoothstep(0, 0.3, sunHeight));
    float3 betaRayleigh = 1.0 / pow(_Rayleigh, 4.0);
    float3 sky = ComputeRayleigh(V, L, upDir, betaRayleigh, opticalDepth);
    float3 sunTransmittance = SunTransmittance(V, L, betaRayleigh);
    float3 T = exp(-betaRayleigh / mu_v / _Scale);
    sky += MieScattering(LdotV, _MieScatter) * sunTransmittance * mieAngle * opticalDepth;
    
    // float3 ozone = float3(0.65, 1.0, 1.4);
    sky *= exp(-opticalDepth * _Ozone * 0.15);

    // Fake multiscatter
    float3 multiScatter = sky * 0.15 * (1.0 - exp(- (1 / opticalDepth)));
    sky += multiScatter;

    // Desaturate horizon
    float h = pow(1.0 - mu_v, 2.0);
    sky = lerp(sky, float(dot(sky, 0.333)), h * 0.15);

    // Clouds
    float2 speed = -_Wind.xy * _Time.x;
    // float2 cloudUV = _CloudScale * i.uv.xz / i.uv.y;
    float3 cloudWorldPos = _WorldSpaceCameraPos + ((_CloudHeight - _WorldSpaceCameraPos.y) / V.y) * V;
    float2 cloudUV = WorldToCloudUV(cloudWorldPos.xz);
    float2 parallaxOffset = V.xz / max(V.y, 0.1) * _CloudParallaxStrength;
    float cloudsA = GetCloudMask(cloudUV, 2);
    float cloudsB = GetCloudMask(cloudUV + parallaxOffset, 2);
    // float clouds = max(cloudsA, cloudsB);
    float clouds = lerp(cloudsA, cloudsB, _CloudParallaxAmount);
    
    // float cloudLayer1 = fbm(cloudUV + speed, 4);
    // float cloudLayer2 = fbm(cloudUV * 4 - speed / 2, 6);
    // float cloudMask = fbm(cloudUV * _CloudGap + speed, 4);
    // float cloudHorizonFade = smoothstep(0, _CloudHorizon, i.uv.y);
    float cloudHorizonFade = exp(-length(cloudWorldPos - _WorldSpaceCameraPos) * _CloudHorizon / 1000);
    // return length(cloudWorldPos - _CameraPosition)/ 1000;

    // cloudMask = smoothstep(0, 2 - _CloudAmount * 2, cloudMask);
    // float clouds = smoothstep(0, _CloudDensity * 2, cloudLayer1) * 5;
    // clouds += cloudLayer2 * _CloudRigidness * 5;
    // clouds *= 1 - cloudMask;
    // clouds = smoothstep(1, 0, clouds);
    // clouds *= cloudMask;

    // float clouds = smoothstep(_CloudAmount, 0, cloudMask) * _CloudDensity * 5;
    // clouds -= cloudLayer2 * _CloudRigidness;
    // clouds += smoothstep(0, 2.01 - _CloudSharpness * (1 - cloudLayer1) * 2, clouds);

    // Cloud shadow mask
    // float offset = smoothstep(-0.4, 0.4, V.y - L.y);
    // offset = (offset - 0.5) * 0.4 + 0.2;
    // float cloudShadow = fbm(cloudUV * _CloudGap + L.xz * offset + speed, 3);
    // float cloudShadow2 = fbm(cloudUV + L.xz * offset * 0.5 + speed, 2);
    // cloudShadow = 1 - smoothstep(0.1, 0.8, cloudShadow + cloudShadow2 * 0.2);
    float cloudShadow = GetCloudShadowMask(cloudUV, V, L);
    float3 cloudNormal = GetCloudNormal(cloudUV, _CloudNormalStrength);
    // float3 H = normalize(cloudNormal - V);
    float dotNL = dot(cloudNormal, -L);
    float dotInvNL = dot(cloudNormal, L * float3(1,-1,1));
    // float dotUpN = 1 - saturate(dot(cloudNormal, float3(0,-1,0)));
    dotInvNL = pow(saturate(dotInvNL + 0.2), 2);
    // float dotNV = dot(cloudNormal * float3(1,0,1), V);
    float dotNL1 = pow(saturate(dotNL + 0.5), 2);
    float dotNL2 = pow(saturate(-dotNL + 0.2), 2);
    // dotNV = pow(saturate(dotNV * 0.9 + 0.5), 1);
    // dotNL1 = 1;
    
    // Cloud lighting
    // float cloudDensityDarkening = smoothstep(0.8, 1, _CloudAmount) * (cloudLayer2 * 0.2 + 0.8);
    float cloudDensityDarkening = smoothstep(0.7, 1, _CloudAmount);
    // clouds = max(clouds, cloudDensityDarkening);
    float3 cloudColor = _CloudColor.rgb;
    float3 sunTransmitColor = sunTransmittance * mieAngle;
    cloudColor *= sunTransmitColor * saturate(2.5 - _CloudAmount);
    // cloudColor *= sunTransmitColor * saturate(2.5 - _CloudAmount) * (dotNL1 + dotNL2);
    // cloudColor *= mainLight.color * pow(saturate(mainLight.direction.y - 0.01), 0.3) * saturate(2.5 - _CloudAmount);
    float3 cloudShadowColor = _ShadowColor + sky * (1 - cloudDensityDarkening);
    cloudShadowColor *= max(0.01, min(0.3, sunHeight));
    float cloudShadowMix = smoothstep(0.0, _CloudShadowing, clouds) * _CloudShadowingIntensity * (1 - cloudShadow) * smoothstep(0, 0.7, _CloudAmount) + cloudDensityDarkening;
    // cloudColor = lerp(cloudColor, cloudShadowColor , saturate(cloudShadowMix));
    cloudColor = lerp(cloudColor, cloudShadowColor , 1 - exp(-pow(cloudShadowMix, 3) * 5));
    // cloudColor = lerp(cloudColor, _ShadowColor * cloudColor * sunHeight, saturate(_CloudAmount - 1) * clouds * cloudLayer1);
    float silverLining = saturate(MieScattering(LdotV, _LightScattering) * (1.0 - smoothstep(0, _SilverLining, clouds))) * cloudHorizonFade;
    cloudColor += cloudShadow * sunTransmitColor * 10 * silverLining * dotNL1;
    // cloudColor += cloudShadow * mainLight.color * 5 * silverLining * dotNL1;
    // cloudColor += sky * (1.0 - smoothstep(0, 3, clouds)) * (1 - _CloudAmount) * (1 + dotInvNL);
    // cloudColor -= sky * (1 - cloudDensityDarkening) * dotInvNL * (1 - sunHeight) * 0.3;
    cloudColor = max(cloudColor, 0);
    // cloudColor *= cloudShadow;
    float cloudSkyMask = saturate(1 - clouds * (cloudHorizonFade + cloudDensityDarkening) * step(0, V.y));
    sky = lerp(sky, cloudColor, 1 - cloudSkyMask);

    // Sun
    float halo = min(200, MieScattering(LdotV_WS, 0.975 - _SunSize * 10));
    float3 haloCol = halo * sunTransmitColor * mieAngle * 0.01;
    float sun = smoothstep(_SunSize, _SunSize * 0.5, 1 - saturate(LdotV_WS));
    sun *= V.y > 0;
    float3 sunCol = lerp(haloCol, sunTransmittance + 0.8 * mieAngle * mieAngle,sun);
    sky += smoothstep(0.8, 1, cloudSkyMask) * sunCol;



    // Moon
    float3 moonDir = normalize(_MoonDir);
    // orthonormal basis
    float t = step(0.99, abs(moonDir.y));
    float3 up = lerp(float3(0,1,0), float3(0,0,1), t);
    float3 moonRight = normalize(cross(up, moonDir));
    float3 moonUp = cross(moonDir, moonRight);
    // project view to moon plane
    float x = dot(V, moonRight);
    float y = dot(V, moonUp);
    float z = dot(V, moonDir);
    float2 p = float2(x, y) / z / _MoonSize;
    // float2 p = float2(x, y) * 10;
    float2 moonUV = p * 0.5 + 0.5;

    float4 moon = _MoonTex.SampleLevel(sampler_MoonTex, moonUV, 0);
    float moonMask = (moonUV.x > 0) * (moonUV.x < 1) * (moonUV.y > 0) * (moonUV.y < 1) * moon.a * cloudSkyMask; 
    // sky = moonMask * cloudSkyMask * moon.rgb * _MoonColor * T + (1 - moonMask * cloudSkyMask) * sky;
    float3 moonColor = moon.rgb * _MoonColor * T;
    sky = lerp(sky, moonColor + sky, moonMask);


    // Night sky
    float3 stars = _NightTex.SampleLevel(sampler_NightTex, rectUV, 0).xyz;
    float visibility = exp(-CalcLuminance(sky) * _StarVisibility) * smoothstep(0, -0.3, mainLight.direction.y);
    visibility = saturate(visibility - 0.02);
    sky += stars * visibility * cloudSkyMask * (1 - moonMask) / opticalDepth;


    // Ground
    float groundMask = smoothstep(0, -0.5, V.y);
    groundMask = 1 - exp(-groundMask * 40);
    float lambert = saturate(dot(L, float3(0,1,0)) + 0.2);
    lambert = max(0.1, lambert);
    float3 groundColor = lambert * sunTransmittance * _Ground * saturate(1.2 - cloudDensityDarkening);
    sky = lerp(sky, groundColor, groundMask);

    return float4(sky, 1);
}