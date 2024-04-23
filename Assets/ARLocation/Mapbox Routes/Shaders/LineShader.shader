Shader "ARLocation/LineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0, 0, 0, 0.6)
        _Origin ("Origin", Vector) = (0, 0, 0, 1)
        _MaxDistance ("Max Distance", Float) = 4
        _FadeDistance ("Fade Distance", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            // Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members worldPos)
            #pragma exclude_renderers d3d11
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _Color;
            float4 _Origin;
            float _MaxDistance;
            float _FadeDistance;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                UNITY_TRANSFER_FOG(o,o.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);

                float d = distance(i.worldPos, _Origin);
                //i.uv.x -= d;
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;

                if (d > _MaxDistance)
                {
                    col.a *= 0;
                }
                else if (d < (_MaxDistance - _FadeDistance))
                {
                    col.a *= 1;
                }
                else
                {
                    col.a *= (_MaxDistance - d)/_FadeDistance;
                }

                //float maxDistance = 2.0;

                //if (d >= maxDistance)
                //{
                //    col.a = 0;
                //}
                //else
                //{
                //    col.a = 1 - d/maxDistance;
                //}

                return col;
            }
            ENDCG
        }
    }
}
