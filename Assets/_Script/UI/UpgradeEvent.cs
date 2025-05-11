using System.Collections.Generic;

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
    void DrawCardUI(UpgradeCardUI upgradeCardUI);
}

public struct AttrUpgradeCardData : IUpgradeCardData
{

    private GameAttribute _posAttr;
    private int _posValue;
    private GameAttribute _negAttr;
    private int _negValue;

    public UpgradeCardType upgradeCardType => UpgradeCardType.AttrUpgrade;

    public AttrUpgradeCardData(GameAttribute posAttr, int posValue, GameAttribute negAttr, int negValue)
    {
        _posAttr = posAttr;
        _posValue = posValue;
        _negAttr = negAttr;
        _negValue = negValue;
    }

    public void DrawCardUI(UpgradeCardUI upgradeCardUI)
    {
        float rate = GameBoardManager.GAME_ATTR_BASE_RATE;
        upgradeCardUI.SetupColor(
            new Color(0.600f, 1.000f, 1.000f, 1.000f),
            new Color(1.000f, 0.537f, 0.537f, 1.000f));
        upgradeCardUI.SetupText(
            "+ " + ConfigManager.Instance.GetString(_posAttr.ToString()) + " by " + (_posValue * 100 / rate).ToString("0.00") + "%",
            "You gain some, you lose some. Be careful with what you bet, as it will not always be fair.",
            "+ " + ConfigManager.Instance.GetString(_negAttr.ToString()) + " by " + (_negValue * 100 / rate).ToString("0.00") + "%"
        );
        upgradeCardUI.SetupUpgrade(Upgrade);
    }

    public void Upgrade()
    {
        GameBoardManager.Instance.UpgradeGameAttribute(_posAttr, _posValue);
        GameBoardManager.Instance.UpgradeGameAttribute(_negAttr, _negValue);
    }
}

public struct SkillLearnCardData : IUpgradeCardData
{
    private int _skillID;
    private GameAttribute _negAttr;
    private int _negValue;

    public UpgradeCardType upgradeCardType => UpgradeCardType.SkillLearn;

    public SkillLearnCardData(int skillID, GameAttribute negAttr, int negValue)
    {
        _skillID = skillID;
        _negAttr = negAttr;
        _negValue = negValue;
    }

    public void DrawCardUI(UpgradeCardUI upgradeCardUI)
    {
        float rate = GameBoardManager.GAME_ATTR_BASE_RATE;
        upgradeCardUI.SetupColor(
            new Color(1.000f, 0.992f, 0.447f, 1.000f),
            new Color(0.580f, 0.694f, 1.000f, 1.000f));
        upgradeCardUI.SetupText(
            "Learn " + SkillManager.Instance.GetSkillName(_skillID),
            "It's always cost you something to learn something.",
            "+ " + ConfigManager.Instance.GetString(_negAttr.ToString()) + " by " + (_negValue * 100 / rate).ToString("0.00") + "%"
        );
        upgradeCardUI.SetupUpgrade(Upgrade);
    }

    public void Upgrade()
    {
        SkillManager.Instance.UnlockSkill(_skillID);
        GameBoardManager.Instance.UpgradeGameAttribute(_negAttr, _negValue);
    }
}

public struct SkillUpgradeCardData : IUpgradeCardData
{
    private int _skillID;
    private ISkillUpgrade _skillUpgrade;
    private GameAttribute _negAttr;
    private int _negValue;

    public UpgradeCardType upgradeCardType => UpgradeCardType.SkillUpgrade;

    public SkillUpgradeCardData(int skillID, ISkillUpgrade skillUpgrade, GameAttribute negAttr, int negValue)
    {
        _skillID = skillID;
        _skillUpgrade = skillUpgrade;
        _negAttr = negAttr;
        _negValue = negValue;
    }

    public void DrawCardUI(UpgradeCardUI upgradeCardUI)
    {
        float rate = GameBoardManager.GAME_ATTR_BASE_RATE;
        upgradeCardUI.SetupColor(
            new Color(0.667f, 1.000f, 0.447f, 1.000f),
            new Color(1.000f, 0.580f, 0.965f, 1.000f));
        upgradeCardUI.SetupText(
            "Upgrade " + SkillManager.Instance.GetSkillName(_skillID),
            _skillUpgrade.upgradeDes,
            "+ " + ConfigManager.Instance.GetString(_negAttr.ToString()) + " by " + (_negValue * 100 / rate).ToString("0.00") + "%"
        );
        upgradeCardUI.SetupUpgrade(Upgrade);
    }

    public void Upgrade()
    {
        SkillManager.Instance.UpgradeSkill(_skillID, _skillUpgrade);
        GameBoardManager.Instance.UpgradeGameAttribute(_negAttr, _negValue);
    }
}
