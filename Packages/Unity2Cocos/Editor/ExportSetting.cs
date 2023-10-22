using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Unity2Cocos
{
	[CreateAssetMenu(menuName = "Unity2Cocos/ExportSetting", fileName = "ExportSetting")]
	public class ExportSetting : ScriptableObject
	{
		public static ExportSetting Instance { get; set; }
		
		public List<SceneAsset> Scenes = new();
		public List<AssetMapper> AssetMappers = new();
		public bool ExportWebLikePaths = true;

		public enum MeshIDMatchMethodType
		{
			MeshName,
			Triangles,
		}
		public MeshIDMatchMethodType meshIDMatchMethod = MeshIDMatchMethodType.MeshName;
		
		[Serializable]
		public class AdvancedSettings
		{
			/// <summary>
			/// Convert from the left-hand coordinate system to the right-hand coordinate system.
			/// Normally ON
			/// </summary>
			public bool ConvertToRightHanded = true;
			/// <summary>
			/// Default Unity's aniso = 2, Cocos = 0
			/// </summary>
			[Range(-2, 0)] public int TextureAnisoLevelShift = -2;
		}
		public AdvancedSettings Advanced;

		private void Reset()
		{
			var builtInResourcesMapperPath = AssetDatabase.GUIDToAssetPath(AssetMapper.BuiltInResourcesMapperGUID);
			AssetMappers.Add(AssetDatabase.LoadMainAssetAtPath(builtInResourcesMapperPath) as AssetMapper);
			var urpResourcesMapperPath = AssetDatabase.GUIDToAssetPath(AssetMapper.URPResourcesMapperGUID);
			AssetMappers.Add(AssetDatabase.LoadMainAssetAtPath(urpResourcesMapperPath) as AssetMapper);
		}
	}

	[CustomEditor(typeof(ExportSetting))]
	public class SceneExportSettingEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var exportSetting = target as ExportSetting;
			if (exportSetting && GUILayout.Button("Convert for Cocos"))
			{
				try
				{
					ExportSetting.Instance = exportSetting;
					Exporter.Export();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
				finally
				{
					EditorUtility.ClearProgressBar();
				}
			}
		}
	}
}
