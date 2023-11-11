using System.Collections.Generic;
using cc;
using UnityEngine;
using UnityEngine.Rendering;
using Camera = UnityEngine.Camera;
using UnityEngine.Rendering.Universal;

namespace cc
{
	public class PostProcess : Component
	{
		public float _shadingScale = 1;
	}
	
	public class FXAA : Component
	{
	}
	
	public class Bloom : Component
	{
		public float _threshold;
		public int _iterations = 3;
		public float _intensity;
	}
}

namespace Unity2Cocos
{
	[ComponentConverter(typeof(Volume))]
	public class VolumeConverter : ComponentConverter<Volume>
	{
		protected override IEnumerable<CCType> Convert(Volume component, int currentId)
		{
			// Only GlobalVolume is converted at the same time as the camera.
			if (!component.isGlobal)
			{
				var transformPath = Utils.GetTransformPath(component.transform);
				Debug.LogWarning(
					$"[Converter] Non Global Volume unsupported. -> {transformPath}<Volume>");
				return System.Array.Empty<CCType>();
			}

			var list = new List<CCType>();
			list.Add(new PostProcess
			{
				_enabled = component.enabled
			});

			var camera = Camera.main;
			var additionalData = camera.GetUniversalAdditionalCameraData();
			if (additionalData && additionalData.antialiasing != AntialiasingMode.None)
			{
				list.Add(new FXAA());
			}

			var profile = component.sharedProfile;
			if (!profile)
			{
				return list;
			}

			if (profile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out var bloom))
			{
				list.Add(new cc.Bloom
				{
					_threshold = bloom.threshold.value + ExportSetting.Instance.Advanced.BloomThresholdOffset,
					_intensity = bloom.intensity.value
				});
			}
			
			return list;
		}
	}
}
