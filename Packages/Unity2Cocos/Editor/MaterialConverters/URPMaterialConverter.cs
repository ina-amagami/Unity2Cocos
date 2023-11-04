using System.Collections.Generic;
using cc;
using UnityEditor;
using UnityEngine;

namespace Unity2Cocos
{
	public static class URPMaterialConverter
	{
		private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
		private static readonly int MainTex = Shader.PropertyToID("_MainTex");
		private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
		private static readonly int Color = Shader.PropertyToID("_Color");
		private static readonly int Cutoff = Shader.PropertyToID("_Cutoff");
		private static readonly int Surface = Shader.PropertyToID("_Surface");
		private static readonly int Cull = Shader.PropertyToID("_Cull");
		private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
		private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
		private static readonly int SrcBlendAlpha = Shader.PropertyToID("_SrcBlendAlpha");
		private static readonly int DstBlendAlpha = Shader.PropertyToID("_DstBlendAlpha");
		private static readonly int QueueOffset = Shader.PropertyToID("_QueueOffset");
		
		public static void BuildCocosMaterial(
			UnityEngine.Material urpMat,
			ref cc.Material ccMat,
			string cocosTextureUseKeyword)
		{
			var define = ccMat._defines[0];
			var state = ccMat._states[0];
			var prop = ccMat._props[0];
			
			// Texture
			var hasMainTexture = urpMat.HasTexture(BaseMap) || urpMat.HasTexture(MainTex);
			if (hasMainTexture)
			{
				var mainTexture = urpMat.mainTexture;
				if (mainTexture)
				{
					define.Add(cocosTextureUseKeyword, true);
					prop.Add("mainTexture", new AssetReference<cc.Texture2D>(Exporter.GetUuidOrExportAsset(mainTexture)));
				}
				prop.Add("tilingOffset", new Vec4()
				{
					x = urpMat.mainTextureScale.x,
					y = urpMat.mainTextureScale.y,
					z = urpMat.mainTextureOffset.x,
					w = urpMat.mainTextureOffset.y
				});
			}
			
			// Color
			var hasColor = urpMat.HasColor(BaseColor) || urpMat.HasColor(Color);
			if (hasColor)
			{
				prop.Add("mainColor", Utils.Color32ToCocosColor(urpMat.color));
			}
			
			// Alpha Test
			if (urpMat.IsKeywordEnabled("_ALPHATEST_ON"))
			{
				define.Add("USE_ALPHA_TEST", true);
				prop.Add("alphaThreshold", urpMat.GetFloat(Cutoff));
			}
			
			// Cull Mode
			if (urpMat.HasFloat(Cull) && urpMat.GetInt(Cull) != 2)
			{
				state.rasterizerState.Add("cullMode", urpMat.GetInt(Cull));
			}
			
			// Blend Mode
			if (urpMat.HasFloat(Surface))
			{
				// 0: Opaque, 1: Transparent
				ccMat._techIdx = urpMat.GetInt(Surface);
				
				if (ccMat._techIdx > 0)
				{
					var target = state.blendState.targets[0];
					target.Add("blendSrc", Utils.BlendModeToCocos(urpMat.GetInt(SrcBlend)));
					target.Add("blendDst", Utils.BlendModeToCocos(urpMat.GetInt(DstBlend)));
					target.Add("blendSrcAlpha", Utils.BlendModeToCocos(urpMat.GetInt(SrcBlendAlpha)));
					target.Add("blendDstAlpha", Utils.BlendModeToCocos(urpMat.GetInt(DstBlendAlpha)));
				}
			}
			
			// Render Queue
			if (urpMat.HasInt(QueueOffset) && urpMat.GetInt(QueueOffset) != 0)
			{
				state.priority += urpMat.GetInt(QueueOffset);
			}
		}

		private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
		private static readonly int BumpScale = Shader.PropertyToID("_BumpScale");
		private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
		private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
		
		public static void BuildLitParams(
			UnityEngine.Material urpMat,
			ref cc.Material ccMat)
		{
			var define = ccMat._defines[0];
			var prop = ccMat._props[0];

			// Normal Map
			var hasNormalMap = urpMat.IsKeywordEnabled("_NORMALMAP") && urpMat.HasTexture(BumpMap);
			if (hasNormalMap)
			{
				var normalMap = urpMat.GetTexture(BumpMap);
				if (normalMap)
				{
					define.Add("USE_NORMAL_MAP", true);
					prop.Add("normalMap", new AssetReference<cc.Texture2D>(Exporter.GetUuidOrExportAsset(normalMap)));
					prop.Add("normalStrength", Mathf.Clamp(urpMat.GetFloat(BumpScale), 0, 5));
				}
			}
			
			// Emission
			var useEmission = urpMat.IsKeywordEnabled("_EMISSION");
			if (useEmission)
			{
				var hasEmissionMap = urpMat.HasTexture(EmissionMap);
				if (hasEmissionMap)
				{
					var emissionMap = urpMat.GetTexture(EmissionMap);
					if (emissionMap)
					{
						define.Add("USE_EMISSIVE_MAP", true);
						prop.Add("emissiveMap", new AssetReference<cc.Texture2D>(Exporter.GetUuidOrExportAsset(emissionMap)));
					}
				}

				var emission = urpMat.GetColor(EmissionColor);
				var intensity = Utils.GetHDRColorIntensity(emission);
				var factor = 1f / Mathf.Pow(2f, intensity);
				var ldrColor = new UnityEngine.Color(
					emission.r * factor, emission.g * factor, emission.b * factor, emission.a);
				prop.Add("emissive", Utils.Color32ToCocosColor(ldrColor));
				prop.Add("emissiveScale", new cc.Vec3() { x = intensity, y = intensity, z = intensity });
			}
		}

		private static readonly Dictionary<int, string> _pbrTextureMap = new();

		public static void Initialize()
		{
			_pbrTextureMap.Clear();
		}
		
		public static string ExportPBRMap(UnityEngine.Texture2D src, bool isMetallicMap)
		{
			var srcPath = AssetDatabase.GetAssetPath(src);
			var key = src.GetHashCode();
			if (_pbrTextureMap.TryGetValue(key, out var cocosUuid))
			{
				return cocosUuid;
			}
			var isReadable = src.isReadable;
			if (!isReadable)
			{
				src = Utils.CreateReadableTexture2D(src);
			}
			var colors = src.GetPixels(0, 0, src.width, src.height);
			for (var i = 0; i < colors.Length; ++i)
			{
				// Cocos PBR Map
				//
				// OCCLUSION_CHANNEL          r
				// ROUGHNESS_CHANNEL          g
				// METALLIC_CHANNEL           b
				// SPECULAR_INTENSITY_CHANNEL a
				var c = colors[i];
				colors[i].r = 1f;
				colors[i].g = 1f - c.a;
				colors[i].b = isMetallicMap ? c.r : 1f;
				colors[i].a = 1f;
			}

			var dst = new UnityEngine.Texture2D(src.width, src.height);
			dst.SetPixels(colors);
			dst.Apply();

			cocosUuid = Texture2DExporter.ExportPBRMap(dst, srcPath);
			_pbrTextureMap.Add(key, cocosUuid);
			
			if (!isReadable)
			{
				Object.DestroyImmediate(src);
			}
			Object.DestroyImmediate(dst);
			return cocosUuid;
		}
	}
}
