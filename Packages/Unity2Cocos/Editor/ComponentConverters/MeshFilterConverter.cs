using System.Collections.Generic;
using cc;
using UnityEngine;

namespace Unity2Cocos
{
	[ComponentConverter(typeof(MeshFilter))]
	public class MeshFilterConverter : ComponentConverter<MeshFilter>
	{
		protected override IEnumerable<CCType> Convert(MeshFilter component, int currentId)
		{
			// In Cocos, MeshFilter is integrated into MeshRenderer.
			return System.Array.Empty<CCType>();
		}
	}
}
