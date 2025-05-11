public interface ISkillUpgrade
{
    float possibility { get; }
    GameAttribute negAttr {get; }
    float priceMin { get; }
    float priceMax { get; }
    string upgradeDes { get; }
    SkillUpgradeType upgradeType { get; }
    SkillUpgradeType[] upgradeTypes { get; }
}

public interface IRangeUpgrade : ISkillUpgrade
{
    float rangePer { get; }
    float rangeAdd { get; }
    float rangeMax { get; }
}

public interface ITargetJellyIndexUpgrade : ISkillUpgrade
{
}

public interface IMissileCountUpgrade : ISkillUpgrade
{
    float missilePer { get; }
    float missileAdd { get; }
    float missileMax { get; }
}