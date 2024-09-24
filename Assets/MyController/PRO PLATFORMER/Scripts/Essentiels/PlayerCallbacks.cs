using System;
using UnityEngine.Events;

namespace VEOController
{
    [Serializable]
    public class PlayerCallbacks
    {
        [Serializable]
        public class AttackCallbacks
        {
            public UnityEvent OnAttack;
            public UnityEvent OnHeavyAttackStart;
            public UnityEvent OnHeavyAttackEnd;
            public UnityEvent OnAirAttack;
            public UnityEvent OnWallAttack;
            public UnityEvent OnTakeHit;
        }
        public AttackCallbacks attack;
        [Serializable]
        public class MovementCallbacks
        {
            public UnityEvent OnWalkStart;
            public UnityEvent OnWalkStop;
        }
        public MovementCallbacks movement;
        [Serializable]
        public class JumpCallbacks
        {
            // Jump Callbacks
            public UnityEvent OnGroundJump;
            public UnityEvent OnAirJump;
            public UnityEvent OnWallJump;
            public UnityEvent OnEndWallJump;
            public UnityEvent OnAirJumpReady;
            public UnityEvent OnDashJump;
        }
        public JumpCallbacks jump;

        [Serializable]
        public class CollisionCallbacks
        {
            public UnityEvent OnWallHit;
            public UnityEvent OnGroundHit;
        }
        public CollisionCallbacks collision;

        [Serializable]
        public class WallGrabCallbacks
        {
            public UnityEvent OnClimbStart;
            public UnityEvent OnClimbStop;

            public UnityEvent OnSlideStart;
            public UnityEvent OnSlideStop;

            public UnityEvent OnWallGrab;
        }
        public WallGrabCallbacks wallGrab;

        [Serializable]
        public class DashCallbacks
        {
            // Dash Callbacks
            public UnityEvent OnDashStart;
            public UnityEvent OnDashEnd;
            public UnityEvent OnDashReady;
            public UnityEvent OnDashInterruption;
        }
        public DashCallbacks dash;

    }
}