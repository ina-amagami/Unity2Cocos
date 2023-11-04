using cc;
using UnityEngine;

namespace Unity2Cocos
{
	/// <summary>
	/// URP Lit to Cocos Standard Material.
	/// </summary>
	/// <remarks>
	/// Keep the appearance as close as possible. Not a perfect.
	/// </remarks>
	[MaterialConverter("Universal Render Pipeline/Simple Lit")]
	public class SimpleLit2StandardMaterialConverter : StandardMaterialConverter
	{
		private static readonly int Smoothness = Shader.PropertyToID("_Smoothness");
		private static readonly int SpecGlossMap = Shader.PropertyToID("_SpecGlossMap");
		private static readonly int SpecColor = Shader.PropertyToID("_SpecColor");

		public override cc.Material Convert(UnityEngine.Material material)
		{
			var ccMat = GetStandardMaterial(material);
			URPMaterialConverter.BuildLitParams(material, ref ccMat);
			
			var define = ccMat._defines[0];
			var prop = ccMat._props[0];
			
			var specColor = material.GetColor(SpecColor);
			var specular = (specColor.r + specColor.g + specColor.b) / 3f;
			var roughness = Mathf.Max(1f - material.GetFloat(Smoothness), specular);
			var isGlossinessFromBaseAlpha = material.IsKeywordEnabled("_GLOSSINESS_FROM_BASE_ALPHA");
			
			if (material.IsKeywordEnabled("_SPECULAR_COLOR"))
			{
				if (isGlossinessFromBaseAlpha)
				{
					var albedoMap = material.mainTexture;
					if (albedoMap)
					{
						define.Add("USE_PBR_MAP", true);
						prop.Add("pbrMap", new AssetReference<cc.Texture2D>(Exporter.GetUuidOrExportAsset(albedoMap)));
					}
					// NOTE: Source BaseMap Alpha is not supported by Cocos, so use default values.
					roughness = 0.5f;
					specular = 0.5f;
				}
			}
			else if (material.IsKeywordEnabled("_SPECGLOSSMAP"))
			{
				var specMap = material.GetTexture(SpecGlossMap);
				if (specMap)
				{
					define.Add("USE_PBR_MAP", true);
					prop.Add("pbrMap", new AssetReference<cc.Texture2D>(Exporter.GetUuidOrExportAsset(specMap)));
				}
				else if (isGlossinessFromBaseAlpha)
				{
					var albedoMap = material.mainTexture;
					if (albedoMap)
					{
						define.Add("USE_PBR_MAP", true);
						prop.Add("pbrMap", new AssetReference<cc.Texture2D>(Exporter.GetUuidOrExportAsset(albedoMap)));
					}
					// NOTE: Source BaseMap Alpha is not supported by Cocos, so use default values.
					roughness = 0.5f;
					specular = 0.5f;
				}
			}
			else
			{
				roughness = 0;
				specular = 0;
			}
			
			prop.Add("roughness", roughness);
			prop.Add("specularIntensity", specular);
			
			return ccMat;
		}
	}
}
