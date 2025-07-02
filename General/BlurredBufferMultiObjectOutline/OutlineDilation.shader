Shader "Hidden/Dilation"
{
    Properties
    {
        _Spread("Spread", Integer) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        ZWrite Off Cull Off

        HLSLINCLUDE
      //  #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureXR.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DynamicScaling.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

        SAMPLER(sampler_BlitTexture);

        float _Spread;
        ENDHLSL

        Pass
        {
            Name "HorizontalDilation"
            ZTest Always

            HLSLPROGRAM
            //#pragma vertex Vert
            #pragma vertex Vert
            #pragma fragment frag_vertical

            struct v2f {
                float4 pos : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f Vert_t(Attributes input){
                //v2f output = (v2f)0;
                //UNITY_SETUP_INSTANCE_ID(input);
                
			    //UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

			   // output.pos = float4(0,0, UNITY_NEAR_CLIP_VALUE, 1);
                //output.pos = UnityObjectToClipPos(input.pos);

			    //output.texcoord = input.texcoord;
                //Varyings output_ = Vert(input);
                //output.pos = output_.positionCS;
                //output.texcoord = output_.texcoord; //output_.texcoord;
                //output.texcoord = TRANSFORM_TEX(output_.texcoord, _MainTex); 
               // UNITY_TRANSFER_INSTANCE_ID(input, output);
                //UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
               //Varyings_t output;
                //UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                //output.pos = UnityMetaVertexPosition(input.pos.xyz, input.uv1, input.uv2);
                //output.texcoord = TRANSFORM_TEX(input.uv0, _BlitTexture);
                //output.texcoord = input.uv0;

                v2f output;
                //UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);

                output.pos = pos;
                output.texcoord   = DYNAMIC_SCALING_APPLY_SCALEBIAS(uv);

			    return output;
            }

            float4 frag_vertical(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Dilate
                float totalWeightedValues = 0;
                int shortestActivePixelDistance = _Spread + 1;
                float3 nearestActivePixelColor = float3(0, 0, 0);

                for (int x = -_Spread; x <= _Spread; x++)
                {
                    float2 uv = UnityStereoTransformScreenSpaceTex(i.texcoord) + float2(_BlitTexture_TexelSize.x * x, 0.0f);
                    float4 buffer = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);

                    // Check if this is the nearest occupied pixel
                    // (Occupied means non-black here)
                    int distance = abs(x);
                    float falloff = 1.0f - distance / _Spread;
                    totalWeightedValues += buffer.a * falloff;

                    if (distance < shortestActivePixelDistance &&
                        buffer.a >= 1.0)
                    {
                        shortestActivePixelDistance = distance;
                        nearestActivePixelColor = buffer.xyz;
                    }
                }

                return float4(nearestActivePixelColor, 1 - saturate(shortestActivePixelDistance / _Spread));
            }
            ENDHLSL
        }

        Pass
        {
            Name "VerticalDilation"
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            Stencil
            {
                Ref 15
                Comp NotEqual
                Pass Zero
                Fail Zero
                ZFail Zero
            }

            HLSLPROGRAM
            //#pragma vertex Vert
            #pragma vertex Vert
            #pragma fragment frag_horizontal

            struct v2f {
                float4 pos : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f Vert_t(Attributes input){
                v2f output;
                //UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);

                output.pos = pos;
                output.texcoord  = DYNAMIC_SCALING_APPLY_SCALEBIAS(uv);

			    return output;
            }

            //float4 frag_horizontal(Varyings i) : SV_Target
            float4 frag_horizontal(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Dilate
                float totalWeightedValues = 0;
                float3 brightestActivePixelColor = float3(0, 0, 0);
                float brightestWeightedAlpha = 0;
                for (int y = -_Spread; y <= _Spread; y++)
                {
                    float2 uv = UnityStereoTransformScreenSpaceTex(i.texcoord) + float2(0.0f, _BlitTexture_TexelSize.y * y);
                    //float4 buffer = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);
                    float4 buffer = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);

                    // Check if this is the nearest occupied pixel
                    // (Occupied means non-black here)
                    int distance = abs(y);
                    float falloff = 1.0f - distance / _Spread;
                    float weightedValue = buffer.a * falloff;
                    totalWeightedValues += weightedValue;

                    // favor the brightest, nearest alpha
                    if (weightedValue > brightestWeightedAlpha)
                    {
                        brightestWeightedAlpha = weightedValue;
                        brightestActivePixelColor = buffer.xyz;
                    }
                }

                return float4(brightestActivePixelColor, Smoothstep01(brightestWeightedAlpha));
            }
            ENDHLSL
        }
    }
}