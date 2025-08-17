import requests
import sys
import zipfile
import io
import os

def download_and_extract_glb(url, output_path):
    try:
        print(f"[DownloadAvatar] from: {url}")
        response = requests.get(url, allow_redirects=True)
        response.raise_for_status()

        print("[DownloadAvatar] searching inside zip archive...")
        
        with zipfile.ZipFile(io.BytesIO(response.content)) as thezip:
            
            glb_files = {}
            for file_info in thezip.infolist():
                if file_info.filename.endswith('.glb') and not file_info.is_dir():
                    glb_files[file_info.filename] = file_info.file_size
            
            if not glb_files:
                print("[DownloadAvatar-Error] no .glb found inside zip")
                return False

            # assume main model file based on size
            main_glb_file = max(glb_files, key=glb_files.get)
            print(f"[DownloadAvatar] main model: '{main_glb_file}' (size: {glb_files[main_glb_file]} bytes)")

            source = thezip.open(main_glb_file)
            target = open(output_path, "wb")
            with source, target:
                target.write(source.read())
            
            print(f"[DownloadAvatar] successful extraction saved to: {output_path}")
            return True

    except requests.exceptions.RequestException as e:
        print(f"[DownloadAvatar-Error] {e}")
        return False
    except zipfile.BadZipFile:
        print("[DownloadAvatar-Error] downloaded file not valid zip archive")
        return False

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("[DownloadAvatar] usage: python download_avatar.py <url> <output_path.glb>")
    else:
        avatar_url = sys.argv[1]
        output_file_path = sys.argv[2]
        download_and_extract_glb(avatar_url, output_file_path)