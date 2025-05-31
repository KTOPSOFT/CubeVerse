// Made with Amplify Shader Editor v1.9.1.6
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Malbers/Color3x3"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin][Header(Albedo (Alpha  Gradient Power))]_Tint("Tint", Color) = (1,1,1,1)
		[Space(10)]_Color1("Color 1", Color) = (1,0.1544118,0.1544118,0)
		_Color2("Color 2", Color) = (1,0.1544118,0.8017241,1)
		_Color3("Color 3", Color) = (0.2535501,0.1544118,1,1)
		[Space(10)]_Color4("Color 4", Color) = (0.9533468,1,0.1544118,1)
		_Color5("Color 5", Color) = (0.2669384,0.3207547,0.0226949,1)
		_Color6("Color 6", Color) = (1,0.4519259,0.1529412,1)
		[Space(10)]_Color7("Color 7", Color) = (0.9099331,0.9264706,0.6267301,1)
		_Color8("Color 8", Color) = (0.1544118,0.1602434,1,1)
		_Color9("Color 9", Color) = (0.1529412,0.9929401,1,1)
		[Header(Metallic(R) Rough(G) Emmission(B))]_MRE1("MRE 1", Color) = (0,1,0,0)
		_MRE2("MRE 2", Color) = (0,1,0,0)
		_MRE3("MRE 3", Color) = (0,1,0,0)
		[Space(10)]_MRE4("MRE 4", Color) = (0,1,0,0)
		_MRE5("MRE 5", Color) = (0,1,0,0)
		_MRE6("MRE 6", Color) = (0,1,0,0)
		[Space()]_MRE7("MRE 7", Color) = (0,1,0,0)
		_MRE8("MRE 8", Color) = (0,1,0,0)
		_MRE9("MRE 9", Color) = (0,1,0,0)
		[Header(Emission)]_EmissionPower("Emission Power", Float) = 1
		[HDR]_EmissionColor("EmissionColor", Color) = (0,0,0,0)
		[Header(Detail Texture (UV2))]_DetailsUV2("Details (UV2)", 2D) = "white" {}
		_DetailOpacity("Opacity", Range( 0 , 1)) = 0
		[Header(Gradient Properties)][SingleLineTexture][Space(10)]_Gradient("Gradient", 2D) = "white" {}
		_GradientIntensity("Gradient Intensity", Range( 0 , 1)) = 0.75
		_GradientColor("Gradient Color", Color) = (0,0,0,0)
		_GradientScale("Gradient Scale", Float) = 1
		_GradientOffset("Gradient Offset", Float) = 0
		[ASEEnd]_GradientPower("Gradient Power", Float) = 1
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}


		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Trans Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Trans Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Trans Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Trans Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Trans Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Trans Shadow", Range( 0, 1 ) ) = 0.5
		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		[HideInInspector][ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[HideInInspector][ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0
		[HideInInspector][ToggleOff] _ReceiveShadows("Receive Shadows", Float) = 1.0

		[HideInInspector] _QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl("_QueueControl", Float) = -1

        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" "UniversalMaterialType"="Lit" }

		Cull Back
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		AlphaToMask Off

		

		HLSLINCLUDE
		#pragma target 4.5
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 140008


			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile_fragment _ _LIGHT_LAYERS
			#pragma multi_compile_fragment _ _LIGHT_COOKIES
			#pragma multi_compile _ _FORWARD_PLUS

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_FORWARD

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
				#define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 fogFactorAndVertexLight : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					float4 shadowCoord : TEXCOORD2;
				#endif
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
					float4 screenPos : TEXCOORD6;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
					float2 dynamicLightmapUV : TEXCOORD7;
				#endif
				float4 ase_texcoord8 : TEXCOORD8;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DetailsUV2_ST;
			float4 _MRE8;
			float4 _MRE7;
			float4 _MRE6;
			float4 _MRE5;
			float4 _MRE4;
			float4 _MRE3;
			float4 _MRE2;
			float4 _MRE1;
			float4 _Tint;
			float4 _Color9;
			float4 _Color8;
			float4 _MRE9;
			float4 _Color7;
			float4 _Color5;
			float4 _Color4;
			float4 _Color3;
			float4 _Color2;
			float4 _Color1;
			float4 _GradientColor;
			float4 _Color6;
			float4 _EmissionColor;
			float _EmissionPower;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			float _DetailOpacity;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			// Property used by ScenePickingPass
			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			// Properties used by SceneSelectionPass
			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _DetailsUV2;
			sampler2D _Gradient;


			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"

			//#ifdef HAVE_VFX_MODIFICATION
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
			//#endif

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord8.xy = v.texcoord1.xy;
				o.ase_texcoord8.zw = v.texcoord.xy;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				#if defined(LIGHTMAP_ON)
					OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				#endif

				#if !defined(LIGHTMAP_ON)
					OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)
					o.dynamicLightmapUV.xy = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord.xy;
					o.lightmapUVOrVertexSH.xy = v.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );

				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif

				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;

				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
					o.screenPos = ComputeScreenPos(positionCS);
				#endif

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag ( VertexOutput IN
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						, out float4 outRenderingLayers : SV_Target1
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.clipPos );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif

				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
					float4 ScreenPos = IN.screenPos;
				#endif

				float2 NormalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.clipPos);

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#endif

				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float2 uv1_DetailsUV2 = IN.ase_texcoord8.xy * _DetailsUV2_ST.xy + _DetailsUV2_ST.zw;
				float4 clampResult310 = clamp( ( tex2D( _DetailsUV2, uv1_DetailsUV2 ) + ( 1.0 - _DetailOpacity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float2 texCoord258 = IN.ase_texcoord8.zw * float2( 3,3 ) + float2( 0,0 );
				float4 clampResult206 = clamp( ( ( tex2D( _Gradient, texCoord258 ) + _GradientColor ) + ( 1.0 - _GradientIntensity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 saferPower254 = abs( (clampResult206*_GradientScale + _GradientOffset) );
				float4 temp_cast_0 = (_GradientPower).xxxx;
				float2 texCoord2_g512 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g512 = 1.0;
				float temp_output_7_0_g512 = 3.0;
				float temp_output_9_0_g512 = 3.0;
				float temp_output_8_0_g512 = 3.0;
				float2 texCoord2_g511 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g511 = 2.0;
				float temp_output_7_0_g511 = 3.0;
				float temp_output_9_0_g511 = 3.0;
				float temp_output_8_0_g511 = 3.0;
				float2 texCoord2_g507 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g507 = 3.0;
				float temp_output_7_0_g507 = 3.0;
				float temp_output_9_0_g507 = 3.0;
				float temp_output_8_0_g507 = 3.0;
				float2 texCoord2_g506 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g506 = 1.0;
				float temp_output_7_0_g506 = 3.0;
				float temp_output_9_0_g506 = 2.0;
				float temp_output_8_0_g506 = 3.0;
				float2 texCoord2_g509 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g509 = 2.0;
				float temp_output_7_0_g509 = 3.0;
				float temp_output_9_0_g509 = 2.0;
				float temp_output_8_0_g509 = 3.0;
				float2 texCoord2_g508 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g508 = 3.0;
				float temp_output_7_0_g508 = 3.0;
				float temp_output_9_0_g508 = 2.0;
				float temp_output_8_0_g508 = 3.0;
				float2 texCoord2_g514 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g514 = 1.0;
				float temp_output_7_0_g514 = 3.0;
				float temp_output_9_0_g514 = 1.0;
				float temp_output_8_0_g514 = 3.0;
				float2 texCoord2_g513 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g513 = 2.0;
				float temp_output_7_0_g513 = 3.0;
				float temp_output_9_0_g513 = 1.0;
				float temp_output_8_0_g513 = 3.0;
				float2 texCoord2_g510 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g510 = 3.0;
				float temp_output_7_0_g510 = 3.0;
				float temp_output_9_0_g510 = 1.0;
				float temp_output_8_0_g510 = 3.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( texCoord2_g512.x , ( ( temp_output_3_0_g512 - 1.0 ) / temp_output_7_0_g512 ) ) ) * ( step( texCoord2_g512.x , ( temp_output_3_0_g512 / temp_output_7_0_g512 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g512.y , ( ( temp_output_9_0_g512 - 1.0 ) / temp_output_8_0_g512 ) ) ) * ( step( texCoord2_g512.y , ( temp_output_9_0_g512 / temp_output_8_0_g512 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( texCoord2_g511.x , ( ( temp_output_3_0_g511 - 1.0 ) / temp_output_7_0_g511 ) ) ) * ( step( texCoord2_g511.x , ( temp_output_3_0_g511 / temp_output_7_0_g511 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g511.y , ( ( temp_output_9_0_g511 - 1.0 ) / temp_output_8_0_g511 ) ) ) * ( step( texCoord2_g511.y , ( temp_output_9_0_g511 / temp_output_8_0_g511 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( texCoord2_g507.x , ( ( temp_output_3_0_g507 - 1.0 ) / temp_output_7_0_g507 ) ) ) * ( step( texCoord2_g507.x , ( temp_output_3_0_g507 / temp_output_7_0_g507 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g507.y , ( ( temp_output_9_0_g507 - 1.0 ) / temp_output_8_0_g507 ) ) ) * ( step( texCoord2_g507.y , ( temp_output_9_0_g507 / temp_output_8_0_g507 ) ) * 1.0 ) ) ) ) ) + ( ( _Color4 * ( ( ( 1.0 - step( texCoord2_g506.x , ( ( temp_output_3_0_g506 - 1.0 ) / temp_output_7_0_g506 ) ) ) * ( step( texCoord2_g506.x , ( temp_output_3_0_g506 / temp_output_7_0_g506 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g506.y , ( ( temp_output_9_0_g506 - 1.0 ) / temp_output_8_0_g506 ) ) ) * ( step( texCoord2_g506.y , ( temp_output_9_0_g506 / temp_output_8_0_g506 ) ) * 1.0 ) ) ) ) + ( _Color5 * ( ( ( 1.0 - step( texCoord2_g509.x , ( ( temp_output_3_0_g509 - 1.0 ) / temp_output_7_0_g509 ) ) ) * ( step( texCoord2_g509.x , ( temp_output_3_0_g509 / temp_output_7_0_g509 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g509.y , ( ( temp_output_9_0_g509 - 1.0 ) / temp_output_8_0_g509 ) ) ) * ( step( texCoord2_g509.y , ( temp_output_9_0_g509 / temp_output_8_0_g509 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( texCoord2_g508.x , ( ( temp_output_3_0_g508 - 1.0 ) / temp_output_7_0_g508 ) ) ) * ( step( texCoord2_g508.x , ( temp_output_3_0_g508 / temp_output_7_0_g508 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g508.y , ( ( temp_output_9_0_g508 - 1.0 ) / temp_output_8_0_g508 ) ) ) * ( step( texCoord2_g508.y , ( temp_output_9_0_g508 / temp_output_8_0_g508 ) ) * 1.0 ) ) ) ) ) + ( ( _Color7 * ( ( ( 1.0 - step( texCoord2_g514.x , ( ( temp_output_3_0_g514 - 1.0 ) / temp_output_7_0_g514 ) ) ) * ( step( texCoord2_g514.x , ( temp_output_3_0_g514 / temp_output_7_0_g514 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g514.y , ( ( temp_output_9_0_g514 - 1.0 ) / temp_output_8_0_g514 ) ) ) * ( step( texCoord2_g514.y , ( temp_output_9_0_g514 / temp_output_8_0_g514 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( texCoord2_g513.x , ( ( temp_output_3_0_g513 - 1.0 ) / temp_output_7_0_g513 ) ) ) * ( step( texCoord2_g513.x , ( temp_output_3_0_g513 / temp_output_7_0_g513 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g513.y , ( ( temp_output_9_0_g513 - 1.0 ) / temp_output_8_0_g513 ) ) ) * ( step( texCoord2_g513.y , ( temp_output_9_0_g513 / temp_output_8_0_g513 ) ) * 1.0 ) ) ) ) + ( _Color9 * ( ( ( 1.0 - step( texCoord2_g510.x , ( ( temp_output_3_0_g510 - 1.0 ) / temp_output_7_0_g510 ) ) ) * ( step( texCoord2_g510.x , ( temp_output_3_0_g510 / temp_output_7_0_g510 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g510.y , ( ( temp_output_9_0_g510 - 1.0 ) / temp_output_8_0_g510 ) ) ) * ( step( texCoord2_g510.y , ( temp_output_9_0_g510 / temp_output_8_0_g510 ) ) * 1.0 ) ) ) ) ) );
				float4 clampResult255 = clamp( ( pow( saferPower254 , temp_cast_0 ) + ( 1.0 - (temp_output_155_0).a ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				
				float2 texCoord2_g498 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g498 = 1.0;
				float temp_output_7_0_g498 = 3.0;
				float temp_output_9_0_g498 = 3.0;
				float temp_output_8_0_g498 = 3.0;
				float2 texCoord2_g501 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g501 = 2.0;
				float temp_output_7_0_g501 = 3.0;
				float temp_output_9_0_g501 = 3.0;
				float temp_output_8_0_g501 = 3.0;
				float2 texCoord2_g505 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g505 = 3.0;
				float temp_output_7_0_g505 = 3.0;
				float temp_output_9_0_g505 = 3.0;
				float temp_output_8_0_g505 = 3.0;
				float2 texCoord2_g497 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g497 = 1.0;
				float temp_output_7_0_g497 = 3.0;
				float temp_output_9_0_g497 = 2.0;
				float temp_output_8_0_g497 = 3.0;
				float2 texCoord2_g502 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g502 = 2.0;
				float temp_output_7_0_g502 = 3.0;
				float temp_output_9_0_g502 = 2.0;
				float temp_output_8_0_g502 = 3.0;
				float2 texCoord2_g503 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g503 = 3.0;
				float temp_output_7_0_g503 = 3.0;
				float temp_output_9_0_g503 = 2.0;
				float temp_output_8_0_g503 = 3.0;
				float2 texCoord2_g500 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g500 = 1.0;
				float temp_output_7_0_g500 = 3.0;
				float temp_output_9_0_g500 = 1.0;
				float temp_output_8_0_g500 = 3.0;
				float2 texCoord2_g504 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g504 = 2.0;
				float temp_output_7_0_g504 = 3.0;
				float temp_output_9_0_g504 = 1.0;
				float temp_output_8_0_g504 = 3.0;
				float2 texCoord2_g499 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g499 = 3.0;
				float temp_output_7_0_g499 = 3.0;
				float temp_output_9_0_g499 = 1.0;
				float temp_output_8_0_g499 = 3.0;
				float4 temp_output_263_0 = ( ( ( _MRE1 * ( ( ( 1.0 - step( texCoord2_g498.x , ( ( temp_output_3_0_g498 - 1.0 ) / temp_output_7_0_g498 ) ) ) * ( step( texCoord2_g498.x , ( temp_output_3_0_g498 / temp_output_7_0_g498 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g498.y , ( ( temp_output_9_0_g498 - 1.0 ) / temp_output_8_0_g498 ) ) ) * ( step( texCoord2_g498.y , ( temp_output_9_0_g498 / temp_output_8_0_g498 ) ) * 1.0 ) ) ) ) + ( _MRE2 * ( ( ( 1.0 - step( texCoord2_g501.x , ( ( temp_output_3_0_g501 - 1.0 ) / temp_output_7_0_g501 ) ) ) * ( step( texCoord2_g501.x , ( temp_output_3_0_g501 / temp_output_7_0_g501 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g501.y , ( ( temp_output_9_0_g501 - 1.0 ) / temp_output_8_0_g501 ) ) ) * ( step( texCoord2_g501.y , ( temp_output_9_0_g501 / temp_output_8_0_g501 ) ) * 1.0 ) ) ) ) + ( _MRE3 * ( ( ( 1.0 - step( texCoord2_g505.x , ( ( temp_output_3_0_g505 - 1.0 ) / temp_output_7_0_g505 ) ) ) * ( step( texCoord2_g505.x , ( temp_output_3_0_g505 / temp_output_7_0_g505 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g505.y , ( ( temp_output_9_0_g505 - 1.0 ) / temp_output_8_0_g505 ) ) ) * ( step( texCoord2_g505.y , ( temp_output_9_0_g505 / temp_output_8_0_g505 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE4 * ( ( ( 1.0 - step( texCoord2_g497.x , ( ( temp_output_3_0_g497 - 1.0 ) / temp_output_7_0_g497 ) ) ) * ( step( texCoord2_g497.x , ( temp_output_3_0_g497 / temp_output_7_0_g497 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g497.y , ( ( temp_output_9_0_g497 - 1.0 ) / temp_output_8_0_g497 ) ) ) * ( step( texCoord2_g497.y , ( temp_output_9_0_g497 / temp_output_8_0_g497 ) ) * 1.0 ) ) ) ) + ( _MRE5 * ( ( ( 1.0 - step( texCoord2_g502.x , ( ( temp_output_3_0_g502 - 1.0 ) / temp_output_7_0_g502 ) ) ) * ( step( texCoord2_g502.x , ( temp_output_3_0_g502 / temp_output_7_0_g502 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g502.y , ( ( temp_output_9_0_g502 - 1.0 ) / temp_output_8_0_g502 ) ) ) * ( step( texCoord2_g502.y , ( temp_output_9_0_g502 / temp_output_8_0_g502 ) ) * 1.0 ) ) ) ) + ( _MRE6 * ( ( ( 1.0 - step( texCoord2_g503.x , ( ( temp_output_3_0_g503 - 1.0 ) / temp_output_7_0_g503 ) ) ) * ( step( texCoord2_g503.x , ( temp_output_3_0_g503 / temp_output_7_0_g503 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g503.y , ( ( temp_output_9_0_g503 - 1.0 ) / temp_output_8_0_g503 ) ) ) * ( step( texCoord2_g503.y , ( temp_output_9_0_g503 / temp_output_8_0_g503 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE7 * ( ( ( 1.0 - step( texCoord2_g500.x , ( ( temp_output_3_0_g500 - 1.0 ) / temp_output_7_0_g500 ) ) ) * ( step( texCoord2_g500.x , ( temp_output_3_0_g500 / temp_output_7_0_g500 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g500.y , ( ( temp_output_9_0_g500 - 1.0 ) / temp_output_8_0_g500 ) ) ) * ( step( texCoord2_g500.y , ( temp_output_9_0_g500 / temp_output_8_0_g500 ) ) * 1.0 ) ) ) ) + ( _MRE8 * ( ( ( 1.0 - step( texCoord2_g504.x , ( ( temp_output_3_0_g504 - 1.0 ) / temp_output_7_0_g504 ) ) ) * ( step( texCoord2_g504.x , ( temp_output_3_0_g504 / temp_output_7_0_g504 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g504.y , ( ( temp_output_9_0_g504 - 1.0 ) / temp_output_8_0_g504 ) ) ) * ( step( texCoord2_g504.y , ( temp_output_9_0_g504 / temp_output_8_0_g504 ) ) * 1.0 ) ) ) ) + ( _MRE9 * ( ( ( 1.0 - step( texCoord2_g499.x , ( ( temp_output_3_0_g499 - 1.0 ) / temp_output_7_0_g499 ) ) ) * ( step( texCoord2_g499.x , ( temp_output_3_0_g499 / temp_output_7_0_g499 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g499.y , ( ( temp_output_9_0_g499 - 1.0 ) / temp_output_8_0_g499 ) ) ) * ( step( texCoord2_g499.y , ( temp_output_9_0_g499 / temp_output_8_0_g499 ) ) * 1.0 ) ) ) ) ) );
				

				float3 BaseColor = ( clampResult310 * ( ( clampResult255 * temp_output_155_0 ) * _Tint ) ).rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = ( ( temp_output_155_0 * ( _EmissionPower * (temp_output_263_0).b ) ) + _EmissionColor ).rgb;
				float3 Specular = 0.5;
				float Metallic = (temp_output_263_0).r;
				float Smoothness = ( 1.0 - (temp_output_263_0).g );
				float Occlusion = 1;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = 0;
				#endif

				#ifdef _CLEARCOAT
					float CoatMask = 0;
					float CoatSmoothness = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;

				#ifdef _NORMALMAP
						#if _NORMAL_DROPOFF_TS
							inputData.normalWS = TransformTangentToWorld(Normal, half3x3(WorldTangent, WorldBiTangent, WorldNormal));
						#elif _NORMAL_DROPOFF_OS
							inputData.normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							inputData.normalWS = Normal;
						#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = WorldNormal;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					inputData.shadowCoord = ShadowCoords;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
				#else
					inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif

				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif
					inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)
					inputData.bakedGI = SAMPLE_GI(IN.lightmapUVOrVertexSH.xy, IN.dynamicLightmapUV.xy, SH, inputData.normalWS);
				#else
					inputData.bakedGI = SAMPLE_GI(IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS);
				#endif

				#ifdef ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif

				inputData.normalizedScreenSpaceUV = NormalizedScreenSpaceUV;
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH.xy);

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = IN.dynamicLightmapUV.xy;
					#endif
					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = IN.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
				#endif

				SurfaceData surfaceData;
				surfaceData.albedo              = BaseColor;
				surfaceData.metallic            = saturate(Metallic);
				surfaceData.specular            = Specular;
				surfaceData.smoothness          = saturate(Smoothness),
				surfaceData.occlusion           = Occlusion,
				surfaceData.emission            = Emission,
				surfaceData.alpha               = saturate(Alpha);
				surfaceData.normalTS            = Normal;
				surfaceData.clearCoatMask       = 0;
				surfaceData.clearCoatSmoothness = 1;

				#ifdef _CLEARCOAT
					surfaceData.clearCoatMask       = saturate(CoatMask);
					surfaceData.clearCoatSmoothness = saturate(CoatSmoothness);
				#endif

				#ifdef _DBUFFER
					ApplyDecalToSurfaceData(IN.clipPos, surfaceData, inputData);
				#endif

				half4 color = UniversalFragmentPBR( inputData, surfaceData);

				#ifdef ASE_TRANSMISSION
				{
					float shadow = _TransmissionShadow;

					#define SUM_LIGHT_TRANSMISSION(Light)\
						float3 atten = Light.color * Light.distanceAttenuation;\
						atten = lerp( atten, atten * Light.shadowAttenuation, shadow );\
						half3 transmission = max( 0, -dot( inputData.normalWS, Light.direction ) ) * atten * Transmission;\
						color.rgb += BaseColor * transmission;

					SUM_LIGHT_TRANSMISSION( GetMainLight( inputData.shadowCoord ) );

					#if defined(_ADDITIONAL_LIGHTS)
						uint meshRenderingLayers = GetMeshRenderingLayer();
						uint pixelLightCount = GetAdditionalLightsCount();
						#if USE_FORWARD_PLUS
							for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
							{
								FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

								Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
								#ifdef _LIGHT_LAYERS
								if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
								#endif
								{
									SUM_LIGHT_TRANSMISSION( light );
								}
							}
						#endif
						LIGHT_LOOP_BEGIN( pixelLightCount )
							Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
							#ifdef _LIGHT_LAYERS
							if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
							#endif
							{
								SUM_LIGHT_TRANSMISSION( light );
							}
						LIGHT_LOOP_END
					#endif
				}
				#endif

				#ifdef ASE_TRANSLUCENCY
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

					#define SUM_LIGHT_TRANSLUCENCY(Light)\
						float3 atten = Light.color * Light.distanceAttenuation;\
						atten = lerp( atten, atten * Light.shadowAttenuation, shadow );\
						half3 lightDir = Light.direction + inputData.normalWS * normal;\
						half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );\
						half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;\
						color.rgb += BaseColor * translucency * strength;

					SUM_LIGHT_TRANSLUCENCY( GetMainLight( inputData.shadowCoord ) );

					#if defined(_ADDITIONAL_LIGHTS)
						uint meshRenderingLayers = GetMeshRenderingLayer();
						uint pixelLightCount = GetAdditionalLightsCount();
						#if USE_FORWARD_PLUS
							for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
							{
								FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

								Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
								#ifdef _LIGHT_LAYERS
								if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
								#endif
								{
									SUM_LIGHT_TRANSLUCENCY( light );
								}
							}
						#endif
						LIGHT_LOOP_BEGIN( pixelLightCount )
							Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
							#ifdef _LIGHT_LAYERS
							if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
							#endif
							{
								SUM_LIGHT_TRANSLUCENCY( light );
							}
						LIGHT_LOOP_END
					#endif
				}
				#endif

				#ifdef ASE_REFRACTION
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, float4( WorldNormal,0 ) ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos.xy ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
				#endif

				return color;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 140008


			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#define SHADERPASS SHADERPASS_SHADOWCASTER

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD1;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DetailsUV2_ST;
			float4 _MRE8;
			float4 _MRE7;
			float4 _MRE6;
			float4 _MRE5;
			float4 _MRE4;
			float4 _MRE3;
			float4 _MRE2;
			float4 _MRE1;
			float4 _Tint;
			float4 _Color9;
			float4 _Color8;
			float4 _MRE9;
			float4 _Color7;
			float4 _Color5;
			float4 _Color4;
			float4 _Color3;
			float4 _Color2;
			float4 _Color1;
			float4 _GradientColor;
			float4 _Color6;
			float4 _EmissionColor;
			float _EmissionPower;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			float _DetailOpacity;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			// Property used by ScenePickingPass
			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			// Properties used by SceneSelectionPass
			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

			//#ifdef HAVE_VFX_MODIFICATION
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
			//#endif

			
			float3 _LightDirection;
			float3 _LightPosition;

			VertexOutput VertexFunction( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.worldPos = positionWS;
				#endif

				float3 normalWS = TransformObjectToWorldDir(v.ase_normal);

				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
					float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
					float3 lightDirectionWS = _LightDirection;
				#endif

				float4 clipPos = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

				#if UNITY_REVERSED_Z
					clipPos.z = min(clipPos.z, UNITY_NEAR_CLIP_VALUE);
				#else
					clipPos.z = max(clipPos.z, UNITY_NEAR_CLIP_VALUE);
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = clipPos;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag(	VertexOutput IN
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.worldPos;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				

				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					#ifdef _ALPHATEST_SHADOW_ON
						clip(Alpha - AlphaClipThresholdShadow);
					#else
						clip(Alpha - AlphaClipThreshold);
					#endif
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.clipPos );
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 140008


			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
			
			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DetailsUV2_ST;
			float4 _MRE8;
			float4 _MRE7;
			float4 _MRE6;
			float4 _MRE5;
			float4 _MRE4;
			float4 _MRE3;
			float4 _MRE2;
			float4 _MRE1;
			float4 _Tint;
			float4 _Color9;
			float4 _Color8;
			float4 _MRE9;
			float4 _Color7;
			float4 _Color5;
			float4 _Color4;
			float4 _Color3;
			float4 _Color2;
			float4 _Color1;
			float4 _GradientColor;
			float4 _Color6;
			float4 _EmissionColor;
			float _EmissionPower;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			float _DetailOpacity;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			// Property used by ScenePickingPass
			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			// Properties used by SceneSelectionPass
			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

			//#ifdef HAVE_VFX_MODIFICATION
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
			//#endif

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag(	VertexOutput IN
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				

				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.clipPos );
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }

			Cull Off

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 140008


			#pragma vertex vert
			#pragma fragment frag

			#pragma shader_feature EDITOR_VISUALIZATION

			#define SHADERPASS SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD1;
				#endif
				#ifdef EDITOR_VISUALIZATION
					float4 VizUV : TEXCOORD2;
					float4 LightCoord : TEXCOORD3;
				#endif
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DetailsUV2_ST;
			float4 _MRE8;
			float4 _MRE7;
			float4 _MRE6;
			float4 _MRE5;
			float4 _MRE4;
			float4 _MRE3;
			float4 _MRE2;
			float4 _MRE1;
			float4 _Tint;
			float4 _Color9;
			float4 _Color8;
			float4 _MRE9;
			float4 _Color7;
			float4 _Color5;
			float4 _Color4;
			float4 _Color3;
			float4 _Color2;
			float4 _Color1;
			float4 _GradientColor;
			float4 _Color6;
			float4 _EmissionColor;
			float _EmissionPower;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			float _DetailOpacity;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			// Property used by ScenePickingPass
			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			// Properties used by SceneSelectionPass
			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _DetailsUV2;
			sampler2D _Gradient;


			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"

			//#ifdef HAVE_VFX_MODIFICATION
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
			//#endif

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord4.xy = v.texcoord1.xy;
				o.ase_texcoord4.zw = v.texcoord0.xy;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.worldPos = positionWS;
				#endif

				o.clipPos = MetaVertexPosition( v.vertex, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );

				#ifdef EDITOR_VISUALIZATION
					float2 VizUV = 0;
					float4 LightCoord = 0;
					UnityEditorVizData(v.vertex.xyz, v.texcoord0.xy, v.texcoord1.xy, v.texcoord2.xy, VizUV, LightCoord);
					o.VizUV = float4(VizUV, 0, 0);
					o.LightCoord = LightCoord;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = o.clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.texcoord0 = v.texcoord0;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.texcoord0 = patch[0].texcoord0 * bary.x + patch[1].texcoord0 * bary.y + patch[2].texcoord0 * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.worldPos;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 uv1_DetailsUV2 = IN.ase_texcoord4.xy * _DetailsUV2_ST.xy + _DetailsUV2_ST.zw;
				float4 clampResult310 = clamp( ( tex2D( _DetailsUV2, uv1_DetailsUV2 ) + ( 1.0 - _DetailOpacity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float2 texCoord258 = IN.ase_texcoord4.zw * float2( 3,3 ) + float2( 0,0 );
				float4 clampResult206 = clamp( ( ( tex2D( _Gradient, texCoord258 ) + _GradientColor ) + ( 1.0 - _GradientIntensity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 saferPower254 = abs( (clampResult206*_GradientScale + _GradientOffset) );
				float4 temp_cast_0 = (_GradientPower).xxxx;
				float2 texCoord2_g512 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g512 = 1.0;
				float temp_output_7_0_g512 = 3.0;
				float temp_output_9_0_g512 = 3.0;
				float temp_output_8_0_g512 = 3.0;
				float2 texCoord2_g511 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g511 = 2.0;
				float temp_output_7_0_g511 = 3.0;
				float temp_output_9_0_g511 = 3.0;
				float temp_output_8_0_g511 = 3.0;
				float2 texCoord2_g507 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g507 = 3.0;
				float temp_output_7_0_g507 = 3.0;
				float temp_output_9_0_g507 = 3.0;
				float temp_output_8_0_g507 = 3.0;
				float2 texCoord2_g506 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g506 = 1.0;
				float temp_output_7_0_g506 = 3.0;
				float temp_output_9_0_g506 = 2.0;
				float temp_output_8_0_g506 = 3.0;
				float2 texCoord2_g509 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g509 = 2.0;
				float temp_output_7_0_g509 = 3.0;
				float temp_output_9_0_g509 = 2.0;
				float temp_output_8_0_g509 = 3.0;
				float2 texCoord2_g508 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g508 = 3.0;
				float temp_output_7_0_g508 = 3.0;
				float temp_output_9_0_g508 = 2.0;
				float temp_output_8_0_g508 = 3.0;
				float2 texCoord2_g514 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g514 = 1.0;
				float temp_output_7_0_g514 = 3.0;
				float temp_output_9_0_g514 = 1.0;
				float temp_output_8_0_g514 = 3.0;
				float2 texCoord2_g513 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g513 = 2.0;
				float temp_output_7_0_g513 = 3.0;
				float temp_output_9_0_g513 = 1.0;
				float temp_output_8_0_g513 = 3.0;
				float2 texCoord2_g510 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g510 = 3.0;
				float temp_output_7_0_g510 = 3.0;
				float temp_output_9_0_g510 = 1.0;
				float temp_output_8_0_g510 = 3.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( texCoord2_g512.x , ( ( temp_output_3_0_g512 - 1.0 ) / temp_output_7_0_g512 ) ) ) * ( step( texCoord2_g512.x , ( temp_output_3_0_g512 / temp_output_7_0_g512 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g512.y , ( ( temp_output_9_0_g512 - 1.0 ) / temp_output_8_0_g512 ) ) ) * ( step( texCoord2_g512.y , ( temp_output_9_0_g512 / temp_output_8_0_g512 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( texCoord2_g511.x , ( ( temp_output_3_0_g511 - 1.0 ) / temp_output_7_0_g511 ) ) ) * ( step( texCoord2_g511.x , ( temp_output_3_0_g511 / temp_output_7_0_g511 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g511.y , ( ( temp_output_9_0_g511 - 1.0 ) / temp_output_8_0_g511 ) ) ) * ( step( texCoord2_g511.y , ( temp_output_9_0_g511 / temp_output_8_0_g511 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( texCoord2_g507.x , ( ( temp_output_3_0_g507 - 1.0 ) / temp_output_7_0_g507 ) ) ) * ( step( texCoord2_g507.x , ( temp_output_3_0_g507 / temp_output_7_0_g507 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g507.y , ( ( temp_output_9_0_g507 - 1.0 ) / temp_output_8_0_g507 ) ) ) * ( step( texCoord2_g507.y , ( temp_output_9_0_g507 / temp_output_8_0_g507 ) ) * 1.0 ) ) ) ) ) + ( ( _Color4 * ( ( ( 1.0 - step( texCoord2_g506.x , ( ( temp_output_3_0_g506 - 1.0 ) / temp_output_7_0_g506 ) ) ) * ( step( texCoord2_g506.x , ( temp_output_3_0_g506 / temp_output_7_0_g506 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g506.y , ( ( temp_output_9_0_g506 - 1.0 ) / temp_output_8_0_g506 ) ) ) * ( step( texCoord2_g506.y , ( temp_output_9_0_g506 / temp_output_8_0_g506 ) ) * 1.0 ) ) ) ) + ( _Color5 * ( ( ( 1.0 - step( texCoord2_g509.x , ( ( temp_output_3_0_g509 - 1.0 ) / temp_output_7_0_g509 ) ) ) * ( step( texCoord2_g509.x , ( temp_output_3_0_g509 / temp_output_7_0_g509 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g509.y , ( ( temp_output_9_0_g509 - 1.0 ) / temp_output_8_0_g509 ) ) ) * ( step( texCoord2_g509.y , ( temp_output_9_0_g509 / temp_output_8_0_g509 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( texCoord2_g508.x , ( ( temp_output_3_0_g508 - 1.0 ) / temp_output_7_0_g508 ) ) ) * ( step( texCoord2_g508.x , ( temp_output_3_0_g508 / temp_output_7_0_g508 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g508.y , ( ( temp_output_9_0_g508 - 1.0 ) / temp_output_8_0_g508 ) ) ) * ( step( texCoord2_g508.y , ( temp_output_9_0_g508 / temp_output_8_0_g508 ) ) * 1.0 ) ) ) ) ) + ( ( _Color7 * ( ( ( 1.0 - step( texCoord2_g514.x , ( ( temp_output_3_0_g514 - 1.0 ) / temp_output_7_0_g514 ) ) ) * ( step( texCoord2_g514.x , ( temp_output_3_0_g514 / temp_output_7_0_g514 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g514.y , ( ( temp_output_9_0_g514 - 1.0 ) / temp_output_8_0_g514 ) ) ) * ( step( texCoord2_g514.y , ( temp_output_9_0_g514 / temp_output_8_0_g514 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( texCoord2_g513.x , ( ( temp_output_3_0_g513 - 1.0 ) / temp_output_7_0_g513 ) ) ) * ( step( texCoord2_g513.x , ( temp_output_3_0_g513 / temp_output_7_0_g513 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g513.y , ( ( temp_output_9_0_g513 - 1.0 ) / temp_output_8_0_g513 ) ) ) * ( step( texCoord2_g513.y , ( temp_output_9_0_g513 / temp_output_8_0_g513 ) ) * 1.0 ) ) ) ) + ( _Color9 * ( ( ( 1.0 - step( texCoord2_g510.x , ( ( temp_output_3_0_g510 - 1.0 ) / temp_output_7_0_g510 ) ) ) * ( step( texCoord2_g510.x , ( temp_output_3_0_g510 / temp_output_7_0_g510 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g510.y , ( ( temp_output_9_0_g510 - 1.0 ) / temp_output_8_0_g510 ) ) ) * ( step( texCoord2_g510.y , ( temp_output_9_0_g510 / temp_output_8_0_g510 ) ) * 1.0 ) ) ) ) ) );
				float4 clampResult255 = clamp( ( pow( saferPower254 , temp_cast_0 ) + ( 1.0 - (temp_output_155_0).a ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				
				float2 texCoord2_g498 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g498 = 1.0;
				float temp_output_7_0_g498 = 3.0;
				float temp_output_9_0_g498 = 3.0;
				float temp_output_8_0_g498 = 3.0;
				float2 texCoord2_g501 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g501 = 2.0;
				float temp_output_7_0_g501 = 3.0;
				float temp_output_9_0_g501 = 3.0;
				float temp_output_8_0_g501 = 3.0;
				float2 texCoord2_g505 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g505 = 3.0;
				float temp_output_7_0_g505 = 3.0;
				float temp_output_9_0_g505 = 3.0;
				float temp_output_8_0_g505 = 3.0;
				float2 texCoord2_g497 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g497 = 1.0;
				float temp_output_7_0_g497 = 3.0;
				float temp_output_9_0_g497 = 2.0;
				float temp_output_8_0_g497 = 3.0;
				float2 texCoord2_g502 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g502 = 2.0;
				float temp_output_7_0_g502 = 3.0;
				float temp_output_9_0_g502 = 2.0;
				float temp_output_8_0_g502 = 3.0;
				float2 texCoord2_g503 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g503 = 3.0;
				float temp_output_7_0_g503 = 3.0;
				float temp_output_9_0_g503 = 2.0;
				float temp_output_8_0_g503 = 3.0;
				float2 texCoord2_g500 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g500 = 1.0;
				float temp_output_7_0_g500 = 3.0;
				float temp_output_9_0_g500 = 1.0;
				float temp_output_8_0_g500 = 3.0;
				float2 texCoord2_g504 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g504 = 2.0;
				float temp_output_7_0_g504 = 3.0;
				float temp_output_9_0_g504 = 1.0;
				float temp_output_8_0_g504 = 3.0;
				float2 texCoord2_g499 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g499 = 3.0;
				float temp_output_7_0_g499 = 3.0;
				float temp_output_9_0_g499 = 1.0;
				float temp_output_8_0_g499 = 3.0;
				float4 temp_output_263_0 = ( ( ( _MRE1 * ( ( ( 1.0 - step( texCoord2_g498.x , ( ( temp_output_3_0_g498 - 1.0 ) / temp_output_7_0_g498 ) ) ) * ( step( texCoord2_g498.x , ( temp_output_3_0_g498 / temp_output_7_0_g498 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g498.y , ( ( temp_output_9_0_g498 - 1.0 ) / temp_output_8_0_g498 ) ) ) * ( step( texCoord2_g498.y , ( temp_output_9_0_g498 / temp_output_8_0_g498 ) ) * 1.0 ) ) ) ) + ( _MRE2 * ( ( ( 1.0 - step( texCoord2_g501.x , ( ( temp_output_3_0_g501 - 1.0 ) / temp_output_7_0_g501 ) ) ) * ( step( texCoord2_g501.x , ( temp_output_3_0_g501 / temp_output_7_0_g501 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g501.y , ( ( temp_output_9_0_g501 - 1.0 ) / temp_output_8_0_g501 ) ) ) * ( step( texCoord2_g501.y , ( temp_output_9_0_g501 / temp_output_8_0_g501 ) ) * 1.0 ) ) ) ) + ( _MRE3 * ( ( ( 1.0 - step( texCoord2_g505.x , ( ( temp_output_3_0_g505 - 1.0 ) / temp_output_7_0_g505 ) ) ) * ( step( texCoord2_g505.x , ( temp_output_3_0_g505 / temp_output_7_0_g505 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g505.y , ( ( temp_output_9_0_g505 - 1.0 ) / temp_output_8_0_g505 ) ) ) * ( step( texCoord2_g505.y , ( temp_output_9_0_g505 / temp_output_8_0_g505 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE4 * ( ( ( 1.0 - step( texCoord2_g497.x , ( ( temp_output_3_0_g497 - 1.0 ) / temp_output_7_0_g497 ) ) ) * ( step( texCoord2_g497.x , ( temp_output_3_0_g497 / temp_output_7_0_g497 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g497.y , ( ( temp_output_9_0_g497 - 1.0 ) / temp_output_8_0_g497 ) ) ) * ( step( texCoord2_g497.y , ( temp_output_9_0_g497 / temp_output_8_0_g497 ) ) * 1.0 ) ) ) ) + ( _MRE5 * ( ( ( 1.0 - step( texCoord2_g502.x , ( ( temp_output_3_0_g502 - 1.0 ) / temp_output_7_0_g502 ) ) ) * ( step( texCoord2_g502.x , ( temp_output_3_0_g502 / temp_output_7_0_g502 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g502.y , ( ( temp_output_9_0_g502 - 1.0 ) / temp_output_8_0_g502 ) ) ) * ( step( texCoord2_g502.y , ( temp_output_9_0_g502 / temp_output_8_0_g502 ) ) * 1.0 ) ) ) ) + ( _MRE6 * ( ( ( 1.0 - step( texCoord2_g503.x , ( ( temp_output_3_0_g503 - 1.0 ) / temp_output_7_0_g503 ) ) ) * ( step( texCoord2_g503.x , ( temp_output_3_0_g503 / temp_output_7_0_g503 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g503.y , ( ( temp_output_9_0_g503 - 1.0 ) / temp_output_8_0_g503 ) ) ) * ( step( texCoord2_g503.y , ( temp_output_9_0_g503 / temp_output_8_0_g503 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE7 * ( ( ( 1.0 - step( texCoord2_g500.x , ( ( temp_output_3_0_g500 - 1.0 ) / temp_output_7_0_g500 ) ) ) * ( step( texCoord2_g500.x , ( temp_output_3_0_g500 / temp_output_7_0_g500 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g500.y , ( ( temp_output_9_0_g500 - 1.0 ) / temp_output_8_0_g500 ) ) ) * ( step( texCoord2_g500.y , ( temp_output_9_0_g500 / temp_output_8_0_g500 ) ) * 1.0 ) ) ) ) + ( _MRE8 * ( ( ( 1.0 - step( texCoord2_g504.x , ( ( temp_output_3_0_g504 - 1.0 ) / temp_output_7_0_g504 ) ) ) * ( step( texCoord2_g504.x , ( temp_output_3_0_g504 / temp_output_7_0_g504 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g504.y , ( ( temp_output_9_0_g504 - 1.0 ) / temp_output_8_0_g504 ) ) ) * ( step( texCoord2_g504.y , ( temp_output_9_0_g504 / temp_output_8_0_g504 ) ) * 1.0 ) ) ) ) + ( _MRE9 * ( ( ( 1.0 - step( texCoord2_g499.x , ( ( temp_output_3_0_g499 - 1.0 ) / temp_output_7_0_g499 ) ) ) * ( step( texCoord2_g499.x , ( temp_output_3_0_g499 / temp_output_7_0_g499 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g499.y , ( ( temp_output_9_0_g499 - 1.0 ) / temp_output_8_0_g499 ) ) ) * ( step( texCoord2_g499.y , ( temp_output_9_0_g499 / temp_output_8_0_g499 ) ) * 1.0 ) ) ) ) ) );
				

				float3 BaseColor = ( clampResult310 * ( ( clampResult255 * temp_output_155_0 ) * _Tint ) ).rgb;
				float3 Emission = ( ( temp_output_155_0 * ( _EmissionPower * (temp_output_263_0).b ) ) + _EmissionColor ).rgb;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = BaseColor;
				metaInput.Emission = Emission;
				#ifdef EDITOR_VISUALIZATION
					metaInput.VizUV = IN.VizUV.xy;
					metaInput.LightCoord = IN.LightCoord;
				#endif

				return UnityMetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 140008


			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_2D

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DetailsUV2_ST;
			float4 _MRE8;
			float4 _MRE7;
			float4 _MRE6;
			float4 _MRE5;
			float4 _MRE4;
			float4 _MRE3;
			float4 _MRE2;
			float4 _MRE1;
			float4 _Tint;
			float4 _Color9;
			float4 _Color8;
			float4 _MRE9;
			float4 _Color7;
			float4 _Color5;
			float4 _Color4;
			float4 _Color3;
			float4 _Color2;
			float4 _Color1;
			float4 _GradientColor;
			float4 _Color6;
			float4 _EmissionColor;
			float _EmissionPower;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			float _DetailOpacity;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			// Property used by ScenePickingPass
			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			// Properties used by SceneSelectionPass
			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _DetailsUV2;
			sampler2D _Gradient;


			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"

			//#ifdef HAVE_VFX_MODIFICATION
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
			//#endif

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				o.ase_texcoord2.xy = v.ase_texcoord1.xy;
				o.ase_texcoord2.zw = v.ase_texcoord.xy;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord1 = v.ase_texcoord1;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.worldPos;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 uv1_DetailsUV2 = IN.ase_texcoord2.xy * _DetailsUV2_ST.xy + _DetailsUV2_ST.zw;
				float4 clampResult310 = clamp( ( tex2D( _DetailsUV2, uv1_DetailsUV2 ) + ( 1.0 - _DetailOpacity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float2 texCoord258 = IN.ase_texcoord2.zw * float2( 3,3 ) + float2( 0,0 );
				float4 clampResult206 = clamp( ( ( tex2D( _Gradient, texCoord258 ) + _GradientColor ) + ( 1.0 - _GradientIntensity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 saferPower254 = abs( (clampResult206*_GradientScale + _GradientOffset) );
				float4 temp_cast_0 = (_GradientPower).xxxx;
				float2 texCoord2_g512 = IN.ase_texcoord2.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g512 = 1.0;
				float temp_output_7_0_g512 = 3.0;
				float temp_output_9_0_g512 = 3.0;
				float temp_output_8_0_g512 = 3.0;
				float2 texCoord2_g511 = IN.ase_texcoord2.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g511 = 2.0;
				float temp_output_7_0_g511 = 3.0;
				float temp_output_9_0_g511 = 3.0;
				float temp_output_8_0_g511 = 3.0;
				float2 texCoord2_g507 = IN.ase_texcoord2.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g507 = 3.0;
				float temp_output_7_0_g507 = 3.0;
				float temp_output_9_0_g507 = 3.0;
				float temp_output_8_0_g507 = 3.0;
				float2 texCoord2_g506 = IN.ase_texcoord2.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g506 = 1.0;
				float temp_output_7_0_g506 = 3.0;
				float temp_output_9_0_g506 = 2.0;
				float temp_output_8_0_g506 = 3.0;
				float2 texCoord2_g509 = IN.ase_texcoord2.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g509 = 2.0;
				float temp_output_7_0_g509 = 3.0;
				float temp_output_9_0_g509 = 2.0;
				float temp_output_8_0_g509 = 3.0;
				float2 texCoord2_g508 = IN.ase_texcoord2.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g508 = 3.0;
				float temp_output_7_0_g508 = 3.0;
				float temp_output_9_0_g508 = 2.0;
				float temp_output_8_0_g508 = 3.0;
				float2 texCoord2_g514 = IN.ase_texcoord2.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g514 = 1.0;
				float temp_output_7_0_g514 = 3.0;
				float temp_output_9_0_g514 = 1.0;
				float temp_output_8_0_g514 = 3.0;
				float2 texCoord2_g513 = IN.ase_texcoord2.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g513 = 2.0;
				float temp_output_7_0_g513 = 3.0;
				float temp_output_9_0_g513 = 1.0;
				float temp_output_8_0_g513 = 3.0;
				float2 texCoord2_g510 = IN.ase_texcoord2.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g510 = 3.0;
				float temp_output_7_0_g510 = 3.0;
				float temp_output_9_0_g510 = 1.0;
				float temp_output_8_0_g510 = 3.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( texCoord2_g512.x , ( ( temp_output_3_0_g512 - 1.0 ) / temp_output_7_0_g512 ) ) ) * ( step( texCoord2_g512.x , ( temp_output_3_0_g512 / temp_output_7_0_g512 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g512.y , ( ( temp_output_9_0_g512 - 1.0 ) / temp_output_8_0_g512 ) ) ) * ( step( texCoord2_g512.y , ( temp_output_9_0_g512 / temp_output_8_0_g512 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( texCoord2_g511.x , ( ( temp_output_3_0_g511 - 1.0 ) / temp_output_7_0_g511 ) ) ) * ( step( texCoord2_g511.x , ( temp_output_3_0_g511 / temp_output_7_0_g511 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g511.y , ( ( temp_output_9_0_g511 - 1.0 ) / temp_output_8_0_g511 ) ) ) * ( step( texCoord2_g511.y , ( temp_output_9_0_g511 / temp_output_8_0_g511 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( texCoord2_g507.x , ( ( temp_output_3_0_g507 - 1.0 ) / temp_output_7_0_g507 ) ) ) * ( step( texCoord2_g507.x , ( temp_output_3_0_g507 / temp_output_7_0_g507 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g507.y , ( ( temp_output_9_0_g507 - 1.0 ) / temp_output_8_0_g507 ) ) ) * ( step( texCoord2_g507.y , ( temp_output_9_0_g507 / temp_output_8_0_g507 ) ) * 1.0 ) ) ) ) ) + ( ( _Color4 * ( ( ( 1.0 - step( texCoord2_g506.x , ( ( temp_output_3_0_g506 - 1.0 ) / temp_output_7_0_g506 ) ) ) * ( step( texCoord2_g506.x , ( temp_output_3_0_g506 / temp_output_7_0_g506 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g506.y , ( ( temp_output_9_0_g506 - 1.0 ) / temp_output_8_0_g506 ) ) ) * ( step( texCoord2_g506.y , ( temp_output_9_0_g506 / temp_output_8_0_g506 ) ) * 1.0 ) ) ) ) + ( _Color5 * ( ( ( 1.0 - step( texCoord2_g509.x , ( ( temp_output_3_0_g509 - 1.0 ) / temp_output_7_0_g509 ) ) ) * ( step( texCoord2_g509.x , ( temp_output_3_0_g509 / temp_output_7_0_g509 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g509.y , ( ( temp_output_9_0_g509 - 1.0 ) / temp_output_8_0_g509 ) ) ) * ( step( texCoord2_g509.y , ( temp_output_9_0_g509 / temp_output_8_0_g509 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( texCoord2_g508.x , ( ( temp_output_3_0_g508 - 1.0 ) / temp_output_7_0_g508 ) ) ) * ( step( texCoord2_g508.x , ( temp_output_3_0_g508 / temp_output_7_0_g508 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g508.y , ( ( temp_output_9_0_g508 - 1.0 ) / temp_output_8_0_g508 ) ) ) * ( step( texCoord2_g508.y , ( temp_output_9_0_g508 / temp_output_8_0_g508 ) ) * 1.0 ) ) ) ) ) + ( ( _Color7 * ( ( ( 1.0 - step( texCoord2_g514.x , ( ( temp_output_3_0_g514 - 1.0 ) / temp_output_7_0_g514 ) ) ) * ( step( texCoord2_g514.x , ( temp_output_3_0_g514 / temp_output_7_0_g514 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g514.y , ( ( temp_output_9_0_g514 - 1.0 ) / temp_output_8_0_g514 ) ) ) * ( step( texCoord2_g514.y , ( temp_output_9_0_g514 / temp_output_8_0_g514 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( texCoord2_g513.x , ( ( temp_output_3_0_g513 - 1.0 ) / temp_output_7_0_g513 ) ) ) * ( step( texCoord2_g513.x , ( temp_output_3_0_g513 / temp_output_7_0_g513 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g513.y , ( ( temp_output_9_0_g513 - 1.0 ) / temp_output_8_0_g513 ) ) ) * ( step( texCoord2_g513.y , ( temp_output_9_0_g513 / temp_output_8_0_g513 ) ) * 1.0 ) ) ) ) + ( _Color9 * ( ( ( 1.0 - step( texCoord2_g510.x , ( ( temp_output_3_0_g510 - 1.0 ) / temp_output_7_0_g510 ) ) ) * ( step( texCoord2_g510.x , ( temp_output_3_0_g510 / temp_output_7_0_g510 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g510.y , ( ( temp_output_9_0_g510 - 1.0 ) / temp_output_8_0_g510 ) ) ) * ( step( texCoord2_g510.y , ( temp_output_9_0_g510 / temp_output_8_0_g510 ) ) * 1.0 ) ) ) ) ) );
				float4 clampResult255 = clamp( ( pow( saferPower254 , temp_cast_0 ) + ( 1.0 - (temp_output_155_0).a ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				

				float3 BaseColor = ( clampResult310 * ( ( clampResult255 * temp_output_155_0 ) * _Tint ) ).rgb;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				half4 color = half4(BaseColor, Alpha );

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				return color;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }

			ZWrite On
			Blend One Zero
			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 140008


			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD1;
				#endif
				float3 worldNormal : TEXCOORD2;
				float4 worldTangent : TEXCOORD3;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DetailsUV2_ST;
			float4 _MRE8;
			float4 _MRE7;
			float4 _MRE6;
			float4 _MRE5;
			float4 _MRE4;
			float4 _MRE3;
			float4 _MRE2;
			float4 _MRE1;
			float4 _Tint;
			float4 _Color9;
			float4 _Color8;
			float4 _MRE9;
			float4 _Color7;
			float4 _Color5;
			float4 _Color4;
			float4 _Color3;
			float4 _Color2;
			float4 _Color1;
			float4 _GradientColor;
			float4 _Color6;
			float4 _EmissionColor;
			float _EmissionPower;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			float _DetailOpacity;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			// Property used by ScenePickingPass
			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			// Properties used by SceneSelectionPass
			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"

			//#ifdef HAVE_VFX_MODIFICATION
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
			//#endif

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 normalWS = TransformObjectToWorldNormal( v.ase_normal );
				float4 tangentWS = float4(TransformObjectToWorldDir( v.ase_tangent.xyz), v.ase_tangent.w);
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.worldPos = positionWS;
				#endif

				o.worldNormal = normalWS;
				o.worldTangent = tangentWS;

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			void frag(	VertexOutput IN
						, out half4 outNormalWS : SV_Target0
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						, out float4 outRenderingLayers : SV_Target1
						#endif
						 )
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.worldPos;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				float3 WorldNormal = IN.worldNormal;
				float4 WorldTangent = IN.worldTangent;

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				

				float3 Normal = float3(0, 0, 1);
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.clipPos );
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				#if defined(_GBUFFER_NORMALS_OCT)
					float2 octNormalWS = PackNormalOctQuadEncode(WorldNormal);
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
					outNormalWS = half4(packedNormalWS, 0.0);
				#else
					#if defined(_NORMALMAP)
						#if _NORMAL_DROPOFF_TS
							float crossSign = (WorldTangent.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
							float3 bitangent = crossSign * cross(WorldNormal.xyz, WorldTangent.xyz);
							float3 normalWS = TransformTangentToWorld(Normal, half3x3(WorldTangent.xyz, bitangent, WorldNormal.xyz));
						#elif _NORMAL_DROPOFF_OS
							float3 normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							float3 normalWS = Normal;
						#endif
					#else
						float3 normalWS = WorldNormal;
					#endif
					outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "GBuffer"
			Tags { "LightMode"="UniversalGBuffer" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 140008


			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile_fragment _ _RENDER_PASS_ENABLED

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
			#pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_GBUFFER

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
			
			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
				#define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 fogFactorAndVertexLight : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD6;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
				float2 dynamicLightmapUV : TEXCOORD7;
				#endif
				float4 ase_texcoord8 : TEXCOORD8;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DetailsUV2_ST;
			float4 _MRE8;
			float4 _MRE7;
			float4 _MRE6;
			float4 _MRE5;
			float4 _MRE4;
			float4 _MRE3;
			float4 _MRE2;
			float4 _MRE1;
			float4 _Tint;
			float4 _Color9;
			float4 _Color8;
			float4 _MRE9;
			float4 _Color7;
			float4 _Color5;
			float4 _Color4;
			float4 _Color3;
			float4 _Color2;
			float4 _Color1;
			float4 _GradientColor;
			float4 _Color6;
			float4 _EmissionColor;
			float _EmissionPower;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			float _DetailOpacity;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			// Property used by ScenePickingPass
			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			// Properties used by SceneSelectionPass
			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _DetailsUV2;
			sampler2D _Gradient;


			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRGBufferPass.hlsl"

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord8.xy = v.texcoord1.xy;
				o.ase_texcoord8.zw = v.texcoord.xy;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				#if defined(LIGHTMAP_ON)
					OUTPUT_LIGHTMAP_UV(v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy);
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)
					o.dynamicLightmapUV.xy = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif

				#if !defined(LIGHTMAP_ON)
					OUTPUT_SH(normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz);
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord.xy;
					o.lightmapUVOrVertexSH.xy = v.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );

				o.fogFactorAndVertexLight = half4(0, vertexLight);

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

					o.clipPos = positionCS;

				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
					o.screenPos = ComputeScreenPos(positionCS);
				#endif

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			FragmentOutput frag ( VertexOutput IN
								#ifdef ASE_DEPTH_WRITE_ON
								,out float outputDepth : ASE_SV_DEPTH
								#endif
								 )
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.clipPos );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif

				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
					float4 ScreenPos = IN.screenPos;
				#endif

				float2 NormalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.clipPos);

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#else
					ShadowCoords = float4(0, 0, 0, 0);
				#endif

				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float2 uv1_DetailsUV2 = IN.ase_texcoord8.xy * _DetailsUV2_ST.xy + _DetailsUV2_ST.zw;
				float4 clampResult310 = clamp( ( tex2D( _DetailsUV2, uv1_DetailsUV2 ) + ( 1.0 - _DetailOpacity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float2 texCoord258 = IN.ase_texcoord8.zw * float2( 3,3 ) + float2( 0,0 );
				float4 clampResult206 = clamp( ( ( tex2D( _Gradient, texCoord258 ) + _GradientColor ) + ( 1.0 - _GradientIntensity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 saferPower254 = abs( (clampResult206*_GradientScale + _GradientOffset) );
				float4 temp_cast_0 = (_GradientPower).xxxx;
				float2 texCoord2_g512 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g512 = 1.0;
				float temp_output_7_0_g512 = 3.0;
				float temp_output_9_0_g512 = 3.0;
				float temp_output_8_0_g512 = 3.0;
				float2 texCoord2_g511 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g511 = 2.0;
				float temp_output_7_0_g511 = 3.0;
				float temp_output_9_0_g511 = 3.0;
				float temp_output_8_0_g511 = 3.0;
				float2 texCoord2_g507 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g507 = 3.0;
				float temp_output_7_0_g507 = 3.0;
				float temp_output_9_0_g507 = 3.0;
				float temp_output_8_0_g507 = 3.0;
				float2 texCoord2_g506 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g506 = 1.0;
				float temp_output_7_0_g506 = 3.0;
				float temp_output_9_0_g506 = 2.0;
				float temp_output_8_0_g506 = 3.0;
				float2 texCoord2_g509 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g509 = 2.0;
				float temp_output_7_0_g509 = 3.0;
				float temp_output_9_0_g509 = 2.0;
				float temp_output_8_0_g509 = 3.0;
				float2 texCoord2_g508 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g508 = 3.0;
				float temp_output_7_0_g508 = 3.0;
				float temp_output_9_0_g508 = 2.0;
				float temp_output_8_0_g508 = 3.0;
				float2 texCoord2_g514 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g514 = 1.0;
				float temp_output_7_0_g514 = 3.0;
				float temp_output_9_0_g514 = 1.0;
				float temp_output_8_0_g514 = 3.0;
				float2 texCoord2_g513 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g513 = 2.0;
				float temp_output_7_0_g513 = 3.0;
				float temp_output_9_0_g513 = 1.0;
				float temp_output_8_0_g513 = 3.0;
				float2 texCoord2_g510 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g510 = 3.0;
				float temp_output_7_0_g510 = 3.0;
				float temp_output_9_0_g510 = 1.0;
				float temp_output_8_0_g510 = 3.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( texCoord2_g512.x , ( ( temp_output_3_0_g512 - 1.0 ) / temp_output_7_0_g512 ) ) ) * ( step( texCoord2_g512.x , ( temp_output_3_0_g512 / temp_output_7_0_g512 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g512.y , ( ( temp_output_9_0_g512 - 1.0 ) / temp_output_8_0_g512 ) ) ) * ( step( texCoord2_g512.y , ( temp_output_9_0_g512 / temp_output_8_0_g512 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( texCoord2_g511.x , ( ( temp_output_3_0_g511 - 1.0 ) / temp_output_7_0_g511 ) ) ) * ( step( texCoord2_g511.x , ( temp_output_3_0_g511 / temp_output_7_0_g511 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g511.y , ( ( temp_output_9_0_g511 - 1.0 ) / temp_output_8_0_g511 ) ) ) * ( step( texCoord2_g511.y , ( temp_output_9_0_g511 / temp_output_8_0_g511 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( texCoord2_g507.x , ( ( temp_output_3_0_g507 - 1.0 ) / temp_output_7_0_g507 ) ) ) * ( step( texCoord2_g507.x , ( temp_output_3_0_g507 / temp_output_7_0_g507 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g507.y , ( ( temp_output_9_0_g507 - 1.0 ) / temp_output_8_0_g507 ) ) ) * ( step( texCoord2_g507.y , ( temp_output_9_0_g507 / temp_output_8_0_g507 ) ) * 1.0 ) ) ) ) ) + ( ( _Color4 * ( ( ( 1.0 - step( texCoord2_g506.x , ( ( temp_output_3_0_g506 - 1.0 ) / temp_output_7_0_g506 ) ) ) * ( step( texCoord2_g506.x , ( temp_output_3_0_g506 / temp_output_7_0_g506 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g506.y , ( ( temp_output_9_0_g506 - 1.0 ) / temp_output_8_0_g506 ) ) ) * ( step( texCoord2_g506.y , ( temp_output_9_0_g506 / temp_output_8_0_g506 ) ) * 1.0 ) ) ) ) + ( _Color5 * ( ( ( 1.0 - step( texCoord2_g509.x , ( ( temp_output_3_0_g509 - 1.0 ) / temp_output_7_0_g509 ) ) ) * ( step( texCoord2_g509.x , ( temp_output_3_0_g509 / temp_output_7_0_g509 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g509.y , ( ( temp_output_9_0_g509 - 1.0 ) / temp_output_8_0_g509 ) ) ) * ( step( texCoord2_g509.y , ( temp_output_9_0_g509 / temp_output_8_0_g509 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( texCoord2_g508.x , ( ( temp_output_3_0_g508 - 1.0 ) / temp_output_7_0_g508 ) ) ) * ( step( texCoord2_g508.x , ( temp_output_3_0_g508 / temp_output_7_0_g508 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g508.y , ( ( temp_output_9_0_g508 - 1.0 ) / temp_output_8_0_g508 ) ) ) * ( step( texCoord2_g508.y , ( temp_output_9_0_g508 / temp_output_8_0_g508 ) ) * 1.0 ) ) ) ) ) + ( ( _Color7 * ( ( ( 1.0 - step( texCoord2_g514.x , ( ( temp_output_3_0_g514 - 1.0 ) / temp_output_7_0_g514 ) ) ) * ( step( texCoord2_g514.x , ( temp_output_3_0_g514 / temp_output_7_0_g514 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g514.y , ( ( temp_output_9_0_g514 - 1.0 ) / temp_output_8_0_g514 ) ) ) * ( step( texCoord2_g514.y , ( temp_output_9_0_g514 / temp_output_8_0_g514 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( texCoord2_g513.x , ( ( temp_output_3_0_g513 - 1.0 ) / temp_output_7_0_g513 ) ) ) * ( step( texCoord2_g513.x , ( temp_output_3_0_g513 / temp_output_7_0_g513 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g513.y , ( ( temp_output_9_0_g513 - 1.0 ) / temp_output_8_0_g513 ) ) ) * ( step( texCoord2_g513.y , ( temp_output_9_0_g513 / temp_output_8_0_g513 ) ) * 1.0 ) ) ) ) + ( _Color9 * ( ( ( 1.0 - step( texCoord2_g510.x , ( ( temp_output_3_0_g510 - 1.0 ) / temp_output_7_0_g510 ) ) ) * ( step( texCoord2_g510.x , ( temp_output_3_0_g510 / temp_output_7_0_g510 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g510.y , ( ( temp_output_9_0_g510 - 1.0 ) / temp_output_8_0_g510 ) ) ) * ( step( texCoord2_g510.y , ( temp_output_9_0_g510 / temp_output_8_0_g510 ) ) * 1.0 ) ) ) ) ) );
				float4 clampResult255 = clamp( ( pow( saferPower254 , temp_cast_0 ) + ( 1.0 - (temp_output_155_0).a ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				
				float2 texCoord2_g498 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g498 = 1.0;
				float temp_output_7_0_g498 = 3.0;
				float temp_output_9_0_g498 = 3.0;
				float temp_output_8_0_g498 = 3.0;
				float2 texCoord2_g501 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g501 = 2.0;
				float temp_output_7_0_g501 = 3.0;
				float temp_output_9_0_g501 = 3.0;
				float temp_output_8_0_g501 = 3.0;
				float2 texCoord2_g505 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g505 = 3.0;
				float temp_output_7_0_g505 = 3.0;
				float temp_output_9_0_g505 = 3.0;
				float temp_output_8_0_g505 = 3.0;
				float2 texCoord2_g497 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g497 = 1.0;
				float temp_output_7_0_g497 = 3.0;
				float temp_output_9_0_g497 = 2.0;
				float temp_output_8_0_g497 = 3.0;
				float2 texCoord2_g502 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g502 = 2.0;
				float temp_output_7_0_g502 = 3.0;
				float temp_output_9_0_g502 = 2.0;
				float temp_output_8_0_g502 = 3.0;
				float2 texCoord2_g503 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g503 = 3.0;
				float temp_output_7_0_g503 = 3.0;
				float temp_output_9_0_g503 = 2.0;
				float temp_output_8_0_g503 = 3.0;
				float2 texCoord2_g500 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g500 = 1.0;
				float temp_output_7_0_g500 = 3.0;
				float temp_output_9_0_g500 = 1.0;
				float temp_output_8_0_g500 = 3.0;
				float2 texCoord2_g504 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g504 = 2.0;
				float temp_output_7_0_g504 = 3.0;
				float temp_output_9_0_g504 = 1.0;
				float temp_output_8_0_g504 = 3.0;
				float2 texCoord2_g499 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g499 = 3.0;
				float temp_output_7_0_g499 = 3.0;
				float temp_output_9_0_g499 = 1.0;
				float temp_output_8_0_g499 = 3.0;
				float4 temp_output_263_0 = ( ( ( _MRE1 * ( ( ( 1.0 - step( texCoord2_g498.x , ( ( temp_output_3_0_g498 - 1.0 ) / temp_output_7_0_g498 ) ) ) * ( step( texCoord2_g498.x , ( temp_output_3_0_g498 / temp_output_7_0_g498 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g498.y , ( ( temp_output_9_0_g498 - 1.0 ) / temp_output_8_0_g498 ) ) ) * ( step( texCoord2_g498.y , ( temp_output_9_0_g498 / temp_output_8_0_g498 ) ) * 1.0 ) ) ) ) + ( _MRE2 * ( ( ( 1.0 - step( texCoord2_g501.x , ( ( temp_output_3_0_g501 - 1.0 ) / temp_output_7_0_g501 ) ) ) * ( step( texCoord2_g501.x , ( temp_output_3_0_g501 / temp_output_7_0_g501 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g501.y , ( ( temp_output_9_0_g501 - 1.0 ) / temp_output_8_0_g501 ) ) ) * ( step( texCoord2_g501.y , ( temp_output_9_0_g501 / temp_output_8_0_g501 ) ) * 1.0 ) ) ) ) + ( _MRE3 * ( ( ( 1.0 - step( texCoord2_g505.x , ( ( temp_output_3_0_g505 - 1.0 ) / temp_output_7_0_g505 ) ) ) * ( step( texCoord2_g505.x , ( temp_output_3_0_g505 / temp_output_7_0_g505 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g505.y , ( ( temp_output_9_0_g505 - 1.0 ) / temp_output_8_0_g505 ) ) ) * ( step( texCoord2_g505.y , ( temp_output_9_0_g505 / temp_output_8_0_g505 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE4 * ( ( ( 1.0 - step( texCoord2_g497.x , ( ( temp_output_3_0_g497 - 1.0 ) / temp_output_7_0_g497 ) ) ) * ( step( texCoord2_g497.x , ( temp_output_3_0_g497 / temp_output_7_0_g497 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g497.y , ( ( temp_output_9_0_g497 - 1.0 ) / temp_output_8_0_g497 ) ) ) * ( step( texCoord2_g497.y , ( temp_output_9_0_g497 / temp_output_8_0_g497 ) ) * 1.0 ) ) ) ) + ( _MRE5 * ( ( ( 1.0 - step( texCoord2_g502.x , ( ( temp_output_3_0_g502 - 1.0 ) / temp_output_7_0_g502 ) ) ) * ( step( texCoord2_g502.x , ( temp_output_3_0_g502 / temp_output_7_0_g502 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g502.y , ( ( temp_output_9_0_g502 - 1.0 ) / temp_output_8_0_g502 ) ) ) * ( step( texCoord2_g502.y , ( temp_output_9_0_g502 / temp_output_8_0_g502 ) ) * 1.0 ) ) ) ) + ( _MRE6 * ( ( ( 1.0 - step( texCoord2_g503.x , ( ( temp_output_3_0_g503 - 1.0 ) / temp_output_7_0_g503 ) ) ) * ( step( texCoord2_g503.x , ( temp_output_3_0_g503 / temp_output_7_0_g503 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g503.y , ( ( temp_output_9_0_g503 - 1.0 ) / temp_output_8_0_g503 ) ) ) * ( step( texCoord2_g503.y , ( temp_output_9_0_g503 / temp_output_8_0_g503 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE7 * ( ( ( 1.0 - step( texCoord2_g500.x , ( ( temp_output_3_0_g500 - 1.0 ) / temp_output_7_0_g500 ) ) ) * ( step( texCoord2_g500.x , ( temp_output_3_0_g500 / temp_output_7_0_g500 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g500.y , ( ( temp_output_9_0_g500 - 1.0 ) / temp_output_8_0_g500 ) ) ) * ( step( texCoord2_g500.y , ( temp_output_9_0_g500 / temp_output_8_0_g500 ) ) * 1.0 ) ) ) ) + ( _MRE8 * ( ( ( 1.0 - step( texCoord2_g504.x , ( ( temp_output_3_0_g504 - 1.0 ) / temp_output_7_0_g504 ) ) ) * ( step( texCoord2_g504.x , ( temp_output_3_0_g504 / temp_output_7_0_g504 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g504.y , ( ( temp_output_9_0_g504 - 1.0 ) / temp_output_8_0_g504 ) ) ) * ( step( texCoord2_g504.y , ( temp_output_9_0_g504 / temp_output_8_0_g504 ) ) * 1.0 ) ) ) ) + ( _MRE9 * ( ( ( 1.0 - step( texCoord2_g499.x , ( ( temp_output_3_0_g499 - 1.0 ) / temp_output_7_0_g499 ) ) ) * ( step( texCoord2_g499.x , ( temp_output_3_0_g499 / temp_output_7_0_g499 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g499.y , ( ( temp_output_9_0_g499 - 1.0 ) / temp_output_8_0_g499 ) ) ) * ( step( texCoord2_g499.y , ( temp_output_9_0_g499 / temp_output_8_0_g499 ) ) * 1.0 ) ) ) ) ) );
				

				float3 BaseColor = ( clampResult310 * ( ( clampResult255 * temp_output_155_0 ) * _Tint ) ).rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = ( ( temp_output_155_0 * ( _EmissionPower * (temp_output_263_0).b ) ) + _EmissionColor ).rgb;
				float3 Specular = 0.5;
				float Metallic = (temp_output_263_0).r;
				float Smoothness = ( 1.0 - (temp_output_263_0).g );
				float Occlusion = 1;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = WorldPosition;
				inputData.positionCS = IN.clipPos;
				inputData.shadowCoord = ShadowCoords;

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
						inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
						inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
						inputData.normalWS = Normal;
					#endif
				#else
					inputData.normalWS = WorldNormal;
				#endif

				inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				inputData.viewDirectionWS = SafeNormalize( WorldViewDirection );

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				#ifdef ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#else
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, IN.dynamicLightmapUV.xy, SH, inputData.normalWS);
					#else
						inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
					#endif
				#endif

				inputData.normalizedScreenSpaceUV = NormalizedScreenSpaceUV;
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH.xy);

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = IN.dynamicLightmapUV.xy;
						#endif
					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = IN.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
				#endif

				#ifdef _DBUFFER
					ApplyDecal(IN.clipPos,
						BaseColor,
						Specular,
						inputData.normalWS,
						Metallic,
						Occlusion,
						Smoothness);
				#endif

				BRDFData brdfData;
				InitializeBRDFData
				(BaseColor, Metallic, Specular, Smoothness, Alpha, brdfData);

				Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
				half4 color;
				MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);
				color.rgb = GlobalIllumination(brdfData, inputData.bakedGI, Occlusion, inputData.positionWS, inputData.normalWS, inputData.viewDirectionWS);
				color.a = Alpha;

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return BRDFDataToGbuffer(brdfData, inputData, Smoothness, Emission + color.rgb, Occlusion);
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "SceneSelectionPass"
			Tags { "LightMode"="SceneSelectionPass" }

			Cull Off

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 140008


			#pragma vertex vert
			#pragma fragment frag

			#define SCENESELECTIONPASS 1

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DetailsUV2_ST;
			float4 _MRE8;
			float4 _MRE7;
			float4 _MRE6;
			float4 _MRE5;
			float4 _MRE4;
			float4 _MRE3;
			float4 _MRE2;
			float4 _MRE1;
			float4 _Tint;
			float4 _Color9;
			float4 _Color8;
			float4 _MRE9;
			float4 _Color7;
			float4 _Color5;
			float4 _Color4;
			float4 _Color3;
			float4 _Color2;
			float4 _Color1;
			float4 _GradientColor;
			float4 _Color6;
			float4 _EmissionColor;
			float _EmissionPower;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			float _DetailOpacity;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			// Property used by ScenePickingPass
			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			// Properties used by SceneSelectionPass
			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"

			//#ifdef HAVE_VFX_MODIFICATION
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
			//#endif

			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );

				o.clipPos = TransformWorldToHClip(positionWS);

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				

				surfaceDescription.Alpha = 1;
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = 0;

				#ifdef SCENESELECTIONPASS
					outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				#elif defined(SCENEPICKINGPASS)
					outColor = _SelectionID;
				#endif

				return outColor;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ScenePickingPass"
			Tags { "LightMode"="Picking" }

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 140008


			#pragma vertex vert
			#pragma fragment frag

		    #define SCENEPICKINGPASS 1

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DetailsUV2_ST;
			float4 _MRE8;
			float4 _MRE7;
			float4 _MRE6;
			float4 _MRE5;
			float4 _MRE4;
			float4 _MRE3;
			float4 _MRE2;
			float4 _MRE1;
			float4 _Tint;
			float4 _Color9;
			float4 _Color8;
			float4 _MRE9;
			float4 _Color7;
			float4 _Color5;
			float4 _Color4;
			float4 _Color3;
			float4 _Color2;
			float4 _Color1;
			float4 _GradientColor;
			float4 _Color6;
			float4 _EmissionColor;
			float _EmissionPower;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			float _DetailOpacity;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			// Property used by ScenePickingPass
			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			// Properties used by SceneSelectionPass
			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"

			//#ifdef HAVE_VFX_MODIFICATION
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
			//#endif

			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				o.clipPos = TransformWorldToHClip(positionWS);

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				

				surfaceDescription.Alpha = 1;
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
						clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = 0;

				#ifdef SCENESELECTIONPASS
					outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				#elif defined(SCENEPICKINGPASS)
					outColor = _SelectionID;
				#endif

				return outColor;
			}

			ENDHLSL
		}
		
	}
	
	CustomEditor "UnityEditor.ShaderGraphLitGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}
/*ASEBEGIN
Version=19106
Node;AmplifyShaderEditor.TextureCoordinatesNode;258;-840.2042,-1312.028;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;3,3;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;280;1072.05,1897.946;Inherit;True;ColorShartSlot;-1;;497;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;2;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;285;2191.846,574.1987;Inherit;False;Property;_EmissionPower;Emission Power;19;1;[Header];Create;False;1;Emission;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;263;1761.779,1591.684;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;307;2745.48,-894.4118;Inherit;False;Property;_DetailOpacity;Opacity;22;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;292;1144.19,-1097.765;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;261;1513.617,1678.717;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;294;993.1301,-772.0289;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;254;851.5485,-1096.463;Inherit;True;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;262;1509.151,1956.105;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;260;1506.911,1450.623;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;278;1073.824,1263.506;Inherit;True;ColorShartSlot;-1;;498;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;3;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;273;1065.782,2963.45;Inherit;True;ColorShartSlot;-1;;499;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;1;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;276;1067.94,2529.334;Inherit;True;ColorShartSlot;-1;;500;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;1;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;275;1074.009,1474.637;Inherit;True;ColorShartSlot;-1;;501;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;3;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;277;1068.635,2106.349;Inherit;True;ColorShartSlot;-1;;502;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;2;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;209;539.2196,-1097.01;Inherit;True;3;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;302;2848.933,-1096.259;Inherit;True;Property;_DetailsUV2;Details (UV2);21;1;[Header];Create;True;1;Detail Texture (UV2);0;0;False;0;False;-1;None;None;True;1;False;white;LockedToTexture2D;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;281;1066.266,2314.42;Inherit;True;ColorShartSlot;-1;;503;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;2;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;296;2551.944,-583.7661;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;274;1063.897,2751.832;Inherit;True;ColorShartSlot;-1;;504;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;1;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;308;3325.422,-1050.786;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;317;3248.402,386.0329;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;282;2187.501,72.77197;Inherit;True;True;False;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;306;3671.715,-947.4663;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;284;2862.686,251.0539;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;290;2187.83,1603.08;Inherit;True;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;310;3598.422,-1078.786;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;283;2193.479,270.7187;Inherit;True;False;True;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;315;2865.045,707.1369;Inherit;False;Property;_EmissionColor;EmissionColor;20;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;255;1410.724,-1100.765;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;288;2835.367,473.9925;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;291;1651.518,502.5378;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;287;2523.534,562.9523;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;210;2216.373,-635.7195;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;253;560.5085,-873.6618;Float;False;Property;_GradientPower;Gradient Power;28;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;295;2212.28,-359.0309;Inherit;False;Property;_Tint;Tint;0;1;[Header];Create;True;1;Albedo (Alpha  Gradient Power);0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;286;2204.463,687.576;Inherit;True;False;False;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;309;3011.915,-891.5721;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;289;807.1936,-766.9284;Inherit;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;279;1072.083,1685.052;Inherit;True;ColorShartSlot;-1;;505;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;3;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;266;764.9778,2103.387;Float;False;Property;_MRE5;MRE 5;14;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;233;13.07732,530.6414;Inherit;True;ColorShartSlot;-1;;506;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;2;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;238;2.790063,246.9754;Inherit;True;ColorShartSlot;-1;;507;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;3;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;240;14.66442,1076.863;Inherit;True;ColorShartSlot;-1;;508;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;2;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;156;-369.1905,827.4952;Float;False;Property;_Color5;Color 5;5;0;Create;True;0;0;0;False;0;False;0.2669384,0.3207547,0.0226949,1;0.2669383,0.3207546,0.0226949,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;152;-377.5372,262.0459;Float;False;Property;_Color3;Color 3;3;0;Create;True;0;0;0;False;0;False;0.2535501,0.1544118,1,1;0.25355,0.1544117,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;183;-251.6285,1359.862;Float;False;Property;_Color7;Color 7;7;0;Create;True;0;0;0;False;1;Space(10);False;0.9099331,0.9264706,0.6267301,1;0.9099331,0.9264706,0.6267301,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;23;-380.4475,-229.5;Float;False;Property;_Color1;Color 1;1;0;Create;True;0;0;0;False;1;Space(10);False;1,0.1544118,0.1544118,0;1,0.1544117,0.1544117,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;181;-243.7083,1591.022;Float;False;Property;_Color8;Color 8;8;0;Create;True;0;0;0;False;0;False;0.1544118,0.1602434,1,1;0.1544117,0.1602433,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;150;-391.0649,27.18103;Float;False;Property;_Color2;Color 2;2;0;Create;True;0;0;0;False;0;False;1,0.1544118,0.8017241,1;1,0.1544117,0.8017241,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;231;11.13652,815.7118;Inherit;True;ColorShartSlot;-1;;509;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;2;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;256;-244.6775,1818.924;Float;False;Property;_Color9;Color 9;9;0;Create;True;0;0;0;False;0;False;0.1529412,0.9929401,1,1;0.1529411,0.9929401,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;159;-367.2498,538.3683;Float;False;Property;_Color4;Color 4;4;0;Create;True;0;0;0;False;1;Space(10);False;0.9533468,1,0.1544118,1;0.9533468,1,0.1544117,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;200;-606.7432,-1071.044;Float;False;Property;_GradientColor;Gradient Color;25;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;202;-586.4118,-1313.797;Inherit;True;Property;_Gradient;Gradient;23;2;[Header];[SingleLineTexture];Create;True;1;Gradient Properties;0;0;False;1;Space(10);False;-1;0f424a347039ef447a763d3d4b4782b0;0f424a347039ef447a763d3d4b4782b0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;201;-615.6998,-887.8829;Float;False;Property;_GradientIntensity;Gradient Intensity;24;0;Create;True;0;0;0;False;0;False;0.75;0.75;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;257;28.45395,1819.095;Inherit;True;ColorShartSlot;-1;;510;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;1;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;236;-10.73773,16.68434;Inherit;True;ColorShartSlot;-1;;511;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;3;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;208;185.9342,-778.8436;Float;False;Property;_GradientOffset;Gradient Offset;27;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;268;790.1596,1258.979;Float;False;Property;_MRE1;MRE 1;10;1;[Header];Create;True;1;Metallic(R) Rough(G) Emmission(B);0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;204;-244.8239,-942.0975;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;267;797.8815,1892.403;Float;False;Property;_MRE4;MRE 4;13;0;Create;True;0;0;0;False;1;Space(10);False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;157;-235.251,1079.311;Float;False;Property;_Color6;Color 6;6;0;Create;True;0;0;0;False;0;False;1,0.4519259,0.1529412,1;1,0.4519259,0.1529411,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;264;800.6055,1472.631;Float;False;Property;_MRE2;MRE 2;11;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;271;760.3356,2757.597;Float;False;Property;_MRE8;MRE 8;17;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;239;-2.797049,-241.6734;Inherit;True;ColorShartSlot;-1;;512;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;3;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;207;187.0312,-851.6188;Float;False;Property;_GradientScale;Gradient Scale;26;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;270;762.3789,2542.473;Float;False;Property;_MRE7;MRE 7;16;0;Create;True;0;0;0;False;1;Space();False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;269;794.9661,1683.904;Float;False;Property;_MRE3;MRE 3;12;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;206;212.3874,-1105.371;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;265;762.6077,2329.814;Float;False;Property;_MRE6;MRE 6;15;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;272;756.025,2958.429;Float;False;Property;_MRE9;MRE 9;18;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;155;891.6702,382.979;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;164;643.5082,470.012;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;193;639.0421,747.4011;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;146;636.8021,241.9187;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;205;-37.61683,-1102.151;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;203;-218.1167,-1071.731;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;232;25.81848,1594.321;Inherit;True;ColorShartSlot;-1;;513;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;1;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;235;25.18534,1368.447;Inherit;True;ColorShartSlot;-1;;514;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;1;False;7;FLOAT;3;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;318;3622.263,125.6268;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;324;3622.263,125.6268;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthNormals;0;6;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormals;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;323;3622.263,125.6268;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=Universal2D;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;321;3622.263,125.6268;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;322;3622.263,125.6268;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;320;3622.263,125.6268;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;319;3622.263,125.6268;Float;False;True;-1;2;UnityEditor.ShaderGraphLitGUI;0;12;Malbers/Color3x3;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;19;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForward;False;False;0;;0;0;Standard;41;Workflow;1;0;Surface;0;0;  Refraction Model;0;0;  Blend;0;0;Two Sided;1;0;Fragment Normal Space,InvertActionOnDeselection;0;0;Forward Only;0;0;Transmission;0;0;  Transmission Shadow;0.5,False,;0;Translucency;0;0;  Translucency Strength;1,False,;0;  Normal Distortion;0.5,False,;0;  Scattering;2,False,;0;  Direct;0.9,False,;0;  Ambient;0.1,False,;0;  Shadow;0.5,False,;0;Cast Shadows;1;0;  Use Shadow Threshold;0;0;Receive Shadows;1;0;GPU Instancing;1;0;LOD CrossFade;1;0;Built-in Fog;1;0;_FinalColorxAlpha;0;0;Meta Pass;1;0;Override Baked GI;0;0;Extra Pre Pass;0;0;DOTS Instancing;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Write Depth;0;0;  Early Z;0;0;Vertex Position,InvertActionOnDeselection;1;0;Debug Display;0;0;Clear Coat;0;0;0;10;False;True;True;True;True;True;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;325;3622.263,125.6268;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;GBuffer;0;7;GBuffer;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalGBuffer;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;326;3622.263,125.6268;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;SceneSelectionPass;0;8;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;327;3622.263,125.6268;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ScenePickingPass;0;9;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;280;38;267;0
WireConnection;263;0;260;0
WireConnection;263;1;261;0
WireConnection;263;2;262;0
WireConnection;292;0;254;0
WireConnection;292;1;294;0
WireConnection;261;0;280;0
WireConnection;261;1;277;0
WireConnection;261;2;281;0
WireConnection;294;0;289;0
WireConnection;254;0;209;0
WireConnection;254;1;253;0
WireConnection;262;0;276;0
WireConnection;262;1;274;0
WireConnection;262;2;273;0
WireConnection;260;0;278;0
WireConnection;260;1;275;0
WireConnection;260;2;279;0
WireConnection;278;38;268;0
WireConnection;273;38;272;0
WireConnection;276;38;270;0
WireConnection;275;38;264;0
WireConnection;277;38;266;0
WireConnection;209;0;206;0
WireConnection;209;1;207;0
WireConnection;209;2;208;0
WireConnection;281;38;265;0
WireConnection;296;0;210;0
WireConnection;296;1;295;0
WireConnection;274;38;271;0
WireConnection;308;0;302;0
WireConnection;308;1;309;0
WireConnection;317;0;288;0
WireConnection;317;1;315;0
WireConnection;282;0;263;0
WireConnection;306;0;310;0
WireConnection;306;1;296;0
WireConnection;284;0;283;0
WireConnection;290;0;263;0
WireConnection;310;0;308;0
WireConnection;283;0;263;0
WireConnection;255;0;292;0
WireConnection;288;0;291;0
WireConnection;288;1;287;0
WireConnection;291;0;155;0
WireConnection;287;0;285;0
WireConnection;287;1;286;0
WireConnection;210;0;255;0
WireConnection;210;1;155;0
WireConnection;286;0;263;0
WireConnection;309;0;307;0
WireConnection;289;0;155;0
WireConnection;279;38;269;0
WireConnection;233;38;159;0
WireConnection;238;38;152;0
WireConnection;240;38;157;0
WireConnection;231;38;156;0
WireConnection;202;1;258;0
WireConnection;257;38;256;0
WireConnection;236;38;150;0
WireConnection;204;0;201;0
WireConnection;239;38;23;0
WireConnection;206;0;205;0
WireConnection;155;0;146;0
WireConnection;155;1;164;0
WireConnection;155;2;193;0
WireConnection;164;0;233;0
WireConnection;164;1;231;0
WireConnection;164;2;240;0
WireConnection;193;0;235;0
WireConnection;193;1;232;0
WireConnection;193;2;257;0
WireConnection;146;0;239;0
WireConnection;146;1;236;0
WireConnection;146;2;238;0
WireConnection;205;0;203;0
WireConnection;205;1;204;0
WireConnection;203;0;202;0
WireConnection;203;1;200;0
WireConnection;232;38;181;0
WireConnection;235;38;183;0
WireConnection;319;0;306;0
WireConnection;319;2;317;0
WireConnection;319;3;282;0
WireConnection;319;4;284;0
ASEEND*/
//CHKSM=CC3EAAC85F0441A6BC451DB732B778F5E738EB78