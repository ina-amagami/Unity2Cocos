using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

namespace Unity2Cocos
{
	[CreateAssetMenu(menuName = "Unity2Cocos/ExportSetting", fileName = "ExportSetting")]
	public class ExportSetting : ScriptableObject
	{
		public static ExportSetting Instance { get; set; }
		
		[Tooltip("Scene to be converted.")]
		public List<SceneAsset> Scenes = new();
		
		[Tooltip("Replace assets referenced in Unity with assets on the Cocos side.")]
		public List<AssetMapper> AssetMappers = new();
		
		[Tooltip("Converts directory paths to web-like.")]
		public bool ExportWebLikePaths = true;
		
		[Tooltip("If not specified, the currently used Asset is adopted.")]
		public UniversalRenderPipelineAsset URPAsset = null;
		
		[Tooltip("Ambient light of Unity and Cocos is very different, turn ON if you want to use the Cocos defaults.")]
		public bool UseCocosAmbientLightInfo = false;

		public enum MeshIDMatchMethodType
		{
			MeshName,
			Triangles,
		}
		[Tooltip("For identifying imported meshes in Cocos. More details in the documentation.")]
		public MeshIDMatchMethodType meshIDMatchMethod = MeshIDMatchMethodType.MeshName;

		[Serializable]
		public class AdvancedSettings
		{
			[Tooltip("Convert from the left-hand coordinate system to the right-hand coordinate system.")]
			public bool ConvertToRightHanded = true;

			[Tooltip("Default Unity's aniso = 2, Cocos = 0")] [Range(-2, 0)]
			public int TextureAnisoLevelShift = -2;

			[Tooltip("Cocos doesn't apply HDR to the brightness of lights, so lower the Bloom threshold.")]
			public float BloomThresholdOffset = -0.1f;

			[Tooltip("Unity light intensity to Cocos illuminance.")]
			public float IntensityToLightIlluminance = 38400;

			[Tooltip("Ambient light intensity adjusted.")]
			public float AmbientIntensityMultiply = 0.5f;
			
			[Tooltip("Ground is too dark, to use Equator.")]
			public bool IsAmbientGroundUseEquator = true;

			[Tooltip("Cocos defaults to 0.01, but that's too small.")]
			public float SpotLightSize = 1;
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
