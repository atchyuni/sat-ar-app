# flask web server
from flask import Flask, request, jsonify, send_from_directory
import subprocess
import os
import uuid

app = Flask(__name__)

BLENDER_EXECUTABLE_PATH = "/Applications/Blender.app/Contents/MacOS/blender" # MacOS-specific
OUTPUT_FOLDER = "output"

os.makedirs(OUTPUT_FOLDER, exist_ok=True)

@app.route('/process-avatar', methods=['POST'])
def process_avatar():
    # --- 1. CLEANUP ---
    print("clearing output folder...")
    for filename in os.listdir(OUTPUT_FOLDER):
        file_path = os.path.join(OUTPUT_FOLDER, filename)
        try:
            if os.path.isfile(file_path) or os.path.islink(file_path):
                os.unlink(file_path) # deletes files and symlinks
        except Exception as e:
            print(f'failed to delete {file_path} due to: {e}')
    
    # --- 2. FETCH original url ---
    data = request.json
    if not data or 'url' not in data:
        return jsonify({"error": "missing 'url' in request body"}), 400
    
    original_url = data['url']
    
    # generate unique filenames
    unique_id = str(uuid.uuid4())
    original_filename = f"{unique_id}_original.glb"
    modified_filename = f"{unique_id}_child.glb"
    
    original_filepath = os.path.join(OUTPUT_FOLDER, original_filename)
    modified_filepath = os.path.join(OUTPUT_FOLDER, modified_filename)
    
    # --- 3. RUN download/processing scripts ---
    try:
        print(f"downloading {original_url}...")
        subprocess.run(
            ["python", "download_avatar.py", original_url, original_filepath],
            check=True
        )

        print(f"processing {original_filepath} with blender...")
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
        
        # --- 4. RETURN url to new file ---
        new_avatar_url = request.host_url.rstrip('/') + f"/files/{modified_filename}"
        
        # clean up original download
        os.remove(original_filepath)
        
        return jsonify({"newUrl": new_avatar_url})

    except subprocess.CalledProcessError as e:
        print(f"error in subprocess: {e}")
        return jsonify({"error": "pipeline failed during processing"}), 500
    except Exception as e:
        print(f"unexpected error: {e}")
        return jsonify({"error": "internal server error"}), 500

# endpoint to serve generated file
@app.route('/files/<path:filename>')
def get_file(filename):
    return send_from_directory(OUTPUT_FOLDER, filename)

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)