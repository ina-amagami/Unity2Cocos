using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;

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
			public string OutputFolderPath;
			public string UnityAssetPath;
			public string UnityAssetDirectory;
			public string UnityAssetName;
			public string CocosAssetDirectory;
			public string CocosAssetName;
			public string CocosAssetPath;
			public string CocosAssetOutputPath;
			public string CocosMetaOutputPath;
			
			public ExportInfo(UnityEngine.Object asset, string outputFolderPath, string extension)
			{
				OutputFolderPath = outputFolderPath;
				
				UnityAssetPath = AssetDatabase.GetAssetPath(asset);
				UnityAssetDirectory = Path.GetDirectoryName(UnityAssetPath);

				if (!string.IsNullOrEmpty(extension))
				{
					UnityAssetName = Path.GetFileNameWithoutExtension(UnityAssetPath);
				}
				else
				{
					UnityAssetName = Path.GetFileName(UnityAssetPath);
				}

				CocosAssetDirectory = Utils.ConvertToOutputPathFormat(UnityAssetDirectory);
				CocosAssetName = Utils.ConvertToOutputPathFormat(UnityAssetName);
				CocosAssetPath = Path.Combine(CocosAssetDirectory, CocosAssetName);
				
				CocosAssetOutputPath = Path.Combine(OutputFolderPath, CocosAssetPath) + extension;
				CocosMetaOutputPath = CocosAssetOutputPath + ".meta";
			}
		}
		protected ExportInfo Info { get; set; }

		protected abstract string Extension { get; }
		
		public abstract string ExportExecute(UnityEngine.Object asset, string outputFolderPath);
		
		protected void ExportAssetCopy()
		{
			Directory.CreateDirectory(Path.Combine(Info.OutputFolderPath, Info.CocosAssetDirectory));
			if (File.Exists(Info.CocosAssetOutputPath))
			{
				File.Delete(Info.CocosAssetOutputPath);
			}
			File.Copy(Info.UnityAssetPath, Info.CocosAssetOutputPath);
		}
		
		protected void ExportAssetToJson<TCCAsset>(TCCAsset asset)
		{
			Directory.CreateDirectory(Path.Combine(Info.OutputFolderPath, Info.CocosAssetDirectory));
			var jsonString = JsonConvert.SerializeObject(asset, Formatting.Indented);
			File.WriteAllText(Info.CocosAssetOutputPath, jsonString);
		}

		protected void ExportMeta<TMeta>(TMeta meta)
		{
			Directory.CreateDirectory(Path.Combine(Info.OutputFolderPath, Info.CocosAssetDirectory));
			var jsonString = JsonConvert.SerializeObject(meta, Formatting.Indented);
			File.WriteAllText(Info.CocosMetaOutputPath, jsonString);
		}
	}

	public abstract class AssetExporter<TUnityAsset> : AssetExporter where TUnityAsset : UnityEngine.Object
	{
		public override string ExportExecute(UnityEngine.Object asset, string outputFolderPath)
		{
			Info = new ExportInfo(asset, outputFolderPath, Extension);
			return Export(asset as TUnityAsset);
		}
		public abstract string Export(TUnityAsset asset);
	}
}
