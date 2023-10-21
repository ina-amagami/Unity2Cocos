using System;
using cc;

namespace Unity2Cocos
{
	[AttributeUsage(AttributeTargets.Class)]
	public class MaterialConverterAttribute : Attribute
	{
		public string Shader { get; }

		public MaterialConverterAttribute(string shader)
		{
			Shader = shader;
		}
	}

	public abstract class MaterialConverter
	{
		public abstract Material Convert(UnityEngine.Material material);

		public static Material CreateMaterial(string effectUuid, int passCount)
		{
			return new Material
			{
				_name = "",
				_effectAsset = new(effectUuid),
				_defines = new MaterialDefine[passCount],
				_states = new MaterialState[passCount],
				_props = new MaterialProp[passCount]
			};
		}
	}

	public abstract class StandardMaterialConverter : MaterialConverter
	{
		public const string CocosStandardShaderUuid = "c8f66d17-351a-48da-a12c-0212d28575c4";
		
		protected class StandardMaterialDefine : MaterialDefine
		{
			public bool USE_ALBEDO_MAP;
		}

		protected class StandardMaterialProp : MaterialProp
		{
			public AssetReference<cc.Texture2D> mainTexture;
			public Vec4 tilingOffset = new() { x = 1, y = 1, z = 0, w = 0 };
			public Color mainColor = Color.White;
		}
		
		public static Material GetStandardMaterial(UnityEngine.Material material)
		{
			var ccMat = CreateMaterial(CocosStandardShaderUuid, 6);

			var define = new StandardMaterialDefine();
			define.USE_ALBEDO_MAP = material.mainTexture;
			ccMat._defines[0] = define;
			for (var i = 1; i < ccMat._defines.Length; ++i)
			{
				ccMat._defines[i] = new MaterialDefine();
			}

			for (var i = 0; i < ccMat._states.Length; ++i)
			{
				var state = new MaterialState();
				state.blendState.targets[0] = new object();
				ccMat._states[i] = state;
			}
			
			var prop = new StandardMaterialProp();
			if (material.mainTexture)
			{
				prop.mainTexture = new AssetReference<Texture2D>(Exporter.GetUuidOrExportAsset(material.mainTexture));
				prop.tilingOffset = new()
				{
					x = material.mainTextureScale.x,
					y = material.mainTextureScale.y,
					z = material.mainTextureOffset.x,
					w = material.mainTextureOffset.y
				};
			}
			prop.mainColor = Utils.Color32ToCocosColor(material.color);
			ccMat._props[0] = prop;
			for (var i = 1; i < ccMat._props.Length; ++i)
			{
				ccMat._props[i] = new MaterialProp();
			}
			
			return ccMat;
		}
	}
}
