# Unity 2 Cocos

![](./Documents/unity2cocos.png)

Export 3D scene and assets in Unity to Cocos Creator.

[Demo](https://amagamina.jp/unity-2-cocos-demo/)

## Feature

- From Unity's left-hand scene to Cocos' right-hand scene.

- Image, FBX meshes copy and auto-generate meta files.

- Support URP material to Cocos material convert. ( If you are using Built-In pipeline, you can convert to URP with the Unity's support. )

- Map Unity's Built-In assets to Cocos' Built-In assets. (ex. Unity's Cube mesh -> Cocos' Box mesh)

- Map MonoBehaviour script to Cocos' TypeScript. (Property migration not supported.)

- Format directory paths to be web-like.

## Under development

- Support 3D physics feature. (Collider / Rigidbody etc)

## ⚠️ Caution ⚠️
This is an experimental project. Complete conversion is not possible.  
Make a backup of your project if you use it.

The following will not be supported. We need your help!

- Prefab support. (Very complicated...)

- ParticleSystem / 2D Feature / uGUI convert.

- Audio / Animation export.

## Install

To install via upm, specify `https://github.com/ina-amagami/Cocos2Unity.git?path=Packages/Unity2Cocos`.

```manifest.json
{
  "dependencies": {
    "jp.amagamina.unity-2-cocos": "https://github.com/ina-amagami/Unity2Cocos.git?path=Packages/Unity2Cocos",
  }
}
```

## Setup: WIP

## Verified

- Unity 2022.3.11f1 / URP 14.0.9

- Cocos Creator 3.8.0

## Package License

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php

Copyright (c) 2023 ina-amagami / Amagamina Games, Inc. (ina@amagamina.jp)

The UniversalRenderPipeline sample scenes included in the repository were created by Unity Software Inc.
