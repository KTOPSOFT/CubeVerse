
Shader "FakeGround"
{
	Properties
	{
		[NoScaleOffset]_ColorTex("Color Tex", 2D) = "gray" {}
		[NoScaleOffset]_HeightTex("Height Tex", 2D) = "black" {}
		_LightEffective("Light Effective", Range( 0 , 1)) = 0.2
		_playerPos( "Player Position", vector ) = (0.0,0.0,0.0)
		_texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _HeightTex;
		uniform sampler2D _ColorTex;
		uniform float _LightEffective;
		uniform float3 _playerPos;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 uv_HeightTex = v.texcoord;
			float4 heightMap = tex2Dlod( _HeightTex, float4( uv_HeightTex, 0, 0.0) );
			if((v.vertex.x<_playerPos.x-_playerPos.z || v.vertex.x>_playerPos.x+_playerPos.z || v.vertex.z<_playerPos.y-_playerPos.z || v.vertex.z>_playerPos.y+_playerPos.z)
			 && v.vertex.x>-99.5 &&	 v.vertex.x<99.5 &&	 v.vertex.z>-99.5 && v.vertex.z<99.5){
			   v.vertex.y= ( ( ( heightMap.r * 5.0 ) + ( heightMap.g * 5.0 ) + ( heightMap.b * 5.0 ) ))+0.5;
			}
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_ColorTex = i.uv_texcoord;
			float4 texMain = tex2D( _ColorTex, uv_ColorTex);
			o.Albedo = texMain.rgb*float3(0.7,0.8,1.0);
			float4 glow = lerp( float4( 0,0,0,0 ) , texMain , ( 1.0 - _LightEffective ));
			o.Emission = glow.rgb*float3(0.7,0.8,1.0);
			o.Metallic = 0.0;
			o.Smoothness = 0.0;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
