namespace Unity2Cocos
{
	/// <summary>
	/// URP Unlit to Cocos Unlit Material.
	/// </summary>
	[MaterialConverter("Universal Render Pipeline/Unlit")]
	public class UnlitMaterialConverter : StandardMaterialConverter
	{
		private const string CocosUnlitShaderUuid = "a3cd009f-0ab0-420d-9278-b9fdab939bbc";

		public override cc.Material Convert(UnityEngine.Material material)
		{
			var ccMat = CreateMaterial(CocosUnlitShaderUuid, 3);
			URPMaterialConverter.BuildCocosMaterial(material, ref ccMat, "USE_TEXTURE");
			return ccMat;
		}
	}
}
