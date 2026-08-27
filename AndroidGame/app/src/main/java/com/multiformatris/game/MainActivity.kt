package com.multiformatris.game

import android.app.Activity
import android.graphics.Color
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.view.Gravity
import android.view.View
import android.view.Window
import android.view.WindowManager
import android.widget.Button
import android.widget.FrameLayout
import android.widget.LinearLayout
import android.widget.TextView

class MainActivity : Activity() {

    private lateinit var controller: TetrisController
    private lateinit var gameView: GameView
    private lateinit var hud: TextView
    private lateinit var overlay: LinearLayout
    private lateinit var btnPrimary: Button
    private lateinit var btnPause: Button

    private val handler = Handler(Looper.getMainLooper())
    private val refreshHud = object : Runnable {
        override fun run() {
            updateHud()
            handler.postDelayed(this, 250)
        }
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        requestWindowFeature(Window.FEATURE_NO_TITLE)
        window.setFlags(
            WindowManager.LayoutParams.FLAG_FULLSCREEN,
            WindowManager.LayoutParams.FLAG_FULLSCREEN
        )
        window.addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)

        controller = TetrisController()
        gameView = GameView(this, controller)

        val root = FrameLayout(this)
        root.addView(gameView, FrameLayout.LayoutParams(
            FrameLayout.LayoutParams.MATCH_PARENT,
            FrameLayout.LayoutParams.MATCH_PARENT
        ))

        // HUD (score / level) at top
        hud = TextView(this).apply {
            textSize = 18f
            setTextColor(Color.WHITE)
            setShadowLayer(4f, 0f, 0f, Color.BLACK)
        }
        val hudParams = FrameLayout.LayoutParams(
            FrameLayout.LayoutParams.WRAP_CONTENT,
            FrameLayout.LayoutParams.WRAP_CONTENT,
            Gravity.TOP or Gravity.CENTER_HORIZONTAL
        )
        hudParams.setMargins(0, 40, 0, 0)
        root.addView(hud, hudParams)

        // Options row (pause + drop) at top-right
        val optionsRow = LinearLayout(this).apply { orientation = LinearLayout.HORIZONTAL }
        btnPause = Button(this).apply {
            text = "PAUSE"
            setOnClickListener {
                if (controller.state == TetrisController.State.PLAYING) {
                    controller.pause()
                } else if (controller.state == TetrisController.State.PAUSED) {
                    controller.resume()
                }
            }
        }
        optionsRow.addView(btnPause, LinearLayout.LayoutParams(
            LinearLayout.LayoutParams.WRAP_CONTENT, LinearLayout.LayoutParams.WRAP_CONTENT))

        val optionsParams = FrameLayout.LayoutParams(
            FrameLayout.LayoutParams.WRAP_CONTENT,
            FrameLayout.LayoutParams.WRAP_CONTENT,
            Gravity.TOP or Gravity.END
        )
        optionsParams.setMargins(0, 40, 20, 0)
        root.addView(optionsRow, optionsParams)

        // Center overlay: game title / start / game over
        overlay = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            gravity = Gravity.CENTER
            setBackgroundColor(Color.argb(200, 0, 0, 0))
        }
        val title = TextView(this).apply {
            text = "MULTIFORMATRIS"
            textSize = 34f
            gravity = Gravity.CENTER
            setTextColor(Color.WHITE)
        }
        val subtitle = TextView(this).apply {
            text = "3D Tetris"
            textSize = 20f
            gravity = Gravity.CENTER
            setTextColor(Color.LTGRAY)
        }
        btnPrimary = Button(this).apply {
            text = "PLAY"
            textSize = 22f
            setOnClickListener { startGame() }
        }
        overlay.addView(title)
        overlay.addView(subtitle)
        overlay.addView(btnPrimary, LinearLayout.LayoutParams(
            400, LinearLayout.LayoutParams.WRAP_CONTENT))

        val overlayParams = FrameLayout.LayoutParams(
            FrameLayout.LayoutParams.MATCH_PARENT,
            FrameLayout.LayoutParams.MATCH_PARENT
        )
        root.addView(overlay, overlayParams)

        setContentView(root)
    }

    private fun startGame() {
        controller.start()
        overlay.visibility = View.GONE
    }

    private fun updateHud() {
        hud.text = "Score: ${controller.score}   Level: ${controller.level}   Lines: ${controller.lines}"

        if (controller.state == TetrisController.State.GAME_OVER) {
            overlay.visibility = View.VISIBLE
            btnPrimary.text = "RETRY"
            overlay.getChildAt(0)?.let { (it as? TextView)?.text = "GAME OVER" }
        }
        btnPause.text = if (controller.state == TetrisController.State.PAUSED) "RESUME" else "PAUSE"
    }

    override fun onResume() {
        super.onResume()
        gameView.onResume()
        handler.post(refreshHud)
    }

    override fun onPause() {
        super.onPause()
        gameView.onPause()
        handler.removeCallbacks(refreshHud)
    }
}
