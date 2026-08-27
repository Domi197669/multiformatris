package com.multiformatris.game

import android.opengl.GLES30
import android.opengl.GLSurfaceView
import android.opengl.Matrix
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

        GLES30.glEnable(GLES30.GL_DEPTH_TEST)
        GLES30.glEnable(GLES30.GL_CULL_FACE)
        GLES30.glCullFace(GLES30.GL_BACK)
        GLES30.glClearColor(0.05f, 0.05f, 0.12f, 1f)
    }

    override fun onSurfaceChanged(gl: GL10?, width: Int, height: Int) {
        sw = width; sh = height
        GLES30.glViewport(0, 0, width, height)
        Matrix.perspectiveM(projMatrix, 0, 45f, width.toFloat() / height.toFloat(), 0.1f, 100f)
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
            CENTER_X, VIEW_DISTANCE_Y, CENTER_Z + VIEW_DISTANCE_Z,
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
        // Draw the bottom outline of the play field using lines (a simple way to see the well)
        // Not strictly required; kept minimal.
    }

    companion object {
        private const val CENTER_X = 3.5f
        private const val CENTER_Y = 5.0f
        private const val CENTER_Z = 3.5f
        private const val VIEW_DISTANCE_Y = 9.0f
        private const val VIEW_DISTANCE_Z = 16.0f

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
    }
}
