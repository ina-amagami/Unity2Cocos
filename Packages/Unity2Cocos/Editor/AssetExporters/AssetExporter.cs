using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Unity2Cocos
{
	[AttributeUsage(AttributeTargets.Class)]
	public class AssetExporterAttribute : Attribute
	{
		public Type Type { get; }

		public AssetExporterAttribute(Type type)
		{
			Type = type;
		}
	}

	public abstract class AssetExporter
	{
		protected struct ExportInfo
		{
			public ExportInfo(UnityEditor.SceneAsset asset, string outputFolderPath, string extension)
			{
			}
		}
		
		protected abstract string Extension { get; }
		
		public abstract string ExportExecute(UnityEngine.Object asset, string outputFolderPath);
		
		protected void ExportAssetToJson<TCCAsset>(TCCAsset asset, string outputPath)
		{
			var jsonString = JsonConvert.SerializeObject(asset, Formatting.Indented);
			var metaPath = outputPath + Extension;
			outputPath = Path.Combine(outputPath, metaPath);
			File.WriteAllText(outputPath, jsonString);
		}

		protected void ExportMeta<TMeta>(TMeta meta, string outputPath)
		{
			var jsonString = JsonConvert.SerializeObject(meta, Formatting.Indented);
			var metaPath = outputPath + Extension + ".meta";
			outputPath = Path.Combine(outputPath, metaPath);
			File.WriteAllText(outputPath, jsonString);
		}
	}

	public abstract class AssetExporter<TUnityAsset> : AssetExporter where TUnityAsset : UnityEngine.Object
	{
		public override string ExportExecute(UnityEngine.Object asset, string outputFolderPath)
		{
			return Export(asset as TUnityAsset, outputFolderPath);
		}
		public abstract string Export(TUnityAsset asset, string outputFolderPath);
	}
}
