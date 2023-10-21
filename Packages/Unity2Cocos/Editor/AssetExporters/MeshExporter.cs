using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cc
{
	public class Mesh : Asset
	{
	}
}

namespace Unity2Cocos
{
	[AssetExporter(typeof(Mesh))]
	public class MeshExporter : AssetExporter<Mesh>
	{
		protected override string Extension => ".mesh";
		
		public override string Export(Mesh asset)
		{
			// Mesh export is not supported.
			return string.Empty;
		}
	}
}
