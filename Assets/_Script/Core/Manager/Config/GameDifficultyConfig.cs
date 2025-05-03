using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDifficultyConfig", menuName = "ScriptableObjects/GameDifficultyConfig")]
public class GameDifficultyConfig : ScriptableObject
{
    [SerializeField] private List<AttributeInstance<GameAttribute>> _baseAttrs;
    [SerializeField] private List<AttributeInstance<GameAttribute>> _changeEachTurn;

    public List<AttributeInstance<GameAttribute>> baseAttrs => _baseAttrs;
    public List<AttributeInstance<GameAttribute>> changeEachTurn => _changeEachTurn;
    //TODO: make this configurable
    public int GetMinColorPerBox(int turnCount) => 1;
    public Vector2Int GetBoardSize(int turnCount) => Vector2Int.zero;
    public int GetBoardColor(int turnCount) => 5;
}
