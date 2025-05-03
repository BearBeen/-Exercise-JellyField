using System;
using UnityEngine;

[Serializable]
public struct RangeMulUpgrade : IRangeUpgrade
{
    [SerializeField] private float _mul;
    public int priority => 10;

    public SkillUpgradeType upgradeType => SkillUpgradeType.RangeUpgrade;

    public SkillUpgradeType[] upgradeTypes => null;

    public int UpgradeRange(int range)
    {
        return Mathf.FloorToInt(range * _mul);
    }
}
