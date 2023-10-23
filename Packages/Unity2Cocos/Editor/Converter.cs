using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using cc;
using UnityEditor;
using MeshRenderer = UnityEngine.MeshRenderer;

namespace Unity2Cocos
{
	public static class Converter
	{
		private static readonly Dictionary<Type, ComponentConverter> _componentConverters = new();
		private static readonly Dictionary<string, MaterialConverter> _materialConverters = new();

		public static void Initialize()
		{
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
			var id = list.Count;
			var node = TransformToNode(t);
			node._parent = new SceneNodeId(parent);
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
					Debug.LogWarning(
						$"[Converter] Skipped of unsupported component. -> {transformPath}<{type.Name}>");
					continue;
				}

				var results = converter.ConvertExecute(component, list.Count);
				foreach (var result in results)
				{
					if (result is cc.Component ccComponent)
					{
						ccComponent.node = new SceneNodeId(id);
						node._components.Add(new SceneNodeId(list.Count));
					}

					list.Add(result);
				}
			}

			for (var i = 0; i < t.childCount; ++i)
			{
				node._children.Add(new SceneNodeId(list.Count));
				ConvertTransformAndChildren(id, t.GetChild(i), list);
			}
		}
		
		private static readonly Dictionary<int, Vector3> _meshDefaultPositions = new();

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
			};
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
