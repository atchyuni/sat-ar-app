# download_avatar: get glb from url provided by unity app
import requests
import sys
import zipfile
import io
import os

def download_and_extract_glb(url, output_path):
    try:
        print(f"downloading from: {url}")
        response = requests.get(url, allow_redirects=True)
        response.raise_for_status()

        print("searching for main .glb file inside zip...")
        
        # open from in-memory bytes
        with zipfile.ZipFile(io.BytesIO(response.content)) as thezip:
            
            glb_files = {}
            for file_info in thezip.infolist():
                if file_info.filename.endswith('.glb') and not file_info.is_dir():
                    glb_files[file_info.filename] = file_info.file_size
            
            if not glb_files:
                print("no .glb files found inside zip archive")
                return False

            # assumption: determine main model file (largest file size)
            main_glb_file = max(glb_files, key=glb_files.get)
            print(f"main model: '{main_glb_file}' (size: {glb_files[main_glb_file]} bytes)")

            source = thezip.open(main_glb_file)
            target = open(output_path, "wb")
            with source, target:
                target.write(source.read())
            
            print(f"successfully extracted and saved to: {output_path}")
            return True

    except requests.exceptions.RequestException as e:
        print(f"error downloading file: {e}")
        return False
    except zipfile.BadZipFile:
        print("error: downloaded file not valid zip archive")
        return False

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("USAGE: python download_avatar.py <URL> <output_path.glb>")
    else:
        avatar_url = sys.argv[1]
        output_file_path = sys.argv[2]
        download_and_extract_glb(avatar_url, output_file_path)