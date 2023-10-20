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
		public const string CocosStandardShaderUuid = "c8f66d17-351a-48da-a12c-0212d28575c4";
		
		public abstract cc.Material Convert(UnityEngine.Material material);
	}
}
