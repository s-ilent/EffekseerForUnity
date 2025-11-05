
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.AssetImporters;

namespace Effekseer.Editor
{
    [ScriptedImporter(1, "efkcurve")]
    public class EffekseerCurveImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var asset = ScriptableObject.CreateInstance<EffekseerCurveAsset>();
            asset.bytes = File.ReadAllBytes(ctx.assetPath);

            ctx.AddObjectToAsset("main", asset);
            ctx.SetMainObject(asset);
        }
    }
}
#endif
