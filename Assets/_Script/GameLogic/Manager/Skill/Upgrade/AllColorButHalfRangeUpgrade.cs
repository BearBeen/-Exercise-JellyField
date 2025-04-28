using System;
using UnityEngine;

[Serializable]
public struct AllColorButHalfRangeUpgrade : ITargetJellyIndexUpgrade, IRangeUpgrade
{
    private static readonly UpgradeType[] UPGRADE_TYPES = new UpgradeType[2]
    {
        UpgradeType.TargetJellyIndexUpgrade,
        UpgradeType.RangeUpgrade,
    };

    [SerializeField] private float _mul;
    
    public int priority => 11;

    public UpgradeType upgradeType => UpgradeType.ComposeUpgrade;

    public UpgradeType[] upgradeTypes => UPGRADE_TYPES;

    public int UpgradeRange(int range)
    {
        return Mathf.FloorToInt(range * _mul);
    }
}
