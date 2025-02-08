using System.Collections.Generic;
using System.IO;
using System.Text;

public static partial class AssetConst
{
    private static readonly string _normalizedRsFolder = NormalizeToUnityPath(AssetConst.ForwardSlash + AssetConst.BuiltInAssetRoot + AssetConst.ForwardSlash);

    public static string CreatPath(params string[] subpaths)
    {
        //if (subpaths.Length == 0)
        //{
        //    return string.Empty;
        //}
        return string.Join(ForwardSlash, subpaths);
    }

    private class PathCache
    {
        private string _assetBundlePath = null;
        private string _builtInAssetPath = null;
        public PathCache(string resPath, string assetBundlePath, string builtInAssetPath)
        {
            _assetBundlePath = assetBundlePath;
            _builtInAssetPath = builtInAssetPath;
        }
        public string AssetBundlePath { get { return _assetBundlePath; } }
        public string BuiltInAssetPath { get { return _builtInAssetPath; } }
    }

#if UNITY_EDITOR
    [UnityEditor.Callbacks.DidReloadScripts(0)]
    private static void OnScriptsReloaded()
    {
        ClearPathCache();
    }
#endif

    private static Dictionary<string, PathCache> _pathCache = new Dictionary<string, PathCache>();

    public static void ClearPathCache()
    {
        _pathCache.Clear();
    }

    public static void ResolveResPath(ref string resPath, out string assetBundlePath, out string builtInAssetPath)
    {
        resPath = NormalizeToUnityPath(resPath);

        if (_pathCache.TryGetValue(resPath, out PathCache pathCache))
        {
            assetBundlePath = pathCache.AssetBundlePath;
            builtInAssetPath = pathCache.BuiltInAssetPath;
        }
        else
        {
            //string extension = "";
            int indexOfRsFolder = resPath.LastIndexOf(_normalizedRsFolder);
            if (indexOfRsFolder >= 0)
            {
                builtInAssetPath = resPath.Substring(indexOfRsFolder + _normalizedRsFolder.Length);
                int extensionSeparatorIndex = builtInAssetPath.LastIndexOf(AssetConst.ExtensionSeparator);
                if (extensionSeparatorIndex > 0)
                {
                    //extension = builtInAssetPath.Substring(extensionSeparatorIndex + 1, builtInAssetPath.Length - 1 - extensionSeparatorIndex);
                    builtInAssetPath = builtInAssetPath.Substring(0, extensionSeparatorIndex);
                }
                else
                {
                    builtInAssetPath = null;
                }
            }
            else
            {
                builtInAssetPath = null;
            }
#if UNITY_EDITOR
            if (EditorConfigManager.Instance.IsUseAssetBundle)
            {
                assetBundlePath = CreatPath(new string[] { AssetBundleRootPath, resPath });
                if (!File.Exists(assetBundlePath))
                {
                    assetBundlePath = null;
                }
            }
            else
            {
                assetBundlePath = null;
            }
#else
            assetBundlePath = assetBundlePath = CreatPath(new string[] { AssetBundleRootPath, resPath });
            if (!File.Exists(assetBundlePath))
            {
                assetBundlePath = null;
            }
#endif
            _pathCache.Add(resPath, new PathCache(resPath, assetBundlePath, builtInAssetPath));
        }
    }

    public static string NormalizeToUnityPath(string resPath)
    {
        return resPath.Replace(BackSlash, ForwardSlash).ToLower();
    }
}
