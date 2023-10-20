using System;
using UnityEngine;
using Unity2Cocos;

namespace cc
{
	public class EffectAsset : Asset
	{
	}

	public class Material : Asset
	{
		public string _native = "";
		public AssetReference<EffectAsset> _effectAsset = new(MaterialConverter.CocosStandardShaderUuid);
		public int _techIdx = 0;
		public object[] _defines = Array.Empty<object>();
		public object[] _states = Array.Empty<object>();
		public object[] _props = Array.Empty<object>();
	}
}

namespace Unity2Cocos
{
	[AssetExporter(typeof(Material))]
	public class MaterialExporter : AssetExporter<Material>
	{
		private class Meta : cc.Meta
		{
			public Meta()
			{
				ver = "1.0.20";
				importer = "material";
			}
		}

		protected override string Extension => ".mtl";
		
		public override string Export(Material asset, string outputFolderPath)
		{
			var ccMat = Converter.ConvertMaterial(asset);
			var ccMeta = new Meta();
			
			return string.Empty;
		}
	}
}
