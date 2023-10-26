using System.Collections.Generic;
using cc;
using UnityEngine;

namespace cc
{
	public class LODGroup : Component
	{
		public Vec3 _localBoundaryCenter;
		public float _objectSize;
		public SceneNodeId[] _LODs = new SceneNodeId[3];
	}
	
	public class LOD : CCType
	{
		public double _screenUsagePercentage;
		public List<SceneNodeIdReplaceable> _renderers = new();
	}
}

namespace Unity2Cocos
{
	[ComponentConverter(typeof(UnityEngine.LODGroup))]
	public class LODGroupConverter : ComponentConverter<UnityEngine.LODGroup>
	{
		private const int LODCount = 3;
		
		protected override IEnumerable<CCType> Convert(UnityEngine.LODGroup component, int currentId)
		{
			var list = new List<CCType>();
			var ccLODGroup = new cc.LODGroup
			{
				_localBoundaryCenter = Utils.Vector3ToVec3(component.localReferencePoint.RightHanded()),
				_objectSize = component.size,
			};
			for (var i = 0; i < LODCount; ++i)
			{
				ccLODGroup._LODs[i] = new SceneNodeId(currentId + i + 1);
			}
			list.Add(ccLODGroup);
			
			var lods = component.GetLODs();
			for (var i = 0; i < LODCount; ++i)
			{
				var ccLOD = new cc.LOD();
				if (i < lods.Length)
				{
					var lod = lods[i];
					ccLOD._screenUsagePercentage = System.Math.Round(
						System.Convert.ToDouble(lod.screenRelativeTransitionHeight), 3);
					foreach (var renderer in lod.renderers)
					{
						if (!renderer)
						{
							continue;
						}
						ccLOD._renderers.Add(new SceneNodeIdReplaceable(renderer));
					}
				}
				else
				{
					ccLOD._screenUsagePercentage = i switch
					{
						0 => 25,
						1 => 12.5f,
						2 => 0.01f,
						_ => 0
					};
				}
				list.Add(ccLOD);
			}
			return list;
		}
	}
}
