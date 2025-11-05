
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Effekseer;
using UnityEditor.AssetImporters;

namespace Effekseer.Editor
{
    [ScriptedImporter(1, new[] { "efkmat", "efkmatd" })]
    public class EffekseerMaterialImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var assetPath = ctx.assetPath;

            if (Path.GetExtension(assetPath) == ".efkmat")
            {
                var importingAsset = new EffekseerMaterialAsset.ImportingAsset();
                importingAsset.Data = System.IO.File.ReadAllBytes(assetPath);
                importingAsset.UserTextureSlotMax = EffekseerTool.Constant.UserTextureSlotCount;
                var info = new Effekseer.Editor.Utils.MaterialInformation();
                info.Load(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), assetPath));

                importingAsset.CustomData1Count = info.CustomData1Count;
                importingAsset.CustomData2Count = info.CustomData2Count;
                importingAsset.HasRefraction = info.HasRefraction;
                importingAsset.ShadingModel = info.ShadingModel;

                foreach (var u in info.Uniforms)
                {
                    var up = new EffekseerMaterialAsset.UniformProperty();
                    up.Name = u.Name;
                    up.UniformName = u.UniformName;
                    up.Count = u.Type;
                    importingAsset.Uniforms.Add(up);
                }

                foreach (var t in info.Textures)
                {
                    var tp = new EffekseerMaterialAsset.TextureProperty();
                    tp.Name = t.Name;
                    tp.UniformName = t.UniformName;
                    tp.Type = (EffekseerMaterialAsset.TextureType)EffekseerTool.Utl.TextureType.Color;
                    importingAsset.Textures.Add(tp);
                }

                // TODO : Refactor
                foreach (var g in info.FixedGradients)
                {
                    var gp = new EffekseerMaterialAsset.GradientProperty();
                    gp.Name = g.Name;
                    gp.UniformName = g.UniformName;

                    gp.ColorMarkers = new EffekseerMaterialAsset.GradientProperty.ColorMarker[g.Data.ColorMarkers.Length];
                    for (int i = 0; i < g.Data.ColorMarkers.Length; i++)
                    {
                        gp.ColorMarkers[i].ColorR = g.Data.ColorMarkers[i].ColorR;
                        gp.ColorMarkers[i].ColorG = g.Data.ColorMarkers[i].ColorG;
                        gp.ColorMarkers[i].ColorB = g.Data.ColorMarkers[i].ColorB;
                        gp.ColorMarkers[i].Intensity = g.Data.ColorMarkers[i].Intensity;
                        gp.ColorMarkers[i].Position = g.Data.ColorMarkers[i].Position;
                    }

                    gp.AlphaMarkers = new EffekseerMaterialAsset.GradientProperty.AlphaMarker[g.Data.AlphaMarkers.Length];
                    for (int i = 0; i < g.Data.AlphaMarkers.Length; i++)
                    {
                        gp.AlphaMarkers[i].Alpha = g.Data.AlphaMarkers[i].Alpha;
                        gp.AlphaMarkers[i].Position = g.Data.AlphaMarkers[i].Position;
                    }

                    importingAsset.FixedGradients.Add(gp);
                }

                foreach (var g in info.Gradients)
                {
                    var gp = new EffekseerMaterialAsset.GradientProperty();
                    gp.Name = g.Name;
                    gp.UniformName = g.UniformName;

                    gp.ColorMarkers = new EffekseerMaterialAsset.GradientProperty.ColorMarker[g.Data.ColorMarkers.Length];
                    for (int i = 0; i < g.Data.ColorMarkers.Length; i++)
                    {
                        gp.ColorMarkers[i].ColorR = g.Data.ColorMarkers[i].ColorR;
                        gp.ColorMarkers[i].ColorG = g.Data.ColorMarkers[i].ColorG;
                        gp.ColorMarkers[i].ColorB = g.Data.ColorMarkers[i].ColorB;
                        gp.ColorMarkers[i].Intensity = g.Data.ColorMarkers[i].Intensity;
                        gp.ColorMarkers[i].Position = g.Data.ColorMarkers[i].Position;
                    }

                    gp.AlphaMarkers = new EffekseerMaterialAsset.GradientProperty.AlphaMarker[g.Data.AlphaMarkers.Length];
                    for (int i = 0; i < g.Data.AlphaMarkers.Length; i++)
                    {
                        gp.AlphaMarkers[i].Alpha = g.Data.AlphaMarkers[i].Alpha;
                        gp.AlphaMarkers[i].Position = g.Data.AlphaMarkers[i].Position;
                    }

                    importingAsset.Gradients.Add(gp);
                }

                importingAsset.IsCacheFile = false;
                importingAsset.Code = info.Code;

                importingAsset.MaterialRequiredFunctionTypes = new EffekseerMaterialAsset.MaterialRequiredFunctionType[info.RequiredFunctionTypes.Length];
                for (int i = 0; i < importingAsset.MaterialRequiredFunctionTypes.Length; i++)
                {
                    importingAsset.MaterialRequiredFunctionTypes[i] = (EffekseerMaterialAsset.MaterialRequiredFunctionType)info.RequiredFunctionTypes[i];
                }

                CreateAsset(ctx, importingAsset);
            }
            else if (Path.GetExtension(assetPath) == ".efkmatd")
            {
                var importingAsset = new EffekseerMaterialAsset.ImportingAsset();
                importingAsset.Data = System.IO.File.ReadAllBytes(assetPath);
                importingAsset.IsCacheFile = true;

                CreateAsset(ctx, importingAsset);
            }
        }

        public static void CreateAsset(AssetImportContext ctx, EffekseerMaterialAsset.ImportingAsset importingAsset)
        {
            var path = ctx.assetPath;

            // modify
            if (importingAsset.CustomData1Count > 0)
                importingAsset.CustomData1Count = Math.Max(2, importingAsset.CustomData1Count);

            if (importingAsset.CustomData2Count > 0)
                importingAsset.CustomData2Count = Math.Max(2, importingAsset.CustomData2Count);

            // modifiy importing asset to avoid invalid name
            foreach (var texture in importingAsset.Textures)
            {
                if (texture.Name == string.Empty)
                {
                    texture.Name = texture.UniformName;
                }

                // Escape
                texture.Name = EscapePropertyName(texture.Name);
            }

            var asset = ScriptableObject.CreateInstance<EffekseerMaterialAsset>();

            if (importingAsset.IsCacheFile)
            {
                asset.cachedMaterialBuffers = importingAsset.Data;
            }
            else
            {
                asset.materialBuffers = importingAsset.Data;
                asset.uniforms = importingAsset.Uniforms.ToArray();
                asset.textures = importingAsset.Textures.ToArray();
                asset.gradients = importingAsset.Gradients.ToArray();
                asset.CustomData1Count = importingAsset.CustomData1Count;
                asset.CustomData2Count = importingAsset.CustomData2Count;
                asset.HasRefraction = importingAsset.HasRefraction;
                var shader = CreateShader(Path.ChangeExtension(path, ".shader"), importingAsset);

                // sometimes return null
                if (shader != null)
                {
                    asset.shader = shader;
                }
            }

            ctx.AddObjectToAsset("main", asset);
            if (asset.shader != null)
            {
                ctx.AddObjectToAsset("shader", asset.shader);
            }
            ctx.SetMainObject(asset);
        }

        static string EscapePropertyName(string name)
        {
            return "_" + name + "_Tex";
        }

        static Shader CreateShader(string path, EffekseerMaterialAsset.ImportingAsset importingAsset)
        {
            string shaderName = Path.GetFileNameWithoutExtension(path);
            string code = GenerateShaderCode(shaderName, importingAsset);

            AssetDatabase.StartAssetEditing();

            using (var writer = new StreamWriter(path))
            {
                writer.Write(code);
            }

            AssetDatabase.StopAssetEditing();

            AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ImportRecursive);

            var asset = AssetDatabase.LoadAssetAtPath<Shader>(path);
            return asset;
        }

        public static string GenerateShaderCode(string shaderName, EffekseerMaterialAsset.ImportingAsset importingAsset)
        {
            var nl = Environment.NewLine;
            var mainVSCode = CreateMainShaderCode(importingAsset, 0);
            var mainPSCode = CreateMainShaderCode(importingAsset, 1);

            var code = string.Empty;

            var functions = string.Empty;

            if (importingAsset.MaterialRequiredFunctionTypes.Contains(EffekseerMaterialAsset.MaterialRequiredFunctionType.Gradient))
            {
                functions += EffekseerShaderGenerator.gradientTemplate;
            }
            else if (importingAsset.MaterialRequiredFunctionTypes.Contains(EffekseerMaterialAsset.MaterialRequiredFunctionType.Noise))
            {
                functions += EffekseerShaderGenerator.noiseTemplate;
            }
            else if (importingAsset.MaterialRequiredFunctionTypes.Contains(EffekseerMaterialAsset.MaterialRequiredFunctionType.Light))
            {
                functions += lightTemplate;
            }

            foreach (var gradient in importingAsset.FixedGradients)
            {
                functions += EffekseerShaderGenerator.GetFixedGradient(gradient.Name, gradient);
            }

            code += shaderTemplate;
            code = code.Replace("@", "#");

            string codeProperty = string.Empty;
            string codeVariable = string.Empty;
            string codeUniforms = string.Empty;

            int actualTextureCount = Math.Min(importingAsset.UserTextureSlotMax, importingAsset.Textures.Count);

            for (int i = 0; i < actualTextureCount; i++)
            {
                codeProperty += importingAsset.Textures[i].Name + @"(""Color (RGBA)"", 2D) = ""white"" {}" + nl;
                codeVariable += "sampler2D " + importingAsset.Textures[i].Name + ";" + nl;
            }

            for (int i = 0; i < importingAsset.Uniforms.Count; i++)
            {
                codeUniforms += "float4 " + importingAsset.Uniforms[i].Name + ";" + nl;
            }

            for (int i = 0; i < importingAsset.Gradients.Count; i++)
            {
                var gradient = importingAsset.Gradients[i];

                for (int j = 0; j < 13; j++)
                {
                    codeUniforms += "float4 " + gradient.UniformName + "_" + j.ToString() + ";" + nl;
                }
            }

            // replace for usability
            // HACK for efk_xxx_1 and efk_xxx_12
            {
                var replacingUniforms = importingAsset.Uniforms.ToArray();

                replacingUniforms = replacingUniforms.OrderByDescending(_ => _.UniformName.Length).ToArray();

                foreach (var kv in replacingUniforms)
                {
                    if (kv.Name == string.Empty)
                    {
                        continue;
                    }

                    code = code.Replace(kv.UniformName, kv.Name);
                    mainVSCode = mainVSCode.Replace(kv.UniformName, kv.Name);
                    mainPSCode = mainPSCode.Replace(kv.UniformName, kv.Name);
                }
            }

            code = code.Replace("%FUNCTIONS%", functions);
            code = code.Replace("%TEX_PROPERTY%", codeProperty);
            code = code.Replace("%TEX_VARIABLE%", codeVariable);
            code = code.Replace("%UNIFORMS%", codeUniforms);
            code = code.Replace("%VSCODE%", mainVSCode);
            code = code.Replace("%PSCODE%", mainPSCode);
            code = code.Replace("%MATERIAL_NAME%", shaderName);

            if (importingAsset.HasRefraction)
            {
                code = code.Replace("//PRAGMA_REFRACTION_FLAG", "#pragma multi_compile _ _MATERIAL_REFRACTION_");
            }

            if (importingAsset.ShadingModel == 0)
            {
                code = code.Replace("//PRAGMA_LIT_FLAG", "#define _MATERIAL_LIT_ 1");
            }

            if (importingAsset.CustomData1Count > 0)
            {
                code = code.Replace("//%CUSTOM_BUF1%", string.Format("StructuredBuffer<float4> buf_customData1;"));
                code = code.Replace("//%CUSTOM_VS_INPUT1%", string.Format("float{0} CustomData1;", importingAsset.CustomData1Count));
                code = code.Replace("//%CUSTOM_VSPS_INOUT1%", string.Format("float{0} CustomData1 : TEXCOORD7;", importingAsset.CustomData1Count));
            }

            if (importingAsset.CustomData2Count > 0)
            {
                code = code.Replace("//%CUSTOM_BUF2%", string.Format("StructuredBuffer<float4> buf_customData2;"));
                code = code.Replace("//%CUSTOM_VS_INPUT2%", string.Format("float{0} CustomData2;", importingAsset.CustomData2Count));
                code = code.Replace("//%CUSTOM_VSPS_INOUT2%", string.Format("float{0} CustomData2 : TEXCOORD8;", importingAsset.CustomData2Count));
            }

            // change return codes
            return code.Replace("\r\n", "\n");
        }

        static string CreateMainShaderCode(EffekseerMaterialAsset.ImportingAsset importingAsset, int stage)
        {
            var baseCode = "";

            if (stage == 0)
            {
                if (importingAsset.CustomData1Count > 0)
                {
                    baseCode += "#if _MODEL_\n";
                    baseCode += string.Format("float4 customData1 = buf_customData1[inst];\n");
                    baseCode += "#else\n";
                    baseCode += string.Format("float{0} customData1 = Input.CustomData1;\n", importingAsset.CustomData1Count);
                    baseCode += "#endif\n";
                }

                if (importingAsset.CustomData2Count > 0)
                {
                    baseCode += "#if _MODEL_\n";
                    baseCode += string.Format("float4 customData2 = buf_customData2[inst];\n");
                    baseCode += "#else\n";
                    baseCode += string.Format("float{0} customData2 = Input.CustomData2;\n", importingAsset.CustomData2Count);
                    baseCode += "#endif\n";
                }
            }
            else if (stage == 1)
            {
                if (importingAsset.CustomData1Count > 0)
                {
                    baseCode += string.Format("float{0} customData1 = Input.CustomData1;", importingAsset.CustomData1Count);
                }

                if (importingAsset.CustomData2Count > 0)
                {
                    baseCode += string.Format("float{0} customData2 = Input.CustomData2;", importingAsset.CustomData2Count);
                }
            }

            baseCode += importingAsset.Code;

            baseCode = baseCode.Replace("$F1$", "float");
            baseCode = baseCode.Replace("$F2$", "float2");
            baseCode = baseCode.Replace("$F3$", "float3");
            baseCode = baseCode.Replace("$F4$", "float4");
            baseCode = baseCode.Replace("$TIME$", "_Time.y");
            baseCode = baseCode.Replace("$EFFECTSCALE$", "predefined_uniform.y");
            baseCode = baseCode.Replace("$LOCALTIME$", "predefined_uniform.w");
            baseCode = baseCode.Replace("$UV$", "uv");

            int actualTextureCount = Math.Min(importingAsset.UserTextureSlotMax, importingAsset.Textures.Count);

            for (int i = 0; i < actualTextureCount; i++)
            {
                var keyP = "$TEX_P" + i + "$";
                var keyS = "$TEX_S" + i + "$";

                var replacedP = string.Empty;
                var replacedS = string.Empty;

                if (stage == 0)
                {
                    replacedP = "tex2Dlod(" + importingAsset.Textures[i].Name + ",float4(GetUV(";
                    replacedS = "),0,0))";
                }
                else
                {
                    replacedP = "tex2D(" + importingAsset.Textures[i].Name + ",GetUV(";
                    replacedS = "))";
                }

                if (importingAsset.Textures[i].Type == EffekseerMaterialAsset.TextureType.Color)
                {
                    replacedP = "ConvertFromSRGBTexture(" + replacedP;
                    replacedS = replacedS + ")";
                }

                baseCode = baseCode.Replace(keyP, replacedP);
                baseCode = baseCode.Replace(keyS, replacedS);

            }

            // invalid texture
            for (int i = actualTextureCount; i < importingAsset.Textures.Count; i++)
            {
                var keyP = "$TEX_P" + i + "$";
                var keyS = "$TEX_S" + i + "$";
                baseCode = baseCode.Replace(keyP, "float4(");
                baseCode = baseCode.Replace(keyS, ",0.0,1.0)");
            }

            if (stage == 0)
            {
                if (importingAsset.CustomData1Count == 1)
                    baseCode += "Output.CustomData1 = customData1;";
                if (importingAsset.CustomData1Count == 2)
                    baseCode += "Output.CustomData1 = customData1.xy;";
                if (importingAsset.CustomData1Count == 3)
                    baseCode += "Output.CustomData1 = customData1.xyz;";
                if (importingAsset.CustomData1Count == 4)
                    baseCode += "Output.CustomData1 = customData1.xyzw;";

                if (importingAsset.CustomData2Count == 1)
                    baseCode += "Output.CustomData2 = customData2;";
                if (importingAsset.CustomData2Count == 2)
                    baseCode += "Output.CustomData2 = customData2.xy;";
                if (importingAsset.CustomData2Count == 3)
                    baseCode += "Output.CustomData2 = customData2.xyz;";
                if (importingAsset.CustomData2Count == 4)
                    baseCode += "Output.CustomData2 = customData2.xyzw;";
            }

            return baseCode;
        }

        const string lightTemplate = @"
float3 GetLightDirection() {
	return lightDirection.xyz;
}
float3 GetLightColor() {
	return lightColor.xyz;
}
float3 GetLightAmbientColor() {
	return lightAmbientColor.xyz;
}
";

        const string shaderTemplate = @"

Shader ""EffekseerMaterial/%MATERIAL_NAME%"" {

Properties{
	[Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc(""Blend Src"", Float) = 0
	[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst(""Blend Dst"", Float) = 0
	_BlendOp(""Blend Op"", Float) = 0
	_Cull(""Cull"", Float) = 0
	[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest(""ZTest Mode"", Float) = 0
	[Toggle] _ZWrite(""ZWrite"", Float) = 0

	%TEX_PROPERTY%
}

SubShader{

Blend[_BlendSrc][_BlendDst]
BlendOp[_BlendOp]
ZTest[_ZTest]
ZWrite[_ZWrite]
Cull[_Cull]

	Pass {

		CGPROGRAM

		@define FLT_EPSILON 1.192092896e-07f

		float convertColorSpace;

		float3 PositivePow(float3 base, float3 power)
		{
			return pow(max(abs(base), float3(FLT_EPSILON, FLT_EPSILON, FLT_EPSILON)), power);
		}

		// based on http://chilliant.blogspot.com/2012/08/srgb-approximations-for-hlsl.html?m=1
		float3 SRGBToLinear(float3 c)
		{
			return min(c, c * (c * (c * 0.305306011 + 0.682171111) + 0.012522878));
		}

		float4 SRGBToLinear(float4 c)
		{
			return float4(SRGBToLinear(c.rgb), c.a);
		}

		float3 LinearToSRGB(float3 c)
		{
			return max(1.055 * PositivePow(c, 0.416666667) - 0.055, 0.0);
		}

		float4 LinearToSRGB(float4 c)
		{
			return float4(LinearToSRGB(c.rgb), c.a);
		}

		float4 ConvertFromSRGBTexture(float4 c)
		{
		@if defined(UNITY_COLORSPACE_GAMMA)
			return c;
		@else
			if (convertColorSpace == 0.0f)
			{
				return c;
			}
			return LinearToSRGB(c);
		@endif
		}

		float4 ConvertToScreen(float4 c)
		{
		@if defined(UNITY_COLORSPACE_GAMMA)
			return c;
		@else
			if (convertColorSpace == 0.0f)
			{
				return c;
			}
			return SRGBToLinear(c);
		@endif
		}

		@define MOD fmod
		@define FRAC frac
		@define LERP lerp

		@pragma target 5.0
		@pragma vertex vert
		@pragma fragment frag
		@pragma multi_compile _ _MODEL_
		//PRAGMA_REFRACTION_FLAG
		//PRAGMA_LIT_FLAG

		@include ""UnityCG.cginc""

		@if _MATERIAL_REFRACTION_
		sampler2D _BackTex;
		@endif
		sampler2D _depthTex;

		%TEX_VARIABLE%

		%UNIFORMS%

		@if _MODEL_

		struct SimpleVertex
		{
			float3 Pos;
			float3 Normal;
			float3 Binormal;
			float3 Tangent;
			float2 UV;
			float4 Color;
		};

		struct ModelParameter
		{
			float4x4 Mat;
			float4 UV;
			float4 Color;
			int Time;
		};

		//%CUSTOM_BUF1%
		//%CUSTOM_BUF2%

		StructuredBuffer<SimpleVertex> buf_vertex;
		StructuredBuffer<int> buf_index;

		StructuredBuffer<ModelParameter> buf_model_parameter;
		StructuredBuffer<int> buf_vertex_offsets;
		StructuredBuffer<int> buf_index_offsets;

		@else

		struct Vertex
		{
			float3 Pos;
			float4 Color;
			float3 Normal;
			float3 Tangent;
			float2 UV1;
			float2 UV2;
			//%CUSTOM_VS_INPUT1%
			//%CUSTOM_VS_INPUT2%
		};

		StructuredBuffer<Vertex> buf_vertex;
		float buf_offset;

		@endif

		struct ps_input
		{
			float4 Position		: SV_POSITION;
			float4 VColor		: COLOR;
			float2 UV1		: TEXCOORD0;
			float2 UV2		: TEXCOORD1;
			float3 WorldP	: TEXCOORD2;
			float3 WorldN : TEXCOORD3;
			float3 WorldT : TEXCOORD4;
			float3 WorldB : TEXCOORD5;
			float4 PosP : TEXCOORD6;
			//float2 ScreenUV : TEXCOORD6;
			//%CUSTOM_VSPS_INOUT1%
			//%CUSTOM_VSPS_INOUT2%
		};

		float4 lightDirection;
		float4 lightColor;
		float4 lightAmbientColor;
		float4 predefined_uniform;
		float4 reconstructionParam1;
		float4 reconstructionParam2;

		float2 GetUV(float2 uv)
		{
			uv.y = 1.0 - uv.y;
			return uv;
		}

		float2 GetUVBack(float2 uv)
		{
			uv.y = uv.y;
			return uv;
		}

		float CalcDepthFade(float2 screenUV, float meshZ, float softParticleParam)
		{
			float backgroundZ = tex2D(_depthTex, GetUVBack(screenUV)).x;
			float distance = softParticleParam * predefined_uniform.y;
			float2 rescale = reconstructionParam1.xy;
			float4 params = reconstructionParam2;

			float2 zs = float2(backgroundZ * rescale.x + rescale.y, meshZ);

			float2 depth = (zs * params.w - params.y) / (params.x - zs* params.z);
			float dir = sign(depth.x);
			depth *= dir;
			return min(max((depth.x - depth.y) / distance, 0.0), 1.0);
		}

		%FUNCTIONS%

		ps_input vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
		{
			// Unity
			float4 cameraPosition = float4(UNITY_MATRIX_V[3].xyzw);

			@if _MODEL_

			uint v_id = id;

			float4x4 buf_matrix = buf_model_parameter[inst].Mat;
			float4 buf_uv = buf_model_parameter[inst].UV;
			float4 buf_color = buf_model_parameter[inst].Color;
			float buf_vertex_offset = buf_vertex_offsets[buf_model_parameter[inst].Time];
			float buf_index_offset = buf_index_offsets[buf_model_parameter[inst].Time];

			SimpleVertex Input = buf_vertex[buf_index[v_id + buf_index_offset] + buf_vertex_offset];


			float3 localPos = Input.Pos;


			@else

			int qind = (id) / 6;
			int vind = (id) % 6;

			int v_offset[6];
			v_offset[0] = 2;
			v_offset[1] = 1;
			v_offset[2] = 0;
			v_offset[3] = 1;
			v_offset[4] = 2;
			v_offset[5] = 3;

			Vertex Input = buf_vertex[buf_offset + qind * 4 + v_offset[vind]];

			@endif

			ps_input Output;

			@if _MODEL_
			float3x3 matRotModel = (float3x3)buf_matrix;
			float3 worldPos = mul(buf_matrix, float4(localPos, 1.0f)).xyz;
			float3 worldNormal = normalize(mul(matRotModel, Input.Normal));
			float3 worldTangent = normalize(mul(matRotModel, Input.Tangent));
			float3 worldBinormal = cross(worldNormal, worldTangent);
			@else
			float3 worldPos = Input.Pos;
			float3 worldNormal = Input.Normal;
			float3 worldTangent = Input.Tangent;
			float3 worldBinormal = cross(worldNormal, worldTangent);
			@endif

			@if _MODEL_
			float3 objectScale = float3(1.0, 1.0, 1.0);
			objectScale.x = length(mul(matRotModel, float3(1.0, 0.0, 0.0)));
			objectScale.y = length(mul(matRotModel, float3(0.0, 1.0, 0.0)));
			objectScale.z = length(mul(matRotModel, float3(0.0, 0.0, 1.0)));
			@else
			float3 objectScale = float3(1.0, 1.0, 1.0);
			@endif

			// UV
			@if _MODEL_
			float2 uv1 = Input.UV.xy * buf_uv.zw + buf_uv.xy;
			float2 uv2 = Input.UV.xy * buf_uv.zw + buf_uv.xy;
			@else
			float2 uv1 = Input.UV1;
			float2 uv2 = Input.UV2;
			@endif

			// NBT
			Output.WorldN = worldNormal;
			Output.WorldB = worldBinormal;
			Output.WorldT = worldTangent;

			float3 pixelNormalDir = worldNormal;

			@if _MODEL_
			float4 vcolor = Input.Color * buf_color;
			@else
			float4 vcolor = Input.Color;
			@endif

			// Dummy
			float2 screenUV = float2(0.0, 0.0);
			float meshZ =  0.0f;

			%VSCODE%

			worldPos = worldPos + worldPositionOffset;

			// Unity Ext
			float4 cameraPos = mul(UNITY_MATRIX_V, float4(worldPos, 1.0f));
			cameraPos = cameraPos / cameraPos.w;
			Output.Position = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0f));

			Output.WorldP = worldPos;
			Output.VColor = vcolor;
			Output.UV1 = uv1;
			Output.UV2 = uv2;
			Output.PosP = Output.Position;
			// Output.ScreenUV = Output.Position.xy / Output.Position.w;
			// Output.ScreenUV.xy = float2(Output.ScreenUV.x + 1.0, 1.0 - Output.ScreenUV.y) * 0.5;

			return Output;
		}

		@ifdef _MATERIAL_LIT_

		@define lightScale 3.14

		float calcD_GGX(float roughness, float dotNH)
		{
			float alpha = roughness*roughness;
			float alphaSqr = alpha*alpha;
			float pi = 3.14159;
			float denom = dotNH * dotNH *(alphaSqr-1.0) + 1.0;
			return (alpha / denom) * (alpha / denom) / pi;
		}

		float calcF(float F0, float dotLH)
		{
			float dotLH5 = pow(1.0-dotLH,5.0);
			return F0 + (1.0-F0)*(dotLH5);
		}

		float calcG_Schlick(float roughness, float dotNV, float dotNL)
		{
			// UE4
			float k = (roughness + 1.0) * (roughness + 1.0) / 8.0;
			// float k = roughness * roughness / 2.0;

			float gV = dotNV*(1.0 - k) + k;
			float gL = dotNL*(1.0 - k) + k;

			return 1.0 / (gV * gL);
		}

		float calcLightingGGX(float3 N, float3 V, float3 L, float roughness, float F0)
		{
			float3 H = normalize(V+L);

			float dotNL = saturate( dot(N,L) );
			float dotLH = saturate( dot(L,H) );
			float dotNH = saturate( dot(N,H) ) - 0.001;
			float dotNV = saturate( dot(N,V) ) + 0.001;

			float D = calcD_GGX(roughness, dotNH);
			float F = calcF(F0, dotLH);
			float G = calcG_Schlick(roughness, dotNV, dotNL);

			return dotNL * D * F * G / 4.0;
		}

		float3 calcDirectionalLightDiffuseColor(float3 diffuseColor, float3 normal, float3 lightDir, float ao)
		{
			float3 color = float3(0.0,0.0,0.0);

			float NoL = dot(normal,lightDir);
			color.xyz = lightColor.xyz * lightScale * max(NoL,0.0) * ao / 3.14;
			color.xyz = color.xyz * diffuseColor.xyz;
			return color;
		}

		@endif

		float4 frag(ps_input Input) : COLOR
		{
			// Unity
			float4 cameraPosition = float4(_WorldSpaceCameraPos, 1.0);
			float4x4 cameraMat = UNITY_MATRIX_V;

			float2 uv1 = Input.UV1;
			float2 uv2 = Input.UV2;
			float3 worldPos = Input.WorldP;
			float3 worldNormal = Input.WorldN;
			float3 worldBinormal = Input.WorldB;
			float3 worldTangent = Input.WorldT;

			float3 pixelNormalDir = worldNormal;
			float4 vcolor = Input.VColor;
			float2 screenUV = Input.PosP.xy / Input.PosP.w;
			float meshZ =  Input.PosP.z / Input.PosP.w;
			screenUV.xy = float2(screenUV.x + 1.0, 1.0 - screenUV.y) * 0.5;

			float3 objectScale = float3(1.0, 1.0, 1.0);

			%PSCODE%

			@if _MATERIAL_REFRACTION_
			float airRefraction = 1.0;
			float3 dir = mul((float3x3)cameraMat, pixelNormalDir);
			dir.y = -dir.y;

			float2 distortUV = 	dir.xy * (refraction - airRefraction);

			distortUV += screenUV;
			distortUV = GetUVBack(distortUV);

			float4 bg = tex2D(_BackTex, distortUV);
			float4 Output = bg;

			if(opacityMask <= 0.0) discard;
			if(opacity <= 0.0) discard;

			return Output;

			@elif defined(_MATERIAL_LIT_)
			float3 viewDir = normalize(cameraPosition.xyz - worldPos);
			float3 diffuse = calcDirectionalLightDiffuseColor(baseColor, pixelNormalDir, lightDirection.xyz, ambientOcclusion);
			float3 specular = lightColor.xyz * lightScale * calcLightingGGX(pixelNormalDir, viewDir, lightDirection.xyz, roughness, 0.9);

			float4 Output =  float4(metallic * specular + (1.0 - metallic) * diffuse + baseColor * lightAmbientColor.xyz * ambientOcclusion, opacity);
			Output.xyz = Output.xyz + emissive.xyz;

			if(opacityMask <= 0.0) discard;
			if(opacity <= 0.0) discard;

			return Output;

			@else

			float4 Output = float4(emissive, opacity);

			if(opacityMask <= 0.0f) discard;
			if(opacity <= 0.0) discard;

			return ConvertToScreen(Output);
			@endif
		}

		ENDCG
	}

}

Fallback Off

}

";
    }
}
#endif
