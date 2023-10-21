using cc;
using UnityEngine;

namespace Unity2Cocos
{
	/// <summary>
	/// URP Lit to Cocos Standard Material.
	/// </summary>
	[MaterialConverter("Universal Render Pipeline/Lit")]
	public class Lit2StandardMaterialConverter : StandardMaterialConverter
	{
		private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
		private static readonly int BumpScale = Shader.PropertyToID("_BumpScale");

		public override cc.Material Convert(UnityEngine.Material material)
		{
			var ccMat = GetStandardMaterial(material);
			var define = ccMat._defines[0];
			var prop = ccMat._props[0];

			var hasNormalMap = material.IsKeywordEnabled("_NORMALMAP") && material.HasTexture(BumpMap);
			if (hasNormalMap)
			{
				var normalMap = material.GetTexture(BumpMap);
				if (normalMap)
				{
					define.Add("USE_NORMAL_MAP", true);
					prop.Add("normalMap", new AssetReference<cc.Texture2D>(Exporter.GetUuidOrExportAsset(normalMap)));
					prop.Add("normalStrength", Mathf.Clamp(material.GetFloat(BumpScale), 0, 5));
				}
			}
			
			return ccMat;
		}
	}
}
