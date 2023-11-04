using System.Linq;
using System.Collections.Generic;
using cc;
using Unity2Cocos;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace cc
{
	public abstract class LightBase : Component
	{
		public cc.Color _color;
		public bool _useColorTemperature;
		public int _colorTemperature;
		public SceneNodeId _staticSettings;
		public int _visibility;
		public bool _shadowEnabled;
		public int _shadowPcf;
		public float _shadowBias;
		public float _shadowNormalBias;
		public float _shadowSaturation;

		protected LightBase(Light unityLight, int currentId)
		{
			var additionalData = unityLight.GetUniversalAdditionalLightData();
			_color = Utils.Color32ToCocosColor(unityLight.color);
			_useColorTemperature = unityLight.useColorTemperature;
			_colorTemperature = (int)unityLight.colorTemperature;
			// TODO: Support culling mask.
			_visibility = 1 << Utils.LayerConvert(0);
			_shadowEnabled = unityLight.shadows != LightShadows.None;
			_shadowBias = unityLight.shadowBias;
			_shadowNormalBias = unityLight.shadowNormalBias;
			_shadowSaturation = unityLight.shadowStrength;
			
			if (additionalData && unityLight.shadows == LightShadows.Soft)
			{
				if (additionalData.softShadowQuality == SoftShadowQuality.UsePipelineSettings)
				{
					var pipelineAsset = Utils.GetURPAsset();
					if (pipelineAsset)
					{
						var so = new SerializedObject(pipelineAsset);
						_shadowPcf = so.FindProperty("m_SoftShadowQuality").intValue;
					}
				}
				else
				{
					_shadowPcf = (int)additionalData.softShadowQuality;
				}
			}

			_staticSettings = new SceneNodeId(currentId + 1);
		}
	}
	
	public class DirectionalLight : LightBase
	{
		public int _illuminanceHDR;
		public int _illuminance;
		public float _illuminanceLDR;
		public bool _shadowFixedArea = false;
		public float _shadowNear;
		public float _shadowFar = 10;
		public float _shadowOrthoSize = 5;
		public float _shadowDistance = 50;
		public float _shadowInvisibleOcclusionRange = 200;
		public int _csmLevel = 4;
		public float _csmLayerLambda = 0.75f;
		public int _csmOptimizationMode = 2;
		public bool _csmAdvancedOptions = false;
		public bool _csmLayersTransition = false;
		public float _csmTransitionRange = 0.05f;

		public DirectionalLight(Light unityLight, int currentId) : base(unityLight, currentId)
		{
			_illuminance = _illuminanceHDR = (int)(unityLight.intensity * ExportSetting.Instance.Advanced.IntensityToLightIlluminance);
			_illuminanceLDR = unityLight.intensity;
			_shadowNear = unityLight.shadowNearPlane;
			
			var pipelineAsset = Utils.GetURPAsset();
			if (pipelineAsset)
			{
				_shadowDistance = pipelineAsset.shadowDistance;
				_csmLevel = pipelineAsset.shadowCascadeCount > 1 ? 4 : 1;
				/* NOTE: CascadeShadowMap parameters are different from Unity, default values are used. */
			}
		}
	}
	
	public class PointLight : LightBase
	{
		public float _luminanceHDR;
		public float _luminance;
		public float _luminanceLDR = 498.3970260184439f; // fixed value
		public int _term = 1;
		public float _range;

		public PointLight(Light unityLight, int currentId) : base(unityLight, currentId)
		{
			_luminance = _luminanceHDR = unityLight.intensity;
			_range = unityLight.range;
		}
	}
	
	public class SpotLight : PointLight
	{
		public float _size;
		public float _spotAngle;

		public SpotLight(Light unityLight, int currentId) : base(unityLight, currentId)
		{ 
			_size = _spotAngle = unityLight.spotAngle;
		}
	}
	
	public class StaticLightSettings : CCType
	{
		public bool _baked = false;
		public bool _editorOnly = false;
		public bool _castShadow = false;
	}
}

namespace Unity2Cocos
{
	[ComponentConverter(typeof(Light))]
	public class LightConverter : ComponentConverter<Light>
	{
		protected override IEnumerable<CCType> Convert(Light component, int currentId)
		{
			var list = new List<CCType>();
			switch (component.type)
			{
				case LightType.Directional:
					list.Add(new DirectionalLight(component, currentId));
					break;
				case LightType.Point:
					list.Add(new PointLight(component, currentId));
					break;
				case LightType.Spot:
					list.Add(new SpotLight(component, currentId));
					break;
				// TODO: Area light support. (Cocos area light is RangedDirectionalLight?)
				default:
					var path = Utils.GetTransformPath(component.transform);
					Debug.LogWarning($"[LightConverter] Unsupported light type. -> {path}<{component.type.ToString()}Light>");
					break;
			}
			if (list.Count > 0)
			{
				list.Add(new StaticLightSettings
				{
					_baked = component.lightmapBakeType == LightmapBakeType.Baked,
					_castShadow = component.shadows != LightShadows.None
				});
			}
			return list;
		}
	}
}
