from flask import Flask, request, jsonify, send_from_directory
import subprocess
import os
import uuid

app = Flask(__name__)

BLENDER_EXECUTABLE_PATH = "/Applications/Blender.app/Contents/MacOS/blender"
OUTPUT_FOLDER = "output"

os.makedirs(OUTPUT_FOLDER, exist_ok=True)


@app.route('/process-avatar', methods=['POST'])
def process_avatar():
    data = request.json
    if not data or 'url' not in data:
        return jsonify({"Error": "Missing 'url' in request body"}), 400
    
    original_url = data['url']
    
    # generate unique filenames
    unique_id = str(uuid.uuid4())
    original_filename = f"{unique_id}_original.glb"
    modified_filename = f"{unique_id}_child.glb"
    
    original_filepath = os.path.join(OUTPUT_FOLDER, original_filename)
    modified_filepath = os.path.join(OUTPUT_FOLDER, modified_filename)
    
    try:
        print(f"[Server] downloading {original_url}...")
        subprocess.run(
            ["python", "download_avatar.py", original_url, original_filepath],
            check=True
        )

        print(f"[Server] processing {original_filepath} with blender...")
        subprocess.run(
            [
                BLENDER_EXECUTABLE_PATH,
                "--background",
                "--python", "process_avatar.py",
                "--",
                original_filepath,
                modified_filepath
            ],
            check=True
        )
        
        new_avatar_url = request.host_url.rstrip('/') + f"/files/{modified_filename}"
        
        os.remove(original_filepath)
        
        return jsonify({"newUrl": new_avatar_url})

    except subprocess.CalledProcessError as e:
        print(f"[Server-Error] {e}")
        for filepath in [original_filepath, modified_filepath]:
            if os.path.exists(filepath):
                try:
                    os.remove(filepath)
                    print(f"[Server] cleaned up {filepath}")
                except Exception as cleanup_error:
                    print(f"[Server-Error] {cleanup_error}")
        
        return jsonify({"Error": "Pipeline failed during processing"}), 500
    except Exception as e:
        print(f"[Server-Error] {e}")
        for filepath in [original_filepath, modified_filepath]:
            if os.path.exists(filepath):
                try:
                    os.remove(filepath)
                    print(f"[Server] cleaned up {filepath}")
                except Exception as cleanup_error:
                    print(f"[Server-Error] {cleanup_error}")
        
        return jsonify({"Error": "Internal server error"}), 500


@app.route('/files/<path:filename>')
def get_file(filename):
    return send_from_directory(OUTPUT_FOLDER, filename)


@app.route('/delete-file/<path:filename>', methods=['DELETE'])
def delete_file(filename):
    filepath = os.path.join(OUTPUT_FOLDER, filename)
    
    try:
        if os.path.exists(filepath):
            os.remove(filepath)
            print(f"[Server] successfully deleted: {filepath}")
            return jsonify({"Message": f"File {filename} deleted successfully"}), 200
        else:
            print(f"[Server-Error] file not found for deletion: {filepath}")
            return jsonify({"Error": f"File {filename} not found"}), 404
            
    except Exception as e:
        print(f"[Server-Error] unable to delete {filepath}: {e}")
        return jsonify({"Error": f"Failed to delete file {filename}: {str(e)}"}), 500


@app.route('/list-files', methods=['GET'])
def list_files():
    try:
        files = os.listdir(OUTPUT_FOLDER)
        return jsonify({"Files": files}), 200
    except Exception as e:
        return jsonify({"Error": str(e)}), 500


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)