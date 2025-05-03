using System;
using UnityEngine;

[Serializable]
public struct AllColorButHalfRangeUpgrade : ITargetJellyIndexUpgrade, IRangeUpgrade
{
    private static readonly SkillUpgradeType[] UPGRADE_TYPES = new SkillUpgradeType[2]
    {
        SkillUpgradeType.TargetJellyIndexUpgrade,
        SkillUpgradeType.RangeUpgrade,
    };

    [SerializeField] private float _mul;
    
    public int priority => 11;

    public SkillUpgradeType upgradeType => SkillUpgradeType.ComposeUpgrade;

    public SkillUpgradeType[] upgradeTypes => UPGRADE_TYPES;

    public int UpgradeRange(int range)
    {
        return Mathf.FloorToInt(range * _mul);
    }
}
