Shader "Hidden/Outline Color And Stencil"
{
    // The _BaseColor variable is visible in the Material's Inspector, as a field 
    // called Base Color. You can use it to select a custom color. This variable
    // has the default value (1, 1, 1, 1).
    Properties
    {
        [MainColor][HDR]_BaseColor("Base Color", Color) = (0, 1, 0, 1)
    }

    SubShader
    {
        Name "Draw Solid Color"
        
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"
        }

        Pass
        {
            Stencil
            {
                Ref 15
                Comp Always
                Pass Replace
                Fail Keep
                ZFail Keep
            }
            
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes_t
            {
                float4 positionOS : POSITION;
                float2  uv          : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings_t
            {
                float4 positionHCS : SV_POSITION;
                float2  uv          : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // To make the Unity shader SRP Batcher compatible, declare all
            // properties related to a Material in a a single CBUFFER block with 
            // the name UnityPerMaterial.
            CBUFFER_START(UnityPerMaterial)
                // The following line declares the _BaseColor variable, so that you
                // can use it in the fragment shader.
                half4 _BaseColor;
            CBUFFER_END

            Varyings_t vert(Attributes_t IN)
            {
                Varyings_t OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings_t input) : SV_Target
            {

                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}