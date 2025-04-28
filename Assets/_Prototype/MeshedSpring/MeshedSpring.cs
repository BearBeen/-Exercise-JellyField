using UnityEngine;

public class MeshedSpring : MonoBehaviour
{
    //Those are simulation size in 3d.
    private const int X_SIZE = 8;
    private const int Y_SIZE = 8;
    private const int Z_SIZE = 8;
    //////////////////////////////

    private struct SpringMesh
    {
        public Vector3 position;
        public float volume;
        public Vector3 move;
        public float fixity;
    }

    [SerializeField] private ComputeShader _computeShader;
    [SerializeField] private float _stiffness;
    [SerializeField] private float _loss;
    [SerializeField] private Material _mat;
    private SpringMesh[] _data;
    private ComputeBuffer _dataBuffer;
    private Vector3 _kernalThreadGroupSize;
    private Vector3 _laspos = Vector3.zero;
    private int _positionKernalID;
    private int _volumeKernalID;

    //for review only of course
    private GameObject[] _reviews;

    private void Awake()
    {
        _positionKernalID = _computeShader.FindKernel("PositionUpdate");
        _volumeKernalID = _computeShader.FindKernel("VolumeUpdate");

        _data = new SpringMesh[X_SIZE * Y_SIZE * Z_SIZE];
        _reviews = new GameObject[X_SIZE * Y_SIZE * Z_SIZE];
        for (int x = 0; x < X_SIZE; x++)
        {
            for (int y = 0; y < Y_SIZE; y++)
            {
                for (int z = 0; z < Z_SIZE; z++)
                {
                    _data[x + y * X_SIZE + z * Y_SIZE * X_SIZE] = new SpringMesh()
                    {
                        position = Vector3.zero,
                        volume = 1,
                        move = Vector3.zero,
                        fixity = 1 - (float)y / Y_SIZE,
                    };
                }
            }
        }

        //calculate your struct size some where.
        _dataBuffer = new ComputeBuffer(_data.Length, 32);
        _dataBuffer.SetData(_data);
        _computeShader.SetBuffer(_positionKernalID, AssetConst.SHADERPROP_MESHED_SPRING, _dataBuffer);
        _computeShader.SetBuffer(_volumeKernalID, AssetConst.SHADERPROP_MESHED_SPRING, _dataBuffer);

        _computeShader.SetFloat(AssetConst.SHADERPROP_LOSS, _loss);
        _computeShader.SetFloat(AssetConst.SHADERPROP_STIFFNESS, _stiffness);
        _computeShader.SetInt(AssetConst.SHADERPROP_X_SIZE, X_SIZE);
        _computeShader.SetInt(AssetConst.SHADERPROP_Y_SIZE, Y_SIZE);
        _computeShader.SetInt(AssetConst.SHADERPROP_Z_SIZE, Z_SIZE);

        _computeShader.GetKernelThreadGroupSizes(_positionKernalID, out uint xThreadGroupSize, out uint yThreadGroupSize, out uint zThreadGroupSize);
        _kernalThreadGroupSize = new Vector3(xThreadGroupSize, yThreadGroupSize, zThreadGroupSize);

        for (int i = 0, length = X_SIZE * Y_SIZE * Z_SIZE; i < length; i++)
        {
            _reviews[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _reviews[i].transform.parent = transform;
        }
        //push buffer to material
        _mat.SetBuffer(AssetConst.SHADERPROP_MESHED_SPRING, _dataBuffer);
    }

    void Update()
    {
        _computeShader.SetVector(AssetConst.SHADERPROP_PINCH, Vector3.ClampMagnitude(_laspos - transform.position, 1));
        _computeShader.SetVector(AssetConst.SHADERPROP_UNITY_DELTATIME, Shader.GetGlobalVector(AssetConst.SHADERPROP_UNITY_DELTATIME));

        _computeShader.Dispatch(_volumeKernalID, Mathf.CeilToInt(X_SIZE / _kernalThreadGroupSize.x), Mathf.CeilToInt(Y_SIZE / _kernalThreadGroupSize.y), Mathf.CeilToInt(Z_SIZE / _kernalThreadGroupSize.z));
        _computeShader.Dispatch(_positionKernalID, Mathf.CeilToInt(X_SIZE / _kernalThreadGroupSize.x), Mathf.CeilToInt(Y_SIZE / _kernalThreadGroupSize.y), Mathf.CeilToInt(Z_SIZE / _kernalThreadGroupSize.z));

        //For quick demo only.
        //Real project will need a shader to read the structured buffer and apply spring data on each vertex.
        //Need to profiling to check if the spring simulation is burdening the GPU (especially on mobile)
        _dataBuffer.GetData(_data);
        Vector3 basePos = Vector3.zero;
        float xCenterOffset = (X_SIZE - 1) * 0.5f;
        float zCenterOffset = (Z_SIZE - 1) * 0.5f;
        for (int x = 0; x < X_SIZE; x++)
        {
            for (int y = 0; y < Y_SIZE; y++)
            {
                for (int z = 0; z < Z_SIZE; z++)
                {
                    basePos.x = x - xCenterOffset;
                    basePos.y = y;
                    basePos.z = z - zCenterOffset;
                    basePos = 3 * basePos + _data[x + y * X_SIZE + z * Y_SIZE * X_SIZE].position;
                    _reviews[x + y * X_SIZE + z * Y_SIZE * X_SIZE].transform.localPosition = basePos;
                }
            }
        }

        _laspos = transform.position;
    }

    private void OnDestroy()
    {
        _dataBuffer.Dispose();
    }
}
