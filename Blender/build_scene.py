# Builds a reference 3D scene of the Multiformatris game well + pieces.
# Run headless:  blender --background --python Blender/build_scene.py
import bpy
import math

bpy.ops.wm.read_factory_settings(use_empty=True)

# --- piece definitions (game coords: planar in game XZ, y=0), colors from code ---
PIECES = [
    ("Cyan",   [(0,0),(0,1),(0,2),(0,3)],        (0.0, 1.0, 1.0)),
    ("Yellow", [(0,0),(1,0),(0,1),(1,1)],        (1.0, 1.0, 0.0)),
    ("Purple", [(0,0),(1,0),(2,0),(1,1)],        (0.8, 0.2, 0.9)),
    ("Green",  [(0,0),(1,0),(1,1),(2,1)],        (0.2, 0.9, 0.3)),
    ("Red",    [(1,0),(2,0),(0,1),(1,1)],        (0.9, 0.2, 0.2)),
    ("Blue",   [(0,0),(0,1),(1,1),(2,1)],        (0.2, 0.4, 0.9)),
    ("Orange", [(1,0),(0,1),(1,1),(2,1)],        (1.0, 0.6, 0.1)),
]

def make_material(name, rgb, alpha=1.0, rough=0.4, metallic=0.0):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (rgb[0], rgb[1], rgb[2], 1.0)
        bsdf.inputs["Roughness"].default_value = rough
        bsdf.inputs["Metallic"].default_value = metallic
    mat.diffuse_color = (rgb[0], rgb[1], rgb[2], alpha)
    return mat

PIECE_MATS = {}
for name, cells, rgb in PIECES:
    PIECE_MATS[name] = make_material("Mat_" + name, rgb)

GRID_MAT = make_material("Mat_Frame", (0.15, 0.16, 0.18), rough=0.6)
FLOOR_MAT = make_material("Mat_Floor", (0.28, 0.30, 0.34), rough=0.8)
FLOOR_PAD_MAT = make_material("Mat_FloorPad", (0.07, 0.08, 0.09), rough=1.0)

UNIT = 0.985  # cell edge so there is a small gap between pieces

def add_box(loc, scale, mat):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc)
    obj = bpy.context.object
    obj.scale = (scale[0], scale[1], scale[2])
    obj.name = "cube"
    obj.data.materials.append(mat)
    return obj

def add_bar(p1, p2, mat, t=0.09):
    dx = p2[0] - p1[0]
    dy = p2[1] - p1[1]
    dz = p2[2] - p1[2]
    length = math.sqrt(dx * dx + dy * dy + dz * dz)
    cx = (p1[0] + p2[0]) / 2.0
    cy = (p1[1] + p2[1]) / 2.0
    cz = (p1[2] + p2[2]) / 2.0
    add_box((cx, cy, cz), (t, t, t), mat)
    obj = bpy.context.object
    # orient the bar along the axis
    if abs(dx) > abs(dy) and abs(dx) > abs(dz):
        obj.scale = (length + t, t, t)
    elif abs(dy) > abs(dx) and abs(dy) > abs(dz):
        obj.scale = (t, length + t, t)
    else:
        obj.scale = (t, t, length + t)
    obj.name = "bar"
    return obj

def add_cell(cx, cy, cz, mat):
    return add_box((cx + 0.5, cy + 0.5, cz + 0.5), (UNIT, UNIT, UNIT), mat)

# --- well: 7 x 7 footprint, height 10 (blender Z-up) ---
W = 7.0
H = 10.0

xs = [0.0, W]
ys = [0.0, W]
zs = [0.0, H]

# 4 vertical edges
for x in xs:
    for y in ys:
        add_bar((x, y, 0), (x, y, H), GRID_MAT)
# bottom rectangle (z=0)
add_bar((0, 0, 0), (W, 0, 0), GRID_MAT)
add_bar((W, 0, 0), (W, W, 0), GRID_MAT)
add_bar((W, W, 0), (0, W, 0), GRID_MAT)
add_bar((0, W, 0), (0, 0, 0), GRID_MAT)
# top rectangle (z=H)
add_bar((0, 0, H), (W, 0, H), GRID_MAT)
add_bar((W, 0, H), (W, W, H), GRID_MAT)
add_bar((W, W, H), (0, W, H), GRID_MAT)
add_bar((0, W, H), (0, 0, H), GRID_MAT)

# transparent floor slab at the bottom of the well
add_box((W / 2, W / 2, 0.05), (W, W, 0.1), FLOOR_MAT)

# large ground pad under everything (for shadows / context)
add_box((W / 2 - 6, W / 2 - 6, -0.06), (W + 18, W + 18, 0.1), FLOOR_PAD_MAT)

# --- the 7 pieces laid flat, in a row in front of the well (y negative) ---
ROW_Y = -3.0
for i, (name, cells, rgb) in enumerate(PIECES):
    cx_center = sum(c[0] for c in cells) / float(len(cells))
    cy_center = sum(c[1] for c in cells) / float(len(cells))
    place_x = i * 4.0 - 12.0
    for (gx, gz) in cells:
        loc = (gx - cx_center + place_x,
               gz - cy_center + ROW_Y,
               0.5)
        add_box(loc, (UNIT, UNIT, UNIT), PIECE_MATS[name])
        bpy.context.object.name = "%s_%d_%d" % (name, gx, gz)

# --- camera (3/4 view) looking at the well center ---
bpy.ops.object.empty_add(type="PLAIN_AXES", location=(W / 2, W / 2, H / 2))
target = bpy.context.object
target.name = "Target_well"
bpy.ops.object.camera_add(location=(W + 8, W - 14, H + 2))
cam = bpy.context.object
cam.name = "MainCamera"
cam.data.lens = 45
trk = cam.constraints.new(type="TRACK_TO")
trk.target = target
trk.track_axis = "TRACK_NEGATIVE_Z"
trk.up_axis = "UP_Y"
bpy.context.scene.camera = cam

# --- lighting ---
bpy.ops.object.light_add(type="SUN", location=(4, 4, 15))
sun = bpy.context.object
sun.data.energy = 3.0
sun.data.angle = math.radians(0.7)
sun.rotation_euler = (math.radians(55), 0, math.radians(25))

bpy.ops.object.light_add(type="AREA", location=(7 - 6, 7 - 8, 6))
area = bpy.context.object
area.data.energy = 200.0
area.data.size = 2.0

# --- world background ---
if len(bpy.data.worlds) == 0:
    bpy.data.worlds.new("World")
bpy.data.worlds[0].use_nodes = True
bg = bpy.data.worlds[0].node_tree.nodes.get("Background")
if bg:
    bg.inputs[0].default_value = (0.055, 0.07, 0.09, 1.0)
    bg.inputs[1].default_value = 1.0

# render settings for a nicer preview
bpy.context.scene.render.engine = "CYCLES"
bpy.context.scene.cycles.samples = 64

out = "/home/domi/multiformatris/Blender/multiformatris.blend"
bpy.ops.wm.save_as_mainfile(filepath=out)
print("SAVED", out)
