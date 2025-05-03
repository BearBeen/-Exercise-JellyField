using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public partial class GameBoardManager : MonoSingleton<GameBoardManager>
{
    public const float BOX_SIZE = 2f;
    public const float JELLY_SCALE = 0.9f;
    public const float JELLY_ACTION_DELAY = 0.1f;
    public const float JELLY_DEAD_TIME = 0.2f;
    public const float JELLY_EXPAND_TIME = 0.2f;
    private const float SPACING = 1.2f;

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
    //private int _turnIndex = 0;
    private int _turnCount = 0;
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
    public int xSize => _xSize;
    public int ySize => _ySize;
    public Transform poolRoot => _poolRoot;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 BoardIndexToPosition(int x, int y)
    {
        return BOX_SIZE * SPACING * new Vector3(x - Instance.xCenterOffset, 0, y - Instance.yCenterOffset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PositionToBoardIndex(Vector3 position, out int x, out int y)
    {
        x = Mathf.RoundToInt((position.x / SPACING / BOX_SIZE) + Instance.xCenterOffset);
        y = Mathf.RoundToInt((position.z / SPACING / BOX_SIZE) + Instance.yCenterOffset);
    }

    //TODO: make a specific logic for this

    //Refactoring config. Need a better way to config this.
    //Base logic: every level have 1 palette. this is where all playable jelly on that level being config.
    //Each box contains some jelly index. those index point to the config.
    //well. maybe a gg work sheet + 1 import mechanic is okie then

    private void Start()
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
        _boxDraggers = new List<BoxDragger>(2)
        {
            Instantiate(_boxDraggerPrototype.gameObject).GetComponent<BoxDragger>(),
            Instantiate(_boxDraggerPrototype.gameObject).GetComponent<BoxDragger>()
        };
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
            _turnCount++;
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
            CheckChange();
        }
        _dropIndicator.gameObject.SetActive(false);
    }

    public void MakeGoal(int index)
    {
        Goal goal;
        for (int i = 0, length = _goals.Count; i < length; i++)
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
        //init game difficulty
        _gameAttributeSet = new AttributeSet<GameAttribute>();
        _gameAttributeSet.Add(_gameDifficultyConfig.baseAttrs);
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
        //_turnIndex = 0;
        _turnCount = 0;
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
                _goalUIs.RemoveAt(_goalUIs.Count - 1);
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
            //TODO: make it work. this just a prototype for skill
            jellyConfig = boxConfig.Jellies[i];
            newJelly = _jellyPools[jellyConfig.Index].Get();
            //MissileSkillInstance skillInstance = SkillManager.Instance.GetSkillInstance<MissileSkillData, MissileSkillInstance>();
            //SlashSkillInstance skillInstance = SkillManager.Instance.GetSkillInstance<SlashSkillData, SlashSkillInstance>();
            //BombSkillInstance skillInstance = SkillManager.Instance.GetSkillInstance<BombSkillData, BombSkillInstance>();
            //skillInstance.Init(newJelly, jellyConfig.Index);
            newJelly.InitJelly(jellyConfig.Layout, jellyConfig.Index, null);
            result.Add(newJelly);
        }
        return result;
    }

    private void RefillDragger(int index)
    {
        int boardColorCount = _gameDifficultyConfig.GetBoardColor(_turnCount);
        int minColorPerBox = _gameDifficultyConfig.GetMinColorPerBox(_turnCount);
        int indicesCount = UnityEngine.Random.Range(minColorPerBox, JELLY_LAYOUTS.Length);
        List<int> paletteIndices = new List<int>();
        for (int i = 0, length = boardColorCount; i < length; i++)
        {
            paletteIndices.Add(i);
        }
        paletteIndices = paletteIndices.GetRandoms(indicesCount);
        JellyBox newBox = GenerateJellyBox(JELLY_LAYOUTS[indicesCount].GetRandom(), paletteIndices, null);
        _boxDraggers[index].Refill(newBox);
    }

    private void CheckChange()
    {
        float delay = 0;
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
                _jellyBoxes[cache[i].x][cache[i].y].ClearDead();
            }
            delay += JELLY_DEAD_TIME + JELLY_ACTION_DELAY * 2;
            for (int i = 0, length = cache.Count; i < length; i++)
            {
                _jellyBoxes[cache[i].x][cache[i].y].ExpandJellies(delay);
            }
            delay += JELLY_EXPAND_TIME;
        }

        //check losing
        if (CountEmptyBox() <= 0)
        {
            ToLosing();
        }
    }

    private int CountEmptyBox()
    {
        int count = 0;
        for (int x = 0; x < _xSize; x++)
        {
            for (int y = 0; y < _ySize; y++)
            {
                if (_jellyBoxes[x][y] != null && _jellyBoxes[x][y].jellyCount == 0)
                {
                    count++;
                }
            }
        }
        return count;
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

    public void FindAllJellies(int paletteIndex, int x, int y, int xExpand, int yExpand, bool isIncludeCenter, ref List<Jelly> jellies)
    {
        xExpand = Mathf.Max(0, xExpand);
        yExpand = Mathf.Max(yExpand);
        int startX = Mathf.Max(0, x - xExpand);
        int endX = Mathf.Min(_xSize - 1, x + xExpand);
        int startY = Mathf.Max(0, y - yExpand);
        int endY = Mathf.Min(_ySize - 1, y + yExpand);
        for (int xIndex = startX; xIndex <= endX; xIndex++)
        {
            for (int yIndex = startY; yIndex <= endY; yIndex++)
            {
                JellyBox jellyBox = _jellyBoxes[xIndex][yIndex];
                if (jellyBox == null || (!isIncludeCenter && xIndex == x && yIndex == y)) continue;
                jellyBox.FindJelly(paletteIndex, ref jellies);
            }
        }
    }

    public void KillJelly(int paletteIndex, int x, int y, int xExpand, int yExpand)
    {
        xExpand = Mathf.Max(0, xExpand);
        yExpand = Mathf.Max(yExpand);
        if (x + xExpand < 0 || x - xExpand > _xSize - 1 || y + yExpand < 0 || y - yExpand > _ySize - 1) return;
        int startX = Mathf.Max(0, x - xExpand);
        int endX = Mathf.Min(_xSize - 1, x + xExpand);
        int startY = Mathf.Max(0, y - yExpand);
        int endY = Mathf.Min(_ySize - 1, y + yExpand);
        for (int xIndex = startX; xIndex <= endX; xIndex++)
        {
            for (int yIndex = startY; yIndex <= endY; yIndex++)
            {
                JellyBox jellyBox = _jellyBoxes[xIndex][yIndex];
                if (jellyBox != null)
                {
                    jellyBox.KillJelly(paletteIndex);
                }
            }
        }
    }
}

public enum GameState
{
    Playing,
    Losing,
    Winning,
}
