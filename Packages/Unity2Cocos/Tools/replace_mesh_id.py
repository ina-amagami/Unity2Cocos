import os
import json
import re

def replace_uuid_with_id(scene_path, meta_files):
    with open(scene_path, 'r', encoding='utf-8') as f:
        content = f.read()

    matches = re.findall(r'([0-9a-fA-F-]{36})@([\w\d_-]+\.mesh)', content)

    for uuid, mesh_name in matches:
        for meta_file in meta_files:
            with open(meta_file, 'r', encoding='utf-8') as meta_f:
                meta_content = meta_f.read()
                if uuid in meta_content:
                    # UUIDが存在する場合、IDを探す
                    meta_data = json.loads(meta_content)
                    for key, value in meta_data["subMetas"].items():
                        if value["name"] == mesh_name:
                            id_ = key
                            # UUIDをUUID@IDに置き換える
                            content = content.replace(f"{uuid}@{mesh_name}", f"{uuid}@{id_}")

    with open(scene_path, 'w', encoding='utf-8') as f:
        f.write(content)

def main(folder_path):
    files = [os.path.join(dp, f) for dp, dn, filenames in os.walk(folder_path) for f in filenames]

    scene_files = [f for f in files if f.endswith(".scene")]
    meta_files = [f for f in files if f.endswith(".fbx.meta")]

    for scene_file in scene_files:
        replace_uuid_with_id(scene_file, meta_files)

if __name__ == "__main__":
    folder_path = input("Enter the folder path: ")
    main(folder_path)
