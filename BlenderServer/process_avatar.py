import bpy
import sys
import os

def adjust_proportions(armature, bone_name, scale_factor):
    """
    scales bone in edit mode
    """
    if bpy.context.object.mode != 'EDIT':
        bpy.ops.object.mode_set(mode='EDIT')
    
    bpy.ops.armature.select_all(action='DESELECT')
    bone = armature.data.edit_bones.get(bone_name)

    if bone:
        bone.select = True
        bone.select_head = True
        bone.select_tail = True
        bpy.ops.transform.resize(value=scale_factor)
    else:
        print(f"WARNING: Bone '{bone_name}' not found")

def run_pipeline(input_path, output_path):
    """
    imports, modifies armature, re-parents mesh, and exports
    """
    # --- 1. clean scene, import ---
    if bpy.data.objects:
        bpy.ops.object.select_all(action='SELECT')
        bpy.ops.object.delete()

    if not os.path.exists(input_path):
        print(f"ERROR: Input file not found at {input_path}")
        return
    bpy.ops.import_scene.gltf(filepath=input_path)
    print(f"Successfully imported {input_path}")

    # --- 2. find armature and mesh objects ---
    armature = None
    mesh_objects = []
    for obj in bpy.context.scene.objects:
        if obj.type == 'ARMATURE':
            armature = obj
        elif obj.type == 'MESH':
            mesh_objects.append(obj)
    
    if not armature or not mesh_objects:
        print("ERROR: Armature or mesh objects not found")
        return

    # --- 3. modify armature in edit mode ---
    bpy.context.view_layer.objects.active = armature
    bpy.ops.object.mode_set(mode='EDIT')

    head_scale = (1.3, 1.3, 1.3)
    shorten_scale = (0.65, 0.65, 0.65)
    bone_map = {
        "Head": head_scale, "RightUpLeg": shorten_scale, "LeftUpLeg": shorten_scale,
        "RightLeg": shorten_scale, "LeftLeg": shorten_scale, "RightArm": shorten_scale,
        "LeftArm": shorten_scale, "RightForeArm": shorten_scale, "LeftForeArm": shorten_scale,
        "Hips": shorten_scale, "Spine": shorten_scale, "Spine1": shorten_scale, "Spine2": shorten_scale,
    }

    print("Applying new proportions in Edit Mode...")
    for name, scale in bone_map.items():
        adjust_proportions(armature, name, scale)
    
    bpy.ops.object.mode_set(mode='OBJECT')

    # --- 4. re-parent mesh to new skeleton ---
    print("Re-calculating mesh skinning...")
    bpy.ops.object.select_all(action='DESELECT')

    for mesh_obj in mesh_objects:
        mesh_obj.select_set(True)

    bpy.ops.object.parent_clear(type='CLEAR_KEEP_TRANSFORM')

    for mesh_obj in mesh_objects:
        mesh_obj.select_set(True)
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    bpy.ops.object.parent_set(type='ARMATURE_AUTO')
    print("Mesh skinning recalculated successfully")

    # --- 5. export modified glb ---
    print(f"Exporting modified avatar to {output_path}...")
    bpy.ops.export_scene.gltf(
        filepath=output_path,
        export_format='GLB',
        export_apply=True 
    )
    print("Export complete")

if __name__ == "__main__":
    argv = sys.argv
    try:
        args = argv[argv.index("--") + 1:]
        input_file = args[0]
        output_file = args[1]
        run_pipeline(input_file, output_file)
    except (ValueError, IndexError):
        print("USAGE: blender --background --python your_script_name.py -- <input.glb> <output.glb>")