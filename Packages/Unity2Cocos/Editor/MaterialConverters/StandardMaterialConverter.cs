using cc;
using UnityEngine;

namespace Unity2Cocos
{
	public abstract class StandardMaterialConverter : MaterialConverter
	{
		public const string CocosStandardShaderUuid = "c8f66d17-351a-48da-a12c-0212d28575c4";
		
		public static cc.Material GetStandardMaterial(UnityEngine.Material material)
		{
			var ccMat = CreateMaterial(material, CocosStandardShaderUuid, 6);
			URPMaterialConverter.BuildCocosMaterial(material, ref ccMat, "USE_ALBEDO_MAP");
			return ccMat;
		}
	}
}
