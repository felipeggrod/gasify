Shader "Custom/StylizedRimLight" {
    Properties{
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _RimColor ("Rim Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _RimPower ("Rim Power", Range(0.0, 10.0)) = 3.0
    }
    SubShader{
        Tags {"Queue"="Transparent" "RenderType"="Opaque"}
        LOD 100

        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform float4 _Color;
            uniform float4 _RimColor;
            uniform float _RimPower;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldNormal = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                float rim = 1.0 - saturate(dot(i.worldNormal, -_WorldSpaceCameraPos));

                col.rgb += _RimColor.rgb * pow(rim, _RimPower);

                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}