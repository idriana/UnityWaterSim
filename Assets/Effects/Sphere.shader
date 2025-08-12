Shader "Custom/Sphere"
{
    Properties {
        _Center("Center", Vector) = (0, 0, 0)
        _Radius("Radius", Float) = 1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Cull Off
        Pass
        {
            CGPROGRAM
            #include "UnityCG.cginc"
            #include "UnityInstancing.cginc"
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            float _Radius;

            struct VS_Input{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float2 center : TEXCOORD1;
                float radius : TEXCOORD2;
            };

            struct InstanceData {
                float3 center;
                float radius;
                float4 color;
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float3, _Center)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (VS_Input input)
            {
                v2f o;
                o.uv = input.uv;
                float3 center = UNITY_ACCESS_INSTANCED_PROP(Props, _Center);
                float4 clipCenter = mul(UNITY_MATRIX_VP, float4(center, 1.0));
                o.pos = UnityObjectToClipPos(input.vertex + float4(center, 0));
                o.center = (clipCenter.xy / clipCenter.w);
                o.radius = _Radius / clipCenter.w;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 screenUV = i.pos.xy;
                float2 screenCenter = i.center;
                screenCenter.y = -screenCenter.y;
                screenCenter = (screenCenter + 1) / 2 * _ScreenParams.xy;
                
                float2 dir = screenUV - screenCenter;
                float dist = length(dir);

                if (dist > i.radius)
                    return float4(1, 0, 0, 0);
                
                float3 normal = normalize(
                    float3(
                        dir / i.radius,                                   // x и y компоненты
                        sqrt(1.0 - (dist * dist) / (i.radius * i.radius)) // z-компонента
                    )
                );
                
                float3 lightDir = float3(0, 0, 1);
                float intensity = saturate(dot(normal, lightDir));

                float3 sphereColor = float3(0, 1, 0);
                float3 lightColor = float3(1, 1, 1);
                float3 color = sphereColor * lightColor * intensity; 

                return float4(color, 1);
            }
            ENDCG
        }
    }
}