Shader "Custom/BillboardSphereInstanced"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Cull Off
            ZWrite On
            ZTest LEqual
            Lighting Off
            Blend One OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float3 vertex : POSITION;   // offset from center
                float2 uv     : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv               : TEXCOORD0;
                float4 pos              : SV_POSITION;
                float3 lightDir         : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color) // xyz = center pos
            UNITY_INSTANCING_BUFFER_END(Props)

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.pos = UnityObjectToClipPos(float4(v.vertex, 1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                float3 lightWorld = float3(0, 0, -1);
                o.lightDir = normalize(mul((float3x3)unity_WorldToObject, lightWorld));

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float2 p = (i.uv - 0.5) * 2.0; // в диапазон [-1, 1]
                float dist2 = dot(p, p);

                // Прозрачность вне круга
                if (dist2 > 1.0)
                    discard;

                float4 diffuse = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                fixed4 texDiffuse = diffuse; //  tex2D(_MainTex, i.uv) * 

                float z = sqrt(1.0 - dist2);
                float3 normal = normalize(float3(p.x, p.y, z));

                // Ламберт
                float NdotL = saturate(dot(normal, normalize(i.lightDir)));
                NdotL = (NdotL + 1) / 2;
                float3 litColor = texDiffuse.rgb * NdotL;

                return float4(litColor, texDiffuse.a);
            }
            ENDCG
        }
    }
}
