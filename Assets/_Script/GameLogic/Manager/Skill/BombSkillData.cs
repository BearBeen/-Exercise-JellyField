using UnityEngine;

[CreateAssetMenu(fileName = "BombSkillData", menuName = "ScriptableObjects/Skill/BombSkillData")]
public class BombSkillData : AbsSkillData<BombSkillData, BombSkillInstance>
{
    [SerializeField, AssetPath(typeof(GameObject))] private string _bombPath;
    [SerializeField] private int _explosionRadius;

    private float _rangePer;
    private float _rangeAdd;
    private bool _isKillAllColor = false;

    public override int skillID => (int)SkillID.Bomb;
    public string bombPath => _bombPath;
    public int explosionRadius => (int)((_explosionRadius + _rangeAdd) * (_rangePer + 1));
    public bool isKillAllColor => _isKillAllColor;

    protected override bool InternalIsUpgradable(SkillUpgradeType skillUpgradeType, ISkillUpgrade skillUpgrade)
    {
        switch (skillUpgradeType)
        {
            case SkillUpgradeType.RangeUpgrade:
                return IsRangeUpgradable(explosionRadius, skillUpgrade);
            case SkillUpgradeType.TargetJellyIndexUpgrade:
                return IsJellyIndexUpgradable(isKillAllColor, skillUpgrade);
            default:
                return false;
        }
    }

    protected override void InternalUpgrade(SkillUpgradeType skillUpgradeType, ISkillUpgrade skillUpgrade)
    {
        switch (skillUpgradeType)
        {
            case SkillUpgradeType.RangeUpgrade:
                UpgradeRange(skillUpgrade as IRangeUpgrade, ref _rangePer, ref _rangeAdd);
                break;
            case SkillUpgradeType.TargetJellyIndexUpgrade:
                UpgradeTargetJellyIndex(skillUpgrade as ITargetJellyIndexUpgrade, ref _isKillAllColor);
                break;
        }
    }
}

public class BombSkillInstance : AbsSkillInstance<BombSkillData, BombSkillInstance>
{
    private PoolableGameObject _bomb;
    private int _targetJellyIndex;

    public override void Init(Jelly caster, int targetJellyIndex)
    {
        _targetJellyIndex = _skillData.isKillAllColor ? -1 : targetJellyIndex;
        _bomb = ResourceManager.Instance.GetGameObjectAsync<PoolableGameObject>(_skillData.bombPath);
        _bomb.AddLoadCallback((obj) =>
        {
            obj.gameObject.transform.SetParent(caster.dockerTrs, false);
            obj.gameObject.transform.SetLocalPositionAndRotation(
                0.5f * Vector3.up,
                Quaternion.LookRotation(Vector3.forward + 0.25f * Random.onUnitSphere, Vector3.up));
        });
    }

    protected override void ClearExcute()
    {
        _bomb.Recycle();
    }

    protected override void Excute()
    {
    }

    protected override void InitExcute(Jelly caster)
    {
        GameBoardManager.PositionToBoardIndex(caster.transform.position, out int x, out int y);
        GameBoardManager.Instance.KillJelly(_targetJellyIndex, x, y, _skillData.explosionRadius, _skillData.explosionRadius);
    }

    protected override bool IsExcuteCompleted()
    {
        //TODO: make it delay for the effect to complete
        return true;
    }
}