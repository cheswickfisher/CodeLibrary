Shader "Unlit/MarkUVIslandsShader"
{
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



            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma shader_feature _MARK_ISLANDS

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f
            {
			    float4 vertex : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				float2 uv : TEXCOORD1;
            };

			float4x4 mesh_Object2World;

            v2f vert (appdata v)
            {
                v2f o;

				float2 uvRemapped   = v.uv.xy;
				       uvRemapped.y = 1. - uvRemapped.y;
					   uvRemapped   = uvRemapped *2. - 1.;

					   o.vertex     = float4(uvRemapped.xy, 0., 1.);
				       o.positionWS   = mul(mesh_Object2World, v.vertex);
				       o.uv         = v.uv;

				return o;
            }

            float4 frag (v2f i) : SV_Target
            {

				//float4 col  = float4(1,1,1,1);
				float4 col  = float4(1,1,1,0);

				return col;
            }
            ENDCG
        }
    }

}
