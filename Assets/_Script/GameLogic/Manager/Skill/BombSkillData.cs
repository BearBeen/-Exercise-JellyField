using UnityEngine;

[CreateAssetMenu(fileName = "BombSkillData", menuName = "ScriptableObjects/Skill/BombSkillData")]
public class BombSkillData : AbsSkillData<BombSkillData, BombSkillInstance>
{
    [SerializeField, AssetPath(typeof(GameObject))] private string _bombPath;
    [SerializeField] private int _explosionRadius;
    [SerializeField] private bool _isKillAllColor = false;

    public string bombPath => _bombPath;
    public int explosionRadius => _explosionRadius;
    public bool isKillAllColor => _isKillAllColor;

    protected override void UpgradeRange(IRangeUpgrade rangeUpgrade)
    {
        _explosionRadius = rangeUpgrade.UpgradeRange(_explosionRadius);
    }

    protected override void UpgradeTargetJellyIndex(ITargetJellyIndexUpgrade targetJellyIndexUpgrade)
    {
        _isKillAllColor = true;
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