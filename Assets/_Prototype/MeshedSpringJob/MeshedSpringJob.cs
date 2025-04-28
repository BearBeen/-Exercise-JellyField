using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class MeshedSpringJob : MonoBehaviour
{
    private struct PositionJob : IJob
    {
        [ReadOnly] public float3 pinch;
        [ReadOnly] public float loss;
        [ReadOnly] public float stiffness;
        [ReadOnly] public int xSize;
        [ReadOnly] public int ySize;
        [ReadOnly] public int zSize;
        [ReadOnly] public float deltaTime;
        public NativeArray<float4> data1;//position + volume
        public NativeArray<float4> data2;//move + fixity

        public void Execute()
        {
            for (int myIndex = 0; myIndex < xSize * ySize * zSize; ++myIndex)
            {
                int z = myIndex / (ySize * xSize);
                int y = (myIndex - z * ySize * xSize) / (xSize);
                int x = myIndex - z * ySize * xSize - y * xSize;

                float3 position = data1[myIndex].xyz;
                float volume = data1[myIndex].w;
                float invertVolume = 1f / volume;

                int leftIndex = (x > 0 ? x - 1 : x) + y * xSize + z * ySize * xSize;
                int rightIndex = (x < xSize - 1 ? x + 1 : x) + y * xSize + z * ySize * xSize;
                int forwardIndex = x + y * xSize + (z < zSize - 1 ? z + 1 : z) * ySize * xSize;
                int backwardIndex = x + y * xSize + (z > 0 ? z - 1 : z) * ySize * xSize;
                int upwardIndex = x + (y < ySize - 1 ? y + 1 : y) * xSize + z * ySize * xSize;
                int downwardIndex = x + (y > 0 ? y - 1 : y) * xSize + z * ySize * xSize;

                float3 toLeft = (invertVolume - 1f / (leftIndex == myIndex ? 1 : data1[leftIndex].w)) * new float3(-1, 0, 0);
                float3 toRight = (invertVolume - 1f / (rightIndex == myIndex ? 1 : data1[rightIndex].w)) * new float3(1, 0, 0);
                float3 toForward = (invertVolume - 1f / (forwardIndex == myIndex ? 1 : data1[forwardIndex].w)) * new float3(0, 0, 1);
                float3 toBackward = (invertVolume - 1f / (backwardIndex == myIndex ? 1 : data1[backwardIndex].w)) * new float3(0, 0, -1);
                float3 toUpward = (invertVolume - 1f / (upwardIndex == myIndex ? 1 : data1[upwardIndex].w)) * new float3(0, 1, 0);
                float3 toDownward = (invertVolume - 1f / (downwardIndex == myIndex ? 1 : data1[downwardIndex].w)) * new float3(0, -1, 0);

                float3 move = data2[myIndex].xyz;
                float fixity = data2[myIndex].w;

                position += (1 - fixity) * pinch;
                move *= (1 - loss);
                move -= deltaTime * stiffness * position;
                move += deltaTime * 30f * (toLeft + toRight + toForward + toBackward + toUpward + toDownward); //30 is pick by feeling. it is the constant compress value of the material
                position += move;

                data1[myIndex] = new float4(position, volume);
                data2[myIndex] = new float4(move, fixity);
            }
        }
    }

    [SerializeField] private int _xSize;
    [SerializeField] private int _ySize;
    [SerializeField] private int _zSize;
    [SerializeField] private float _stiffness;
    [SerializeField] private float _loss;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
