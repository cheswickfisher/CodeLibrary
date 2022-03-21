Shader "UI/HealthBarShader"
{
    Properties
    {
		_HealthColor("HealthColor", Color) = (1,1,1,1)
		_DamageColor("DamageColor", Color) = (0,0,0,0)
		_Fill ("Fill", float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Overlay" }

        Pass
        {
			ZTest Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			 #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float4 _MainTex_ST;
			fixed4 _HealthColor;
			fixed4 _DamageColor;

			UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float, _Fill)
			UNITY_INSTANCING_BUFFER_END(Props)


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_SETUP_INSTANCE_ID(v);
				float fill = UNITY_ACCESS_INSTANCED_PROP(Props, _Fill);
				// generate UVs from fill level (assumed texture is clamped)
				o.uv.xy = v.uv.xy;
				o.uv.z = fill;
				//o.uv.x += 0.5 - fill;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed mask = step(i.uv.z, i.uv.x);
				fixed4 col = lerp( _HealthColor, _DamageColor, mask);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
