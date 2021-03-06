Shader "A Rat King/SelfIllumDiffuseNoDepth" {
	Properties {
		_Color ("Main Color", Color) = (0.5, 0.5, 0.5, 0.5)
		_Emission ("Emission Factor", float) = 0.2 // (0.5, 0.5, 0.5, 0.5)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags {"Queue"="Transparent-1" "RenderType"="Opaque"}
		LOD 200
		Lighting On
		
        
		CGPROGRAM
		#pragma surface surf Lambert
		
		sampler2D _MainTex;
		float4 _Color;
		float _Emission;
		
		struct Input {
			float2 uv_MainTex;
		};
		
		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Emission = c * _Emission;
			o.Alpha = _Color.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
