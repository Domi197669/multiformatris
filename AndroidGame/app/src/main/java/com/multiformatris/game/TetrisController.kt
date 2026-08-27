package com.multiformatris.game

import kotlin.random.Random

/**
 * Core 3D Tetris logic. Grid is W(7) x H(10) x D(7).
 * Coordinates: x = left/right, y = up/down (gravity axis base), z = back/forward.
 */
class TetrisController(
    val width: Int = 7,
    val height: Int = 10,
    val depth: Int = 7
) {

    enum class State { MENU, PLAYING, PAUSED, GAME_OVER }

    /** Linear grid, values: 0 = empty, otherwise color index */
    val grid = IntArray(width * height * depth)

    var state: State = State.MENU
        private set

    var score = 0
        private set
    var lines = 0
        private set
    var level = 1
        private set

    // current piece state
    var piece: List<IntArray> = emptyList()   // list of (x,y,z) relative cells
    var pieceColor: Int = 0
    var px = 0
    var py = 0
    var pz = 0

    private val random = Random(System.currentTimeMillis())
    private var tickAccum = 0f

    fun start() {
        grid.fill(0)
        score = 0; lines = 0; level = 1
        state = State.PLAYING
        spawn()
    }

    fun pause() { if (state == State.PLAYING) state = State.PAUSED }
    fun resume() { if (state == State.PAUSED) state = State.PLAYING }

    fun index(x: Int, y: Int, z: Int): Int = (y * depth + z) * width + x

    fun inBounds(x: Int, y: Int, z: Int): Boolean =
        x in 0 until width && y in 0 until height && z in 0 until depth

    fun collides(cells: List<IntArray>, ox: Int, oy: Int, oz: Int): Boolean {
        for (c in cells) {
            val x = c[0] + ox
            val y = c[1] + oy
            val z = c[2] + oz
            if (!inBounds(x, y, z)) return true
            if (grid[index(x, y, z)] != 0) return true
        }
        return false
    }

    fun move(dx: Int, dy: Int, dz: Int): Boolean {
        if (state != State.PLAYING) return false
        if (!collides(piece, px + dx, py + dy, pz + dz)) {
            px += dx; py += dy; pz += dz
            return true
        }
        return false
    }

    /** Rotate the piece around the given axis (0=x,1=y,2=z). Always tries to keep within bounds. */
    fun rotate(axis: Int) {
        if (state != State.PLAYING) return
        val rotated = piece.map { c ->
            when (axis) {
                0 -> intArrayOf(c[0], c[2], -c[1])          // around X
                1 -> intArrayOf(c[2], c[1], -c[0])          // around Y
                else -> intArrayOf(-c[1], c[0], c[2])       // around Z
            }
        }
        val kicks = listOf(
            intArrayOf(0, 0, 0), intArrayOf(1, 0, 0), intArrayOf(-1, 0, 0),
            intArrayOf(0, 0, 1), intArrayOf(0, 0, -1), intArrayOf(0, 1, 0)
        )
        for (k in kicks) {
            if (!collides(rotated, px + k[0], py + k[1], pz + k[2])) {
                // represent rotation relative to current origin
                piece = rotated
                px += k[0]; py += k[1]; pz += k[2]
                return
            }
        }
    }

    /** Advance gravity one step; returns true if the piece was locked. */
    fun step(): Boolean {
        if (state != State.PLAYING) return false
        if (move(0, -1, 0)) return false
        lockPiece()
        return true
    }

    fun hardDrop() {
        if (state != State.PLAYING) return
        while (move(0, -1, 0)) { score += 2 }
        lockPiece()
    }

    private fun lockPiece() {
        for (c in piece) {
            grid[index(px + c[0], py + c[1], pz + c[2])] = pieceColor
        }
        clearLines()
        spawn()
    }

    private fun clearLines() {
        val clearedY = ArrayList<Int>()
        for (y in 0 until height) {
            var full = true
            for (z in 0 until depth) for (x in 0 until width) {
                if (grid[index(x, y, z)] == 0) { full = false; break }
            }
            if (full) clearedY.add(y)
        }
        if (clearedY.isEmpty()) return

        for (y in clearedY.sortedDescending()) {
            for (yy in y downTo 1) {
                for (z in 0 until depth) for (x in 0 until width) {
                    grid[index(x, yy, z)] = grid[index(x, yy - 1, z)]
                }
            }
            for (z in 0 until depth) for (x in 0 until width) grid[index(x, 0, z)] = 0
        }
        lines += clearedY.size
        score += clearedY.size * 100 * level
        level = 1 + lines / 10
    }

    private fun spawn() {
        val def = PIECES[random.nextInt(PIECES.size)]
        piece = def.cells.map { it.clone() }
        pieceColor = def.color
        px = width / 2 - 1
        py = height - 2
        pz = depth / 2 - 1
        if (collides(piece, px, py, pz)) {
            state = State.GAME_OVER
        }
    }

    /** Evaluate gravity based on time delta given a fall speed (cells per second). */
    fun tick(dt: Float) {
        if (state != State.PLAYING) return
        tickAccum += dt
        val fallInterval = 1.0f / fallSpeed(level)
        if (tickAccum >= fallInterval) {
            tickAccum = 0f
            step()
        }
    }

    private fun fallSpeed(lvl: Int): Float = 0.5f + (lvl - 1) * 0.25f

    private class PieceDef(val cells: List<IntArray>, val color: Int)

    companion object {
        const val COLOR_CYAN = 1
        const val COLOR_YELLOW = 2
        const val COLOR_PURPLE = 3
        const val COLOR_GREEN = 4
        const val COLOR_RED = 5
        const val COLOR_BLUE = 6
        const val COLOR_ORANGE = 7

        private val PIECES = listOf(
            PieceDef(listOf(intArrayOf(0,0,0), intArrayOf(0,0,1), intArrayOf(0,0,2), intArrayOf(0,0,3)), COLOR_CYAN),
            PieceDef(listOf(intArrayOf(0,0,0), intArrayOf(1,0,0), intArrayOf(0,0,1), intArrayOf(1,0,1)), COLOR_YELLOW),
            PieceDef(listOf(intArrayOf(0,0,0), intArrayOf(1,0,0), intArrayOf(2,0,0), intArrayOf(1,0,1)), COLOR_PURPLE),
            PieceDef(listOf(intArrayOf(0,0,0), intArrayOf(1,0,0), intArrayOf(1,0,1), intArrayOf(2,0,1)), COLOR_GREEN),
            PieceDef(listOf(intArrayOf(1,0,0), intArrayOf(2,0,0), intArrayOf(0,0,1), intArrayOf(1,0,1)), COLOR_RED),
            PieceDef(listOf(intArrayOf(0,0,0), intArrayOf(0,0,1), intArrayOf(1,0,1), intArrayOf(2,0,1)), COLOR_BLUE),
            PieceDef(listOf(intArrayOf(1,0,0), intArrayOf(0,0,1), intArrayOf(1,0,1), intArrayOf(2,0,1)), COLOR_ORANGE)
        )
    }
}
