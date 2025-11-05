
using System.IO;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AssetImporters;

namespace Effekseer.Editor
{
    [ScriptedImporter(1, new[] { "efk", "efkefc", "efkproj" })]
    public class EffekseerEffectImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var assetPath = ctx.assetPath;

            var extension = Path.GetExtension(assetPath);

            // efk, efkproj, efkefc
            if (extension == ".efk" || extension == ".efkproj" || extension == ".efkefc")
            {
                var fullpath = System.IO.Path.GetFullPath(assetPath);
                if (!System.IO.File.Exists(fullpath)) return;
                var allData = System.IO.File.ReadAllBytes(fullpath);

                if (allData.Length < 24) return;

                byte[] bin = null;

                if (allData[0] != 'E' ||
                    allData[1] != 'F' ||
                    allData[2] != 'K' ||
                    allData[3] != 'E')
                {
                    // Before 1.5
                    if (EffekseerTool.Core.LoadFrom(assetPath))
                    {
                        var exporter = new EffekseerTool.Binary.Exporter();
                        bin = exporter.Export(1);
                    }
                }
                else
                {
                    // After 1.5
                    var version = System.BitConverter.ToInt32(allData, 4);

                    var chunkData = allData.Skip(8).ToArray();

                    var chunk = new EffekseerTool.IO.Chunk();
                    chunk.Load(chunkData);

                    var binBlock = chunk.Blocks.FirstOrDefault(_ => _.Chunk == "BIN_");
                    if (binBlock == null)
                    {
                        return;
                    }
                    bin = binBlock.Buffer;
                }

                if(bin == null)
                {
                    return;
                }

                var resourcePath = new EffekseerResourcePath();
                if (!EffekseerEffectAsset.ReadResourcePath(bin, ref resourcePath))
                {
                    return;
                }

                float defaultScale = 1.0f;

                var asset = ScriptableObject.CreateInstance<EffekseerEffectAsset>();
                asset.efkBytes = bin;
                asset.Scale = defaultScale;

                var assetDir = Path.GetDirectoryName(assetPath);

                asset.textureResources = new Internal.EffekseerTextureResource[resourcePath.TexturePathList.Count];
                for (int i = 0; i < resourcePath.TexturePathList.Count; i++)
                {
                    asset.textureResources[i] = Internal.EffekseerTextureResource.LoadAsset(assetDir, resourcePath.TexturePathList[i]);

                    if (asset.textureResources[i].texture == null)
                    {
                        Debug.LogWarning(string.Format("Failed to load {0}", resourcePath.TexturePathList[i]));
                    }
                }

                asset.soundResources = new Internal.EffekseerSoundResource[resourcePath.SoundPathList.Count];
                for (int i = 0; i < resourcePath.SoundPathList.Count; i++)
                {
                    asset.soundResources[i] = Internal.EffekseerSoundResource.LoadAsset(assetDir, resourcePath.SoundPathList[i]);

                    if (asset.soundResources[i].clip == null)
                    {
                        Debug.LogWarning(string.Format("Failed to load {0}", resourcePath.SoundPathList[i]));
                    }
                }

                asset.modelResources = new Internal.EffekseerModelResource[resourcePath.ModelPathList.Count];
                for (int i = 0; i < resourcePath.ModelPathList.Count; i++)
                {
                    asset.modelResources[i] = Internal.EffekseerModelResource.LoadAsset(assetDir, resourcePath.ModelPathList[i]);

                    if (asset.modelResources[i].asset == null)
                    {
                        Debug.LogWarning(string.Format("Failed to load {0}", resourcePath.ModelPathList[i]));
                    }
                }

                asset.materialResources = new Internal.EffekseerMaterialResource[resourcePath.MaterialPathList.Count];
                for (int i = 0; i < resourcePath.MaterialPathList.Count; i++)
                {
                    asset.materialResources[i] = Internal.EffekseerMaterialResource.LoadAsset(assetDir, resourcePath.MaterialPathList[i]);

                    if (asset.materialResources[i].asset == null)
                    {
                        Debug.LogWarning(string.Format("Failed to load {0}", resourcePath.MaterialPathList[i]));
                    }
                }

                asset.curveResources = new Internal.EffekseerCurveResource[resourcePath.CurvePathList.Count];
                for (int i = 0; i < resourcePath.CurvePathList.Count; i++)
                {
                    asset.curveResources[i] = Internal.EffekseerCurveResource.LoadAsset(assetDir, resourcePath.CurvePathList[i]);

                    if (asset.curveResources[i].asset == null)
                    {
                        Debug.LogWarning(string.Format("Failed to load {0}", resourcePath.CurvePathList[i]));
                    }
                }

                ctx.AddObjectToAsset("main", asset);
                ctx.SetMainObject(asset);
            }
        }
    }
}
#endif
