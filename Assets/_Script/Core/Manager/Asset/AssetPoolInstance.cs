#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public partial class AssetManager
{
    private class AssetPoolInstance
    {
        private Object _asset; //the loaded asset
        private AssetBundle _bundle; //the bundle that contained the asset
        private int _refCount; //number of instance that using this asset

        public bool isAssetBundle
        {
            get
            {
                return _bundle != null;
            }
        }

        public int refCount
        {
            get
            {
                return _refCount;
            }
        }

        public Object Get()
        {
            _refCount++;
            return _asset;
        }

        public void Release()
        {
            _refCount--;
        }

        public AssetPoolInstance(AssetBundle bundle, Object asset, int refCount)
        {
            if (bundle)
            {
                _bundle = bundle;
                _asset = asset;
            }
            else
            {
                _asset = asset;
            }
            _refCount = refCount;
        }

        public void UnLoad(bool unloadAllLoadedObject = false)
        {
            if (isAssetBundle)
            {
                _asset = null;
                _bundle.Unload(unloadAllLoadedObject);
                _bundle = null;
            }
            else
            {
#if UNITY_EDITOR
                //AssetDatabase dont have unload method. so just dont do anything in editor
#else
                Resources.UnloadAsset(_asset);
#endif
                }
        }
    }
}