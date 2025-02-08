#if UNITY_EDITOR
using UnityEditor;

public class EditorConfigManager : Singleton<EditorConfigManager>
{
    private bool _isUseAssetBundle = false;
    public bool IsUseAssetBundle
    {
        get
        {
            if (_isUseAssetBundle)
            {
                if (!string.IsNullOrEmpty(_assetBundlePath))
                {
                    return true;
                }
                else
                {
                    DebugManager.Instance.LogError($"Need to set Editor AssetBundlePath first before use assetbundle mode on editor");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
    private string _assetBundlePath = string.Empty;
    public string AssetBundlePath
    {
        get { return _assetBundlePath; }
    }
    
    private const string EditorConfigManager_IsUseAssetBundle = "EditorConfigManager_IsUseAssetBundle";
    private const string EditorConfigManager_AssetBundlePath = "EditorConfigManager_AssetBundlePath";

    public EditorConfigManager()
    {
        _isUseAssetBundle = EditorPrefs.GetBool(EditorConfigManager_IsUseAssetBundle, false);
        _assetBundlePath = EditorPrefs.GetString(EditorConfigManager_AssetBundlePath, null);
    }

    public void SetConfig(bool isUseAssetBundle, string assetBundlePath)
    {
        _isUseAssetBundle = isUseAssetBundle;
        _assetBundlePath = assetBundlePath;
        EditorPrefs.SetBool(EditorConfigManager_IsUseAssetBundle, _isUseAssetBundle);
        EditorPrefs.SetString(EditorConfigManager_AssetBundlePath, _assetBundlePath);
    }
}
#endif