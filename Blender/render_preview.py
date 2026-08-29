# Renders a preview image of the Multiformatris scene.
# Usage: blender --background --python Blender/render_preview.py
import bpy

bpy.ops.wm.open_mainfile(filepath="/home/domi/multiformatris/Blender/multiformatris.blend")

scene = bpy.context.scene
scene.render.engine = "CYCLES"
scene.cycles.samples = 8
scene.cycles.use_denoising = False
scene.render.image_settings.file_format = "PNG"
scene.render.resolution_x = 640
scene.render.resolution_y = 360
scene.render.filepath = "/home/domi/multiformatris/Blender/multiformatris_preview.png"

bpy.ops.render.render(write_still=True)
print("RENDER_DONE")
