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
	[MaterialConverter("Universal Render Pipeline/Lit")]
	public class Lit2StandardMaterialConverter : StandardMaterialConverter
	{
		private static readonly int Smoothness = Shader.PropertyToID("_Smoothness");
		private static readonly int MetallicGlossMap = Shader.PropertyToID("_MetallicGlossMap");
		private static readonly int Metallic = Shader.PropertyToID("_Metallic");
		private static readonly int OcclusionMap = Shader.PropertyToID("_OcclusionMap");
		private static readonly int SpecColor = Shader.PropertyToID("_SpecColor");
		private static readonly int SpecularHighlights = Shader.PropertyToID("_SpecularHighlights");

		public override cc.Material Convert(UnityEngine.Material material)
		{
			var ccMat = GetStandardMaterial(material);
			URPMaterialConverter.BuildLitParams(material, ref ccMat);
			
			var define = ccMat._defines[0];
			var prop = ccMat._props[0];

			var metallic = material.GetFloat(Metallic);
			var specColor = material.GetColor(SpecColor);
			var specular = (specColor.r + specColor.g + specColor.b) / 3f;
			var isSpecHighlight = material.GetInt(SpecularHighlights) > 0;
			var smoothness = material.GetFloat(Smoothness);
			
			// NOTE: Cocos has an offset of 0.2 for roughness, and a roughness value of 0 will cause highlights to disappear.
			// Just Lit shader internally holds a specular of 0.2, so use this.
			var roughness = Mathf.Max(1f - smoothness, isSpecHighlight ? specular : 0);
			
			var metallicMap = material.GetTexture(MetallicGlossMap) as UnityEngine.Texture2D;
			if (metallicMap)
			{
				define.Add("USE_PBR_MAP", true);
				var pbrMapUuid = URPMaterialConverter.ExportPBRMap(
					metallicMap, smoothness, isSpecHighlight ? specular : 0, URPMaterialConverter.PBRMapSourceType.LitMetallic);
				prop.Add("pbrMap", new AssetReference(pbrMapUuid));

				metallic = 1f;
				roughness = 1f;
			}
			else if (material.IsKeywordEnabled("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A"))
			{
				// NOTE: Source Albedo Alpha is not supported by Cocos, Generate PBR map.
				// No support for use with MetallicMap
				var albedoMap = material.mainTexture as UnityEngine.Texture2D;;
				if (albedoMap)
				{
					define.Add("USE_PBR_MAP", true);
					var pbrMapUuid = URPMaterialConverter.ExportPBRMap(
						albedoMap, smoothness, isSpecHighlight ? specular : 0, URPMaterialConverter.PBRMapSourceType.Albedo);
					prop.Add("pbrMap", new AssetReference(pbrMapUuid));
					roughness = 1f;
				}
			}
			
			prop.Add("metallic", metallic);
			prop.Add("roughness", roughness);
			prop.Add("specularIntensity", specular);
			
			var occlusionMap = material.GetTexture(OcclusionMap);
			if (occlusionMap)
			{
				define.Add("USE_OCCLUSION_MAP", true);
				prop.Add("occlusionMap", new AssetReference<cc.Texture2D>(Exporter.GetUuidOrExportAsset(occlusionMap)));
			}
			
			return ccMat;
		}
	}
}
