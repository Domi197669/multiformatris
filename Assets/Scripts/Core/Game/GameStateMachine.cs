using System;
using UnityEngine;

namespace Multiformatris.Core.Game
{
    public enum GameState
    {
        Menu,
        Spawning,
        Falling,
        Locking,
        Clearing,
        RotatingWell,
        Paused,
        GameOver
    }

    public class GameStateMachine
    {
        public event Action<GameState, GameState> OnStateChanged;

        public GameState CurrentState { get; private set; }
        public GameState PreviousState { get; private set; }

        public GameStateMachine(GameState startState = GameState.Menu)
        {
            CurrentState = startState;
        }

        public bool CanTransitionTo(GameState newState)
        {
            return true;
        }

        public void TransitionTo(GameState newState)
        {
            if (!CanTransitionTo(newState))
            {
                Debug.LogWarning($"Cannot transition from {CurrentState} to {newState}");
                return;
            }

            PreviousState = CurrentState;
            CurrentState = newState;
            OnStateChanged?.Invoke(PreviousState, CurrentState);
        }

        public bool IsPlaying()
        {
            return CurrentState == GameState.Falling ||
                   CurrentState == GameState.Spawning ||
                   CurrentState == GameState.Locking ||
                   CurrentState == GameState.Clearing;
        }
    }
}
