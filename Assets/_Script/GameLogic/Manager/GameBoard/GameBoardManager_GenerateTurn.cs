using System.Collections.Generic;

using UnityEngine;

public partial class GameBoardManager : MonoSingleton<GameBoardManager>
{
    private static readonly JellyLayout[][] _1_IDX_BOX = new JellyLayout[1][]
    {
        new JellyLayout[]{JellyLayout.Center},
    };

    private static readonly JellyLayout[][] _2_IDX_BOX = new JellyLayout[2][]
    {
        new JellyLayout[]{JellyLayout.Left, JellyLayout.Right},
        new JellyLayout[]{JellyLayout.Top, JellyLayout.Bottom},
    };

    private static readonly JellyLayout[][] _3_IDX_BOX = new JellyLayout[4][]
    {
        new JellyLayout[]{JellyLayout.Left, JellyLayout.TopRight, JellyLayout.BottomRight},
        new JellyLayout[]{JellyLayout.TopLeft, JellyLayout.BottomLeft, JellyLayout.Right},
        new JellyLayout[]{JellyLayout.Top, JellyLayout.BottomLeft, JellyLayout.BottomRight},
        new JellyLayout[]{JellyLayout.TopLeft, JellyLayout.TopRight, JellyLayout.Bottom},
    };

    private static readonly JellyLayout[][] _4_IDX_BOX = new JellyLayout[1][]
    {
        new JellyLayout[]{JellyLayout.TopLeft, JellyLayout.TopRight, JellyLayout.BottomLeft, JellyLayout.BottomRight},
    };

    private static readonly JellyLayout[][][] JELLY_LAYOUTS = new JellyLayout[5][][]
    {
        null,
        _1_IDX_BOX,
        _2_IDX_BOX,
        _3_IDX_BOX,
        _4_IDX_BOX,
    };
    
    [SerializeField] private GameDifficultyConfig _gameDifficultyConfig;
    private AttributeSet<GameAttribute> _gameAttributeSet;

    private float _cubeGenCount;
    private float _skillGenCount;

    private void UpdateBoardAttribute()
    {
        _cubeGenCount += _gameAttributeSet.GetFloat(GameAttribute.BoxGenEachTurn) / 1000f;
        _skillGenCount += _gameAttributeSet.GetFloat(GameAttribute.SkillCountEachTurn) / 1000f;
        Vector2Int boardSize = _gameDifficultyConfig.GetBoardSize(_turnCount);
        //TODO: expanding board.
    }
    
    private void GenerateTurn()
    {
        int boxThisTurn = (int) _cubeGenCount;
        _cubeGenCount -= boxThisTurn;
        if (UnityEngine.Random.Range(0f, 1f) < _cubeGenCount)
        {
            _cubeGenCount = 0;
            boxThisTurn++;
        }

        int skillThisTurn = (int)_skillGenCount;
        _skillGenCount -= skillThisTurn;
        if (UnityEngine.Random.Range(0f, 1f) < _skillGenCount)
        {
            _skillGenCount = 0;
            skillThisTurn++;
        }

        List<Vector2Int> generatableBoardIndices = new List<Vector2Int>();
        List<int>[,] nearbyIndicesCache = new List<int>[_xSize, _ySize];
        int boardColorCount = _gameDifficultyConfig.GetBoardColor(_turnCount);
        int minColorPerBox = _gameDifficultyConfig.GetMinColorPerBox(_turnCount);

        for (int x = 0; x < _xSize; x++)
        {
            for (int y = 0; y < _ySize; y++)
            {
                if (IsIndexGeneratable(x, y, boardColorCount,
                    minColorPerBox,
                    ref nearbyIndicesCache))
                {
                    generatableBoardIndices.Add(new Vector2Int(x, y));
                }
            }
        }

        int genMax = Mathf.Min(CountEmptyBox() - 1, boxThisTurn, generatableBoardIndices.Count);
        int genCount = 0;
        while (genMax > 0 && genCount < genMax)
        {
            Vector2Int boardIndex = generatableBoardIndices.GetRandom();
            List<int> nearbyIndices = nearbyIndicesCache[boardIndex.x, boardIndex.y];
            int indicesCount = Mathf.Min(
                UnityEngine.Random.Range(minColorPerBox, JELLY_LAYOUTS.Length),
                boardColorCount - (nearbyIndices?.Count ?? 0));
            List<int> paletteIndices = new List<int>();
            for (int i = 0; i < boardColorCount; i++)
            {
                if (nearbyIndices == null || nearbyIndices.IndexOf(i) < 0) paletteIndices.Add(i);
            }
            paletteIndices = paletteIndices.GetRandoms(indicesCount);
            AbsSkillInstance skillInstance = null;
            if (skillThisTurn > 0)
            {
                skillThisTurn--;
                skillInstance = SkillManager.Instance.GetRandomSkillInstance();
            }
            JellyBox newBox = GenerateJellyBox(JELLY_LAYOUTS[indicesCount].GetRandom(), paletteIndices, skillInstance);
            _jellyBoxes[boardIndex.x][boardIndex.y].Recycle();
            newBox.transform.parent = null;
            newBox.transform.position = BoardIndexToPosition(boardIndex.x, boardIndex.y);
            _jellyBoxes[boardIndex.x][boardIndex.y] = newBox;
            newBox.name = boardIndex.x + "_" + boardIndex.y;
            generatableBoardIndices.Remove(boardIndex);

            Vector2Int left, right, top, bottom;
            left = right = top = bottom = boardIndex;
            left.x--; right.x++; top.y++; bottom.y--;
            int indexInGeneratable;
            if ((indexInGeneratable = generatableBoardIndices.IndexOf(left)) >= 0 &&
                !IsIndexGeneratable(left.x, left.y, boardColorCount, minColorPerBox, ref nearbyIndicesCache))
            {
                generatableBoardIndices.RemoveAt(indexInGeneratable);
            }
            if ((indexInGeneratable = generatableBoardIndices.IndexOf(right)) >= 0 &&
                !IsIndexGeneratable(right.x, right.y, boardColorCount, minColorPerBox, ref nearbyIndicesCache))
            {
                generatableBoardIndices.RemoveAt(indexInGeneratable);
            }
            if ((indexInGeneratable = generatableBoardIndices.IndexOf(top)) >= 0 &&
                !IsIndexGeneratable(top.x, top.y, boardColorCount, minColorPerBox, ref nearbyIndicesCache))
            {
                generatableBoardIndices.RemoveAt(indexInGeneratable);
            }
            if ((indexInGeneratable = generatableBoardIndices.IndexOf(bottom)) >= 0 &&
                !IsIndexGeneratable(bottom.x, bottom.y, boardColorCount, minColorPerBox, ref nearbyIndicesCache))
            {
                generatableBoardIndices.RemoveAt(indexInGeneratable);
            }
            
            genCount++;
            genMax = Mathf.Min(CountEmptyBox() - 1, boxThisTurn, generatableBoardIndices.Count);
        }

        bool IsIndexGeneratable(int x, int y, int maxIndicesCount, int requireIndicesCount, ref List<int>[,] nearbyIndicesCache)
        {
            if (x < 0 || x >= _xSize || y < 0 || y >= _ySize) return false;
            JellyBox box = _jellyBoxes[x][y];
            if (box == null || box.jellyCount > 0) return false;
            JellyBox left = x > 0 ? _jellyBoxes[x - 1][y] : null;
            JellyBox right = x < _xSize - 1 ? _jellyBoxes[x + 1][y] : null;
            JellyBox top = y < _ySize - 1 ? _jellyBoxes[x][y + 1] : null;
            JellyBox bottom = y > 0 ? _jellyBoxes[x][y - 1] : null;
            if ((left == null || left.jellyCount == 0) &&
                (right == null || right.jellyCount == 0) &&
                (top == null || top.jellyCount == 0) &&
                (bottom == null || bottom.jellyCount == 0)) return true;

            List<int> nearbyIndices = JellyBox.CountJellyIndices(left, right, top, bottom);
            nearbyIndicesCache[x, y] = nearbyIndices;
            return maxIndicesCount - nearbyIndices.Count >= requireIndicesCount;
        }
    }

    JellyBox GenerateJellyBox(JellyLayout[] jellyLayouts, List<int> paletteIndices, AbsSkillInstance skillInstance)
    {
        JellyBox newBox = _jellyBoxPool.Get();
        List<Jelly> jellies = new List<Jelly>();
        int jellyIndexGotSkill = -1;
        int boardColorCount = _gameDifficultyConfig.GetBoardColor(_turnCount);
        if (skillInstance != null)
        {
            jellyIndexGotSkill = UnityEngine.Random.Range(0, jellyLayouts.Length);
        }
        for (int i = 0; i < jellyLayouts.Length; i++)
        {
            Jelly newJelly = _jellyPools[paletteIndices[i]].Get();
            if (i == jellyIndexGotSkill)
            {
                newJelly.InitJelly(jellyLayouts[i], paletteIndices[i], skillInstance);
                skillInstance.Init(newJelly, UnityEngine.Random.Range(0, boardColorCount));
            }
            else
            {
                newJelly.InitJelly(jellyLayouts[i], paletteIndices[i], null);
            }
            jellies.Add(newJelly);
        }
        newBox.InitJellies(jellies);
        return newBox;
    }

    public void UpgradeGameAttribute(AttributeInstance<GameAttribute> attributeInstance)
    {
        _gameAttributeSet.Add(attributeInstance, false);
    }

    public void UpgradeGameAttribute(GameAttribute attribute, int value)
    {
        _gameAttributeSet.Add(attribute, value);
    }
}
