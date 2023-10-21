using cc;

namespace Unity2Cocos
{
	/// <summary>
	/// URP Lit to Cocos Standard Material.
	/// </summary>
	[MaterialConverter("Universal Render Pipeline/Lit")]
	public class LitMaterialConverter : StandardMaterialConverter
	{
		public override cc.Material Convert(UnityEngine.Material material)
		{
			var ccMat = GetStandardMaterial(material);
			return ccMat;
		}
	}
}
