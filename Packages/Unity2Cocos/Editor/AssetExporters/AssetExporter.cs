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
		public class ExportInfo
		{
			public readonly string OutputFolderPath;
			public readonly string UnityAssetPath;
			public readonly string UnityAssetDirectory;
			public readonly string UnityAssetName;
			public readonly string CocosAssetDirectory;
			public readonly string CocosAssetName;
			public readonly string CocosAssetPath;
			public readonly string CocosAssetOutputPath;
			public readonly string CocosMetaOutputPath;

			public ExportInfo(UnityEngine.Object asset, string outputFolderPath, string extension) :
				this(AssetDatabase.GetAssetPath(asset), outputFolderPath, extension)
			{
			}
			
			public ExportInfo(string assetPath, string outputFolderPath, string extension)
			{
				OutputFolderPath = outputFolderPath;

				UnityAssetPath = assetPath;
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
			ExportAssetCopy(Info);
		}
		public static void ExportAssetCopy(ExportInfo info)
		{
			Directory.CreateDirectory(Path.Combine(info.OutputFolderPath, info.CocosAssetDirectory));
			if (File.Exists(info.CocosAssetOutputPath))
			{
				File.Delete(info.CocosAssetOutputPath);
			}
			File.Copy(info.UnityAssetPath, info.CocosAssetOutputPath);
		}
		
		protected void ExportAssetToJson(object asset)
		{
			ExportAssetToJson(asset, Info);
		}
		public static void ExportAssetToJson(object asset, ExportInfo info)
		{
			Directory.CreateDirectory(Path.Combine(info.OutputFolderPath, info.CocosAssetDirectory));
			var jsonString = JsonConvert.SerializeObject(asset, Formatting.Indented);
			File.WriteAllText(info.CocosAssetOutputPath, jsonString);
		}

		protected void ExportMeta(object meta)
		{
			ExportMeta(meta, Info);
		}
		public static void ExportMeta(object meta, ExportInfo info)
		{
			Directory.CreateDirectory(Path.Combine(info.OutputFolderPath, info.CocosAssetDirectory));
			var jsonString = JsonConvert.SerializeObject(meta, Formatting.Indented);
			File.WriteAllText(info.CocosMetaOutputPath, jsonString);
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
