// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sky2D/SkyShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SunPosition("Sun position", Vector) = (216, 55, 6.78, 1)
        _SunSize("Sun Size", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
         

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 _SunPosition;
            float _SunSize;
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float4 sunpos = float4(_SunPosition.y*_ScreenParams.x/24,_SunPosition.y*_ScreenParams.y/24,1,0);
                if(_SunPosition.y < 12){
                    sunpos.y = (24-_SunPosition.y)*_ScreenParams.y/24;
                }
                float distanceToSunpoint = distance(sunpos, i.vertex);
                if(distanceToSunpoint < _SunSize){
                    col = fixed4(col.x+_SunSize*distanceToSunpoint,col.y+_SunSize/distanceToSunpoint,0,1);
                }
                return col ;
            }
            ENDCG
        }
    }
}
