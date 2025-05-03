using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public enum UpgradeEvent
{
    ShowUpgradePanelUI,
}

public interface UpgradeEventBinder : IGameEventBinder<UpgradeEvent, UpgradeEventData> { }

public struct UpgradeEventData
{
    public List<IUpgradeCardData> upgradeCardDatas;
}

public interface IUpgradeCardData
{
    UpgradeCardType upgradeCardType { get; }
    void Upgrade();
    void FillCardData(UpgradeCardUI upgradeCardUI);
}

public class AttrUpgradeCardData : IUpgradeCardData
{
    private GameAttribute _attrtibute;
    private int _posValue;
    private int _negValue;

    public UpgradeCardType upgradeCardType => UpgradeCardType.AttrUpgrade;

    public AttrUpgradeCardData(GameAttribute gameAttribute, int posValue, int negValue)
    {
        _attrtibute = gameAttribute;
        _posValue = posValue;
        _negValue = negValue;
    }

    public void FillCardData(UpgradeCardUI upgradeCardUI)
    {
        upgradeCardUI.SetupColor(Color.cyan, Color.blue);
    }

    public void Upgrade()
    {
        GameBoardManager.Instance.UpgradeGameAttribute(_attrtibute, _posValue);
        GameBoardManager.Instance.UpgradeGameAttribute(GameAttribute.SkillCountEachTurn_Add, _negValue);
    }
}
