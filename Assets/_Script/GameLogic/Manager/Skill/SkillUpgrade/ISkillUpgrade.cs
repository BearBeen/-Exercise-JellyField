public interface ISkillUpgrade
{
    int priority { get; }//smaller priority get it value apply first.
    SkillUpgradeType upgradeType { get; }
    SkillUpgradeType[] upgradeTypes { get; }
}

public interface IRangeUpgrade : ISkillUpgrade
{
    int UpgradeRange(int range);
}

public interface ITargetJellyIndexUpgrade : ISkillUpgrade
{
}