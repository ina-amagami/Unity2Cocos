# Unity 2 Cocos

Export 3D scenes and assets created in Unity to CocosCreator.

⚠️ This is an experimental project. ⚠️  
Make a backup of your project if you use it.

## Under development

- Some meshes are not placed in the correct position.
- Support more material property convert.
- Support more 3D feature. (Camera / Light / Reflection Probe / Collider / Rigidbody etc)
- TypeScript only generate and set property. (logic is empty)

## Caution

The following will not be supported.

- ParticleSystem / 2D Feature / uGUI convert.
- Prefab / Audio / Animation export.

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
- Cocos Creator 3.8

## Package License

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php

Copyright (c) 2023 ina-amagami / Amagamina Games, Inc. (ina@amagamina.jp)

The UniversalRenderPipeline sample scenes included in the repository were created by Unity Software Inc.
