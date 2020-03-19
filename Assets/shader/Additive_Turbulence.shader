// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/FX/Additive Turbulence " {
Properties {
	_MainTex("Main_Texture", 2D) = "white" {}
	_Color01("Main_Texture_Color", Color) = (1,1,1,1)
	_Blend_Texture("Blend_Texture", 2D) = "white" {}
	_Color02("Blend_Texture_Color", Color) = (1,1,1,1)
	_Blend_Texture01("Blend_Texture01", 2D) = "white" {}

	_Speeds("TextureSpeeds", Vector) = (0,0,0,0)
	
	_Lighten("Lighten", Float) = 1
}

SubShader {
	
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	
	Blend One One
	
//	Blend One OneMinusSrcColor
	Cull Off Lighting Off ZWrite Off Fog { Mode Off }
		
	
	LOD 100
	
	CGINCLUDE
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
	float4 _MainTex_ST;
	sampler2D _Blend_Texture;
	float4 _Blend_Texture_ST;
	sampler2D _Blend_Texture01;
	float4 _Blend_Texture01_ST;
	
	float4 _Color01;
	float4 _Color02;

	float4 _Speeds;
	
	float _Lighten;
	
	struct vertexInput{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 texcoord : TEXCOORD0;
		float4 color : COLOR;
	};
	
	struct vertexOutput{
		float4 pos : SV_POSITION;
		
		float2 uv_MainTex :TEXCOORD0;
		float2 uv_Blend_Texture :TEXCOORD1;
		float2 uv_Blend_Texture01 :TEXCOORD2;
		float4 color : TEXCOORD3;
	};
	
	//vertex function
	vertexOutput vert(vertexInput v){
		vertexOutput o;
		
		o.pos = UnityObjectToClipPos(v.vertex);
		
		float3 speeds = _Speeds * _Time.xxx;
		o.uv_MainTex = TRANSFORM_TEX(v.texcoord.xy,_MainTex) + frac(float2(0, speeds.x) );
		o.uv_Blend_Texture = TRANSFORM_TEX(v.texcoord.xy,_Blend_Texture) + frac(float2(0, speeds.y) );
		o.uv_Blend_Texture01 = TRANSFORM_TEX(v.texcoord.xy,_Blend_Texture01) + frac(float2(0, speeds.z) );
		
		o.color = v.color;
		
		return o;
	}
	ENDCG
	
		
	pass{
		CGPROGRAM
		//fragment function
		float4 frag(vertexOutput i) : COLOR
		{
			float4 color;
			
			
			float3 lightDirection = normalize( _WorldSpaceLightPos0.xyz );
			
			float4 mainTex = tex2D(_MainTex,i.uv_MainTex);
			float4 blendTex = tex2D(_Blend_Texture,i.uv_Blend_Texture);
			float4 blendTex1 = tex2D(_Blend_Texture01,i.uv_Blend_Texture01);
			
						
			color = (mainTex * _Color01 + blendTex * _Color02) * mainTex * blendTex * _Lighten * blendTex1 * i.color ;
			
			
			return color; 
			//return float4(main.xyz, rim);
		}
		ENDCG
	}
}
	
	//Fallback "Specular"
}