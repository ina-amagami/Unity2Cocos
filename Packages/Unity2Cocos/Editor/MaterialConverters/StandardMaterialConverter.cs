using System;
using cc;

namespace Unity2Cocos
{
	public abstract class StandardMaterialConverter : MaterialConverter
	{
		public const string CocosStandardShaderUuid = "c8f66d17-351a-48da-a12c-0212d28575c4";

		public static Material GetStandardMaterial(UnityEngine.Material material)
		{
			var ccMat = CreateMaterial(CocosStandardShaderUuid, 6);
			
			var define = ccMat._defines[0];
			var prop = ccMat._props[0];
			
			var hasMainTexture = material.HasTexture("_BaseMap") || material.HasTexture("_MainTex");
			if (hasMainTexture)
			{
				var mainTexture = material.mainTexture;
				if (mainTexture)
				{
					define.Add("USE_ALBEDO_MAP", true);
					prop.Add("mainTexture", new AssetReference<Texture2D>(Exporter.GetUuidOrExportAsset(mainTexture)));
				}
				prop.Add("tilingOffset", new Vec4()
				{
					x = material.mainTextureScale.x,
					y = material.mainTextureScale.y,
					z = material.mainTextureOffset.x,
					w = material.mainTextureOffset.y
				});
			}
			var hasColor = material.HasColor("_BaseColor") || material.HasColor("_BaseColor");
			if (hasColor)
			{
				prop.Add("mainColor", Utils.Color32ToCocosColor(material.color));
			}
			
			return ccMat;
		}
	}
}
