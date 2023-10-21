using System.IO;
using UnityEditor;
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
		protected override string Extension => "";
		
		public override string Export(Mesh asset)
		{
			var mainAsset = AssetDatabase.LoadMainAssetAtPath(Info.UnityAssetPath);
			if (mainAsset == asset)
			{
				Debug.LogWarning($"[MeshExporter] Mesh-only asset is not supported. -> {Info.UnityAssetName}");
				return string.Empty;
			}

			if (!Path.GetExtension(Info.UnityAssetName).Equals(".fbx"))
			{
				Debug.LogWarning($"[MeshExporter] Only FBX is supported. -> {Info.UnityAssetName}");
				return string.Empty;
			}

			return FBXExporter.Export(Info, asset);
		}
	}
}
