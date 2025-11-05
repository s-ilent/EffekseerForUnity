using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

namespace Effekseer.Internal
{
    [Serializable]
    public class EffekseerMaterialResource
    {
        [SerializeField]
        public string path;
        [SerializeField]
        public EffekseerMaterialAsset asset;

#if UNITY_EDITOR
		public static EffekseerMaterialResource LoadAsset(string dirPath, string resPath)
		{

			EffekseerMaterialAsset asset = AssetDatabase.LoadAssetAtPath<EffekseerMaterialAsset>(EffekseerEffectAsset.NormalizeAssetPath(dirPath + "/" + resPath));

			var res = new EffekseerMaterialResource();
			res.path = resPath;
			res.asset = asset;
			return res;
		}
		public static bool InspectorField(EffekseerMaterialResource res)
		{
			EditorGUILayout.LabelField(res.path);
			var result = EditorGUILayout.ObjectField(res.asset, typeof(EffekseerMaterialAsset), false) as EffekseerMaterialAsset;
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
    public partial class EffekseerMaterialAsset : ScriptableObject
    {
        [System.Serializable]
        public enum TextureType
        {
            Color,
            Value,
        }

        [System.Serializable]
        public class TextureProperty
        {
            [SerializeField]
            public TextureType Type = TextureType.Color;

            [SerializeField]
            public string Name;

            [SerializeField]
            public string UniformName;
        }

        [System.Serializable]
        public class UniformProperty
        {
            [SerializeField]
            public string Name;

            [SerializeField]
            public string UniformName;

            [SerializeField]
            public int Count;
        }

        [System.Serializable]
        public class GradientProperty
        {
            public string Name;

            public string UniformName;

            public ColorMarker[] ColorMarkers;
            public AlphaMarker[] AlphaMarkers;

            public struct ColorMarker
            {
                public float Position;
                public float ColorR;
                public float ColorG;
                public float ColorB;
                public float Intensity;
            }

            public struct AlphaMarker
            {
                public float Position;
                public float Alpha;
            }
        }

        public enum MaterialRequiredFunctionType : int
        {
            Gradient = 0,
            Noise = 1,
            Light = 2,
        }

        public class ImportingAsset
        {
            public byte[] Data = new byte[0];
            public string Code = string.Empty;
            public bool IsCacheFile = false;
            public int CustomData1Count = 0;
            public int CustomData2Count = 0;
            public int UserTextureSlotMax = 6;
            public bool HasRefraction = false;
            public List<TextureProperty> Textures = new List<TextureProperty>();
            public List<UniformProperty> Uniforms = new List<UniformProperty>();
            public List<GradientProperty> FixedGradients = new List<GradientProperty>();
            public List<GradientProperty> Gradients = new List<GradientProperty>();
            public int ShadingModel = 0;
            public MaterialRequiredFunctionType[] MaterialRequiredFunctionTypes = new MaterialRequiredFunctionType[0];
        }

        [SerializeField]
        public byte[] materialBuffers;

        [SerializeField]
        public byte[] cachedMaterialBuffers;

        [SerializeField]
        public Shader shader = null;

        [SerializeField]
        public TextureProperty[] textures = new TextureProperty[0];

        [SerializeField]
        public UniformProperty[] uniforms = new UniformProperty[0];

        [SerializeField]
        public GradientProperty[] gradients = new GradientProperty[0];

        [SerializeField]
        public int CustomData1Count = 0;

        [SerializeField]
        public int CustomData2Count = 0;

        [SerializeField]
        public bool HasRefraction = false;

#if UNITY_EDITOR
		/// <summary>
		/// to avoid unity bug
		/// </summary>
		/// <param name="path"></param>
		public void AttachShader(string path)
		{
			if (this.shader != null) return;

			var resource = AssetDatabase.LoadAssetAtPath<Shader>(Path.ChangeExtension(path, ".shader"));
			this.shader = resource;
			EditorUtility.SetDirty(this);
			AssetDatabase.Refresh();
		}
#endif
    }
}
