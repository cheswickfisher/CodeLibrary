Shader "Custom/Water"
{
	Properties
	{
	// color of the water
	  _Color("Color", Color) = (1, 1, 1, 1)
	  _EdgeBrightness("Edge Brightness", Range(0,1)) = 0.5
	  _DepthRampTex("Depth Ramp Text", 2D) = "white" {}
	  // width of the edge effect
	  _DepthFactor("Depth Factor", float) = 1.0
	 }
	SubShader
	{
	    Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

		Pass
		{

			CGPROGRAM
			// required to use ComputeScreenPos()
			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag
 
				// Unity built-in - NOT required in Properties
				sampler2D _CameraDepthTexture, _DepthRampTex;
				float _DepthFactor, _EdgeBrightness;
				float4 _Color;


			struct vertexInput
				{
				float4 vertex : POSITION;
				};

			struct vertexOutput
				{
				float4 pos : SV_POSITION;
				float4 screenPos : TEXCOORD1;
				};

			vertexOutput vert(vertexInput input)
				{
				vertexOutput output;

				// convert obj-space position to camera clip space
				output.pos = UnityObjectToClipPos(input.vertex);

				// compute depth (screenPos is a float4)
				output.screenPos = ComputeScreenPos(output.pos);

				return output;
				}

				float4 frag(vertexOutput input) : COLOR
				{
				float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, (input.screenPos));
				float depth = LinearEyeDepth(depthSample);

				//input.screenPos.w is the view depth from the camera. depth is 1-0 value from the camera's depth texture.
				//the lower screenPos.w is relative to depth, the further underwater the vertex is. if it's too far underwater it should just be
				//blue or whatever the water color is. if it's close to the surface, add some foam.
				float foamLine = 1 - saturate(_DepthFactor * (depth - input.screenPos.w));
				float4 foamRamp = float4(tex2D(_DepthRampTex, float2(foamLine, 0.5)).rgb, 1.0);
				float4 col = _Color + foamLine * foamRamp * _EdgeBrightness;
				return col;
				}

				ENDCG
		}
	}
}