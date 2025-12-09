Shader "Custom/UI_Gradient"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Space]
        _ColorTopLeft ("Color Top Left", Color) = (1, 1, 1, 1)
        _ScaleLeft ("Scale Left", Range(0, 1)) = 1
        [Space]
        _ColorTopRight ("Color Top Right", Color) = (1, 1, 1, 1)
        _ScaleRight ("Scale Right", Range(0, 1)) = 1
        [Space]
        _ColorBottomLeft ("Color Bottom Left", Color) = (1, 1, 1, 1)
        _ScaleBottomLeft ("Scale Bottom Left", Range(0, 1)) = 1
        [Space]
        _ColorBottomRight ("Color Bottom Right", Color) = (1, 1, 1, 1)
        _ScaleBottomRight ("Scale Bottom Right", Range(0, 1)) = 1
        
        // Свойства для поддержки маски
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        // Настройки Stencil для работы с Mask
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_UI_CLIP_RECT
            #pragma multi_compile _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            
            half _ScaleLeft;
            half _ScaleRight;
            half _ScaleBottomLeft;
            half _ScaleBottomRight;

            half4 _ColorTopLeft;
            half4 _ColorTopRight;
            half4 _ColorBottomLeft;
            half4 _ColorBottomRight;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Базовый цвет текстуры
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                
                // 4-цветный градиент
                fixed4 colTop = ((IN.texcoord.x) * _ScaleRight * _ColorTopRight) + 
                               ((1 - IN.texcoord.x) * _ScaleLeft * _ColorTopLeft);
                
                fixed4 colBottom = ((IN.texcoord.x) * _ScaleBottomRight * _ColorBottomRight) + 
                                  ((1 - IN.texcoord.x) * _ScaleBottomLeft * _ColorBottomLeft);
                
                fixed4 gradient = (colTop * IN.texcoord.y) + (colBottom * (1 - IN.texcoord.y));
                
                // Умножаем градиент на альфа цвета текстуры для корректной прозрачности
                gradient.a *= color.a;
                
                // Комбинируем цвет текстуры с градиентом
                color.rgb = color.rgb * gradient.rgb * gradient.a + gradient.rgb * (1 - gradient.a);
                color.a = max(color.a, gradient.a);
                
                // Применяем маскирование от UI
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
