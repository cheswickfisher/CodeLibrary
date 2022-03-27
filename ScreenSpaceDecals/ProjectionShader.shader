Shader "Custom/ProjectionShader"
{
    Properties
    {
        _Decal ("Decal", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
		AlphaToMask Off
		Cull Off
		ColorMask RGBA
		ZWrite Off
		ZTest Always

        Pass
        {
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                //float2 uv : TEXCOORD0;
				float4 uv : TEXCOORD0;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                //float2 uv : TEXCOORD0;
				float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float3 positionWS : TEXCOORD1;
				float4 uvShadow : TEXCOORD2;
				float4 clipUV : TEXCOORD3;
            };

            sampler2D _Decal;
			float4 _Color;
			float4x4 _projection;
			float4x4 _view;

            v2f vert (appdata v)
            {
                v2f o;

				float2 uvRemapped   = v.uv.xy;
				       uvRemapped.y = 1. - uvRemapped.y;
					   uvRemapped   = uvRemapped *2. - 1.;

					   o.vertex      = float4(uvRemapped.xy, 0., 1.);
				       o.positionWS  = mul(unity_ObjectToWorld, v.vertex);
				       o.uv          = v.uv;
					   float4x4 mv   = mul(_view, unity_ObjectToWorld);
					   float4x4 vp   = mul(_projection, mv);
					   o.uvShadow    = mul(vp, v.vertex);
					   o.uvShadow.xy = o.uvShadow + float2(0.5, 0.5);

					   float3 worldNormal  = normalize(UnityObjectToWorldNormal(v.normal));
					   float3 worldViewDir = UnityWorldSpaceViewDir(o.positionWS);

					   float clipValue     = dot(worldNormal, worldViewDir);
					   clipValue		   = saturate(sign(clipValue));

					   o.uvShadow.z = clipValue;

				return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 col = tex2D(_Decal, i.uvShadow) * _Color;
				col.a *= i.uvShadow.z;
                return col;
            }
            ENDCG
        }
    }
}
