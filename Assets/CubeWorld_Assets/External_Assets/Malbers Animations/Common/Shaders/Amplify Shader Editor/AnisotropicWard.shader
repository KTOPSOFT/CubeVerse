// Made with Amplify Shader Editor v1.9.2.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Malbers/Anisotropic/Ward"
{
	Properties
	{
		_TillingOffset("Tilling & Offset", Vector) = (1,1,0,0)
		_Albedo("Albedo", 2D) = "white" {}
		_AlbedoTint("Albedo Tint", Color) = (1,1,1,1)
		_NormalMap("Normal Map", 2D) = "bump" {}
		_NormalAmount("Normal Amount", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Off
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _NormalMap;
		uniform float4 _TillingOffset;
		uniform float _NormalAmount;
		uniform sampler2D _Albedo;
		uniform float4 _AlbedoTint;

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 appendResult85 = (float2(_TillingOffset.z , _TillingOffset.w));
			float2 uv_TexCoord82 = i.uv_texcoord * appendResult85;
			float3 tex2DNode25 = UnpackScaleNormal( tex2D( _NormalMap, uv_TexCoord82 ), _NormalAmount );
			o.Normal = tex2DNode25;
			float2 UV95 = uv_TexCoord82;
			o.Albedo = ( tex2D( _Albedo, UV95 ) * _AlbedoTint ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19202
Node;AmplifyShaderEditor.CommentaryNode;9;-3964.098,372.1005;Inherit;False;851.9999;454.2498;View Direction Vector;5;8;7;6;2;10;;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;85;-4751.145,-956.6329;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;2;-3945.903,420.5996;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;10;-3913.506,582.1996;Float;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;58;-4477.512,-483.7573;Float;False;Property;_NormalAmount;Normal Amount;7;0;Create;True;0;0;0;False;0;False;1;1.66;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;82;-4452.483,-973.6951;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;6;-3655.7,439.3;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;25;-4161.5,-542.8994;Inherit;True;Property;_NormalMap;Normal Map;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;57;-3695.204,-319.6251;Inherit;False;528.1206;281.0598;Normal Direction Vector;2;23;56;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;13;-3703.892,27.7998;Inherit;False;563.3999;295.5665;Light Direction Vector;2;4;12;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;23;-3653.093,-276.5013;Inherit;True;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;4;-3632.809,94.25116;Inherit;True;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;7;-3440.398,449.4003;Inherit;True;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;24;-2701.581,-418.403;Inherit;False;776.0801;271.7102;Binormal Direction Vector;4;76;19;22;77;;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexBinormalNode;81;-2948.264,-752.8102;Inherit;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;17;-2678.596,51.89973;Inherit;False;515.2925;268.7952;Halfway Vector;3;14;71;16;;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexTangentNode;77;-2664.848,-301.4307;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;12;-3369.497,104.7998;Float;False;LightDirection;2;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;56;-3403.503,-255.7254;Float;False;NormalDirection;1;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;8;-3379.309,699.1993;Float;False;ViewDirection;3;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;21;-2691.003,-805.1019;Inherit;False;921.1089;263.5102;Tangent Direction Vector;3;78;79;80;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CrossProductOpNode;80;-2461.048,-763.3286;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;14;-2666.7,97.40108;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CrossProductOpNode;19;-2454.699,-372.1008;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;79;-2225.546,-761.8284;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;71;-2528.846,98.07848;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;76;-2296.958,-333.5321;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;16;-2371.798,98.70107;Float;True;HalfVector;6;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;-2149.901,-309.2012;Float;False;BinormalDirection;5;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;78;-2057.547,-760.6287;Float;True;TangentDirection;4;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-1649.396,265.4982;Float;False;Property;_AnisotropyY;Anisotropy Y;9;0;Create;True;0;0;0;False;0;False;1;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;35;-1516.398,97.39842;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;34;-1605.774,-255.2514;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-1613.096,9.198168;Float;False;Property;_AnisotropyX;Anisotropy X;8;0;Create;True;0;0;0;False;0;False;1;0.5508147;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;29;-1957.698,373.9995;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;37;-1286.598,165.5984;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;36;-1287.598,-96.40155;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;68;-1094.196,-99.32366;Float;True;HX;10;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;41;-1096.196,160.9981;Float;True;HY;11;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;30;-1810.798,388.1992;Float;False;NdotH;9;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;67;-795.595,-20.32379;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;48;-1089.397,396.2962;Inherit;True;2;2;0;FLOAT;1;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-854.1968,154.4969;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;92;-2942.107,377.6998;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;91;-2925.875,612.3439;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;26;-2655.965,602.4556;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;93;-639.1787,390.1096;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;45;-604.8968,9.19689;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-2370.183,559.6396;Float;True;NdotL;8;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;47;-419.2962,64.19641;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;94;-872.1787,600.1096;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-245.9968,79.59687;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ExpOpNode;44;-74.2966,99.3969;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;88;-79.22814,349.0702;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;59;256.7027,-603.4249;Float;False;Property;_AlbedoTint;Albedo Tint;3;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.3235294,0.2737639,0.2355294,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;61;221.8039,-293.2251;Inherit;True;Property;_Specular;Specular;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;65;269.2042,-95.6257;Float;False;Property;_SpecularTint;Specular Tint;5;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.7279412,0.5353365,0.1712803,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;87;312.5681,206.1701;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;592.9036,-665.226;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;569.3049,-159.126;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;33;-2375.544,339.7971;Float;True;NdotV;7;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;817.605,-68.02586;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;84;-4750.445,-1062.533;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DotProductOpNode;32;-2661.953,359.5889;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1333.799,-451.6001;Float;False;True;-1;2;ASEMaterialInspector;0;0;StandardSpecular;Malbers/Anisotropic/Ward;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;;3;False;;False;0;False;;0;False;;False;0;Masked;0.6;True;True;0;False;TransparentCutout;;AlphaTest;All;0;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SamplerNode;1;236.7004,-798.4986;Inherit;True;Property;_Albedo;Albedo;2;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;83;-4943.552,-1060.335;Float;True;Property;_TillingOffset;Tilling & Offset;1;0;Create;True;0;0;0;False;0;False;1,1,0,0;1,1,4.51,1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;95;-4115.02,-1011.774;Inherit;False;UV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;96;-83.13474,-602.5987;Inherit;False;95;UV;1;0;OBJECT;;False;1;FLOAT2;0
WireConnection;85;0;83;3
WireConnection;85;1;83;4
WireConnection;82;0;85;0
WireConnection;6;0;2;0
WireConnection;6;1;10;0
WireConnection;25;1;82;0
WireConnection;25;5;58;0
WireConnection;23;0;25;0
WireConnection;7;0;6;0
WireConnection;12;0;4;0
WireConnection;56;0;23;0
WireConnection;8;0;7;0
WireConnection;80;0;56;0
WireConnection;80;1;81;0
WireConnection;14;0;12;0
WireConnection;14;1;8;0
WireConnection;19;0;56;0
WireConnection;19;1;77;0
WireConnection;79;0;80;0
WireConnection;71;0;14;0
WireConnection;76;0;19;0
WireConnection;16;0;71;0
WireConnection;22;0;76;0
WireConnection;78;0;79;0
WireConnection;35;0;16;0
WireConnection;35;1;22;0
WireConnection;34;0;16;0
WireConnection;34;1;78;0
WireConnection;29;0;56;0
WireConnection;29;1;16;0
WireConnection;37;0;35;0
WireConnection;37;1;39;0
WireConnection;36;0;34;0
WireConnection;36;1;38;0
WireConnection;68;0;36;0
WireConnection;41;0;37;0
WireConnection;30;0;29;0
WireConnection;67;0;68;0
WireConnection;67;1;68;0
WireConnection;48;0;30;0
WireConnection;43;0;41;0
WireConnection;43;1;41;0
WireConnection;92;0;56;0
WireConnection;91;0;12;0
WireConnection;26;0;92;0
WireConnection;26;1;91;0
WireConnection;93;0;48;0
WireConnection;45;0;67;0
WireConnection;45;1;43;0
WireConnection;27;0;26;0
WireConnection;47;0;45;0
WireConnection;47;1;93;0
WireConnection;94;0;27;0
WireConnection;46;0;47;0
WireConnection;44;0;46;0
WireConnection;88;0;94;0
WireConnection;61;1;96;0
WireConnection;87;0;44;0
WireConnection;87;1;88;0
WireConnection;60;0;1;0
WireConnection;60;1;59;0
WireConnection;62;0;61;0
WireConnection;62;1;65;0
WireConnection;33;0;32;0
WireConnection;63;0;62;0
WireConnection;63;1;87;0
WireConnection;84;0;83;1
WireConnection;84;1;83;2
WireConnection;32;0;56;0
WireConnection;32;1;8;0
WireConnection;0;0;60;0
WireConnection;0;1;25;0
WireConnection;1;1;96;0
WireConnection;95;0;82;0
ASEEND*/
//CHKSM=2F3D71C850956EE1E7FC4A2B0BFC70DA8FFB63A8