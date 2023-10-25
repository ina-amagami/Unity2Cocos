using System.Collections.Generic;
using cc;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Unity2Cocos
{
	[ComponentConverter(typeof(UniversalAdditionalCameraData))]
	public class UniversalAdditionalCameraDataConverter : ComponentConverter<UniversalAdditionalCameraData>
	{
		protected override IEnumerable<CCType> Convert(UniversalAdditionalCameraData component, int currentId)
		{
			// Integrated into CameraConverter.
			return System.Array.Empty<CCType>();
		}
	}
}
