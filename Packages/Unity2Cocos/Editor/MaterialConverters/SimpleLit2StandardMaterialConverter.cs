using cc;
using UnityEngine;

namespace Unity2Cocos
{
	/// <summary>
	/// URP Lit to Cocos Standard Material.
	/// </summary>
	[MaterialConverter("Universal Render Pipeline/SimpleLit")]
	public class SimpleLit2StandardMaterialConverter : StandardMaterialConverter
	{
		public override cc.Material Convert(UnityEngine.Material material)
		{
			var ccMat = GetStandardMaterial(material);
			URPMaterialConverter.BuildLitParams(material, ref ccMat);
			return ccMat;
		}
	}
}
