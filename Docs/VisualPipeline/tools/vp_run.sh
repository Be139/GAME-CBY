#!/usr/bin/env bash
# Usage: vp_run.sh <spec file>. Joins the shared helpers with a room spec, runs it in the editor, saves the scene.
cd /d/UGit/GAME-CBY
SP="D:/UGit/GAME-CBY/Docs/VisualPipeline/tools"
mkdir -p "$SP/_work/views" "$SP/_work/plans"
cat "$SP/vp_lib.cs" "$SP/vp_lib2.cs" "$SP/vp_lib3.cs" "$SP/vp_lib4.cs" "$SP/$1" > "$SP/_work/vp_run_now.cs"
cat >> "$SP/_work/vp_run_now.cs" <<'EOF'
UnityEditor.AssetDatabase.SaveAssets();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
return "ok\n" + sb.ToString();
EOF
./Tools/Dev/unity-mcp.sh -t 200 -f json code execute -f "$SP/_work/vp_run_now.cs" > "$SP/_work/vp_run.json" 2>&1
PYTHONIOENCODING=utf-8 /d/Tools/Python/python.exe "$SP/show.py" "$SP/_work/vp_run.json" 2>&1 | cut -c1-260 | head -40
