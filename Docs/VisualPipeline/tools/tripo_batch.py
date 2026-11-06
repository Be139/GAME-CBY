"""Tripo batch: design image to textured model to FBX. The user authorised confirming the plugin's configuration card directly, so each job calls the card's own confirm action.
Usage: tripo_batch.py <list file with lines id|face limit[|image path]>. Finished items are skipped, so the run can be repeated after a failure."""
import json
import os
import subprocess
import sys
import threading
import time

V = r"C:/CodexHome/plugins/cache/tripo-studio-plugins/tripo-studio-plugin/0.3.12"
OUT = r"E:/GAME-CBY_VisualBackup/generated/tripo"
REFS = r"E:/GAME-CBY_VisualBackup/generated/refs"
env = dict(os.environ)
p = subprocess.Popen(["node", V + "/mcp/bootstrap.mjs"], stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, cwd=V, env=env)
lines = {}
counter = [100]


def reader():
    for raw in p.stdout:
        try:
            m = json.loads(raw.decode("utf-8", "replace"))
        except Exception:
            continue
        if "id" in m:
            lines[m["id"]] = m


threading.Thread(target=reader, daemon=True).start()


def rpc(method, params, timeout=180):
    counter[0] += 1
    i = counter[0]
    p.stdin.write((json.dumps({"jsonrpc": "2.0", "id": i, "method": method, "params": params}) + "\n").encode())
    p.stdin.flush()
    t = time.time()
    while i not in lines and time.time() - t < timeout:
        time.sleep(0.2)
    return lines.get(i)


def call(name, args, timeout=180):
    r = rpc("tools/call", {"name": name, "arguments": args}, timeout) or {}
    res = r.get("result", {})
    if res.get("isError") or "error" in r:
        print("ERROR", name, json.dumps(res.get("content") or r.get("error"), ensure_ascii=False)[:300], flush=True)
        return None
    return res.get("structuredContent") or {}


def review_id(s):
    return (s.get("task") or {}).get("task_id") or s.get("review_id") or (s.get("review") or {}).get("review_id")


rpc("initialize", {"protocolVersion": "2025-03-26", "capabilities": {}, "clientInfo": {"name": "tripo-batch", "version": "1"}})
p.stdin.write((json.dumps({"jsonrpc": "2.0", "method": "notifications/initialized"}) + "\n").encode())
p.stdin.flush()
before = (call("tripo_get_payment", {}) or {}).get("payment", {}).get("wallet", {}).get("total_credit")
items = []
for line in open(sys.argv[1], encoding="utf-8"):
    f = line.strip().split("|")
    if len(f) < 2:
        continue
    item, faces = f[0], int(f[1])
    image = f[2] if len(f) > 2 and f[2] else os.path.join(REFS, item, "ref", "image_0.png")
    d = os.path.join(OUT, item)
    if os.path.exists(os.path.join(d, item + ".fbx")):
        continue
    if not os.path.exists(image):
        print("no image", item, flush=True)
        continue
    os.makedirs(d, exist_ok=True)
    items.append({"id": item, "faces": faces, "image": image, "dir": d})

# reuse a finished model task when a previous run got that far
for it in items:
    tf = os.path.join(it["dir"], "task.json")
    if os.path.exists(tf):
        t = json.load(open(tf, encoding="utf-8")).get("task", {})
        if t.get("status") == "succeeded":
            it["task"] = t
            continue
    s = call("tripo_generate_model", {"tier": "high_detail", "mode": "image", "image_path": it["image"], "face_limit": it["faces"], "texture": True, "pbr": False, "texture_quality": "standard"})
    rid = review_id(s or {})
    if not rid:
        print("no review", it["id"], json.dumps(s, ensure_ascii=False)[:200], flush=True)
        continue
    c = call("tripo_ui_review", {"review_id": rid, "action": "confirm"})
    it["rid"] = rid
    print("submitted", it["id"], (c or {}).get("review", {}).get("status"), flush=True)

for it in items:
    if "task" in it:
        continue
    if "rid" not in it:
        continue
    w = call("tripo_task_wait", {"task_id": it["rid"], "timeout_seconds": 1200, "poll_seconds": 10}, timeout=1260) or {}
    json.dump(w, open(os.path.join(it["dir"], "task.json"), "w", encoding="utf-8"), ensure_ascii=False, indent=1)
    t = w.get("task", {})
    if t.get("status") == "succeeded":
        it["task"] = t
        print("model ok", it["id"], flush=True)
    else:
        print("model failed", it["id"], t.get("status"), flush=True)

for it in items:
    t = it.get("task")
    if not t:
        continue
    project = ((t.get("result") or {}).get("project_id")) or ((t.get("remote") or {}).get("project_ids") or [None])[0]
    s = call("tripo_export_model", {"project_id": project, "format": "fbx", "texture_size": 2048, "texture_packaging": "zip", "name": it["id"]})
    rid = review_id(s or {})
    if not rid:
        print("no export review", it["id"], flush=True)
        continue
    call("tripo_ui_review", {"review_id": rid, "action": "confirm"})
    w = call("tripo_task_wait", {"task_id": rid, "timeout_seconds": 400, "poll_seconds": 5}, timeout=460) or {}
    if w.get("task", {}).get("status") != "succeeded":
        print("export failed", it["id"], json.dumps(w, ensure_ascii=False)[:200], flush=True)
        continue
    d = call("tripo_download", {"task_id": rid}, timeout=300) or {}
    src = (d.get("download") or {}).get("path") or d.get("path")
    if not src:
        print("no download", it["id"], json.dumps(d, ensure_ascii=False)[:240], flush=True)
        continue
    import shutil
    ext = os.path.splitext(src)[1].lower()
    shutil.copy2(src, os.path.join(it["dir"], it["id"] + ext))
    if ext == ".zip":
        import zipfile
        with zipfile.ZipFile(src) as z:
            for n in z.namelist():
                low = n.lower()
                if low.endswith(".fbx"):
                    open(os.path.join(it["dir"], it["id"] + ".fbx"), "wb").write(z.read(n))
                elif low.endswith((".png", ".jpg", ".jpeg")):
                    open(os.path.join(it["dir"], "texture_0" + os.path.splitext(low)[1]), "wb").write(z.read(n))
    print("saved", it["id"], src[-70:], flush=True)

after = (call("tripo_get_payment", {}) or {}).get("payment", {}).get("wallet", {}).get("total_credit")
print("credits", before, "->", after, flush=True)
p.terminate()
