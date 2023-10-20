using System.Collections.Generic;
using System.Linq;
using cc;
using UnityEngine;
using Mesh = cc.Mesh;

namespace Unity2Cocos
{
	/// <summary>
	/// URP Lit to Cocos Standard Material.
	/// </summary>
	[MaterialConverter("Universal Render Pipeline/Lit")]
	public class LitMaterialConverter : MaterialConverter
	{
		public override cc.Material Convert(UnityEngine.Material material)
		{
			var ccMat = new cc.Material()
			{
			};
			return ccMat;
		}
	}
}
