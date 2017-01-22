Shader "Vertex color unlit" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
	}

		Category{
		Tags{ "Queue" = "Geometry+10" }
		Lighting Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha
		BindChannels{
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}

		SubShader{
		Pass{
		SetTexture[_MainTex]{
		Combine texture * primary DOUBLE
	}
	}
	}
	}
}