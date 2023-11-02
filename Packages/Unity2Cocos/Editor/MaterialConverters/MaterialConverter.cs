using System;
using System.Collections.Generic;
using cc;

namespace Unity2Cocos
{
	[AttributeUsage(AttributeTargets.Class)]
	public class MaterialConverterAttribute : Attribute
	{
		public string Shader { get; }

		public MaterialConverterAttribute(string shader)
		{
			Shader = shader;
		}
	}

	public abstract class MaterialConverter
	{
		public abstract Material Convert(UnityEngine.Material material);

		public static Material CreateMaterial(string effectUuid, int passCount)
		{
			var ccMat = new Material
			{
				_name = "",
				_effectAsset = new(effectUuid),
				_defines = new MaterialDefine[passCount],
				_states = new MaterialState[passCount],
				_props = new MaterialProp[passCount]
			};
			for (var i = 0; i < ccMat._defines.Length; ++i)
			{
				ccMat._defines[i] = new MaterialDefine();
			}
			for (var i = 0; i < ccMat._states.Length; ++i)
			{
				var state = new MaterialState();
				state.blendState.targets[0] = new Dictionary<string, object>();
				ccMat._states[i] = state;
			}
			for (var i = 0; i < ccMat._props.Length; ++i)
			{
				ccMat._props[i] = new MaterialProp();
			}
			return ccMat;
		}
	}
}
