using System.Collections.Generic;
using cc;
using UnityEditor;
using UnityEngine;

namespace cc
{
	public class Script : Component
	{
	}
}

namespace Unity2Cocos
{
	/// <summary>
	/// Does not support script conversion, but allows mapping of assets.
	/// </summary>
	[ComponentConverter(typeof(MonoBehaviour))]
	public class MonoBehaviourConverter : ComponentConverter<MonoBehaviour>
	{
		private readonly Dictionary<string, string> _scriptMap = new();

		public void Initialize(List<ScriptMapper> mappers)
		{
			_scriptMap.Clear();
			foreach (var mapper in mappers)
			{
				if (!mapper)
				{
					continue;
				}
				foreach (var mapping in mapper.Mappings)
				{
					AddScriptMap(mapping.Script, mapping.CocosCompressedUUID);
				}
			}
		}

		private void AddScriptMap(MonoScript script, string ccCompressedUuid)
		{
			var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(script));
			if (string.IsNullOrEmpty(guid) || _scriptMap.ContainsKey(guid))
			{
				return;
			}
			_scriptMap.Add(guid, ccCompressedUuid);
		}
		
		private string GetMappedCompressedUuid(MonoBehaviour monoBehaviour)
		{
			var monoScript = MonoScript.FromMonoBehaviour(monoBehaviour);
			var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(monoScript));
			if (string.IsNullOrEmpty(guid))
			{
				return null;
			}
			return _scriptMap.TryGetValue(guid, out var compressedUuid) ? compressedUuid : null;
		}
		
		protected override IEnumerable<CCType> Convert(MonoBehaviour component, int currentId)
		{
			var compressedUuid = GetMappedCompressedUuid(component);
			if (string.IsNullOrEmpty(compressedUuid))
			{
				var path = Utils.GetTransformPath(component.transform);
				Debug.LogWarning(
					$"[MonoBehaviourConverter] Skipped of unsupported script. -> {path}<{component.GetType().Name}> \n" +
					"To migrate, create a TypeScript in Cocos and set up ScriptMapper");
				return System.Array.Empty<CCType>();
			}
			return new CCType[] { new Script()
			{
				__type__ = compressedUuid
			}};
		}
	}
}
