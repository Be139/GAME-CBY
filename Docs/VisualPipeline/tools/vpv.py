"""Converts a view list in floor coordinates (name|u,y,v|u,y,v|fov|post, floor height first) to world coordinates for vp_views.sh."""
import math
import sys

floor, items = float(sys.argv[1]), sys.argv[2].split(";")
a = math.radians(21.4)
U = (math.cos(a), math.sin(a))
V = (-math.sin(a), math.cos(a))


def w(s):
    u, y, v = [float(x) for x in s.split(",")]
    return f"{-2312 + U[0] * u + V[0] * v:.2f},{floor + y:.2f},{595.6 + U[1] * u + V[1] * v:.2f}"


out = []
for e in items:
    f = e.split("|")
    f[1], f[2] = w(f[1]), w(f[2])
    out.append("|".join(f))
print(";".join(out))
