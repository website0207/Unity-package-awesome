Shader "LSQ/Procedual/Effect/LightLine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Count ("Count", float) = 8
        _RotateSpeed ("RotateSpeed", float) = 60
        _Frequency ("Frequency", float) = 5
        _CenterRange ("CenterRange", float) = 5
        _CenterStrength ("CenterStrength", float) = 5
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "LSQ/UV.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR0;
            };

            sampler2D _MainTex;

            float _Count;
            float _Frequency;
            float _RotateSpeed;
            float _CenterRange;
            float _CenterStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //Angle
                const float PI = 3.1415926;
                float2 uv = i.uv * 2 - 1;
                float2 rotate_uv_1 = Rotate(uv, _Time.y * _RotateSpeed);
				float angle = (atan2(rotate_uv_1.x, rotate_uv_1.y) / PI + 1) * 0.5;//-Pi - Pi -> 0-1
                angle *= _Count;

                //sinÖÜÆÚº¯Êý
                float2 rotate_uv_2 = Rotate(uv, _Time.y * -_RotateSpeed);
                float radAngle = atan2(rotate_uv_2.x, rotate_uv_2.y);
                float sine = sin(radAngle * _Frequency);
                
                //ray
                fixed ray = tex2D(_MainTex, angle).r * sine;

                //center
                float center = pow(1 - length(uv), _CenterRange);

                fixed4 color = saturate(center * ray + center * _CenterStrength) * i.color;
                return color;
            }
            ENDCG
        }
    }
}
