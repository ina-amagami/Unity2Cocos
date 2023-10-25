using System.Collections.Generic;
using cc;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Unity2Cocos
{
	[ComponentConverter(typeof(UniversalAdditionalLightData))]
	public class UniversalAdditionalLightDataConverter : ComponentConverter<UniversalAdditionalLightData>
	{
		protected override IEnumerable<CCType> Convert(UniversalAdditionalLightData component, int currentId)
		{
			// Integrated into LightConverter.
			return System.Array.Empty<CCType>();
		}
	}
}
