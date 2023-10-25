using System.Collections.Generic;
using System.Linq;
using cc;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace cc
{
	public class Camera : Component
	{
		public int _projection;
		public int _priority;
		public float _fov;
		public int _fovAxis;
		public float _orthoHeight;
		public float _near;
		public float _far;
		public cc.Color _color;
		public float _depth = 1;
		public float _stencil = 0;
		public int _clearFlags;
		public cc.Rect _rect;
		public float _aperture = 19;
		public float _shutter = 7;
		public int _iso = 0;
		public float _screenScale;
		public int _visibility = 1073741824;
		public cc.Texture2D _targetTexture = null;
		public SceneNodeIdReplaceable _postProcess = null;
		public bool _usePostProcess;
		public int _cameraType = -1;
		public int _trackingType;
	}
}

namespace Unity2Cocos
{
	[ComponentConverter(typeof(UnityEngine.Camera))]
	public class CameraConverter : ComponentConverter<UnityEngine.Camera>
	{
		protected override IEnumerable<CCType> Convert(UnityEngine.Camera component, int currentId)
		{
			var additionalData = component.GetUniversalAdditionalCameraData();
			var globalVolume = Object.FindObjectsOfType<Volume>().FirstOrDefault(x => x.isGlobal);
			var ccCamera = new cc.Camera()
			{
				_enabled = component.enabled,
				node = new SceneNodeId(currentId),
				_projection = component.orthographic ? 0 : 1,
				_priority = (int)component.depth,
				// _fovAxis = Unknown how to get FovAxis in Unity.
				_fov = component.fieldOfView,
				_orthoHeight = component.orthographicSize,
				_near = component.nearClipPlane,
				_far = component.farClipPlane,
				_color = Utils.Color32ToCocosColor(component.backgroundColor),
				_clearFlags = Utils.CameraClearFlagsToCocos(component.clearFlags),
				_rect = Utils.RectToCocosRect(component.rect),
				_postProcess = globalVolume ? new SceneNodeIdReplaceable(globalVolume) : null,
			};
			
			// TODO: Support culling mask.
			ccCamera._visibility = 1 << Utils.LayerConvert(0);
				
			if (component.usePhysicalProperties)
			{
				// TODO: PhysicalProperties convert Cocos enums.
				// ccCamera._aperture = component.aperture;
				// NOTE: ShutterSpeed in Cocos is enum and not fully convertible.
				// Adjust the brightness of the lights to match?
				// ccCamera._shutter = component.shutterSpeed;
				// ccCamera._iso = Utils.CameraISOToCocos(component.iso);
			}
			
			if (additionalData)
			{
				ccCamera._usePostProcess = additionalData.renderPostProcessing;
			}
			
			return new CCType[] { ccCamera };
		}
	}
}
