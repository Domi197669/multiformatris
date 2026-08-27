package com.multiformatris.game

import android.content.Context
import android.opengl.GLSurfaceView
import android.view.GestureDetector
import android.view.MotionEvent

class GameView(context: Context, private val controller: TetrisController) :
    GLSurfaceView(context), GestureDetector.OnGestureListener {

    private val renderer = GameRenderer(controller)
    private val gestureDetector = GestureDetector(context, this)

    init {
        setEGLContextClientVersion(3)
        setRenderer(renderer)
        renderMode = RENDERMODE_CONTINUOUSLY
        isFocusable = true
    }

    override fun onTouchEvent(event: MotionEvent): Boolean {
        gestureDetector.onTouchEvent(event)
        return true
    }

    override fun onDown(e: MotionEvent): Boolean = true

    override fun onShowPress(e: MotionEvent) {}

    override fun onSingleTapUp(e: MotionEvent): Boolean {
        val w = width.toFloat()
        val x = e.x
        if (x < w * 0.25f) {
            controller.rotate(1)   // rotate around Y (spinning the piece)
        } else if (x > w * 0.75f) {
            controller.rotate(2)   // rotate around Z
        }
        return true
    }

    override fun onScroll(e1: MotionEvent?, e2: MotionEvent, distanceX: Float, distanceY: Float): Boolean {
        if (e1 == null) return true
        // swipe to move the piece
        val dx = e2.x - e1.x
        val dy = e2.y - e1.y
        if (kotlin.math.abs(dx) > kotlin.math.abs(dy)) {
            if (dx > 0) controller.move(1, 0, 0) else controller.move(-1, 0, 0)
        } else {
            if (dy > 0) controller.move(0, 0, -1) else controller.move(0, 0, 1)
        }
        return true
    }

    override fun onFling(
        e1: MotionEvent?, e2: MotionEvent, velocityX: Float, velocityY: Float
    ): Boolean {
        if (e1 != null) {
            val dy = e2.y - e1.y
            if (kotlin.math.abs(dy) > 120 && kotlin.math.abs(velocityY) > 300) {
                if (dy < 0) controller.hardDrop()
            }
        }
        return true
    }

    override fun onLongPress(e: MotionEvent) {}
}
