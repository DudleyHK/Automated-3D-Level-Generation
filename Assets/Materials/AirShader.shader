Shader "AirShader" 
{
	Properties
	{
		_Colour("Colour", Color) = (1, 1, 1, 1)
	}

	SubShader
	{
		Tags {"Queue" = "Transparent"}
		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert 
			#pragma fragment frag

	        float4 _Colour;

			float4 vert(float4 vertexPos : POSITION) : SV_POSITION
			{
				return UnityObjectToClipPos(vertexPos);
			}

			float4 frag(void) : COLOR
			{
				return float4(_Colour.r, _Colour.g, _Colour.b, _Colour.a);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}