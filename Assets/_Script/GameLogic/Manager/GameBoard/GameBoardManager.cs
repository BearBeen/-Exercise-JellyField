using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameBoardManager : MonoSingleton<GameBoardManager>
{
    public const float BOX_SIZE = 2f;
    public const float JELLY_SCALE = 0.9f;
    public const float JELLY_DEAD_TIME = 0.15f;
    private const float SPACING = 1.2f;
    private static readonly WaitForSeconds WAIT_JELLY_DEAD = new WaitForSeconds(JELLY_DEAD_TIME);

    [SerializeField] List<LevelConfig> _levels;
    [SerializeField] JellyBox _boxPrototype;
    [SerializeField] GoalUI _goalUIPrototype;
    [SerializeField] Transform _win;
    [SerializeField] Transform _lose;
    [SerializeField] TMP_Text _level;
    [SerializeField] BoxDragger _boxDraggerPrototype;
    [SerializeField] Transform _dropIndicator;
    [SerializeField] Transform _poolRoot;
    [SerializeField] Transform _draggerAnchor0;
    [SerializeField] Transform _draggerAnchor1;

    private JellyBox[][] _jellyBoxes;
    private int _levelIndex = 0;
    private int _turnIndex = 0;
    private Pool<JellyBox> _jellyBoxPool;
    private Dictionary<int, Pool<Jelly>> _jellyPools;
    private int _xSize;
    private int _ySize;
    private List<BoxDragger> _boxDraggers;
    private List<Goal> _goals;
    private int _passedCount;
    private List<Vector2Int> _changeBoxes;
    private List<GoalUI> _goalUIs;

    public float xCenterOffset => (_xSize - 1) * 0.5f;
    public float yCenterOffset => (_ySize - 1) * 0.5f;
    public Transform poolRoot => _poolRoot;

    public static Vector3 BoardIndexToPosition(int x, int y)
    {
        return BOX_SIZE * SPACING * new Vector3(x - Instance.xCenterOffset, 0, y - Instance.yCenterOffset);
    }

    public static void PositionToBoardIndex(Vector3 position, out int x, out int y)
    {
        x = Mathf.RoundToInt((position.x / SPACING / BOX_SIZE) + Instance.xCenterOffset);
        y = Mathf.RoundToInt((position.z / SPACING / BOX_SIZE) + Instance.yCenterOffset);
    }

    public override void Init()
    {
        if (PlayerPrefs.HasKey(AssetConst.KEY_GAMEBOARDMANAGER_LVLINDEX))
        {
            _levelIndex = PlayerPrefs.GetInt(AssetConst.KEY_GAMEBOARDMANAGER_LVLINDEX);
        }
        else
        {
            _levelIndex = 0;
        }
        _jellyBoxPool = new Pool<JellyBox>(() => Instantiate(_boxPrototype.gameObject).GetComponent<JellyBox>());
        _jellyPools = new Dictionary<int, Pool<Jelly>>();
        _boxDraggers = new List<BoxDragger>(2);
        _boxDraggers.Add(Instantiate(_boxDraggerPrototype.gameObject).GetComponent<BoxDragger>());
        _boxDraggers.Add(Instantiate(_boxDraggerPrototype.gameObject).GetComponent<BoxDragger>());
        _boxDraggers[0].transform.parent = _draggerAnchor0;
        _boxDraggers[0].InitBoxDragger(0);
        _boxDraggers[1].transform.parent = _draggerAnchor1;
        _boxDraggers[1].InitBoxDragger(1);
        _goals = new List<Goal>();
        _changeBoxes = new List<Vector2Int>();
        _goalUIs = new List<GoalUI>();
        _goalUIPrototype.gameObject.SetActive(false);
        ToPlaying();
    }

    public void RecycleBox(JellyBox jellyBox)
    {
        _jellyBoxPool.Return(jellyBox);
    }

    public void RecycleJelly(Jelly jelly)
    {
        _jellyPools[jelly.index].Return(jelly);
    }

    public void ShowDropPoint(Vector3 position)
    {
        PositionToBoardIndex(position, out int x, out int y);
        if (x >= 0 && x < _xSize && y >= 0 && y < _ySize && _jellyBoxes[x][y] != null && _jellyBoxes[x][y].jellyCount == 0)
        {
            _dropIndicator.gameObject.SetActive(true);
            _dropIndicator.position = BoardIndexToPosition(x, y);
        }
        else
        {
            _dropIndicator.gameObject.SetActive(false);
        }
    }

    public void TryPutBox(Vector3 position, int index)
    {
        PositionToBoardIndex(position, out int x, out int y);
        if (x >= 0 && x < _xSize && y >= 0 && y < _ySize && _jellyBoxes[x][y] != null && _jellyBoxes[x][y].jellyCount == 0)
        {
            _jellyBoxes[x][y].Recycle();
            JellyBox newBox = _boxDraggers[index].jellyBox;
            newBox.transform.parent = null;
            newBox.transform.position = BoardIndexToPosition(x, y);
            _jellyBoxes[x][y] = newBox;
            newBox.name = x + "_" + y;
            RefillDragger(index);
            _changeBoxes.Add(new Vector2Int(x, y));
            if (x > 0 && _jellyBoxes[x - 1][y] != null && _jellyBoxes[x - 1][y].jellyCount > 0)
            {
                _changeBoxes.Add(new Vector2Int(x - 1, y));
            }
            if (x < _xSize - 1 && _jellyBoxes[x + 1][y] != null && _jellyBoxes[x + 1][y].jellyCount > 0)
            {
                _changeBoxes.Add(new Vector2Int(x + 1, y));
            }
            if (y < _ySize - 1 && _jellyBoxes[x][y + 1] != null && _jellyBoxes[x][y + 1].jellyCount > 0)
            {
                _changeBoxes.Add(new Vector2Int(x, y + 1));
            }
            if (y > 0 && _jellyBoxes[x][y - 1] != null && _jellyBoxes[x][y - 1].jellyCount > 0)
            {
                _changeBoxes.Add(new Vector2Int(x, y - 1));
            }
            StartCoroutine(CheckChange());
        }
        _dropIndicator.gameObject.SetActive(false);
    }

    public void MakeGoal(int index)
    {
        Goal goal;
        for (int i = 0, length = _goals.Count; i< length; i++)
        {
            goal = _goals[i];
            if (goal.Index == index)
            {
                if (goal.Count == 0)
                {
                    break;
                }
                goal.Count = goal.Count - 1;
                _goalUIs[i].SetCount(goal.Count);
                if (goal.Count == 0)
                {
                    _passedCount++;
                    if (_passedCount == _goals.Count)
                    {
                        ToWinning();
                    }
                }
                break;
            }
        }
    }
        
    public void InitGameBoard()
    {
        //clear old data
        if (_jellyBoxes != null)
        {
            foreach (JellyBox[] column in _jellyBoxes)
            {
                if (column != null)
                {
                    foreach (JellyBox box in column)
                    {
                        box?.Recycle();
                    }
                }
            }
        }
        foreach (int key in _jellyPools.Keys)
        {
            _jellyPools[key].ForceClear();
        }
        _jellyPools.Clear();

        LevelConfig levelConfig = _levels[_levelIndex];
        _xSize = levelConfig.XSize;
        _ySize = levelConfig.YSize;
        //init jelly pool
        for (int i = 0, length = levelConfig.Palette.Count; i < length; i++)
        {
            Jelly prototype = levelConfig.Palette[i];
            _jellyPools[i] = new Pool<Jelly>(() => Instantiate(prototype.gameObject).GetComponent<Jelly>());
        }
        //init game board
        JellyBox newBox;
        _jellyBoxes = new JellyBox[levelConfig.XSize][];
        for (int x = 0; x < levelConfig.XSize; x++)
        {
            _jellyBoxes[x] = new JellyBox[levelConfig.YSize];
            for (int y = 0; y < levelConfig.YSize; y++)
            {
                if (levelConfig.Column[x].Boxes[y] != null)
                {
                    newBox = _jellyBoxPool.Get();
                    newBox.transform.parent = null;
                    newBox.transform.position = BoardIndexToPosition(x, y);
                    newBox.InitJellies(CreateJelliesFromBox(levelConfig.Column[x].Boxes[y]));
                    newBox.name = x + "_" + y;
                    _jellyBoxes[x][y] = newBox;
                }
            }
        }
        //refill dragger
        _turnIndex = 0;
        _boxDraggers[0].Clear();
        _boxDraggers[1].Clear();
        RefillDragger(0);
        RefillDragger(1);
        _dropIndicator.gameObject.SetActive(false);
        //init goals
        _goals.Clear();
        while (_goalUIs.Count != levelConfig.Goals.Count)
        {
            if (_goalUIs.Count > levelConfig.Goals.Count)
            {
                GoalUI goalUI = _goalUIs[_goalUIs.Count - 1];
                _goalUIs.RemoveAt(_goalUIs.Count -1);
                DestroyImmediate(goalUI.gameObject);
            }
            else
            {
                GoalUI goalUI = Instantiate(_goalUIPrototype, _goalUIPrototype.transform.parent).GetComponent<GoalUI>();
                goalUI.gameObject.SetActive(true);
                _goalUIs.Add(goalUI);
            }
        }
        for (int i = 0, length = levelConfig.Goals.Count; i < length; i++)
        {
            _goals.Add(new Goal() { Index = levelConfig.Goals[i].Index, Count = levelConfig.Goals[i].Count });
            _goalUIs[i].InitGoalUI(levelConfig.Palette[_goals[i].Index].GetComponent<Renderer>().sharedMaterial.color, _goals[i].Count);
        }
        _passedCount = 0;
        _changeBoxes.Clear();
    }

    private List<Jelly> CreateJelliesFromBox(JellyBoxConfig boxConfig)
    {
        List<Jelly> result = new List<Jelly>();
        Jelly newJelly;
        JellyConfig jellyConfig;
        for (int i = 0, length = boxConfig.Jellies.Count; i < length; i++)
        {
            jellyConfig = boxConfig.Jellies[i];
            newJelly = _jellyPools[jellyConfig.Index].Get();
            newJelly.InitJelly(jellyConfig.Layout, jellyConfig.Index);
            result.Add(newJelly);
        }
        return result;
    }

    private void RefillDragger(int index)
    {
        LevelConfig levelConfig = _levels[_levelIndex];
        JellyBox newBox = _jellyBoxPool.Get();
        newBox.InitJellies(CreateJelliesFromBox(levelConfig.Turns[_turnIndex]));
        _boxDraggers[index].Refill(newBox);
        _turnIndex = (_turnIndex + 1) % levelConfig.Turns.Count;
    }

    private IEnumerator CheckChange()
    {
        int comboCount = 0;
        int x, y;
        JellyBox left, right, top, bottom;
        List<Vector2Int> cache;
        HashSet<int> check = new HashSet<int>();
        while (_changeBoxes.Count != 0)
        {
            cache = new List<Vector2Int>(_changeBoxes);
            check.Clear();
            _changeBoxes.Clear();
            for (int i = 0, length = cache.Count; i < length; i++)
            {
                x = cache[i].x;
                y = cache[i].y;
                
                if (check.Contains(x + y * _xSize))
                {
                    continue;
                }

                left = x > 0 ? _jellyBoxes[x - 1][y] : null;
                right = x < _xSize - 1 ? _jellyBoxes[x + 1][y] : null;
                top = y < _ySize - 1 ? _jellyBoxes[x][y + 1] : null;
                bottom = y > 0 ? _jellyBoxes[x][y - 1] : null;

                if (_jellyBoxes[x][y].MatchJellyBox(left, right, top, bottom))
                {
                    _changeBoxes.Add(new Vector2Int(x, y));
                    check.Add(x + y * _xSize);
                    if (left != null && left.jellyCount > 0)
                    {
                        _changeBoxes.Add(new Vector2Int(x - 1, y));
                    }
                    if (right != null && right.jellyCount > 0)
                    {
                        _changeBoxes.Add(new Vector2Int(x + 1, y));
                    }
                    if (top != null && top.jellyCount > 0)
                    {
                        _changeBoxes.Add(new Vector2Int(x, y + 1));
                    }
                    if (bottom != null && bottom.jellyCount > 0)
                    {
                        _changeBoxes.Add(new Vector2Int(x, y - 1));
                    }
                }
            }
            for (int i = 0, length = cache.Count; i < length; i++)
            {
                _jellyBoxes[cache[i].x][cache[i].y].ExpandJellies();
            }
            yield return WAIT_JELLY_DEAD;
            for (int i = 0, length = cache.Count; i < length; i++)
            {
                _jellyBoxes[cache[i].x][cache[i].y].ClearDead();
            }
            comboCount = _changeBoxes.Count != 0 ? comboCount + 1 : comboCount;
        }

        //check losing
        for (x = 0; x < _xSize; x++)
        {
            for (y = 0; y < _ySize; y++)
            {
                if (_jellyBoxes[x][y] != null && _jellyBoxes[x][y].jellyCount == 0)
                {
                    yield break;
                }
            }
        }
        //lose
        ToLosing();
    }

    public void ToPlaying()
    {
        _level.text = "Lvl. " + (_levelIndex + 1);
        ShowOnly(null);
        InitGameBoard();
    }

    public void ToLosing()
    {
        ShowOnly(_lose);
    }

    public void ToWinning()
    {
        _levelIndex = (_levelIndex + 1) % _levels.Count;
        PlayerPrefs.SetInt(AssetConst.KEY_GAMEBOARDMANAGER_LVLINDEX, _levelIndex);
        ShowOnly(_win);
    }

    private void ShowOnly(Transform show)
    {
        _win.gameObject.SetActive(_win == show);
        _lose.gameObject.SetActive(_lose == show);
    }
}

public enum GameState
{
    Playing,
    Losing,
    Winning,
}
