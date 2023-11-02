using cc;
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
			var hasCull = urpMat.HasFloat(Cull);
			if (hasCull && urpMat.GetInt(Cull) != 2)
			{
				state.rasterizerState.Add("cullMode", urpMat.GetInt(Cull));
			}
			
			// Blend Mode
			var hasSurface = urpMat.HasFloat(Surface);
			if (hasSurface)
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
	}
}
