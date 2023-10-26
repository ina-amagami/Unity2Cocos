using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unity2Cocos
{
	[CreateAssetMenu(menuName = "Unity2Cocos/ScriptMapper", fileName = "ScriptMapper")]
	public class ScriptMapper : ScriptableObject
	{
		// TODO: Support property mapping.
		// public enum CCPropertyType
		// {
		// 	Number = 0,
		// 	String = 10,
		// 	Vec2 = 20,
		// 	Vec3 = 30,
		// 	Vec4 = 40,
		// 	Quat = 50,
		// 	Color = 60,
		// 	Rect = 70
		// }
		//
		// [System.Serializable]
		// public class PropertyMapping
		// {
		// 	public string UnityPropName;
		// 	public CCPropertyType CocosPropType;
		// 	public string CocosPropName;
		// }
		
		[System.Serializable]
		public class MappingData
		{
			public MonoScript Script;
			public string CocosCompressedUUID;
			// public PropertyMapping[] Props;
		}
		public MappingData[] Mappings;
	}
}
