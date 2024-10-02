using UnityEngine;

namespace VEOController
{
    public class StatesHandler
    {
        private readonly Controller player;

        public enum State
        {
            Idle,
            Walking,
            Running,
            Jumping,
            Falling,
            Sliding,
            Dashing,
            Attacking,
            HeavyAttacking,
            ClimbingUp,
            ClimbingDown,
            WallHanging,
            PrepareToWallJump,
            SlopSliding,
            Dead
        }
        public State currentState;

        public StatesHandler(Controller player)
        {
            this.player = player;
            currentState = State.Idle;
        }


        public void TickState()
        {
            if (AnyState()) { return; }

            if (player.isDead)
            {
                UpdateState(State.Dead);
            }
            else if (player.isGrabing)
            {
                if (player.isClimbingUp)
                {
                    UpdateState(State.ClimbingUp);
                }
                else if (player.isClimbingDown)
                {
                    UpdateState(State.ClimbingDown);
                }
                else if (player.isHanging)
                {
                    if (player.isPushingAwayWall)
                        UpdateState(State.PrepareToWallJump);
                    else
                        UpdateState(State.WallHanging);
                }
            }
            else if (player.isSliding)
            {
                UpdateState(State.Sliding);
            }
            else
            {
                if (player.isGrounded) // In Ground
                {
                    if (player.isMoving)
                    {
                        if (player.isRunning)
                            UpdateState(State.Running);
                        else if (player.isWalking)
                            UpdateState(State.Walking);
                    }
                    else if (player.isSlopSliding)
                    {
                        UpdateState(State.SlopSliding);
                    }
                    else
                    {
                        UpdateState(State.Idle);
                    }
                }
                else // In Air
                {

                    if (player.yVel > 0)
                    {
                        UpdateState(State.Jumping);
                    }
                    else
                    {
                        UpdateState(State.Falling);
                    }
                }
            }
        }

        // Override Current State
        private bool AnyState()
        {
            // if you want to override a state, make a new state here on top,
            // example of adding attack state that override any other state,

            if (player.isDashing)
            {
                UpdateState(State.Dashing);
                return true;
            }

            return false;
        }

        private void UpdateState(State state)
        {
            if (currentState == state) { return; }

            currentState = state;
        }
    }
}
