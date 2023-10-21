using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using cc;
using UnityEditor;
using UnityEngine.Tilemaps;

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
		
		// FIXME: Cocos sub-asset UUID generation rules are unknown.
		public static string NewSubAssetUuid()
		{
			return Guid.NewGuid().ToString().Substring(0, 5);
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

		public static bool ConvertPathFormat => ExportSetting.Instance.ExportWebLikePaths;
		
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
				var path = paths[i];
				input += string.Concat(path.Select(
					(x, charIdx) =>
					{
						if (x.Equals('_'))
						{
							return "-";
						}
						if (charIdx > 0 && char.IsUpper(x) &&
						    !char.IsUpper(path[charIdx - 1]))
						{
							return $"-{x}";
						}
						return x.ToString();
					}));
				if (i < paths.Length - 1)
				{
					input += '/';
				}
			}
			return input.Replace("--", "-").ToLower();
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

		public static bool ConvertToRightHanded => ExportSetting.Instance.Advanced.ConvertToRightHanded;
		
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
		
		public static cc.Color Color32ToCocosColor(Color32 color)
		{
			return new() { r = color.r, g = color.g, b = color.b, a = color.a };
		}

		public static string TextureWrapModeToCocos(TextureWrapMode mode)
		{
			return mode switch
			{
				TextureWrapMode.Repeat => "repeat",
				TextureWrapMode.Clamp => "clamp-to-edge",
				TextureWrapMode.Mirror => "mirrored-repeat",
				// NOTE: Cocos does not support MirrorOnce.
				TextureWrapMode.MirrorOnce => "mirrored-repeat",
				_ => "repeat"
			};
		}

		public static string TextureFilterModeToCocos(FilterMode mode)
		{
			return mode switch
			{
				FilterMode.Point => "nearest",
				FilterMode.Bilinear => "linear",
				FilterMode.Trilinear => "linear",
				_ => "nearest"
			};
		}

		public static string TextureFilterModeToCocosMipFilter(FilterMode mode)
		{
			return mode switch
			{
				FilterMode.Point => "none",
				FilterMode.Bilinear => "nearest",
				FilterMode.Trilinear => "linear",
				_ => "none"
			};
		}
		
		public static int TextureAnisoToCocos(int anisoLevel)
		{
			var shift = ExportSetting.Instance.Advanced.TextureAnisoLevelShift;
			return Mathf.Max(anisoLevel + shift, 0);
		}
	}
}
