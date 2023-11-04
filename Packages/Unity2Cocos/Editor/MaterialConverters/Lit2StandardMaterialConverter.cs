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

			var specColor = material.GetColor(SpecColor);
			var specular = (specColor.r + specColor.g + specColor.b) / 3f;
			var isSpecHighlight = material.GetInt(SpecularHighlights);
			var roughness = Mathf.Max(
				1f - material.GetFloat(Smoothness),
				isSpecHighlight > 0 ? specular : 0);
			
			var metallicMap = material.GetTexture(MetallicGlossMap);
			if (metallicMap)
			{
				define.Add("USE_PBR_MAP", true);
				prop.Add("pbrMap", new AssetReference<cc.Texture2D>(Exporter.GetUuidOrExportAsset(metallicMap)));
			}
			else if (material.IsKeywordEnabled("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A"))
			{
				// NOTE: Source Albedo Alpha is not supported by Cocos, so use default values.
				var albedoMap = material.mainTexture;
				if (albedoMap)
				{
					prop.Add("pbrMap", new AssetReference<cc.Texture2D>(Exporter.GetUuidOrExportAsset(albedoMap)));
				}
				roughness = 0.5f;
				specular = 0.5f;
			}
			
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
