using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cc
{
	public class Texture2D : Asset
	{
	}
}

namespace Unity2Cocos
{
	[AssetExporter(typeof(Texture2D))]
	public class Texture2DExporter : AssetExporter<Texture2D>
	{
		private class Meta : cc.Meta
		{
			public Meta()
			{
				ver = "1.0.26";
				importer = "image";
				files = new[]
				{
					".json",
					".png"
				};
			}
		}

		protected override string Extension => null;
		
		public override string Export(Texture2D asset)
		{
			var ccMeta = new Meta();
			
			ExportAssetCopy();
			ExportMeta(ccMeta);
			
			// Mesh export is not supported.
			return ccMeta.uuid;
		}
	}
}
