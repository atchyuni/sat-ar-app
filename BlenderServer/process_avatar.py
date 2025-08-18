import bpy
import sys
import os
from mathutils import Vector

def clear_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)

def import_glb(filepath):
    try:
        bpy.ops.import_scene.gltf(filepath=filepath)
        print(f"[Blender] successfully imported: {filepath}")
        return True
    except Exception as e:
        print(f"[Blender-Error] {e}")
        return False

def create_child_proportions():
    armature_obj = None
    meshes = []
    
    # 1. find armature & all child meshes
    for obj in bpy.context.scene.objects:
        if obj.type == 'ARMATURE':
            armature_obj = obj
        elif obj.type == 'MESH' and obj.parent and obj.parent.type == 'ARMATURE':
            meshes.append(obj)
    
    if not armature_obj:
        print("[Blender] no armature found")
        return False, False
    
    print(f"[Blender] found armature: {armature_obj.name}")
    print(f"[Blender] found {len(meshes)} mesh(es): {[m.name for m in meshes]}")
    
    # 2. set as active & go to pose mode
    bpy.context.view_layer.objects.active = armature_obj
    bpy.ops.object.mode_set(mode='POSE')
    
    bpy.ops.pose.select_all(action='SELECT')
    bpy.ops.pose.transforms_clear()
    
    # --- BONE SCALING INFO ---
    bone_scales = {
        'HeadTop_End': Vector((1.0, 1.0, 1.0)),
        'Head': Vector((1.0, 1.0, 1.0)),
        'LeftEye': Vector((1.0, 1.0, 1.0)),
        'RightEye': Vector((1.0, 1.0, 1.0)),
        'Neck': Vector((1.0, 0.7, 1.0)),
        'Neck1': Vector((1.0, 0.7, 1.0)),
        'Neck2': Vector((1.0, 0.7, 1.0)),
        'LeftArm': Vector((1.1, 0.8, 1.1)),
        'RightArm': Vector((1.1, 0.8, 1.1)),
        'LeftShoulder': Vector((0.8, 0.8, 0.8)),
        'RightShoulder': Vector((0.8, 0.8, 0.8)),
        'LeftForeArm': Vector((1.1, 0.8, 1.1)),
        'RightForeArm': Vector((1.1, 0.8, 1.1)),
        'LeftHand': Vector((0.8, 0.8, 0.8)),
        'RightHand': Vector((0.8, 0.8, 0.8)),
        'Spine': Vector((0.9, 0.7, 0.9)),
        'Spine1': Vector((0.9, 0.7, 0.9)),
        'Spine2': Vector((0.9, 0.7, 0.9)),
        'Hips': Vector((0.9, 0.9, 0.9)),
        'LeftUpLeg': Vector((0.9, 0.7, 0.9)),
        'RightUpLeg': Vector((0.9, 0.7, 0.9)),
        'LeftLeg': Vector((0.9, 0.7, 0.9)),
        'RightLeg': Vector((0.9, 0.7, 0.9)),
        'LeftFoot': Vector((0.9, 0.8, 0.9)),
        'RightFoot': Vector((0.9, 0.8, 0.9)),
        'LeftToeBase': Vector((0.9, 0.8, 0.9)),
        'RightToeBase': Vector((0.9, 0.8, 0.9)),
        'LeftToe_End': Vector((0.9, 0.8, 0.9)),
        'RightToe_End': Vector((0.9, 0.8, 0.9)),
        'LeftHandThumb1': Vector((0.8, 0.8, 0.8)),
        'LeftHandThumb2': Vector((0.8, 0.8, 0.8)),
        'LeftHandThumb3': Vector((0.8, 0.8, 0.8)),
        'LeftHandIndex1': Vector((0.8, 0.8, 0.8)),
        'LeftHandIndex2': Vector((0.8, 0.8, 0.8)),
        'LeftHandIndex3': Vector((0.8, 0.8, 0.8)),
        'LeftHandMiddle1': Vector((0.8, 0.8, 0.8)),
        'LeftHandMiddle2': Vector((0.8, 0.8, 0.8)),
        'LeftHandMiddle3': Vector((0.8, 0.8, 0.8)),
        'LeftHandRing1': Vector((0.8, 0.8, 0.8)),
        'LeftHandRing2': Vector((0.8, 0.8, 0.8)),
        'LeftHandRing3': Vector((0.8, 0.8, 0.8)),
        'LeftHandPinky1': Vector((0.8, 0.8, 0.8)),
        'LeftHandPinky2': Vector((0.8, 0.8, 0.8)),
        'LeftHandPinky3': Vector((0.8, 0.8, 0.8)),
        'RightHandThumb1': Vector((0.8, 0.8, 0.8)),
        'RightHandThumb2': Vector((0.8, 0.8, 0.8)),
        'RightHandThumb3': Vector((0.8, 0.8, 0.8)),
        'RightHandIndex1': Vector((0.8, 0.8, 0.8)),
        'RightHandIndex2': Vector((0.8, 0.8, 0.8)),
        'RightHandIndex3': Vector((0.8, 0.8, 0.8)),
        'RightHandMiddle1': Vector((0.8, 0.8, 0.8)),
        'RightHandMiddle2': Vector((0.8, 0.8, 0.8)),
        'RightHandMiddle3': Vector((0.8, 0.8, 0.8)),
        'RightHandRing1': Vector((0.8, 0.8, 0.8)),
        'RightHandRing2': Vector((0.8, 0.8, 0.8)),
        'RightHandRing3': Vector((0.8, 0.8, 0.8)),
        'RightHandPinky1': Vector((0.8, 0.8, 0.8)),
        'RightHandPinky2': Vector((0.8, 0.8, 0.8)),
        'RightHandPinky3': Vector((0.8, 0.8, 0.8)),
    }
    
    pose_bones = armature_obj.pose.bones
    
    bones_found = 0
    bones_scaled = 0
    
    # 3. disable scale inheritance for child bones
    bpy.ops.object.mode_set(mode='EDIT')
    for bone_name in bone_scales.keys():
        if bone_name in armature_obj.data.edit_bones:
            bones_found += 1
            parent_bone = armature_obj.data.edit_bones[bone_name]
            for child_bone in parent_bone.children:
                if child_bone.name in bone_scales:
                    child_bone.inherit_scale = 'NONE'
    
    bpy.ops.object.mode_set(mode='POSE')

    # 4. apply scaling
    for bone_name, scale_vector in bone_scales.items():
        if bone_name in pose_bones:
            pose_bone = pose_bones[bone_name]
            pose_bone.scale = scale_vector
            bones_scaled += 1
    
    print(f"[Blender] found {bones_found} / {len(bone_scales)} target bones")
    print(f"[Blender] successfully scaled {bones_scaled} bones")
    
    bpy.context.view_layer.update()
    return armature_obj, meshes

def store_blend_shapes(mesh_obj):
    blend_shape_data = {}
    
    if mesh_obj.data.shape_keys:
        shape_keys = mesh_obj.data.shape_keys
        
        # store each shape key data
        for key_block in shape_keys.key_blocks:
            key_data = []
            for i, vertex in enumerate(key_block.data):
                key_data.append(Vector(vertex.co))
            
            blend_shape_data[key_block.name] = {
                'data': key_data,
                'value': key_block.value,
                'slider_min': key_block.slider_min,
                'slider_max': key_block.slider_max,
                'mute': key_block.mute,
                'interpolation': key_block.interpolation,
                'relative_key': key_block.relative_key.name if key_block.relative_key else None
            }
    
    return blend_shape_data

def apply_blend_shapes_to_new_mesh(new_mesh_obj, blend_shape_data, original_mesh_data, deformed_mesh_data):
    if not blend_shape_data:
        return
    
    print(f"[Blender] restoring {len(blend_shape_data)} blend shapes to {new_mesh_obj.name}")
    
    # get deformation transformation for each vertex
    vertex_transformations = []
    original_vertices = original_mesh_data.vertices
    deformed_vertices = deformed_mesh_data.vertices
    
    for i in range(len(original_vertices)):
        if i < len(deformed_vertices):
            original_pos = Vector(original_vertices[i].co)
            deformed_pos = Vector(deformed_vertices[i].co)
            vertex_transformations.append((original_pos, deformed_pos))
        else:
            vertex_transformations.append((Vector((0,0,0)), Vector((0,0,0))))
    
    # create basis shape key
    if not new_mesh_obj.data.shape_keys:
        new_mesh_obj.shape_key_add(name='Basis')
    
    # add stored shape key
    for key_name, key_info in blend_shape_data.items():
        if key_name == 'Basis':
            continue
            
        new_key = new_mesh_obj.shape_key_add(name=key_name)
        
        # apply transformation to each vertex
        original_key_data = key_info['data']
        
        for i, vertex_transform in enumerate(vertex_transformations):
            if i >= len(new_key.data) or i >= len(original_key_data):
                break
                
            original_base_pos, deformed_base_pos = vertex_transform
            original_key_pos = original_key_data[i]
            
            displacement = original_key_pos - original_base_pos
            
            new_key_pos = deformed_base_pos + displacement
            
            new_key.data[i].co = new_key_pos
        
        # restore other properties
        new_key.value = key_info['value']
        new_key.slider_min = key_info['slider_min']
        new_key.slider_max = key_info['slider_max']
        new_key.mute = key_info['mute']
        new_key.interpolation = key_info['interpolation']

def automate_new_rest_pose(armature_obj, meshes):
    """
    duplicates meshes, applies armature modifier to lock posed shape,
    applies new rest pose, & re-parents new meshes with blend shapes preserved
    """
    if not armature_obj or not meshes:
        print("[Blender] armature or meshes not found")
        return

    # 1. store data from original meshes before deleting
    mesh_data_to_recreate = []
    for mesh_obj in meshes:
        original_mesh_data = mesh_obj.data.copy()
        
        blend_shapes = store_blend_shapes(mesh_obj)
        
        deformed_mesh_data = mesh_obj.evaluated_get(bpy.context.view_layer.depsgraph).data
        
        mesh_data_to_recreate.append({
            'name': mesh_obj.name,
            'data': deformed_mesh_data.copy(),
            'matrix_world': mesh_obj.matrix_world.copy(),
            'blend_shapes': blend_shapes,
            'original_mesh_data': original_mesh_data,
            'deformed_mesh_data': deformed_mesh_data.copy()
        })

    bpy.ops.object.mode_set(mode='OBJECT')
    bpy.ops.object.select_all(action='DESELECT')
    for mesh_obj in meshes:
        mesh_obj.select_set(True)
    bpy.ops.object.delete()

    # 2. recreate meshes from stored data
    # important: keep original naming for compatibility
    for data in mesh_data_to_recreate:
        new_mesh_obj = bpy.data.objects.new(data['name'], data['data'])
        bpy.context.collection.objects.link(new_mesh_obj)

        new_mesh_obj.matrix_world = data['matrix_world']

        # restore blend shapes before adding armature modifier
        apply_blend_shapes_to_new_mesh(
            new_mesh_obj, 
            data['blend_shapes'], 
            data['original_mesh_data'], 
            data['deformed_mesh_data']
        )

        armature_mod = new_mesh_obj.modifiers.new(name="Armature", type='ARMATURE')
        armature_mod.object = armature_obj

        # re-parent to armature
        new_mesh_obj.parent = armature_obj
        
        bpy.context.view_layer.objects.active = new_mesh_obj
        new_mesh_obj.select_set(True)
        
        # normalise vertex groups
        bpy.ops.object.vertex_group_normalize_all()
                
    # 3. set new pose as rest pose
    bpy.context.view_layer.objects.active = armature_obj
    bpy.ops.object.mode_set(mode='POSE')
    bpy.ops.pose.select_all(action='SELECT')
    bpy.ops.pose.armature_apply()

    print("[Blender] all models successfully created and rigged with preserved blend shapes")

def export_glb(output_filepath):    
    bpy.ops.object.mode_set(mode='OBJECT')
    bpy.ops.object.select_all(action='DESELECT')

    armature = None
    meshes = []
    
    # find armature & all meshes parented
    for obj in bpy.context.scene.objects:
        if obj.type == 'ARMATURE':
            armature = obj
            break
            
    for obj in bpy.context.scene.objects:
        if obj.type == 'MESH' and obj.parent == armature:
            meshes.append(obj)
            
    if not armature or not meshes:
        print("[Blender-Error] no armature or meshes found for export")
        return False

    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    for mesh in meshes:
        mesh.select_set(True)
    
    # log blend shape information
    total_blend_shapes = 0
    for mesh in meshes:
        if mesh.data.shape_keys:
            shape_count = len(mesh.data.shape_keys.key_blocks)
            total_blend_shapes += shape_count
            print(f"[Blender] {mesh.name} has {shape_count} shape keys")
        else:
            print(f"[Blender] {mesh.name} has no shape keys")
    
    print(f"[Blender] total blend shapes to export: {total_blend_shapes}")
    
    try:
        bpy.ops.export_scene.gltf(
            filepath=output_filepath, 
            export_format='GLB', 
            use_selection=True,
            export_apply=True,
            export_morph=True
        )
        print(f"[Blender] export successful: {output_filepath}")
        return True
    except Exception as e:
        print(f"[Blender-Error] export failed: {e}")
        return False

def main():
    if len(sys.argv) < 3:
        print("[Blender] usage: blender --background --python process_avatar.py -- <input.glb> <output.glb>")
        sys.exit(1)
    
    argv = sys.argv
    
    args = argv[argv.index("--") + 1:]
    
    if len(args) < 2:
        print("[Blender-Error] need input and output file paths")
        sys.exit(1)
    
    input_filepath = args[0]
    output_filepath = args[1]
    
    clear_scene()
    
    # 1. import glb file
    if not import_glb(input_filepath):
        print("[Blender-Error] failed to import .glb")
        sys.exit(1)
    
    # 2. apply child proportions
    armature, meshes = create_child_proportions()
    if not armature or not meshes:
        print("[Blender-Error] failed to apply child proportions")
        sys.exit(1)
    
    # 3. create new rest pose with preserved blend shapes
    automate_new_rest_pose(armature, meshes)
    
    # 4. export glb file
    if not export_glb(output_filepath):
        print("[Blender-Error] failed to export .glb")
        sys.exit(1)
    
    print(f"[Blender] processed {input_filepath} to {output_filepath}")

if __name__ == "__main__":
    main()