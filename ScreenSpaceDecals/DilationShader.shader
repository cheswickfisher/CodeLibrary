Shader "Custom/DilationShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

		//Blend One Zero

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			#define TEXEL_DIST 1
			#define MAX_STEPS 8

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
			float4 _MainTex_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {

				float2 offsets[8] = {float2(-TEXEL_DIST,0), float2(TEXEL_DIST,0),
									 float2(0,-TEXEL_DIST), float2(0,TEXEL_DIST),
									 float2(-TEXEL_DIST, TEXEL_DIST), float2(-TEXEL_DIST, -TEXEL_DIST),
									 float2(TEXEL_DIST,TEXEL_DIST), float2 (TEXEL_DIST, -TEXEL_DIST)		
									};
				float2 uv = i.uv;
				float4 sample = tex2D(_MainTex, uv);
				float4 sampleMax = sample;

				for(int i = 0; i < MAX_STEPS; i++){
					float2 curUV = uv + offsets[i] * _MainTex_TexelSize.xy;
					float4 offsetSample = tex2D(_MainTex, curUV);
					sampleMax = max(offsetSample, sampleMax);
				}

				sample = sampleMax;
				return sample;
            }
            ENDCG
        }
    }
}
