using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity2Cocos
{
	public static class FBXExporter
	{
		private class MeshSubMeta : cc.Meta
		{
			public string displayName = "";
			public string id;
			public string name;

			public class UserData
			{
				public int gltfIndex;
				public int triangleCount;
			}
			
			public MeshSubMeta()
			{
				ver = "1.1.1";
				importer = "gltf-mesh";
				files = new[]
				{
					".bin",
					".json"
				};
			}
		}
		
		private class Meta : cc.Meta
		{
			public class UserData
			{
				public Dictionary<string, bool> fbx = new()
				{
					{ "smartMaterialEnabled", false }
				};
				// public string redirect;
				public Dictionary<string, List<string>> assetFinder = new();
			}
			
			public Meta()
			{
				ver = "2.3.8";
				importer = "fbx";
			}
		}
		
		public static string Export(AssetExporter.ExportInfo info, Object source)
		{
			var ccMeta = new Meta();
			var subMetas = new Dictionary<string, object>();
			var userData = new Meta.UserData();
			ccMeta.subMetas = subMetas;
			ccMeta.userData = userData;
			
			var result = string.Empty;
			var meshes = AssetDatabase.LoadAllAssetsAtPath(info.UnityAssetPath).OfType<Mesh>().ToArray();
			for (var i = 0; i < meshes.Length; ++i)
			{
				var mesh = meshes[i];
				var subMeta = new MeshSubMeta();
				
				// var subUserData = new MeshSubMeta.UserData();
				// subUserData.gltfIndex = i;
				// subUserData.triangleCount = mesh.triangles.Length / 3;
				// subMeta.userData = subUserData;
				// subMeta.name = mesh.name + ".mesh";
				// subMeta.id = "xxxxx";
				// subMeta.uuid = $"{ccMeta.uuid}@{subMeta.id}";
				
				// FIXME: Originally, a 5-digit hex value is assigned after @ by Cocos,
				// but due to the unknown calculation method,
				// the mesh name is assigned and resolved later by executing a Python script.
				subMeta.uuid = $"{ccMeta.uuid}@{mesh.name}.mesh";
				
				// if (!userData.assetFinder.TryGetValue("meshes", out var list))
				// {
				// 	list = new List<string>();
				// 	userData.assetFinder.Add("meshes", list);
				// }
				// list.Add(subMeta.uuid);
				
				if (mesh.Equals(source))
				{
					result = subMeta.uuid;
				}
				Exporter.AddAssetMap(mesh, subMeta.uuid);
				
				// subMetas.Add(subMeta.id, subMeta);
			}
			
			AssetExporter.ExportAssetCopy(info);
			AssetExporter.ExportMeta(ccMeta, info);
			
			return result;
		}
	}
}
