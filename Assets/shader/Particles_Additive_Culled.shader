Shader "Chp/Particles/Additive Tint TwoSide" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha One

	Cull Off Lighting Off ZWrite Off Fog { Mode Off }
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
	

	SubShader {
		Pass {
			SetTexture [_MainTex] {
				constantColor [_TintColor] 
				combine constant * primary , constant * primary
			}
			SetTexture [_MainTex] {
				combine texture * previous
			}
		}
	}
	
	
}
}
