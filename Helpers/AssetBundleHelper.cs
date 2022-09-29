using UnityEngine;

namespace TrombLoader.Helpers
{
    public static class AssetBundleHelper
    {
        public static T LoadObjectFromAssetBundlePath<T>(string path, string assetPath = "assets/_background.prefab") where T : UnityEngine.Object
        {
            try
            {
                var bundle = AssetBundle.LoadFromFile(path);
                if (bundle == null) return null;

                T asset = bundle.LoadAsset<T>(assetPath);

                bundle.Unload(false);
                return asset;
            }
            catch
            {
                return null;
            }
        }
    }
}
