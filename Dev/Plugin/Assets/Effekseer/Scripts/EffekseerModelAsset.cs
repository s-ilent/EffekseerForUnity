using System;
using System.IO;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Effekseer.Internal
{
    [Serializable]
    public class EffekseerModelResource
    {
        [SerializeField]
        public string path;
        [SerializeField]
        public EffekseerModelAsset asset;

#if UNITY_EDITOR
		public static EffekseerModelResource LoadAsset(string dirPath, string resPath)
		{

			EffekseerModelAsset asset = AssetDatabase.LoadAssetAtPath<EffekseerModelAsset>(EffekseerEffectAsset.NormalizeAssetPath(dirPath + "/" + resPath));

			var res = new EffekseerModelResource();
			res.path = resPath;
			res.asset = asset;
			return res;
		}
		public static bool InspectorField(EffekseerModelResource res)
		{
			EditorGUILayout.LabelField(res.path);
			var result = EditorGUILayout.ObjectField(res.asset, typeof(EffekseerModelAsset), false) as EffekseerModelAsset;
			if (result != res.asset)
			{
				res.asset = result;
				return true;
			}
			return false;
		}
#endif
    };
}

namespace Effekseer
{
    public class EffekseerModelAsset : ScriptableObject
    {
        [SerializeField]
        public byte[] bytes;
    }
}
