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
		// NOTE: Specular minimum required to enable highlighting. Smaller highlights than Lit shader.
		private const float MinRoughness = 0.1f;
		
		private static readonly int Smoothness = Shader.PropertyToID("_Smoothness");
		private static readonly int SpecGlossMap = Shader.PropertyToID("_SpecGlossMap");
		private static readonly int SpecColor = Shader.PropertyToID("_SpecColor");

		public override cc.Material Convert(UnityEngine.Material material)
		{
			var ccMat = GetStandardMaterial(material);
			URPMaterialConverter.BuildLitParams(material, ref ccMat);
			
			var define = ccMat._defines[0];
			var prop = ccMat._props[0];

			// NOTE: Cocos Standard shader does not support specular color, so it grayscale.
			var specColor = material.GetColor(SpecColor);
			var specular = (specColor.r + specColor.g + specColor.b) / 3f;
			
			var isGlossinessFromBaseAlpha = material.IsKeywordEnabled("_GLOSSINESS_FROM_BASE_ALPHA");
			var smoothness = material.GetFloat(Smoothness);
			var roughness = 0f;

			void ExportAlbedoToPBRMap()
			{
				// NOTE: Source BaseMap Alpha is not supported by Cocos, Generate PBR map.
				var albedoMap = material.mainTexture as UnityEngine.Texture2D;
				if (albedoMap)
				{
					define.Add("USE_PBR_MAP", true);
					var pbrMapUuid = URPMaterialConverter.ExportPBRMap(
						albedoMap, smoothness, MinRoughness, URPMaterialConverter.PBRMapSourceType.Albedo);
					prop.Add("pbrMap", new AssetReference(pbrMapUuid));
					roughness = 1f;
				}
			}
			
			if (material.IsKeywordEnabled("_SPECULAR_COLOR"))
			{
				if (isGlossinessFromBaseAlpha)
				{
					ExportAlbedoToPBRMap();
				}
			}
			else if (material.IsKeywordEnabled("_SPECGLOSSMAP"))
			{
				var specMap = material.GetTexture(SpecGlossMap) as UnityEngine.Texture2D;
				if (specMap)
				{
					define.Add("USE_PBR_MAP", true);
					var pbrMapUuid = URPMaterialConverter.ExportPBRMap(
						specMap, smoothness, MinRoughness, URPMaterialConverter.PBRMapSourceType.SimpleLitSpecular);
					prop.Add("pbrMap", new AssetReference(pbrMapUuid));
					roughness = 1f;
				}
				else if (isGlossinessFromBaseAlpha)
				{
					// NOTE: No support for use with SpecGlossMap
					ExportAlbedoToPBRMap();
				}
			}
			else
			{
				specular = 0;
			}
			
			prop.Add("roughness", roughness);
			prop.Add("specularIntensity", specular);
			
			return ccMat;
		}
	}
}
