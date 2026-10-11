Shader "Unlit/SkyboxExp"
{
    Properties
    {
        _Ground ("Ground color", Color) = (1,1,1,1)
        _Rayleigh ("Rayleigh scattering", Vector) = (0.65, 0.57, 0.475, 0)
        _Ozone ("Ozone scattering", Vector) = (0.65, 1.0, 1.4, 0)
        [HDR] _MieColor ("Mie color", Color) = (1.0, 1.0, 1.0, 1)
        _MieScatter ("Mie scattering coefficient", Range(0.01, 1)) = 0.7
        _Density ("Atmosphere density", Range(0, 2)) = 1
        _HorizonFactor ("Horizon thickness", Range(0.001, 0.999)) = 0.5
        _HorizonFactor2 ("Horizon thickness2", Range(0.001, 20)) = 0.5
        _SunSize ("Sun Size", Range(0.0001,0.001)) = 0.0005
        _Scale ("Units scale", int) = 100

        _NightTex ("Night Texture", 2D) = "black" {}
        _MoonTex ("Moon Texture", 2D) = "white" {}
        [HDR] _MoonColor ("Moon Color", Color) = (1,1,1,1)
        _MoonDir ("Moon Direction", Vector) = (0, 1, 0, 0)
        _MoonSize ("Moon Size", Range(0.001, 1)) = 0.1
        _StarVisibility ("Stars visibility", Range(0, 100)) = 1
        // _SecondTex ("Secondary Texture", 2D) = "grey" {}
        // [HDR] _GroundColor ("Ground Color", Color) = (1,1,1,1)
        // [HDR] _HorizonColor ("Horizon Color", Color) = (1,1,1,1)
        // [HDR] _ZenithColor ("Zenith Color", Color) = (1,1,1,1)
        // _Horizon ("Horizon Thickness", Range(0.001,1)) = 0.1
        // _SunSize ("Sun Size", Range(0.0001,0.001)) = 0.0005
        // _Transition ("Transition", Range(0,1)) = 0
        // _TransitionProcedural ("Transition Procedural", Range(0,1)) = 0
        [HDR] _CloudColor ("Cloud Color", Color) = (1,1,1,1)
        _ShadowColor ("Cloud Shadow Color", Color) = (1,1,1,1)
        _CloudHeight ("Cloud height", Range(0,1000)) = 100
        _CloudHorizon ("Cloud Horizon fade", Range(0.1,1)) = 0.2
        _CloudScale ("Cloud Scale", Range(0.001,10)) = 1
        // _CloudAmount ("Cloud Amount", Range(0,1)) = 0.5
        _CloudDensity ("Cloud Density", Range(0,1)) = 1
        _CloudGap ("Cloud Gap", Range(0,1)) = 1
        _CloudAmount ("Cloud Amount", Range(0,1)) = 1
        _CloudRigidness ("Cloud Rigidness", Range(0,1)) = 1
        _CloudSharpness ("Cloud Sharpness", Range(0,1)) = 1
        _SilverLining ("Silver Lining", Range(0,2)) = 1
        _CloudShadowing ("Shadowing Threshold", Range(0,2)) = 1
        _CloudShadowingIntensity ("Shadowing Intensity", Range(0,1)) = 1
        _CloudNormalStrength ("Normal strength", Range(0,5)) = 1
        _CloudParallaxStrength ("Parallax strength", Range(0,1)) = 0.1
        _CloudParallaxAmount ("Parallax amount", Range(0,1)) = 0.1
        _LightScattering ("Light Scattering (Clouds)", Range(0,1)) = 0.7
        _Wind ("Wind Direction (xy)", Vector) = (1,1,0,0)
        // _CloudHeight ("Cloud Height", Range(0, 500)) = 100
        // _CloudOffset ("Cloud Offset", Vector) = (0,0,0,0)
        // _MainTexCube ("Texture", CUBE) = "grey" {}
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "QUEUE"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "SkyboxPass.hlsl"
            ENDHLSL
        }
    }
}
