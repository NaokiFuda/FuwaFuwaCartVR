Shader "Custom/PBR_FurShader"
{
    Properties
    {
        // Base Properties
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Roughness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1.0
        _OcclusionMap ("Occlusion", 2D) = "white" {}
        _OcclusionStrength ("Occlusion Strength", Range(0,1)) = 1.0
        _EmissionColor ("Emission Color", Color) = (0,0,0)
        _EmissionMap ("Emission Map", 2D) = "white" {}
        
        // Fur Properties
        _FurLength ("Fur Length", Range(0.0, 1.0)) = 0.5
        _FurDensity ("Fur Density", Range(0, 2)) = 0.11
        _FurThinness ("Fur Thinness", Range(0.01, 10.0)) = 1.0
        _FurShading ("Fur Shading", Range(0.0, 1.0)) = 0.25
        
        _FurTexture ("Fur Pattern", 2D) = "white" {}
        _FurNoise ("Fur Noise", 2D) = "white" {}
        
        _Gravity ("Gravity Direction", Vector) = (0,-1,0,0)
        _GravityStrength ("Gravity Strength", Range(0.0, 1.0)) = 0.25
        
        // Wind
        [Toggle] _UseWind ("Use Wind", Float) = 0
        _WindFrequency ("Wind Frequency", Range(0, 1)) = 0.5
        _WindStrength ("Wind Strength", Range(0, 1)) = 0.5
        
        // Anisotropic Specular
        [Toggle] _UseAnisotropic ("Use Anisotropic Specular", Float) = 1
        _AnisotropicPower ("Anisotropic Power", Range(0, 1)) = 0.5
        _AnisotropicOffset ("Anisotropic Offset", Range(0, 1)) = 0.2
        
        // Alpha settings
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
        _Cutoff     ("Cutoff"      , Range(0, 1)) = 0.5
        _Parallax    ("Height"              , Range(0.0, 1.0)) = 0.5
        _ParallaxMap ("Heightmap (A)"       , 2D             ) = "black" {}
        _DetailMask("Detail Mask", 2D) = "white" {}

        _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        [Normal] _DetailNormalMap("Normal Map", 2D) = "bump" {}

         [Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200
        Cull Off // Double-sided rendering
        
        // Shadow pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On ZTest LEqual
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            
            struct v2f {
                V2F_SHADOW_CASTER;
            };
            
            v2f vert(appdata_base v) {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }
            
            float4 frag(v2f i) : SV_Target {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
        
        // Base pass
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #pragma shader_feature_local _NORMALMAP
        #pragma shader_feature _EMISSION
        #pragma shader_feature_local _METALLICGLOSSMAP
        
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 viewDir;
            float3 worldPos;
            float3 worldNormal; INTERNAL_DATA
        };
        
        fixed4 _Color;
        sampler2D _MainTex;
        half _Roughness;
        half _Metallic;
        sampler2D _BumpMap;
        half _BumpScale;
        sampler2D _OcclusionMap;
        half _OcclusionStrength;
        fixed4 _EmissionColor;
        sampler2D _EmissionMap;
        
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            
            // Metallic and smoothness (from roughness)
            o.Metallic = _Metallic;
            o.Smoothness = 1.0 - _Roughness;
            
            // Normal mapping
            #ifdef _NORMALMAP
                o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_BumpMap), _BumpScale);
            #endif
            
            // Occlusion
            half occ = tex2D(_OcclusionMap, IN.uv_MainTex).g;
            o.Occlusion = LerpOneTo(occ, _OcclusionStrength);
            
            // Emission
            #ifdef _EMISSION
                o.Emission = tex2D(_EmissionMap, IN.uv_MainTex).rgb * _EmissionColor.rgb;
            #endif
            
            // Alpha
            o.Alpha = c.a;
        }
        ENDCG
        
        // Shell layers for fur
        CGPROGRAM
        #define LAYERS 16
        #pragma surface surf Standard fullforwardshadows vertex:vert alphatest:_Cutoff
        #pragma target 3.0
        #pragma shader_feature _EMISSION
        
        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _FurTexture;
        sampler2D _FurNoise;
        sampler2D _OcclusionMap;
        sampler2D _EmissionMap;
        
        fixed4 _Color;
        half _Roughness;
        half _Metallic;
        half _BumpScale;
        half _OcclusionStrength;
        fixed4 _EmissionColor;
        
        half _FurLength;
        half _FurDensity;
        half _FurThinness;
        half _FurShading;
        
        float4 _Gravity;
        float _GravityStrength;
        
        float _UseWind;
        float _WindFrequency;
        float _WindStrength;
        
        float _UseAnisotropic;
        float _AnisotropicPower;
        float _AnisotropicOffset;
        
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 viewDir;
            float layer;
            float3 worldPos;
            float3 worldNormal; INTERNAL_DATA
        };
        
        float3 worldToTangentDir(float3 worldDir, float3 worldNormal, float3 worldTangent, float tangentSign) {
            float3 worldBitangent = cross(worldNormal, worldTangent) * tangentSign;
            float3x3 worldToTangent = float3x3(worldTangent, worldBitangent, worldNormal);
            return mul(worldToTangent, worldDir);
        }
        
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            
            // Generate layer ID
            o.layer = v.color.a; // Use vertex color alpha for layer ID
            
            float layerBlend = 1.0 - o.layer / LAYERS;
            float offset = o.layer * _FurLength / LAYERS;
            
            // Displace along normal
            float3 forceDir = float3(0, 0, 0);
            
            // Apply gravity
            forceDir += _Gravity.xyz * _GravityStrength * layerBlend;
            
            // Apply procedural wind animation
            if (_UseWind > 0.5) {
                float wind = sin(_Time.y * _WindFrequency + v.vertex.x * 0.5 + v.vertex.z * 0.5) * _WindStrength;
                forceDir += float3(wind, 0, wind) * layerBlend;
            }
            
            // Apply displacement
            v.vertex.xyz += lerp(v.normal, normalize(v.normal + forceDir), layerBlend) * offset;
        }
        
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Base texture with color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            
            // Normal mapping
            o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_BumpMap), _BumpScale);
            
            // Metallic and smoothness (from roughness)
            o.Metallic = _Metallic;
            o.Smoothness = 1.0 - _Roughness;
            
            // Occlusion
            half occ = tex2D(_OcclusionMap, IN.uv_MainTex).g;
            o.Occlusion = LerpOneTo(occ, _OcclusionStrength);
            
            // Emission
            #ifdef _EMISSION
                o.Emission = tex2D(_EmissionMap, IN.uv_MainTex).rgb * _EmissionColor.rgb;
            #endif
            
            // Fur pattern
            half3 furPattern = tex2D(_FurTexture, IN.uv_MainTex * _FurDensity).rgb;
            half noise = tex2D(_FurNoise, IN.uv_MainTex * 2.0 + _Time.xx * 0.1).r;
            
            // Apply thinning to the tips
            half furAlpha = furPattern.r - (IN.layer / LAYERS) * _FurThinness;
            
            // Apply fur shading (darkening toward the tips)
            half furShading = lerp(1.0, 0.7, IN.layer / LAYERS * _FurShading);
            o.Albedo *= furShading;
            
            // Alpha cutout for the fur pattern
            o.Alpha = furAlpha * c.a;
            
            // Clip transparent parts
            clip(o.Alpha - 0.01);
        }
        
        ENDCG
    }
    
    FallBack "Standard"
    CustomEditor "StandardShaderGUI"
}