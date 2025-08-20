## Self-Attachment Therapy in AR
This Unity mobile application allows users to create and interact with their personalised "childhood" self as a 3D avatar. The project integrates:
* MetaPerson Creator API (https://docs.metaperson.avatarsdk.com/)
* Flask server for avatar body proportion editing (using Blender), hosted on AWS (EC2)
* Persistent cloud storage (Supabase PostgreSQL)
* Gamification and intuitive UI/UX elements based on HCI principles

## Getting Started
<b>Prerequisites:</b> Unity Hub and Editor (Version 2022.3.62f1), Xcode (iOS build) or Android Studio (Android)

1. <b>Clone the Repository</b>
2. <b>Backend Server Setup:</b>
```
cd BlenderServer
python3.x -m venv venv
source venv/bin/activate
pip install -r requirements.txt
python server.py
```
3. <b>Open the Unity Project:</b>
* Go to File -> Build Settings and ensure all scenes are added
* Update server_api in MobileUnitySampleHandler.cs to http://localhost:5000/process-avatar
* Update serverUrl in DisplayUI.cs to http://localhost:5000
* Store MetaPerson client ID and secret and Supabase url and anon key as Secrets under `Resources/`
* Build and run project on target device

## Project Structure
* `Fonts/` - custom font files used in UI
* `Materials/` - for UI elements and 3D objects (e.g., indicator for AR placement)
* `Prefabs/` - reusable GameObjects (e.g., EmotionSystems, DebriefPopup, TimerPanel)
* `References/` - image for AR tracking
* `Resources/` - for facial expressions and body animations
* `Samples/` - imports (e.g., MetaPerson Loader)
* `Scenes/`
* `Scripts/` - organised into subfolders by scene or functionality
* `Sprites/` - 2D images and icons used for UI elements
* `TextMesh Pro/` - related to TextMesh Pro text rendering plugin
* `XR/` - related to AR Foundation