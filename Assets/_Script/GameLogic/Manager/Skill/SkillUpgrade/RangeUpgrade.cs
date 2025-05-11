using System;
using SerializeReferenceEditor;
using UnityEngine;

[Serializable, SRName("Range")]
public struct RangeUpgrade : IRangeUpgrade
{
    [SerializeField] private float _possibility;
    [SerializeField] private GameAttribute _negAttr;
    [SerializeField] private float _priceMin;
    [SerializeField] private float _priceMax;
    [SerializeField] private float _add;
    [SerializeField] private float _per;
    [SerializeField] private float _rangeMax;

    public float possibility => _possibility;
    public SkillUpgradeType upgradeType => SkillUpgradeType.RangeUpgrade;
    public SkillUpgradeType[] upgradeTypes => null;
    public float rangePer => _per;
    public float rangeAdd => _add;
    public string upgradeDes => string.Format(ConfigManager.Instance.GetString("RangeUpgradeDes"), _add, _per + 1);
    public float rangeMax => _rangeMax;
    public float priceMin => _priceMin;
    public float priceMax => _priceMax;
    public GameAttribute negAttr => _negAttr;
}
