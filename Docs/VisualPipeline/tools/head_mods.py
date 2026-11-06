"""Lists every prefab override in the committed scene as TSV, so the editor can put city renderers back exactly as they were."""
import re
import sys

src, out = sys.argv[1], sys.argv[2]
head = re.compile(r"^--- !u!(\d+) &(-?\d+)")
target = re.compile(r"^\s+- target: \{fileID: (-?\d+), guid: ([0-9a-f]+)")
ref = re.compile(r"^\s+objectReference: \{fileID: (-?\d+)(?:, guid: ([0-9a-f]+)(?:, type: (\d+))?)?")
rows, inst, cur = [], None, None
for line in open(src, encoding="utf-8"):
    m = head.match(line)
    if m:
        inst = m.group(2) if m.group(1) == "1001" else None
        cur = None
        continue
    if inst is None:
        continue
    m = target.match(line)
    if m:
        cur = {"t": m.group(1), "g": m.group(2), "p": "", "v": ""}
        continue
    if cur is None:
        continue
    s = line.strip()
    if s.startswith("propertyPath: "):
        cur["p"] = s[len("propertyPath: "):]
    elif s.startswith("value: ") or s == "value:":
        cur["v"] = s[len("value:"):].strip()
    else:
        m = ref.match(line)
        if m:
            rows.append("\t".join([inst, cur["t"], cur["g"], cur["p"], cur["v"].replace("\t", " "), m.group(1), m.group(2) or "", m.group(3) or ""]))
            cur = None
open(out, "w", encoding="utf-8").write("\n".join(rows))
print("mods", len(rows))
