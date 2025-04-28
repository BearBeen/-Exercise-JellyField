Shader "Custom/MeshedSpringJellyShader"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };
            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };
            //struct MeshProperties {
                // float4x4 TRSmat;
                // float4 color;
            //};
            //StructuredBuffer<MeshProperties> _Properties;
            struct SpringMesh
            {
                float3 position;
                float volume;
                float3 move;
                float fixity;
            };
            StructuredBuffer<SpringMesh> _MeshedSpring;


            v2f vert(appdata i, uint instanceID : SV_InstanceID)
            {
                int _SimSize = 8;
                float _MeshSize = 2;
                float3 _MeshCenter = float3(0, 1, 0);

                float meshSizeInvert = 1 / _MeshSize;

                float xt = (i.vertex.x + 0.5 * _MeshSize - _MeshCenter.x) * meshSizeInvert * _SimSize - 0.499;
                int x0 = floor(xt);
                int x1 = x0 + 1;
                x0 = lerp(x0, 0, x0 < 0);
                x1 = lerp(x1, _SimSize - 1, x1 > _SimSize - 1);
                xt = lerp(xt, x0, x0 == x1);

                float yt = (i.vertex.y + 0.5 * _MeshSize - _MeshCenter.y) * meshSizeInvert * _SimSize - 0.499;
                int y0 = floor(yt);
                int y1 = y0 + 1;
                y0 = lerp(y0, 0, y0 < 0);
                y1 = lerp(y1, _SimSize - 1, y1 > _SimSize - 1);
                yt = lerp(yt, y0, y0 == y1);

                float zt = (i.vertex.z + 0.5 * _MeshSize - _MeshCenter.z) * meshSizeInvert * _SimSize - 0.499;
                int z0 = floor(zt);
                int z1 = z0 + 1;
                z0 = lerp(z0, 0, z0 < 0);
                z1 = lerp(z1, _SimSize - 1, z1 > _SimSize - 1);
                zt = lerp(zt, z0, z0 == z1);

                float3 mainPos = lerp(
                    _MeshedSpring[x0 + y0 * _SimSize + z0 * _SimSize * _SimSize].position, 
                    _MeshedSpring[x1 + y1 * _SimSize + z1 * _SimSize * _SimSize].position,
                     yt - y0);

                //float4 worldPos = mul(_Properties[instanceID].TRSmat, i.vertex);
                //o.vertex = mul(UNITY_MATRIX_VP, worldPos);
                //o.color = _Properties[instanceID].color;
                i.vertex.xyz += 0.125 * (mainPos);

                v2f o;

                o.vertex = UnityObjectToClipPos(i.vertex);
                o.color = float4(floor(xt) / _SimSize, floor(yt) / _SimSize, floor(zt) / _SimSize, 1);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                return i.color;
            }
            ENDCG
        }
    }
}