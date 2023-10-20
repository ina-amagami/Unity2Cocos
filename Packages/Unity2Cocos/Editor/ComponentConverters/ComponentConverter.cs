using System;
using System.Collections.Generic;
using cc;

namespace Unity2Cocos
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ComponentConverterAttribute : Attribute
	{
		public Type Type { get; }

		public ComponentConverterAttribute(Type type)
		{
			Type = type;
		}
	}

	public abstract class ComponentConverter
	{
		public abstract IEnumerable<CCType> ConvertExecute(UnityEngine.Component component, int currentId);
	}

	public abstract class ComponentConverter<TUnityComponent> : ComponentConverter
		where TUnityComponent : UnityEngine.Component
	{
		public override IEnumerable<CCType> ConvertExecute(UnityEngine.Component component, int currentId)
		{
			return Convert(component as TUnityComponent, currentId);
		}
		protected abstract IEnumerable<CCType> Convert(TUnityComponent component, int currentId);
	}
}
