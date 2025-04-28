using System.Collections.Generic;

using DG.Tweening;

using UnityEngine;

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

//TODO: Try seperate Jelly from MonoBehaviour.
//Make it more configurable and poolable.
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
    [SerializeField] private Transform _dockerTrs;
    private Vector3 _lastpos = Vector3.zero;
    private SimpleSpring _spring = new SimpleSpring();
    private MaterialPropertyBlock _materialPropertyBlock;
    private Queue<Sequence> _queuedSequences = new Queue<Sequence>();
    private bool _isAnySequenceDoing = false;
    private AbsSkillInstance _skillInstance;

    public JellyLayout layout => _layout;
    public int index => _index;
    public Transform dockerTrs => _dockerTrs;

    public static bool TryLayout(JellyLayout layout, ref int surface)
    {
        int newSurface = LAYOUT_SURFACE[(int)layout];
        if ((newSurface & surface) == 0)
        {
            surface |= newSurface;
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
        int leftSurface = LAYOUT_SURFACE[(int)leftLayout];
        int rightSurface = LAYOUT_SURFACE[(int)rightLayout];
        leftMask &= rightSurface;
        rightMask &= leftSurface;
        //convert left mask to right mask
        leftMask <<= 1;
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
        int topSurface = LAYOUT_SURFACE[(int)topLayout];
        int bottomSurface = LAYOUT_SURFACE[(int)bottomLayout];
        topMask &= bottomSurface;
        bottomMask &= topSurface;
        //convert top mask to bottom mask
        topMask <<= 2;
        return (bottomMask & topMask) != 0;
    }

    public static List<int> CountJellyIndices(List<Jelly> left, List<Jelly> right, List<Jelly> top, List<Jelly> bottom)
    {
        int leftMask = LAYOUT_SURFACE[(int)JellyLayout.Left];
        int rightMask = LAYOUT_SURFACE[(int)JellyLayout.Right];
        int topMask = LAYOUT_SURFACE[(int)JellyLayout.Top];
        int bottomMask = LAYOUT_SURFACE[(int)JellyLayout.Bottom];

        List<int> result = new List<int>(10);
        for (int i = 0; left != null && i < left.Count; i++)
        {
            Jelly jelly = left[i];
            if ((LAYOUT_SURFACE[(int)jelly._layout] & rightMask) != 0 && result.IndexOf(jelly._index) < 0)
            {
                result.Add(jelly._index);
            }
        }
        for (int i = 0; right != null && i < right.Count; i++)
        {
            Jelly jelly = right[i];
            if ((LAYOUT_SURFACE[(int)jelly._layout] & leftMask) != 0 && result.IndexOf(jelly._index) < 0)
            {
                result.Add(jelly._index);
            }
        }
        for (int i = 0; top != null && i < top.Count; i++)
        {
            Jelly jelly = top[i];
            if ((LAYOUT_SURFACE[(int)jelly._layout] & bottomMask) != 0 && result.IndexOf(jelly._index) < 0)
            {
                result.Add(jelly._index);
            }
        }
        for (int i = 0; bottom != null && i < bottom.Count; i++)
        {
            Jelly jelly = bottom[i];
            if ((LAYOUT_SURFACE[(int)jelly._layout] & topMask) != 0 && result.IndexOf(jelly._index) < 0)
            {
                result.Add(jelly._index);
            }
        }
        return result;
    }

    public void InitJelly(JellyLayout layout, int index, AbsSkillInstance skillInstance)
    {
        _layout = layout;
        _index = index;
        _skillInstance = skillInstance;
    }

    public int TryExpandLeft(int suface, float delay)
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
            Expand(newLayout, delay);
        }
        return suface;
    }

    public int TryExpandRight(int suface, float delay)
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
            Expand(newLayout, delay);
        }
        return suface;
    }

    public int TryExpandTop(int suface, float delay)
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
            Expand(newLayout, delay);
        }
        return suface;
    }

    public int TryExpandBottom(int suface, float delay)
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
            Expand(newLayout, delay);
        }
        return suface;
    }

    public void Die()
    {
        Sequence sequence = DOTween.Sequence().Pause()
        .AppendInterval(GameBoardManager.JELLY_ACTION_DELAY)
        .AppendCallback(CastSkill)
        .Append(transform.DOScale(DEAD_SCALE, GameBoardManager.JELLY_DEAD_TIME))
        .Join(transform.DOLocalPath(new Vector3[] { DEAD_TO }, GameBoardManager.JELLY_DEAD_TIME))
        .AppendCallback(OnDieComplete);
        EnqueueSequence(sequence);
    }

    private void CastSkill()
    {
        _skillInstance?.Cast(this);
    }

    private void OnDieComplete()
    {
        _queuedSequences.Clear();
        GameBoardManager.Instance.MakeGoal(_index);
        Recycle();
    }

    private void EnqueueSequence(Sequence sequence)
    {
        sequence.OnComplete(() =>
        {
            _isAnySequenceDoing = false;
            OnSequenceComplete();
        });
        if (!_isAnySequenceDoing)
        {
            _isAnySequenceDoing = true;
            sequence.Play();
        }
        else
        {
            _queuedSequences.Enqueue(sequence);
        }
    }

    private void OnSequenceComplete()
    {
        if (_queuedSequences.Count > 0)
        {
            _queuedSequences.Dequeue().Play();
            _isAnySequenceDoing = true;
        }
    }

    private void Expand(JellyLayout newLayout, float delay)
    {
        _layout = newLayout;
        JellyFitInEffect(delay, GameBoardManager.JELLY_EXPAND_TIME);
    }

    public void JellyFitInEffect(float delay, float duration)
    {
        Vector3 finalScale = GameBoardManager.JELLY_SCALE * LAYOUT_SCALE[(int)_layout];
        Vector3 finalDockerScale = new Vector3(1 / finalScale.x, 1 / finalScale.y, 1 / finalScale.z);
        Sequence sequence = DOTween.Sequence().Pause()
        .AppendInterval(delay)
        .Append(_dockerTrs.DOScale(finalDockerScale, duration))
        .Join(transform.DOScale(finalScale, duration))
        .Join(transform.DOLocalPath(new Vector3[] { GameBoardManager.BOX_SIZE * LAYOUT_POS[(int)_layout] }, duration));
        EnqueueSequence(sequence);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void OnReturnToPool()
    {
        _queuedSequences.Clear();
        _isAnySequenceDoing = false;
        transform.parent = GameBoardManager.Instance.poolRoot;
    }

    public void OnGetFomPool()
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

    private void FixedUpdate()
    {
        _spring.Pinch(Vector3.ClampMagnitude(_lastpos - transform.position, 1));
        Vector3 jellyMove = _spring.Simulate(Time.deltaTime, 1.25f, 0.25f);
        if (jellyMove.magnitude > Vector3.kEpsilon)
        {
            _materialPropertyBlock.SetVector(AssetConst.SHADERPROP_JELLY_MOVE, jellyMove);
            _renderer.SetPropertyBlock(_materialPropertyBlock);
        }
        _lastpos = transform.position;
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

    public Vector3 Simulate(float time, float stiffness, float loss)
    {
        _v *= 1 - loss;
        _v -= time * stiffness * _position;
        _position += _v;
        return _position;
    }

    public void Pinch(Vector3 direction)
    {
        _position = Vector3.Lerp(_position, direction, 0.3f);
        //_position += direction;
    }
}