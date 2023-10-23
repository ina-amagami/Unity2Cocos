using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Unity2Cocos
{
	public static class Exporter
	{
		public const string ProgressBarTitle = "Unity2Cocos";
		
		private static readonly Dictionary<Type, AssetExporter> _exporterCache = new();
		private static readonly Dictionary<int, string> _assetMap = new();
		private static string _outputFolderPath;

		public static void Export()
		{
			_outputFolderPath = EditorUtility.OpenFolderPanel("Select Export Folder", "", "");
			
			if (string.IsNullOrEmpty(_outputFolderPath))
			{
				return;
			}
			
			// Setup
			EditorUtility.DisplayProgressBar(
				ProgressBarTitle, $"Setup...", 0);
			
			var setting = ExportSetting.Instance;
			InitializeAssetMapping(setting.AssetMappers);
			CacheExporter();
			Converter.Initialize();

			// Export scenes.
			EditorUtility.DisplayProgressBar(
				ProgressBarTitle, $"Exporting scenes to: {_outputFolderPath}...", 0);
			
			foreach (var scene in setting.Scenes)
			{
				GetUuidOrExportAsset(scene);
			}
		}
		
		private static void CacheExporter()
		{
			_exporterCache.Clear();
			var exporters = Utils.GetTypesIsSubclassOf<AssetExporter>();
			foreach (var exporter in exporters)
			{
				var attribute = Utils.GetAttribute<AssetExporterAttribute>(exporter);
				if (attribute == null)
				{
					Debug.LogError($"[AssetExporter] AssetExporterAttribute is not assigned. -> {exporter.Name}");
					continue;
				}
				_exporterCache.Add(
					attribute.Type,
					(AssetExporter)Activator.CreateInstance(exporter));
			}
		}

		private static void InitializeAssetMapping(List<AssetMapper> mappers)
		{
			_assetMap.Clear();
			foreach (var mapper in mappers)
			{
				foreach (var mapping in mapper.Mappings)
				{
					AddAssetMap(mapping.Asset, mapping.CocosUUID);
				}
			}
		}

		public static void AddAssetMap(UnityEngine.Object asset, string uuid)
		{
			var key = asset.GetHashCode();
			if (_assetMap.ContainsKey(key))
			{
				return;
			}
			_assetMap.Add(key, uuid);
		}

		private static string GetMappedAssetUuid(UnityEngine.Object asset)
		{
			return _assetMap.TryGetValue(asset.GetHashCode(), out var uuid) ? uuid : null;
		}

		public static string GetUuidOrExportAsset(UnityEngine.Object asset)
		{
			var uuid = GetMappedAssetUuid(asset);
			if (!string.IsNullOrEmpty(uuid)) return uuid;
			
			var type = asset.GetType();
			if (!_exporterCache.TryGetValue(type, out var exporter))
			{
				Debug.LogWarning(
					$"[Exporter] Skipped of unsupported asset. -> {asset.name}<{type.Name}>");
				return string.Empty;
			}

			uuid = exporter.ExportExecute(asset, _outputFolderPath);
			AddAssetMap(asset, uuid);
			
			return uuid;
		}
	}
}
