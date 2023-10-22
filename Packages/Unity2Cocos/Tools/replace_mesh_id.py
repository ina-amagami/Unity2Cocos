import os
import json
import re


def replace_uuids_in_scene(folder_path):
    # Gather all .scene and .fbx.meta files
    scene_files = [os.path.join(root, file) for root, _, files in os.walk(folder_path) for file in files if
                   file.endswith('.scene')]
    if len(scene_files) == 0:
        print("Not found scene.")
        return

    meta_files = [os.path.join(root, file) for root, _, files in os.walk(folder_path) for file in files if
                  file.endswith('.fbx.meta')]
    if len(meta_files) == 0:
        print("Not found fbx meta.")
        return

    uuid_mesh_name_pattern = re.compile(r'([a-f0-9\-]+)@mesh-name:([\w\-]+\.mesh)')
    uuid_mesh_triangles_pattern = re.compile(r'([a-f0-9\-]+)@mesh-triangles:(\d+)')

    # For each scene file
    for scene_file in scene_files:
        print("")
        print(f"Scene: {scene_file}")
        with open(scene_file, 'r', encoding='utf-8') as file:
            content = file.read()

            # Replace based on mesh name
            for match in uuid_mesh_name_pattern.findall(content):
                uuid, mesh_name = match
                content = replace_uuid(content, meta_files, uuid, mesh_name, 'name')

            # Replace based on triangle count
            for match in uuid_mesh_triangles_pattern.findall(content):
                uuid, triangle_count = match
                content = replace_uuid(content, meta_files, uuid, int(triangle_count), 'triangleCount')

        # Write the updated content back to the scene file
        with open(scene_file, 'w', encoding='utf-8') as file:
            file.write(content)


def replace_uuid(content, meta_files, uuid, value, search_key):
    # Find corresponding meta file by UUID
    corresponding_meta_file = None
    for meta_file in meta_files:
        with open(meta_file, 'r', encoding='utf-8') as mfile:
            meta_content = json.load(mfile)
            if meta_content.get('uuid') == uuid:
                corresponding_meta_file = meta_file
                break

    if not corresponding_meta_file:
        return content

    with open(corresponding_meta_file, 'r', encoding='utf-8') as mfile:
        meta_content = json.load(mfile)
        mesh_entries = [entry for entry_key, entry in meta_content.get('subMetas', {}).items() if entry.get('name', '').endswith('.mesh')]

        found_id = None
        # if len(mesh_entries) == 1:
        #     found_id = mesh_entries[0].get('id')
        # else:
        for entry in mesh_entries:
            if search_key == 'triangleCount':
                user_data = entry.get('userData')
                if user_data.get(search_key) == value:
                    found_id = entry.get('id')
                    break
            elif search_key == 'name':
                if entry.get(search_key) == value:
                    found_id = entry.get('id')
                    break

        if found_id:
            if search_key == 'name':
                print(f"- Replace: {uuid}@mesh-name:{value} -> {found_id}")
                content = content.replace(f"{uuid}@mesh-name:{value}", f"{uuid}@{found_id}")
            elif search_key == 'triangleCount':
                print(f"- Replace: {uuid}@mesh-triangles:{value} -> {found_id}")
                content = content.replace(f"{uuid}@mesh-triangles:{value}", f"{uuid}@{found_id}")
    return content


if __name__ == "__main__":
    folder_path = input("Enter the folder path: ")
    print("--- Replace mesh id start ---")
    replace_uuids_in_scene(folder_path)
    print("")
    print("--- Replace mesh id end ---")
