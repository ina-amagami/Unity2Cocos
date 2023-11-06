using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using cc;
using UnityEngine.Rendering;

namespace cc
{	
	public class SceneAsset : Asset
	{
		public string _native = "";
		public SceneNodeId scene = new(1);
	}

	public class Scene : Node
	{
		public bool autoReleaseAssets = false;
		public SceneNodeId _globals;
	}
	
	public class SceneGlobals : CCType
	{
		public SceneNodeId ambient;
		public SceneNodeId shadows;
		public SceneNodeId _skybox;
		public SceneNodeId fog;
		public SceneNodeId octree;
		public SceneNodeId skin;
		public SceneNodeId lightProbeInfo;
		public bool bakedWithStationaryMainLight = false;
		public bool bakedWithHighpLightmap = false;
	}

	public class AmbientInfo : CCType
	{
		public Vec4 _skyColorHDR = new() { x = 0.2f, y = 0.5f, z = 0.8f, w = 0.520833125f };
		public Vec4 _skyColor = new() { x = 0.2f, y = 0.5f, z = 0.8f, w = 0.520833125f };
		public float _skyIllumHDR = 20000;
		public float _skyIllum = 20000;
		public Vec4 _groundAlbedoHDR = new() { x = 0.2f, y = 0.2f, z = 0.2f, w = 1 };
		public Vec4 _groundAlbedo = new() { x = 0.2f, y = 0.2f, z = 0.2f, w = 1 };
		public Vec4 _skyColorLDR = new() { x = 0.452588f, y = 0.607642f, z = 0.755699f, w = 0 }; // fixed value
		public float _skyIllumLDR = 0.8f;
		public Vec4 _groundAlbedoLDR = new() { x = 0.618555f, y = 0.577848f, z = 0.544564f, w = 0 };
	}

	public class ShadowsInfo : CCType
	{
		public bool _enabled = false;
		public int _type = 1;
		public Vec3 _normal = new() { x = 0, y = 1, z = 0 };
		public float _distance = 0;
		public cc.Color _shadowColor = new() { r = 76, g = 76, b = 76, a = 255 };
		public int _maxReceived = 4;
		public Vec2 _size = new() { x = 1024, y = 1024 };
	}

	public class SkyboxInfo : CCType
	{
		public int _envLightingType = 0;
		public AssetReference _envmapHDR = new("d032ac98-05e1-4090-88bb-eb640dcb5fc1@b47c0")
		{
			__expectedType__ = "cc.TextureCube"
		};
		public AssetReference _envmap = new("d032ac98-05e1-4090-88bb-eb640dcb5fc1@b47c0")
		{
			__expectedType__ = "cc.TextureCube"
		};
		public AssetReference _envmapLDR = new("6f01cf7f-81bf-4a7e-bd5d-0afc19696480@b47c0")
		{
			__expectedType__ = "cc.TextureCube"
		};
		public AssetReference _diffuseMapHDR = null;
		public AssetReference _diffuseMapLDR = null;
		public bool _enabled = true;
		public bool _useHDR = true;
		public AssetReference _editableMaterial = null;
		public AssetReference _reflectionHDR = null;
		public AssetReference _reflectionLDR = null;
		public float _rotationAngle = 0;
	}

	public class FogInfo : CCType
	{
		public int _type = 0;
		public cc.Color _fogColor = new() { r = 200, g = 200, b = 200, a = 255 };
		public bool _enabled = true;
		public float _fogDensity = 0.3f;
		public float _fogStart = 0.5f;
		public float _fogEnd = 300f;
		public float _fogAtten = 5;
		public float _fogTop = 1.5f;
		public float _fogRange = 1.2f;
		public bool _accurate = false;
	}

	public class OctreeInfo : CCType
	{
		public bool _enabled = false;
		public Vec3 _minPos = new() { x = -1024, y = -1024, z = -1024 };
		public Vec3 _maxPos = new() { x = 1024, y = 1024, z = 1024 };
		public int _depth = 8;
	}

	public class SkinInfo : CCType
	{
		public bool _enabled = true;
		public float _blurRadius = 0.01f;
		public float _sssIntensity = 3;
	}

	public class LightProbeInfo : CCType
	{
		public float _giScale = 1;
		public int _giSamples = 1024;
		public int _bounces = 2;
		public int _reduceRinging = 0;
		public bool _showProbe = true;
		public bool _showWireframe = true;
		public bool _showConvex = false;
		public AssetReference _data = null;
		public float _lightProbeSphereVolume = 1;
	}
}

namespace Unity2Cocos
{
	[AssetExporter(typeof(UnityEditor.SceneAsset))]
	public class SceneExporter : AssetExporter<UnityEditor.SceneAsset>
	{
		private class Meta : cc.Meta
		{
			public Meta()
			{
				ver = "1.1.45";
				importer = "scene";
			}
		}

		protected override string Extension => ".scene";
	
		public override string Export(UnityEditor.SceneAsset asset)
		{
			var ccMeta = new Meta();
			var ccAsset = new List<CCType>();
			
			// SceneAsset
			var ccSceneAsset = new cc.SceneAsset()
			{
				_name = Info.CocosAssetName
			};
			ccAsset.Add(ccSceneAsset);
			
			// Scene
			var ccScene = new cc.Scene()
			{
				_name = Info.CocosAssetName,
				_id = ccMeta.uuid,
				_lpos = new() { x = 0, y = 0, z = 0 },
				_lrot = new() { x = 0, y = 0, z = 0, w = 1 },
				_lscale = new() { x = 1, y = 1, z = 1 },
			};
			ccAsset.Add(ccScene);
			
			// Node & Components
			EditorSceneManager.OpenScene(Info.UnityAssetPath);
			var rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
			foreach (var root in rootGameObjects)
			{
				ccScene._children.Add(new SceneNodeId(ccAsset.Count));
				Converter.ConvertHierarchy(root.transform, ccAsset);
			}
			Converter.ApplySceneNodeIdReplaceable();
			
			// Scene Globals
			var urpAsset = Utils.GetURPAsset();
			var sceneGlobals = new SceneGlobals();
			ccScene._globals = new SceneNodeId(ccAsset.Count);
			ccAsset.Add(sceneGlobals);

			var ambientInfo = new AmbientInfo();
			if (!ExportSetting.Instance.UseCocosAmbientLightInfo)
			{
				var ambientIntensity = RenderSettings.ambientIntensity;
				var skyColor = Utils.Color32ToVec4(RenderSettings.ambientSkyColor);
				skyColor.w = ambientIntensity;
				ambientInfo._skyColorHDR = ambientInfo._skyColor = skyColor;
				var groundColor = Utils.Color32ToVec4(ExportSetting.Instance.Advanced.IsAmbientGroundUseEquator ?
					RenderSettings.ambientEquatorColor : RenderSettings.ambientGroundColor);
				ambientInfo._groundAlbedoHDR = ambientInfo._groundAlbedo = ambientInfo._groundAlbedoLDR = groundColor;
				ambientInfo._skyIllumLDR = ambientIntensity;
				ambientInfo._skyIllumHDR = ambientInfo._skyIllum = 
					ambientInfo._skyIllumLDR * ExportSetting.Instance.Advanced.IntensityToLightIlluminance;
			}
			sceneGlobals.ambient = new SceneNodeId(ccAsset.Count);
			ccAsset.Add(ambientInfo);
			
			var shadowsInfo = new ShadowsInfo
			{
				_shadowColor = Utils.Color32ToCocosColor(RenderSettings.subtractiveShadowColor),
			};
			if (urpAsset)
			{
				shadowsInfo._enabled = urpAsset.supportsMainLightShadows;
				shadowsInfo._maxReceived = urpAsset.maxAdditionalLightsCount;
				shadowsInfo._size = new Vec2
				{
					x = Mathf.Min(urpAsset.mainLightShadowmapResolution, 2048),
					y = Mathf.Min(urpAsset.mainLightShadowmapResolution, 2048)
				};
				shadowsInfo._distance = urpAsset.shadowDistance;
			}
			sceneGlobals.shadows = new SceneNodeId(ccAsset.Count);
			ccAsset.Add(shadowsInfo);
			
			var skyBoxInfo = new SkyboxInfo();
			var sunSource = RenderSettings.sun ? RenderSettings.sun : 
				UnityEngine.Object.FindObjectsOfType<Light>().FirstOrDefault(x => x.type == LightType.Directional);
			if (sunSource)
			{
				skyBoxInfo._rotationAngle = sunSource.transform.rotation.RightHanded().eulerAngles.y;
			}
			sceneGlobals._skybox = new SceneNodeId(ccAsset.Count);
			ccAsset.Add(skyBoxInfo);
			
			sceneGlobals.fog = new SceneNodeId(ccAsset.Count);
			ccAsset.Add(new FogInfo());
			
			sceneGlobals.octree = new SceneNodeId(ccAsset.Count);
			ccAsset.Add(new OctreeInfo());
			
			sceneGlobals.skin = new SceneNodeId(ccAsset.Count);
			ccAsset.Add(new SkinInfo());
			
			sceneGlobals.lightProbeInfo = new SceneNodeId(ccAsset.Count);
			ccAsset.Add(new LightProbeInfo());
			
			// Export scene.
			ExportAssetToJson(ccAsset);
			ExportMeta(ccMeta);

			return ccMeta.uuid;
		}
	}
}
