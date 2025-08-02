# process_avatar: main modifications
import bpy
import sys
import os

def adjust_proportions(bone_name, scale_factor):
    """
    selects bone (and its head/tail) in edit mode for scaling
    """
    if bpy.context.object.mode != 'EDIT':
        bpy.ops.object.mode_set(mode='EDIT')
    
    bpy.ops.armature.select_all(action='DESELECT')

    # find bone in armature's edit bones collection
    bone = bpy.context.object.data.edit_bones.get(bone_name)

    if bone:
        bone.select = True
        bone.select_head = True
        bone.select_tail = True
        bpy.ops.transform.resize(value=scale_factor)
    else:
        print(f"warning: bone '{bone_name}' not found in armature")

def run_pipeline(input_path, output_path):
    """
    imports, modifies, and exports avatar
    """
    # --- 1. clean scene ---
    if bpy.data.objects:
        bpy.ops.object.select_all(action='SELECT')
        bpy.ops.object.delete()

    # --- 2. import glb file ---
    if not os.path.exists(input_path):
        print(f"error: input file not found at {input_path}")
        return
    bpy.ops.import_scene.gltf(filepath=input_path)
    print(f"successfully imported {input_path}")

    # --- 3. modify armature ---
    armature = None
    for obj in bpy.context.scene.objects:
        if obj.type == 'ARMATURE':
            armature = obj
            break

    if not armature:
        print("error: no armature found in imported scene")
        return

    # set armature as active object
    bpy.context.view_layer.objects.active = armature
    bpy.ops.object.mode_set(mode='EDIT')

    # --- fixed proportions ---
    head_scale = (1.3, 1.3, 1.3)
    shorten_scale = (0.65, 0.65, 0.65)

    bone_map = {
        # head
        "Head": head_scale,

        # leg components
        "RightUpLeg": shorten_scale,
        "LeftUpLeg": shorten_scale,
        "RightLeg": shorten_scale,
        "LeftLeg": shorten_scale,

        # arm components
        "RightArm": shorten_scale,
        "LeftArm": shorten_scale,
        "RightForeArm": shorten_scale,
        "LeftForeArm": shorten_scale,

        # torso components
        "Hips": shorten_scale,
        "Spine": shorten_scale,
        "Spine1": shorten_scale,
        "Spine2": shorten_scale,
    }

    print("applying new proportions...")
    for name, scale in bone_map.items():
        adjust_proportions(name, scale)

    # return to object mode
    bpy.ops.object.mode_set(mode='OBJECT')

    # --- 4. export modified glb ---
    print(f"exporting modified avatar to {output_path}...")
    bpy.ops.export_scene.gltf(
        filepath=output_path,
        export_format='GLB',
        use_selection=False
    )
    print("export complete")

if __name__ == "__main__":
    argv = sys.argv
    try:
        args = argv[argv.index("--") + 1:]
        input_file = args[0]
        output_file = args[1]
        run_pipeline(input_file, output_file)
    except (ValueError, IndexError):
        print("USAGE: blender --background --python process_avatar.py -- <input.glb> <output.glb>")