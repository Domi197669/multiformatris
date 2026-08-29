package com.multiformatris.game

import android.opengl.GLES30
import android.opengl.GLSurfaceView
import android.opengl.Matrix
import java.nio.FloatBuffer
import javax.microedition.khronos.egl.EGLConfig
import javax.microedition.khronos.opengles.GL10

class GameRenderer(private val controller: TetrisController) : GLSurfaceView.Renderer {

    private var cubeVbo = 0
    private val mvpMatrix = FloatArray(16)
    private val modelMatrix = FloatArray(16)
    private val viewMatrix = FloatArray(16)
    private val projMatrix = FloatArray(16)
    private val tmpMatrix = FloatArray(16)

    private var cubeProgram = 0
    private var uMVPLoc = 0
    private var uColorLoc = 0
    private var uLightDirLoc = 0
    private var uNormalMatrixLoc = 0

    private var lineProgram = 0
    private var uLineColorLoc = 0
    private var uLineMVPLoc = 0
    private var lineVbo = 0
    private var gridLineCount = 0
    private var edgeLineCount = 0

    private var sw = 1
    private var sh = 1
    private var lastTickNanos = 0L

    override fun onSurfaceCreated(gl: GL10?, config: EGLConfig?) {
        cubeProgram = GlUtil.linkProgram(VERTEX_SHADER, FRAGMENT_SHADER)
        uMVPLoc = GLES30.glGetUniformLocation(cubeProgram, "uMVP")
        uColorLoc = GLES30.glGetUniformLocation(cubeProgram, "uColor")
        uLightDirLoc = GLES30.glGetUniformLocation(cubeProgram, "uLightDir")
        uNormalMatrixLoc = GLES30.glGetUniformLocation(cubeProgram, "uNormalMatrix")

        cubeVbo = CubeMesh().load()

        lineProgram = GlUtil.linkProgram(LINE_VERTEX_SHADER, LINE_FRAGMENT_SHADER)
        uLineColorLoc = GLES30.glGetUniformLocation(lineProgram, "uColor")
        uLineMVPLoc = GLES30.glGetUniformLocation(lineProgram, "uMVP")
        lineVbo = buildFrameLines()

        GLES30.glEnable(GLES30.GL_DEPTH_TEST)
        GLES30.glEnable(GLES30.GL_CULL_FACE)
        GLES30.glCullFace(GLES30.GL_BACK)
        GLES30.glClearColor(0.05f, 0.05f, 0.12f, 1f)
    }

    override fun onSurfaceChanged(gl: GL10?, width: Int, height: Int) {
        sw = width; sh = height
        GLES30.glViewport(0, 0, width, height)
        buildProjection(width.toFloat() / height.toFloat())
    }

    /**
     * Builds an orthographic projection that frames the whole 3D well regardless of
     * screen size or orientation. Corners of the well are projected onto the camera
     * plane, then a non-distorting ortho box is derived that keeps the aspect ratio.
     */
    private fun buildProjection(aspect: Float) {
        // view direction and camera basis (matches setLookAtM with up = (0,1,0))
        val fx = CENTER_X - EYE_X
        val fy = CENTER_Y - EYE_Y
        val fz = CENTER_Z - EYE_Z
        val fl = kotlin.math.sqrt(fx * fx + fy * fy + fz * fz)

        // right = normalize(cross(f, up)), up = (0,1,0)  =>  (-fz, 0, fx)
        var rx = -fz; var rz = fx
        val rl = kotlin.math.sqrt(rx * rx + rz * rz)
        rx /= rl; rz /= rl

        // Project the 8 corners of the well onto the camera basis.
        var minX = Float.MAX_VALUE; var maxX = -Float.MAX_VALUE
        var minY = Float.MAX_VALUE; var maxY = -Float.MAX_VALUE
        var minZ = Float.MAX_VALUE; var maxZ = -Float.MAX_VALUE
        val w = controller.width
        val h = controller.height
        val d = controller.depth
        for (xi in intArrayOf(0, w)) for (yi in intArrayOf(0, h)) for (zi in intArrayOf(0, d)) {
            val px = xi - EYE_X
            val py = yi - EYE_Y
            val pz = zi - EYE_Z
            // forward component
            val fwd = (px * fx + py * fy + pz * fz) / fl
            // right component
            val cx = (px * rx + pz * rz) / rl
            // up component = px,py,pz · upCam
            // upCam = cross(right, f) = cross((rx,0,rz),(fx,fy,fz)/fl)
            val upx = 0f * (fz / fl) - rz * (fy / fl)
            val upy = rz * (fx / fl) - rx * (fz / fl)
            val upz = rx * (fy / fl) - 0f * (fx / fl)
            val cy = px * upx + py * upy + pz * upz
            minX = minOf(minX, cx); maxX = maxOf(maxX, cx)
            minY = minOf(minY, cy); maxY = maxOf(maxY, cy)
            minZ = minOf(minZ, fwd); maxZ = maxOf(maxZ, fwd)
        }

        val geomHalfW = (maxOf(kotlin.math.abs(minX), kotlin.math.abs(maxX)) + 1.2f)
        val geomHalfH = (maxOf(kotlin.math.abs(minY), kotlin.math.abs(maxY)) + 1.2f)
        val margin = 1.12f

        var halfH = geomHalfH * margin
        var halfW = halfH * aspect
        if (halfW < geomHalfW * margin) {
            halfW = geomHalfW * margin
            halfH = halfW / aspect
        }

        val near = maxOf(0.1f, minZ - 4f)
        val far = maxZ + 4f
        Matrix.orthoM(projMatrix, 0, -halfW, halfW, -halfH, halfH, near, far)
    }

    override fun onDrawFrame(gl: GL10?) {
        GLES30.glClear(GLES30.GL_COLOR_BUFFER_BIT or GLES30.GL_DEPTH_BUFFER_BIT)

        // advance game logic with delta time
        val now = System.nanoTime()
        if (lastTickNanos != 0L) {
            val dt = (now - lastTickNanos) / 1_000_000_000f
            controller.tick(dt)
        }
        lastTickNanos = now

        Matrix.setLookAtM(
            viewMatrix, 0,
            EYE_X, EYE_Y, EYE_Z,
            CENTER_X, CENTER_Y, CENTER_Z,
            0f, 1f, 0f
        )

        GLES30.glUseProgram(cubeProgram)

        // draw settled grid cells
        for (y in 0 until controller.height) {
            for (z in 0 until controller.depth) {
                for (x in 0 until controller.width) {
                    val v = controller.grid[controller.index(x, y, z)]
                    if (v != 0) drawCube(x.toFloat(), y.toFloat(), z.toFloat(), colorFor(v))
                }
            }
        }

        // draw current piece
        if (controller.piece.isNotEmpty()) {
            for (c in controller.piece) {
                drawCube(
                    controller.px + c[0].toFloat(),
                    controller.py + c[1].toFloat(),
                    controller.pz + c[2].toFloat(),
                    colorFor(controller.pieceColor)
                )
            }
        }

        // draw ground grid plane (borders)
        drawGroundFrame()
    }

    private fun drawCube(x: Float, y: Float, z: Float, rgb: FloatArray) {
        Matrix.setIdentityM(modelMatrix, 0)
        Matrix.translateM(modelMatrix, 0, x, y, z)
        Matrix.multiplyMM(tmpMatrix, 0, viewMatrix, 0, modelMatrix, 0)
        Matrix.multiplyMM(mvpMatrix, 0, projMatrix, 0, tmpMatrix, 0)

        // cubes are only translated (never rotated/scaled), so the normal matrix is identity
        GLES30.glUniformMatrix4fv(uMVPLoc, 1, false, mvpMatrix, 0)
        GLES30.glUniformMatrix3fv(uNormalMatrixLoc, 1, false, IDENTITY_NORMAL, 0)
        GLES30.glUniform3f(uColorLoc, rgb[0], rgb[1], rgb[2])
        GLES30.glUniform3f(uLightDirLoc, 0.5f, -0.8f, 0.6f)

        GLES30.glBindBuffer(GLES30.GL_ARRAY_BUFFER, cubeVbo)
        GLES30.glEnableVertexAttribArray(0)
        GLES30.glVertexAttribPointer(0, 3, GLES30.GL_FLOAT, false, 24, 0)
        GLES30.glEnableVertexAttribArray(1)
        GLES30.glVertexAttribPointer(1, 3, GLES30.GL_FLOAT, false, 24, 12)
        GLES30.glDrawArrays(GLES30.GL_TRIANGLES, 0, CubeMesh.VERTS_COUNT)
    }

    private fun drawGroundFrame() {
        // Draw the play-field frame (base grid + edges) using lines so the 3D
        // well and the stacking order from bottom to top are easy to read.
        GLES30.glUseProgram(lineProgram)
        Matrix.multiplyMM(tmpMatrix, 0, projMatrix, 0, viewMatrix, 0)
        GLES30.glUniformMatrix4fv(uLineMVPLoc, 1, false, tmpMatrix, 0)
        GLES30.glBindBuffer(GLES30.GL_ARRAY_BUFFER, lineVbo)
        GLES30.glEnableVertexAttribArray(0)
        GLES30.glVertexAttribPointer(0, 3, GLES30.GL_FLOAT, false, 12, 0)

        // grid + base edges (subtle)
        GLES30.glUniform3f(uLineColorLoc, 0.25f, 0.30f, 0.45f)
        GLES30.glLineWidth(1f)
        GLES30.glDrawArrays(GLES30.GL_LINES, 0, gridLineCount)

        // outer well edges (brighter)
        GLES30.glUniform3f(uLineColorLoc, 0.45f, 0.55f, 0.85f)
        GLES30.glLineWidth(2f)
        GLES30.glDrawArrays(GLES30.GL_LINES, gridLineCount, edgeLineCount)
    }

    /** Builds all line vertices once: base grid + outer well edges. Returns the VBO. */
    private fun buildFrameLines(): Int {
        val w = controller.width
        val h = controller.height
        val d = controller.depth

        val grid = ArrayList<Float>()
        // base grid lines on the floor (y=0), parallel to X and Z
        for (z in 0..d) {
            grid.add(0f); grid.add(0f); grid.add(z.toFloat())
            grid.add(w.toFloat()); grid.add(0f); grid.add(z.toFloat())
        }
        for (x in 0..w) {
            grid.add(x.toFloat()); grid.add(0f); grid.add(0f)
            grid.add(x.toFloat()); grid.add(0f); grid.add(d.toFloat())
        }

        val edges = ArrayList<Float>()
        fun edge(x1: Int, y1: Int, z1: Int, x2: Int, y2: Int, z2: Int) {
            edges.add(x1.toFloat()); edges.add(y1.toFloat()); edges.add(z1.toFloat())
            edges.add(x2.toFloat()); edges.add(y2.toFloat()); edges.add(z2.toFloat())
        }
        // bottom ring (y=0)
        edge(0, 0, 0, w, 0, 0)
        edge(w, 0, 0, w, 0, d)
        edge(w, 0, d, 0, 0, d)
        edge(0, 0, d, 0, 0, 0)
        // top ring (y=h-1)
        edge(0, h - 1, 0, w, h - 1, 0)
        edge(w, h - 1, 0, w, h - 1, d)
        edge(w, h - 1, d, 0, h - 1, d)
        edge(0, h - 1, d, 0, h - 1, 0)
        // vertical edges
        edge(0, 0, 0, 0, h - 1, 0)
        edge(w, 0, 0, w, h - 1, 0)
        edge(w, 0, d, w, h - 1, d)
        edge(0, 0, d, 0, h - 1, d)

        val verts = FloatArray(grid.size + edges.size)
        grid.forEachIndexed { i, v -> verts[i] = v }
        edges.forEachIndexed { i, v -> verts[i + grid.size] = v }
        gridLineCount = grid.size / 3
        edgeLineCount = edges.size / 3

        val vbo = IntArray(1)
        GLES30.glGenBuffers(1, vbo, 0)
        GLES30.glBindBuffer(GLES30.GL_ARRAY_BUFFER, vbo[0])
        GLES30.glBufferData(GLES30.GL_ARRAY_BUFFER, verts.size * 4, FloatBuffer.wrap(verts), GLES30.GL_STATIC_DRAW)
        GLES30.glBindBuffer(GLES30.GL_ARRAY_BUFFER, 0)
        return vbo[0]
    }

    companion object {
        private const val CENTER_X = 3.5f
        private const val CENTER_Y = 5.0f
        private const val CENTER_Z = 3.5f

        // 3/4 perspective view: eye up, front-right so width (X), height (Y) and
        // depth (Z) of the well are all visible and the stack reads bottom-up.
        private const val EYE_X = 10.0f
        private const val EYE_Y = 15.0f
        private const val EYE_Z = 16.0f

        private val IDENTITY_NORMAL = floatArrayOf(
            1f, 0f, 0f,
            0f, 1f, 0f,
            0f, 0f, 1f
        )

        fun colorFor(ci: Int): FloatArray = when (ci) {
            TetrisController.COLOR_CYAN -> floatArrayOf(0.0f, 1.0f, 1.0f)
            TetrisController.COLOR_YELLOW -> floatArrayOf(1.0f, 1.0f, 0.0f)
            TetrisController.COLOR_PURPLE -> floatArrayOf(0.8f, 0.2f, 0.9f)
            TetrisController.COLOR_GREEN -> floatArrayOf(0.2f, 0.9f, 0.3f)
            TetrisController.COLOR_RED -> floatArrayOf(0.9f, 0.2f, 0.2f)
            TetrisController.COLOR_BLUE -> floatArrayOf(0.2f, 0.4f, 0.9f)
            TetrisController.COLOR_ORANGE -> floatArrayOf(1.0f, 0.6f, 0.1f)
            TetrisController.COLOR_GOLD -> floatArrayOf(1.0f, 0.84f, 0.1f)
            else -> floatArrayOf(0.5f, 0.5f, 0.5f)
        }

        private const val VERTEX_SHADER = """
            #version 300 es
            layout(location = 0) in vec3 aPos;
            layout(location = 1) in vec3 aNormal;
            uniform mat4 uMVP;
            uniform mat3 uNormalMatrix;
            out vec3 vNormal;
            void main() {
                vNormal = uNormalMatrix * aNormal;
                gl_Position = uMVP * vec4(aPos, 1.0);
            }
        """

        private const val FRAGMENT_SHADER = """
            #version 300 es
            precision mediump float;
            in vec3 vNormal;
            uniform vec3 uColor;
            uniform vec3 uLightDir;
            out vec4 fragColor;
            void main() {
                vec3 n = normalize(vNormal);
                float diff = max(dot(n, normalize(uLightDir)), 0.0);
                float light = 0.45 + 0.55 * diff;
                 fragColor = vec4(uColor * light, 1.0);
            }
        """

        private const val LINE_VERTEX_SHADER = """
            #version 300 es
            layout(location = 0) in vec3 aPos;
            uniform mat4 uMVP;
            void main() {
                gl_Position = uMVP * vec4(aPos, 1.0);
            }
        """

        private const val LINE_FRAGMENT_SHADER = """
            #version 300 es
            precision mediump float;
            uniform vec3 uColor;
            out vec4 fragColor;
            void main() {
                fragColor = vec4(uColor, 1.0);
            }
        """
    }
}
