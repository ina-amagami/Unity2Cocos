using System;
using UnityEngine;
using Unity2Cocos;

namespace cc
{
	public class EffectAsset : Asset
	{
	}

	public class MaterialDefine
	{
	}
	
	public class MaterialState
	{
		public object rasterizerState = new();
		public object depthStencilState = new();
		public BlendState blendState = new();
	}
		
	public class BlendState
	{
		public object[] targets = new object[1];
	}

	public class MaterialProp
	{
	}

	public class Material : Asset
	{
		public string _native = "";
		public AssetReference<EffectAsset> _effectAsset;
		public int _techIdx = 0;
		public MaterialDefine[] _defines = Array.Empty<MaterialDefine>();
		public MaterialState[] _states = Array.Empty<MaterialState>();
		public MaterialProp[] _props = Array.Empty<MaterialProp>();
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
		
		public override string Export(Material asset)
		{
			var ccMat = Converter.ConvertMaterial(asset);
			var ccMeta = new Meta();
			ExportAssetToJson(ccMat);
			ExportMeta(ccMeta);
			return ccMeta.uuid;
		}
	}
}
