using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace cc
{
	public class Texture2D : Asset
	{
	}
}

namespace Unity2Cocos
{
	/* Unsupported features.
	 * 
	 * - Sprite
	 * - Texture Compression
	 * 
	 */
	[AssetExporter(typeof(Texture2D))]
	public class Texture2DExporter : AssetExporter<Texture2D>
	{
		private class SubMeta : cc.Meta
		{
			public string displayName;
			public string id;
			public string name;
			
			public class SubTextureUserData
			{
				public string wrapModeS;
				public string wrapModeT;
				public string minfilter;
				public string magfilter;
				public string mipfilter;
				public int anisotropy;
				public bool isUuid = true;
				public string imageUuidOrDatabaseUri;
			}
			
			public class NormalMapUserData : SubTextureUserData
			{
				public bool visible = false;
			}
			
			public SubMeta()
			{
				ver = "1.0.22";
				importer = "texture";
				files = new[]
				{
					".json"
				};
			}
		}
		
		private class UserData
		{
			public bool fixAlphaTransparencyArtifacts = true;
			public bool hasAlpha;
			public string type;
			public string redirect;
		}
		
		private class Meta : cc.Meta
		{
			public Meta()
			{
				ver = "1.0.26";
				importer = "image";
				files = new[]
				{
					".json",
					".png"
				};
			}
		}

		protected override string Extension => null;
		
		public override string Export(Texture2D asset)
		{
			var ccMeta = new Meta();

			var importer = AssetImporter.GetAtPath(Info.UnityAssetPath) as TextureImporter;
			if (importer == null)
			{
				Debug.LogError($"[Texture2DExporter] Missing importer. -> {asset.name}");
				return string.Empty;
			}

			var isNormalMap = importer.textureType switch
			{
				TextureImporterType.NormalMap => true,
				_ => false
			};
			
			// FIXME: Cocos uses a fixed value. The calculation method has not been guessed.
			var subAssetUuid = isNormalMap ? "3c318" : "6c48a";
			var fullUuid = $"{ccMeta.uuid}@{subAssetUuid}";

			var subMetas = new Dictionary<string, SubMeta>();
			var subMeta = new SubMeta()
			{
				uuid = fullUuid,
				displayName = System.IO.Path.GetFileNameWithoutExtension(Info.CocosAssetName),
				id = subAssetUuid,
				name = isNormalMap ? "normalMap" : "texture"
			};
			var subUserData = isNormalMap
				? new SubMeta.SubTextureUserData()
				: new SubMeta.NormalMapUserData();
			subUserData.wrapModeS = Utils.TextureWrapModeToCocos(importer.wrapModeU);
			subUserData.wrapModeT = Utils.TextureWrapModeToCocos(importer.wrapModeV);
			subUserData.minfilter = subUserData.magfilter = Utils.TextureFilterModeToCocos(importer.filterMode);
			subUserData.mipfilter = importer.mipmapEnabled ? Utils.TextureFilterModeToCocosMipFilter(importer.filterMode) : "none";
			subUserData.anisotropy = Utils.TextureAnisoToCocos(importer.anisoLevel);
			subUserData.imageUuidOrDatabaseUri = ccMeta.uuid;
			subMeta.userData = subUserData;
			subMetas.Add(subAssetUuid, subMeta);
			ccMeta.subMetas = subMetas;
			
			ccMeta.userData = new UserData()
			{
				hasAlpha = importer.DoesSourceTextureHaveAlpha(),
				type = importer.textureType switch
				{
					TextureImporterType.NormalMap => "normal map",
					// TODO: sprite-frame support.
					// TextureImporterType.Sprite => "sprite-frame",
					_ => "texture"
				},
				redirect = fullUuid
			};
			
			ExportAssetCopy();
			ExportMeta(ccMeta);
			
			return fullUuid;
		}
	}
}
