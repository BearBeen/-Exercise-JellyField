using System;
using SerializeReferenceEditor;
using UnityEngine;

[Serializable, SRName("MissileCount")]
public struct MissileCountUpgrade : IMissileCountUpgrade
{
    [SerializeField] private float _possibility;
    [SerializeField] private GameAttribute _negAttr;
    [SerializeField] private float _priceMin;
    [SerializeField] private float _priceMax;
    [SerializeField] private float _add;
    [SerializeField] private float _per;
    [SerializeField] private float _missileMax;

    public float possibility => _possibility;
    public GameAttribute negAttr => _negAttr;
    public float priceMin => _priceMin;
    public float priceMax => _priceMax;
    public SkillUpgradeType upgradeType => SkillUpgradeType.MissileCountUpgrade;
    public SkillUpgradeType[] upgradeTypes => null;
    public float missilePer => _per;
    public float missileAdd => _add;
    public string upgradeDes => string.Format(ConfigManager.Instance.GetString("MissileCountUpgradeDes"), _add, _per + 1);
    public float missileMax => _missileMax;
}
