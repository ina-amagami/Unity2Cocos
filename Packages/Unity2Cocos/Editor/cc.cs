using System;
using System.Collections.Generic;

namespace cc
{
	public class Meta
	{
		public string ver;
		public string importer;
		public bool imported = true;
		public string uuid;
		public string[] files = {".json" };
		public object subMetas = new();
		public object userData = new();

		protected Meta()
		{
			uuid = Unity2Cocos.Utils.NewUuid();
		}
	}
	
	public class SceneNodeId
	{
		public int __id__;
		public SceneNodeId(int id)
		{
			__id__ = id;
		}
	}
	
	public class SceneNodeIdReplaceable : SceneNodeId
	{
		[NonSerialized]
		public readonly UnityEngine.Component TargetUnityComponent;
		
		public SceneNodeIdReplaceable(UnityEngine.Component component) : base(0)
		{
			TargetUnityComponent = component;
			Unity2Cocos.Converter.AddSceneNodeIdReplaceable(this);
		}
	}

	public abstract class CCType
	{
		public string __type__;
		protected CCType()
		{
			__type__ = GetType().FullName;
		}
	}
	
	public class Vec2 : CCType
	{
		public float x;
		public float y;
		
		public static Vec2 Zero => new() { x = 0, y = 0 };
	}
	
	public class Vec3 : CCType
	{
		public float x;
		public float y;
		public float z;

		public static Vec3 Zero => new() { x = 0, y = 0, z = 0 };
	}
	
	public class Vec4 : CCType
	{
		public float x;
		public float y;
		public float z;
		public float w;

		public static Vec4 Zero => new() { x = 0, y = 0, z = 0, w = 0 };
	}
	
	public class Quat : CCType
	{
		public float x;
		public float y;
		public float z;
		public float w;
		
		public static Quat Identity => new() { x = 0, y = 0, z = 0, w = 1 };
	}
	
	public class Color : CCType
	{
		public byte r;
		public byte g;
		public byte b;
		public byte a;
		
		public static Color Zero => new() { r = 0, g = 0, b = 0, a = 0 };
		public static Color White => new() { r = 255, g = 255, b = 255, a = 255 };
	}
	
	public class Rect : CCType
	{
		public float x;
		public float y;
		public float width;
		public float height;
	}

	public class CCObject : CCType
	{
		public string _name;
		public uint _objFlags;
		public object __editorExtras__ = new();
	}
	
	public class BaseNode : CCObject
	{
		public string _id;
		protected BaseNode()
		{
			_id = Unity2Cocos.Utils.GenerateBase64EncodedUuid();
		}
	}
	
	public abstract class Asset : CCObject
	{
	}
	
	public class AssetReference
	{
		public string __uuid__;
		public string __expectedType__;

		public AssetReference(string uuid)
		{
			__uuid__ = uuid;
		}
	}
	
	public class AssetReference<TAsset> : AssetReference where TAsset : Asset
	{
		public AssetReference(string uuid) : base(uuid)
		{
			__expectedType__ = typeof(TAsset).FullName;
		}
	}
	
	public class Node : BaseNode
	{
		public SceneNodeId _parent = null;
		public List<SceneNodeId> _children = new();
		public bool _active = true;
		public List<SceneNodeId> _components = new();
		public string _prefab = null;
		public Vec3 _lpos;
		public Quat _lrot;
		public Vec3 _lscale;
		public int _mobility;
		public int _layer = 1073741824;
		public Vec3 _euler = new();
	}

	public abstract class Component : BaseNode
	{
		public SceneNodeId node;
		public bool _enabled = true;
		public string __prefab = null;
	}
}
