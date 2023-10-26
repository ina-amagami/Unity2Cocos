using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using cc;
using UnityEditor;

namespace Unity2Cocos
{
	public static class Converter
	{
		private static readonly Dictionary<Type, ComponentConverter> _componentConverters = new();
		private static readonly Dictionary<string, MaterialConverter> _materialConverters = new();
		private static readonly List<SceneNodeIdReplaceable> _sceneNodeIdReplaceableList = new();
		private static readonly Dictionary<int, int> _unityComponentToNodeId = new();
		private static readonly Dictionary<int, Vector3> _meshDefaultPositions = new();
		private static MonoBehaviourConverter _monoBehaviourConverter;

		public static void Initialize()
		{
			_sceneNodeIdReplaceableList.Clear();
			_unityComponentToNodeId.Clear();
			_meshDefaultPositions.Clear();
			
			// Component Converter
			_componentConverters.Clear();
			var converters = Utils.GetTypesIsSubclassOf<ComponentConverter>();
			foreach (var converter in converters)
			{
				var attribute = Utils.GetAttribute<ComponentConverterAttribute>(converter);
				if (attribute == null)
				{
					Debug.LogError($"[ComponentConverter] ComponentConverterAttribute is not assigned. -> {converter.Name}");
					continue;
				}
				_componentConverters.Add(attribute.Type, (ComponentConverter)Activator.CreateInstance(converter));
			}
			if (_componentConverters.TryGetValue(typeof(MonoBehaviour), out var monoBehaviourConverter))
			{
				_monoBehaviourConverter = monoBehaviourConverter as MonoBehaviourConverter;
				_monoBehaviourConverter?.Initialize(ExportSetting.Instance.ScriptMapper);
			}
			
			// Material Converter
			_materialConverters.Clear();
			converters = Utils.GetTypesIsSubclassOf<MaterialConverter>();
			foreach (var converter in converters)
			{
				var attribute = Utils.GetAttribute<MaterialConverterAttribute>(converter);
				if (attribute == null)
				{
					Debug.LogError($"[ComponentConverter] ComponentConverterAttribute is not assigned. -> {converter.Name}");
					continue;
				}
				_materialConverters.Add(attribute.Shader, (MaterialConverter)Activator.CreateInstance(converter));
			}
		}
		
		private static bool IsRightHanded => ExportSetting.Instance.Advanced.ConvertToRightHanded;

		public static void ConvertHierarchy(Transform root, List<CCType> list)
		{
			ConvertTransformAndChildren(1, root, list);
		}

		private static void ConvertTransformAndChildren(int parent, Transform t, List<CCType> list)
		{
			var nodeId = list.Count;
			var node = TransformToNode(t);
			node._parent = new SceneNodeId(parent);
			AddUnityComponentToNodeIdCache(t, list.Count);
			list.Add(node);

			var transformPath = Utils.GetTransformPath(t);
			var components = t.GetComponents<UnityEngine.Component>();
			foreach (var component in components)
			{
				if (component is Transform)
				{
					continue;
				}

				var type = component.GetType();
				if (!_componentConverters.TryGetValue(type, out var converter))
				{
					if (!type.IsSubclassOf(typeof(MonoBehaviour)))
					{
						Debug.LogWarning(
							$"[Converter] Skipped of unsupported component. -> {transformPath}<{type.Name}>");
						continue;
					}
					converter = _monoBehaviourConverter;
				}

				var results = converter.ConvertExecute(component, list.Count);
				var ccTypes = results as CCType[] ?? results.ToArray();
				if (!ccTypes.Any()) continue;
				
				AddUnityComponentToNodeIdCache(component, list.Count);
				foreach (var result in ccTypes)
				{
					if (result is cc.Component ccComponent)
					{
						ccComponent.node = new SceneNodeId(nodeId);
						node._components.Add(new SceneNodeId(list.Count));
					}

					list.Add(result);
				}
			}

			for (var i = 0; i < t.childCount; ++i)
			{
				node._children.Add(new SceneNodeId(list.Count));
				ConvertTransformAndChildren(nodeId, t.GetChild(i), list);
			}
		}
		
		private static Node TransformToNode(Transform t)
		{
			var p = t.localPosition;
			var r = t.localRotation;
			if (IsRightHanded)
			{
				p.z = -p.z;
				r = new Quaternion(-r.x, -r.y, r.z, r.w);
			}
			if (t.TryGetComponent<UnityEngine.MeshFilter>(out var meshFilter))
			{
				// In Cocos, meshes below FBX have a value of 0.
				// Instantiate Mesh, check initial coordinates, and take diff.
				var hash = meshFilter.sharedMesh.GetHashCode();
				if (!_meshDefaultPositions.TryGetValue(hash, out var defaultPos))
				{
					var assetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
					if (Path.GetExtension(assetPath) == ".fbx")
					{
						var root = AssetDatabase.LoadMainAssetAtPath(assetPath) as GameObject;
						if (root)
						{
							var obj = GameObject.Instantiate(root);
							defaultPos = obj.GetComponentsInChildren<MeshFilter>()
								.FirstOrDefault(x => x.sharedMesh.Equals(meshFilter.sharedMesh))!
								.transform.localPosition;
							GameObject.DestroyImmediate(obj);
						}
					}
					_meshDefaultPositions.Add(hash, defaultPos);
				}
				p = t.localPosition;
				p -= defaultPos;
				if (IsRightHanded)
				{
					p.z = -p.z;
					
					// BUG: Doesn't work correctly when nested mesh.
					r *= Quaternion.AngleAxis(180f, Vector3.up);
				}
			}
			return new Node
			{
				_name = t.name,
				_active = t.gameObject.activeSelf,
				_lpos = Utils.Vector3ToVec3(p),
				_lrot = Utils.QuaternionToQuat(r),
				_lscale = new Vec3 { x = t.localScale.x, y = t.localScale.y, z = t.localScale.z },
				_mobility = t.gameObject.isStatic ? 0 : 2,
				_euler = Utils.EulerAnglesToVec3(r.eulerAngles),
				_layer = 1 << Utils.LayerConvert(t.gameObject.layer)
			};
		}

		public static void AddSceneNodeIdReplaceable(SceneNodeIdReplaceable replaceable)
		{
			_sceneNodeIdReplaceableList.Add(replaceable);
		}

		private static void AddUnityComponentToNodeIdCache(UnityEngine.Component component, int id)
		{
			var hash = component.GetHashCode();
			if (_unityComponentToNodeId.ContainsKey(hash))
			{
				return;
			}
			_unityComponentToNodeId.Add(hash, id);
		}
		
		public static void ApplySceneNodeIdReplaceable()
		{
			foreach (var replaceable in _sceneNodeIdReplaceableList)
			{
				if (_unityComponentToNodeId.TryGetValue(replaceable.TargetUnityComponent.GetHashCode(), out var id))
				{
					replaceable.__id__ = id;
				}
			}
			_sceneNodeIdReplaceableList.Clear();
		}

		public static cc.Material ConvertMaterial(UnityEngine.Material material)
		{
			var shader = material.shader.name;
			if (_materialConverters.TryGetValue(shader, out var converter))
			{
				return converter.Convert(material);
			}
			Debug.LogWarning(
				$"[Material] Unsupported shader, export standard material. -> {material.name}<{shader}>");
			return StandardMaterialConverter.GetStandardMaterial(material);
		}
	}
}
