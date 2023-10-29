using System.Collections.Generic;
using cc;
using UnityEngine;

namespace cc
{
    public class LightProbeGroup : Component
    {
        public List<Vec3> _probes = new();
        public int _method = 0;
        public Vec3 _minPos;
        public Vec3 _maxPos;
        public int _nProbesX = 3;
        public int _nProbesY = 3;
        public int _nProbesZ = 3;
    }
}

namespace Unity2Cocos
{
    [ComponentConverter(typeof(UnityEngine.LightProbeGroup))]
    public class LightProbeGroupConverter : ComponentConverter<UnityEngine.LightProbeGroup>
    {
        public static int ProbeId = 0;
		
        protected override IEnumerable<CCType> Convert(UnityEngine.LightProbeGroup component, int currentId)
        {
            var ccLightProbeGroup = new cc.LightProbeGroup
            {
                _enabled = component.enabled,
                _minPos = new Vec3 { x = int.MaxValue, y = int.MaxValue, z = int.MaxValue },
                _maxPos = new Vec3 { x = int.MinValue, y = int.MinValue, z = int.MinValue },
            };
            var parentRot = component.transform.parent ? 
                component.transform.parent.rotation : Quaternion.identity;
            foreach (var p in component.probePositions)
            {
                var probe = parentRot * p;
                probe = component.transform.localRotation * probe;
                probe = probe.RightHanded();
                ccLightProbeGroup._probes.Add(Utils.Vector3ToVec3(probe));

                if (ccLightProbeGroup._minPos.x > probe.x)
                {
                    ccLightProbeGroup._minPos.x = probe.x;
                }
                if (ccLightProbeGroup._minPos.y > probe.y)
                {
                    ccLightProbeGroup._minPos.y = probe.y;
                }
                if (ccLightProbeGroup._minPos.z > probe.z)
                {
                    ccLightProbeGroup._minPos.z = probe.z;
                }
				
                if (ccLightProbeGroup._maxPos.x < probe.x)
                {
                    ccLightProbeGroup._maxPos.x = probe.x;
                }
                if (ccLightProbeGroup._maxPos.y < probe.y)
                {
                    ccLightProbeGroup._maxPos.y = probe.y;
                }
                if (ccLightProbeGroup._maxPos.z < probe.z)
                {
                    ccLightProbeGroup._maxPos.z = probe.z;
                }
            }
            return new CCType[] { ccLightProbeGroup };
        }
    }
}
