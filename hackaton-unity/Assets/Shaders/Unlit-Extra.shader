Shader "Unlit/Transparent Extra" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Color("Color", COLOR) = (1, 1, 1, 1)
	}

	SubShader 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
	
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 

		Pass
		{
			Lighting Off
			SetTexture [_MainTex] 
			{ 
				constantColor [_Color]
				combine texture * constant
			} 
		}
	}
}

