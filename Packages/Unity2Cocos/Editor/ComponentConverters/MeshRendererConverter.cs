using System;
using System.Collections.Generic;
using System.Linq;
using cc;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace cc
{
	public class MeshRenderer : Component
	{
		public AssetReference<Material>[] _materials;
		public int _visFlags = 0;
		public SceneNodeId bakeSettings;
		public AssetReference<Mesh> _mesh;
		public int _shadowCastingMode = 0;
		public int _shadowReceivingMode = 1;
		public int _shadowBias = 0;
		public int _shadowNormalBias = 0;
		public int _reflectionProbeId = -1;
		public int _reflectionProbeBlendId = -1;
		public int _reflectionProbeBlendWeight = 0;
		public bool _enabledGlobalStandardSkinObject = false;
		public bool _enableMorph = true;
	}

	public class ModelBakeSettings : CCType
	{
		public AssetReference texture = null;
		public Vec4 uvParam = Vec4.Zero;
		public bool _bakeable = false;
		public bool _castShadow = false;
		public bool _receiveShadow = false;
		public bool _recieveShadow = false;
		public int _lightmapSize = 64;
		public bool _useLightProbe = false;
		public bool _bakeToLightProbe = true;
		public int _reflectionProbeType = 0;
		public bool _bakeToReflectionProbe = true;
	}
}

namespace Unity2Cocos
{
	[ComponentConverter(typeof(UnityEngine.MeshRenderer))]
	public class MeshRendererConverter : ComponentConverter<UnityEngine.MeshRenderer>
	{
		protected override IEnumerable<CCType> Convert(UnityEngine.MeshRenderer component, int currentId)
		{
			var mesh = component.GetComponent<MeshFilter>().sharedMesh;
			var ccMeshRenderer = new cc.MeshRenderer
			{
				_enabled = component.enabled,
				_materials = component.sharedMaterials.Select(mat => 
					new AssetReference<cc.Material>(Exporter.GetUuidOrExportAsset(mat))).ToArray(),
				bakeSettings = new SceneNodeId(currentId + 1),
				_mesh = new AssetReference<cc.Mesh>(Exporter.GetUuidOrExportAsset(mesh)),
				_shadowCastingMode = component.shadowCastingMode != ShadowCastingMode.Off ? 1 : 0,
				_shadowReceivingMode = component.receiveShadows ? 1 : 0,
				_reflectionProbeId = component.reflectionProbeUsage != ReflectionProbeUsage.Off ? 0 : -1,
			};
			var ccModelBakeSettings = new ModelBakeSettings
			{
				_bakeable = ((int)GameObjectUtility.GetStaticEditorFlags(component.gameObject) & (int)StaticEditorFlags.ContributeGI) == 1,
				_castShadow = component.staticShadowCaster,
				_receiveShadow = component.receiveShadows,
				_recieveShadow = component.receiveShadows,
				_useLightProbe = component.lightProbeUsage != LightProbeUsage.Off,
				_reflectionProbeType = component.reflectionProbeUsage switch
				{
					ReflectionProbeUsage.Off => 0,
					ReflectionProbeUsage.Simple => 1,
					ReflectionProbeUsage.BlendProbes => 3,
					ReflectionProbeUsage.BlendProbesAndSkybox => 4,
					_ => 0
				}
			};
			return new CCType[] { ccMeshRenderer, ccModelBakeSettings };
		}
	}
}

