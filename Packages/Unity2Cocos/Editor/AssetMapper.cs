using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unity2Cocos
{
	[CreateAssetMenu(menuName = "Unity2Cocos/AssetMapper", fileName = "AssetMapper")]
	public class AssetMapper : ScriptableObject
	{
		public const string BuiltInResourcesMapperGUID = "bd2fe912b282d7d4e8c0fb890fac7024";
		public const string URPResourcesMapperGUID = "fba91c5ecc4fc1748a7a6902239bf5a1";
		
		[System.Serializable]
		public class MappingData
		{
			public Object Asset;
			public string CocosUUID;
		}
		public MappingData[] Mappings;
	}
}
