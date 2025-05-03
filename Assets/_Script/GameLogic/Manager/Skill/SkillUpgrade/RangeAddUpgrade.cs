using System;
using UnityEngine;

[Serializable]
public struct RangeAddUpgrade: IRangeUpgrade
{
    [SerializeField] private int _add;

    public int priority => 0;

    public SkillUpgradeType upgradeType => SkillUpgradeType.RangeUpgrade;

    public SkillUpgradeType[] upgradeTypes => null;

    public int UpgradeRange(int range)
    {
        return range + _add;
    }
}
