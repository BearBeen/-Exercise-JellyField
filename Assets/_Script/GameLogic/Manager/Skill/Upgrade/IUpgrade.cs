public interface IUpgrade
{
    public int priority { get; }//smaller priority get it value apply first.
    public UpgradeType upgradeType { get; }
    public UpgradeType[] upgradeTypes { get; }
}

public interface IRangeUpgrade : IUpgrade
{
    public int UpgradeRange(int range);
}

public interface ITargetJellyIndexUpgrade : IUpgrade
{
}