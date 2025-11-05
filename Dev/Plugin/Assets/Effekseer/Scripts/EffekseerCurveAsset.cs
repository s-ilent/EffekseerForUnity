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
    public class EffekseerCurveResource
    {
        [SerializeField]
        public string path;
        [SerializeField]
        public EffekseerCurveAsset asset;

#if UNITY_EDITOR
		public static EffekseerCurveResource LoadAsset(string dirPath, string resPath)
		{

			EffekseerCurveAsset asset = AssetDatabase.LoadAssetAtPath<EffekseerCurveAsset>(EffekseerEffectAsset.NormalizeAssetPath(dirPath + "/" + resPath));

			var res = new EffekseerCurveResource();
			res.path = resPath;
			res.asset = asset;
			return res;
		}
		public static bool InspectorField(EffekseerCurveResource res)
		{
			EditorGUILayout.LabelField(res.path);
			var result = EditorGUILayout.ObjectField(res.asset, typeof(EffekseerCurveAsset), false) as EffekseerCurveAsset;
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
    public class EffekseerCurveAsset : ScriptableObject
    {
        [SerializeField]
        public byte[] bytes;
    }
}
