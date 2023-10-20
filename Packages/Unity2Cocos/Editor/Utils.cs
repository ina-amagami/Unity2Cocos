using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using cc;
using UnityEditor;

namespace Unity2Cocos
{
	public static class Utils
	{
		public static string NewUuid()
		{
			return Guid.NewGuid().ToString();
		}
		
		public static string GenerateBase64EncodedUuid()
		{
			var uuidBytes = Guid.NewGuid().ToByteArray();
			var base64EncodedUuid = Convert.ToBase64String(uuidBytes);
			return base64EncodedUuid.TrimEnd('=');
		}
		
		public static IEnumerable<Type> GetTypesIsSubclassOf<T>()
		{
			return Assembly.GetAssembly(typeof(T))
				.GetTypes()
				.Where(x => x.IsSubclassOf(typeof(T)) && !x.IsAbstract);
		}
		
		public static T GetAttribute<T>(Type type) where T : class
		{
			var attributes = type.GetCustomAttributes(typeof(T), false) as T[];
			if (attributes == null || attributes.Length == 0)
			{
				return null;
			}
			return attributes[0];
		}

		public static bool ConvertPathFormat => ExportSetting.Instance.ConvertPathFormat;
		
		public static string ConvertToOutputPathFormat(string input)
		{
			if (!ConvertPathFormat) return input;

#if UNITY_EDITOR_WIN
			var paths = input.Split('\\');
#else
			var paths = input.Split('/');
#endif
			input = string.Empty;
			for (var i = 0; i < paths.Length; ++i)
			{
				input += string.Concat(paths[i].Select(
					(x, i) => i > 0 && char.IsUpper(x) ? $"-{x.ToString()}" : x.ToString()));
				if (i < paths.Length - 1)
				{
					input += '/';
				}
			}
			return input.ToLower();
		}

		public static string GetTransformPath(Transform t)
		{
			List<string> names = new() { t.name };
			while (true)
			{
				if (!t.parent)
				{
					break;
				}
				var parent = t.parent;
				names.Add(parent.name);
				t = parent;
			}
			names.Reverse();
			return string.Concat(names.Select((s, i) => s + (i == names.Count - 1 ? "" : "/")));
		}

		public static bool ConvertToRightHanded => ExportSetting.Instance.ConvertToRightHanded;
		
		public static Vec3 Vector3ToVec3(Vector3 v)
		{
			return ConvertToRightHanded ? new Vec3 {
				x = v.x, y = v.y, z = -v.z
			} : new Vec3 {
				x = v.x, y = v.y, z = v.z
			};
		}
		
		public static Quat QuaternionToQuat(Quaternion q)
		{
			return ConvertToRightHanded ? new Quat {
				x = -q.x, y = -q.y, z = q.z, w = q.w
			} : new Quat {
				x = q.x, y = q.y, z = q.z, w = q.w
			};
		}
		
		public static Vec3 EulerAnglesToVec3(Vector3 ea)
		{
			return new Vec3 { x = -ea.x, y = -ea.y, z = ea.z };
		}
	}
}