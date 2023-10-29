using System.Collections.Generic;
using System.Linq;
using cc;
using UnityEngine;
using UnityEngine.Rendering;

namespace cc
{
	public class ReflectionProbe : Component
	{
		public int _resolution;
		public int _clearFlag;
		public Color _backgroundColor;
		public int _visibility;
		public int _probeType = 0;
		// NOTE: Cubemap baking is performed on the Cocos.
		public AssetReference _cubemap = null;
		public Vec3 _size;
		public object _sourceCamera = null;
		public int _probeId = 0;
		public bool _fastBake = false;
	}
}

namespace Unity2Cocos
{
	[ComponentConverter(typeof(UnityEngine.ReflectionProbe))]
	public class ReflectionProbeConverter : ComponentConverter<UnityEngine.ReflectionProbe>
	{
		public static int ProbeId = 0;
		
		protected override IEnumerable<CCType> Convert(UnityEngine.ReflectionProbe component, int currentId)
		{
			var res = component.resolution;
			if (res > 512)
			{
				res = 768;
			}
			else if (res <= 256)
			{
				res = 256;
			}
			else
			{
				res = 512;
			}
			var size = component.size * 0.5f;
			var ccReflectionProbe = new cc.ReflectionProbe
			{
				_enabled = component.enabled,
				_resolution = res,
				_clearFlag = component.clearFlags == ReflectionProbeClearFlags.SolidColor ? 7 : 14,
				_backgroundColor = Utils.Color32ToCocosColor(component.backgroundColor),
				// TODO: Support culling mask.
				_visibility = 1 << Utils.LayerConvert(0),
				_size = new Vec3 { x = size.x, y = size.y, z = size.z },
				_probeId = ProbeId++
			};
			return new CCType[] { ccReflectionProbe };
		}
	}
}

