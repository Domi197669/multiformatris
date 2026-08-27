package com.multiformatris.game

import android.opengl.GLES30
import java.nio.ByteBuffer
import java.nio.ByteOrder
import java.nio.FloatBuffer

/**
 * A unit cube centered at origin (extends -0.5..+0.5 on each axis).
 * Contains position (3) + normal (3) per vertex. Renders with glDrawArrays.
 */
class CubeMesh {
    fun load(): Int {
        val vbo = IntArray(1)
        GLES30.glGenBuffers(1, vbo, 0)
        GLES30.glBindBuffer(GLES30.GL_ARRAY_BUFFER, vbo[0])
        GLES30.glBufferData(
            GLES30.GL_ARRAY_BUFFER,
            VERTEX_BYTES,
            vertexData(), GLES30.GL_STATIC_DRAW
        )
        GLES30.glBindBuffer(GLES30.GL_ARRAY_BUFFER, 0)
        return vbo[0]
    }

    private fun vertexData(): FloatBuffer {
        val bb = ByteBuffer.allocateDirect(VERTEX_BYTES)
        bb.order(ByteOrder.nativeOrder())
        val fb = bb.asFloatBuffer()
        fb.put(VERTS)
        fb.position(0)
        return fb
    }

    companion object {
        const val FLOATS_PER_VERTEX = 6
        const val VERTS_COUNT = 36
        const val VERTEX_BYTES: Int = VERTS_COUNT * FLOATS_PER_VERTEX * 4

        // Cube faces (position xyz, normal xyz). Normals shade each face distinctly.
        private val VERTS = floatArrayOf(
            // front (+z)
            -0.5f, -0.5f, 0.5f, 0f, 0f, 1f,
            0.5f, -0.5f, 0.5f, 0f, 0f, 1f,
            0.5f, 0.5f, 0.5f, 0f, 0f, 1f,
            -0.5f, -0.5f, 0.5f, 0f, 0f, 1f,
            0.5f, 0.5f, 0.5f, 0f, 0f, 1f,
            -0.5f, 0.5f, 0.5f, 0f, 0f, 1f,
            // back (-z)
            -0.5f, -0.5f, -0.5f, 0f, 0f, -1f,
            -0.5f, 0.5f, -0.5f, 0f, 0f, -1f,
            0.5f, 0.5f, -0.5f, 0f, 0f, -1f,
            -0.5f, -0.5f, -0.5f, 0f, 0f, -1f,
            0.5f, 0.5f, -0.5f, 0f, 0f, -1f,
            0.5f, -0.5f, -0.5f, 0f, 0f, -1f,
            // left (-x)
            -0.5f, -0.5f, -0.5f, -1f, 0f, 0f,
            -0.5f, -0.5f, 0.5f, -1f, 0f, 0f,
            -0.5f, 0.5f, 0.5f, -1f, 0f, 0f,
            -0.5f, -0.5f, -0.5f, -1f, 0f, 0f,
            -0.5f, 0.5f, 0.5f, -1f, 0f, 0f,
            -0.5f, 0.5f, -0.5f, -1f, 0f, 0f,
            // right (+x)
            0.5f, -0.5f, -0.5f, 1f, 0f, 0f,
            0.5f, 0.5f, -0.5f, 1f, 0f, 0f,
            0.5f, 0.5f, 0.5f, 1f, 0f, 0f,
            0.5f, -0.5f, -0.5f, 1f, 0f, 0f,
            0.5f, 0.5f, 0.5f, 1f, 0f, 0f,
            0.5f, -0.5f, 0.5f, 1f, 0f, 0f,
            // bottom (-y)
            -0.5f, -0.5f, -0.5f, 0f, -1f, 0f,
            -0.5f, -0.5f, 0.5f, 0f, -1f, 0f,
            0.5f, -0.5f, 0.5f, 0f, -1f, 0f,
            -0.5f, -0.5f, -0.5f, 0f, -1f, 0f,
            0.5f, -0.5f, 0.5f, 0f, -1f, 0f,
            0.5f, -0.5f, -0.5f, 0f, -1f, 0f,
            // top (+y)
            -0.5f, 0.5f, -0.5f, 0f, 1f, 0f,
            0.5f, 0.5f, -0.5f, 0f, 1f, 0f,
            0.5f, 0.5f, 0.5f, 0f, 1f, 0f,
            -0.5f, 0.5f, -0.5f, 0f, 1f, 0f,
            0.5f, 0.5f, 0.5f, 0f, 1f, 0f,
            -0.5f, 0.5f, 0.5f, 0f, 1f, 0f
        )
    }
}
