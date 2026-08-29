package com.multiformatris.game

import android.app.Activity
import android.content.Context
import android.content.SharedPreferences
import android.graphics.Color
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.view.Gravity
import android.view.MotionEvent
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
    private lateinit var btnDown: Button
    private lateinit var btnRotate: Button
    private lateinit var btnRedeem: Button
    private lateinit var prizeToast: TextView

    private lateinit var prefs: SharedPreferences

    private val handler = Handler(Looper.getMainLooper())
    private val refreshHud = object : Runnable {
        override fun run() {
            updateHud()
            handler.postDelayed(this, 250)
        }
    }
    private val hideToast = object : Runnable {
        override fun run() { prizeToast.visibility = View.GONE }
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
        prefs = getSharedPreferences("multiformatris", Context.MODE_PRIVATE)
        controller.bestScore = prefs.getInt("bestScore", 0)
        controller.onPrize = { total -> showPrizeToast(total) }
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

        // Touch control pads in the bottom corners (kept clear of the well, which
        // is centered on screen, so they never cover the falling pieces).
        btnDown = controlButton("▼ BAJAR") { controller.move(0, -1, 0) }
        val downParams = FrameLayout.LayoutParams(240, 150, Gravity.BOTTOM or Gravity.START)
        downParams.setMargins(24, 0, 0, 30)
        root.addView(btnDown, downParams)

        btnRotate = controlButton("↻ GIRAR") { controller.rotate(1) }
        val rotateParams = FrameLayout.LayoutParams(240, 150, Gravity.BOTTOM or Gravity.END)
        rotateParams.setMargins(0, 0, 24, 30)
        root.addView(btnRotate, rotateParams)

        // Redeem button: convert a golden prize into a wildcard piece.
        btnRedeem = Button(this).apply {
            text = "🏆 CANJEAR"
            textSize = 16f
            setTextColor(Color.BLACK)
            setBackgroundColor(Color.argb(230, 255, 215, 64)) // golden
            setOnClickListener {
                if (controller.redeemWildcard()) {
                    showToast("¡Comodín dorado activado!")
                }
            }
        }
        val redeemParams = FrameLayout.LayoutParams(220, 120, Gravity.BOTTOM or Gravity.CENTER_HORIZONTAL)
        redeemParams.setMargins(0, 0, 0, 34)
        root.addView(btnRedeem, redeemParams)

        // Prize notification (floating banner, not part of the game view).
        prizeToast = TextView(this).apply {
            textSize = 22f
            gravity = Gravity.CENTER
            setTextColor(Color.argb(255, 255, 220, 64))
            setShadowLayer(6f, 0f, 0f, Color.BLACK)
            visibility = View.GONE
        }
        val toastParams = FrameLayout.LayoutParams(
            FrameLayout.LayoutParams.WRAP_CONTENT,
            FrameLayout.LayoutParams.WRAP_CONTENT,
            Gravity.CENTER_HORIZONTAL or Gravity.BOTTOM
        )
        toastParams.setMargins(0, 0, 0, 200)
        root.addView(prizeToast, toastParams)

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

    /** Builds a semi-transparent control pad that repeats [action] while held. */
    private fun controlButton(label: String, action: () -> Unit): Button {
        val b = Button(this)
        b.text = label
        b.textSize = 18f
        b.setTextColor(Color.WHITE)
        b.setBackgroundColor(Color.argb(150, 20, 30, 60))
        b.elevation = 0f

        val repeat = object : Runnable {
            override fun run() {
                action()
                handler.postDelayed(this, 80)
            }
        }
        b.setOnTouchListener { v, event ->
            when (event.actionMasked) {
                MotionEvent.ACTION_DOWN -> {
                    action()
                    handler.postDelayed(repeat, 400)
                    true
                }
                MotionEvent.ACTION_UP, MotionEvent.ACTION_CANCEL -> {
                    handler.removeCallbacks(repeat)
                    true
                }
                else -> false
            }
        }
        return b
    }

    private fun updateHud() {
        // persist a new high score
        if (controller.score > controller.bestScore) {
            controller.bestScore = controller.score
            prefs.edit().putInt("bestScore", controller.bestScore).apply()
        }

        val wildcard = if (controller.wildcardActive) "  [COMODÍN]" else ""
        hud.text = "Score: ${controller.score}   Mejor: ${controller.bestScore}" +
            "   Nivel: ${controller.level}   Líneas: ${controller.lines}" +
            "   🏆 ${controller.prizes}$wildcard"

        // redeem only enabled while playing and with prizes available
        val canRedeem = controller.state == TetrisController.State.PLAYING &&
            controller.prizes > 0 && !controller.wildcardActive
        btnRedeem.isEnabled = canRedeem
        btnRedeem.alpha = if (canRedeem) 1f else 0.4f
        btnRedeem.text = "🏆 CANJEAR (${controller.prizes})"

        if (controller.state == TetrisController.State.GAME_OVER) {
            overlay.visibility = View.VISIBLE
            btnPrimary.text = "RETRY"
            val finalScore = "GAME OVER - Puntos: ${controller.score}"
            overlay.getChildAt(0)?.let { (it as? TextView)?.text = finalScore }
        }
        btnPause.text = if (controller.state == TetrisController.State.PAUSED) "RESUME" else "PAUSE"
    }

    private fun showToast(msg: String) {
        prizeToast.text = msg
        prizeToast.visibility = View.VISIBLE
        handler.removeCallbacks(hideToast)
        handler.postDelayed(hideToast, 1800)
    }

    private fun showPrizeToast(total: Int) {
        showToast("★ ¡PREMIO! Copas doradas: $total")
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
