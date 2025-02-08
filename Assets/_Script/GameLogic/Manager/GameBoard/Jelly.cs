using UnityEngine;
using DG.Tweening;

public enum JellyLayout
{
    //None,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Top,
    Bottom,
    Left,
    Right,
    Center,
}

public class Jelly : MonoBehaviour, IPoolable
{
    private static readonly Vector3 DEAD_SCALE = new Vector3(0.1f, 0.1f, 0.1f);
    private static readonly Vector3 DEAD_TO = new Vector3(0f, 5f, 0f);
    
    //bit mask layout on jelly box surface
    // 0 1
    // 2 3
    private static readonly int[] LAYOUT_SURFACE = new int[9]
    {
        //0,//None,
        1 << 0,//TopLeft,
        1 << 1,//TopRight,
        1 << 2,//BottomLeft,
        1 << 3,//BottomRight,
        1 << 0 | 1 << 1,//Top,
        1 << 2 | 1 << 3,//Bottom,
        1 << 0 | 1 << 2,//Left,
        1 << 1 | 1 << 3,//Right,
        1 << 0 | 1 << 1 | 1 << 2 | 1 << 3,//Center,
    };

    private static readonly Vector3[] LAYOUT_POS = new Vector3[9]
    {
        new Vector3(-0.25f, 0.25f, 0.25f),//TopLeft,
        new Vector3(0.25f, 0.25f, 0.25f),//TopRight,
        new Vector3(-0.25f, 0.25f, -0.25f),//BottomLeft,
        new Vector3(0.25f, 0.25f, -0.25f),//BottomRight,
        new Vector3(0, 0.25f, 0.25f),//Top,
        new Vector3(0f, 0.25f, -0.25f),//Bottom,
        new Vector3(-0.25f, 0.25f, 0f),//Left,
        new Vector3(0.25f, 0.25f,0f),//Right,
        new Vector3(0f, 0.25f, 0f),//Center,
    };

    private static readonly Vector3[] LAYOUT_SCALE = new Vector3[9]
    {
        new Vector3(1, 1, 1),//TopLeft,
        new Vector3(1, 1, 1),//TopRight,
        new Vector3(1, 1, 1),//BottomLeft,
        new Vector3(1, 1, 1),//BottomRight,
        new Vector3(2, 1, 1),//Top,
        new Vector3(2, 1, 1),//Bottom,
        new Vector3(1, 1, 2),//Left,
        new Vector3(1, 1, 2),//Right,
        new Vector3(2, 1, 2),//Center,
    };

    public static readonly int FULL_SURFACE = LAYOUT_SURFACE[(int)JellyLayout.Center];

    [SerializeField] private JellyLayout _layout;
    [SerializeField] private int _index;
    [SerializeField] private Renderer _renderer;
    private SimpleSpring _spring = new SimpleSpring();
    private Vector3 _laspos = Vector3.zero;
    private MaterialPropertyBlock _materialPropertyBlock;

    public JellyLayout layout => _layout;
    public int index => _index;

    public static bool TryLayout(JellyLayout layout, ref int surface)
    {
        //if (layout == JellyLayout.None)
        //{
        //    return false;
        //}

        int newSurface = LAYOUT_SURFACE[(int)layout];
        if ((newSurface & surface) == 0)
        {
            surface = surface | newSurface;
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool MatchJellyInRow(JellyLayout leftLayout, JellyLayout rightLayout, int leftIndex, int rightIndex)
    {
        if (leftIndex != rightIndex)
        {
            return false;
        }

        int leftMask = LAYOUT_SURFACE[(int)JellyLayout.Left];
        int rightMask = LAYOUT_SURFACE[(int)JellyLayout.Right];
        int leftSurface = 0;
        int rightSurface = 0;
        TryLayout(leftLayout, ref leftSurface);
        TryLayout(rightLayout, ref rightSurface);
        leftMask = leftMask & rightSurface;
        rightMask = rightMask & leftSurface;
        //convert left mask to right mask
        leftMask = leftMask << 1;
        return (rightMask & leftMask) != 0;
    }

    public static bool MatchJellyInColumn(JellyLayout topLayout, JellyLayout bottomLayout, int topIndex, int bottomIndex)
    {
        if (topIndex != bottomIndex)
        {
            return false;
        }

        int topMask = LAYOUT_SURFACE[(int)JellyLayout.Top];
        int bottomMask = LAYOUT_SURFACE[(int)JellyLayout.Bottom];
        int topSurface = 0;
        int bottomSurface = 0;
        TryLayout(topLayout, ref topSurface);
        TryLayout(bottomLayout, ref bottomSurface);
        topMask = topMask & bottomSurface;
        bottomMask = bottomMask & topSurface;
        //convert top mask to bottom mask
        topMask = topMask << 2;
        return (bottomMask & topMask) != 0;
    }

    public void InitJelly(JellyLayout layout, int index)
    {
        _layout = layout;
        _index = index;
    }

    public int TryExpandLeft(int suface)
    {
        JellyLayout newLayout;
        JellyLayout expandLayout;
        switch (_layout)
        {
            case JellyLayout.TopRight:
                newLayout = JellyLayout.Top;
                expandLayout = JellyLayout.TopLeft;
                break;
            case JellyLayout.BottomRight:
                newLayout = JellyLayout.Bottom;
                expandLayout = JellyLayout.BottomLeft;
                break;
            case JellyLayout.Right:
                newLayout = JellyLayout.Center;
                expandLayout = JellyLayout.Left;
                break;
            default:
                return suface;
        }
        if (TryLayout(expandLayout, ref suface))
        {
            DoExpand(newLayout);
        }
        return suface;
    }

    public int TryExpandRight(int suface)
    {
        JellyLayout newLayout;
        JellyLayout expandLayout;
        switch (_layout)
        {
            case JellyLayout.TopLeft:
                newLayout = JellyLayout.Top;
                expandLayout = JellyLayout.TopRight;
                break;
            case JellyLayout.BottomLeft:
                newLayout = JellyLayout.Bottom;
                expandLayout = JellyLayout.BottomRight;
                break;
            case JellyLayout.Left:
                newLayout = JellyLayout.Center;
                expandLayout = JellyLayout.Right;
                break;
            default:
                return suface;
        }
        if (TryLayout(expandLayout, ref suface))
        {
            DoExpand(newLayout);
        }
        return suface;
    }

    public int TryExpandTop(int suface)
    {
        JellyLayout newLayout;
        JellyLayout expandLayout;
        switch (_layout)
        {
            case JellyLayout.BottomLeft:
                newLayout = JellyLayout.Left;
                expandLayout = JellyLayout.TopLeft;
                break;
            case JellyLayout.BottomRight:
                newLayout = JellyLayout.Right;
                expandLayout = JellyLayout.TopRight;
                break;
            case JellyLayout.Bottom:
                newLayout = JellyLayout.Center;
                expandLayout = JellyLayout.Top;
                break;
            default:
                return suface;
        }
        if (TryLayout(expandLayout, ref suface))
        {
            DoExpand(newLayout);
        }
        return suface;
    }

    public int TryExpandBottom(int suface)
    {
        JellyLayout newLayout;
        JellyLayout expandLayout;
        switch (_layout)
        {
            case JellyLayout.TopLeft:
                newLayout = JellyLayout.Left;
                expandLayout = JellyLayout.BottomLeft;
                break;
            case JellyLayout.TopRight:
                newLayout = JellyLayout.Right;
                expandLayout = JellyLayout.BottomRight;
                break;
            case JellyLayout.Top:
                newLayout = JellyLayout.Center;
                expandLayout = JellyLayout.Bottom;
                break;
            default:
                return suface;
        }
        if (TryLayout(expandLayout, ref suface))
        {
            DoExpand(newLayout);
        }
        return suface;
    }

    public void DoDead()
    {
        transform.DOScale(DEAD_SCALE, GameBoardManager.JELLY_DEAD_TIME);
        transform.DOLocalPath(new Vector3[] { DEAD_TO }, GameBoardManager.JELLY_DEAD_TIME);
        GameBoardManager.Instance.MakeGoal(_index);
    }

    public void DoExpand(JellyLayout newLayout)
    {
        _layout = newLayout;
        RenderJelly(GameBoardManager.JELLY_DEAD_TIME);
    }

    public void RenderJelly(float time)
    {
        transform.DOScale(GameBoardManager.JELLY_SCALE * LAYOUT_SCALE[(int)_layout], time);
        transform.DOLocalPath(new Vector3[] { GameBoardManager.BOX_SIZE * LAYOUT_POS[(int)_layout] }, time);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void OnDisabled()
    {
        transform.parent = GameBoardManager.Instance.poolRoot;
    }

    public void OnEnabled()
    {
        //throw new System.NotImplementedException();
    }

    public void Recycle()
    {
        GameBoardManager.Instance.RecycleJelly(this);
    }

    private void Awake()
    {
        _materialPropertyBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        _spring.Pinch(Vector3.ClampMagnitude(_laspos - transform.position, 1));
        Vector3 jellyMove = _spring.Simulate(1, Time.deltaTime, 10, 0.6f);
        if (jellyMove.magnitude > Vector3.kEpsilon)
        {
            _materialPropertyBlock.SetVector(AssetConst.SHADERPROP_JELLY_MOVE, jellyMove);
            _renderer.SetPropertyBlock(_materialPropertyBlock);
        }
        _laspos = transform.position;
    }
}

public class SimpleSpring
{
    private Vector3 _position;
    private Vector3 _v;

    public void Reset()
    {
        _position = Vector3.zero;
        _v = Vector3.zero;
    }

    public Vector3 Simulate(float mass, float time, float stiffness, float loss)
    {
        _v *= (1 - loss);
        _v -= time * stiffness / mass * _position;
        _position += _v;
        return _position;
    }

    public void Pinch(Vector3 direction)
    {
        _position += direction;
    }
}