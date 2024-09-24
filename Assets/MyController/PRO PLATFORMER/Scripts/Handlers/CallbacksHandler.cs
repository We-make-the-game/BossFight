using UnityEngine;
using UnityEngine.Events;
using System.Collections;

// Here you can set actions to callbacks
// ex : you can set jump animation & sounds effect & visual effect
// you can check all the available callbacks from the documentation or from the inspector

namespace VEOController
{
    public class CallbacksHandler : MonoBehaviour
    {
        private Controller controller;

        private PlayerCallbacks callbacks; // This holds all the built in callbacks suck as OnGroundJump
        private PlayerEffects effects; // This holds animation functions such as PlaySFX, PlayVFX
        private PlayerAnimations anim; // This holds animation functions such as PlayAnimation


        private void Awake()
        {
            controller = GetComponent<Controller>();
            callbacks = controller.callbacks;
            effects = controller.effects;
            anim = controller.anim;

            SetCallbacks();
        }

        private void SetCallbacks()
        {
            callbacks.collision.OnGroundHit.AddListener(OnGroundHit);
            callbacks.collision.OnWallHit.AddListener(OnWallHit);

            callbacks.jump.OnGroundJump.AddListener(OnGroundJump);
            callbacks.jump.OnAirJump.AddListener(OnDoubleJump);
            callbacks.jump.OnWallJump.AddListener(OnWallJump);
            callbacks.jump.OnDashJump.AddListener(OnDashJump);

            callbacks.movement.OnWalkStart.AddListener(OnStartWalk);
            callbacks.movement.OnWalkStop.AddListener(OnStopWalk);

            callbacks.dash.OnDashStart.AddListener(OnStartDash);
            callbacks.dash.OnDashEnd.AddListener(OnEndDash);

            callbacks.wallGrab.OnClimbStart.AddListener(OnClimbStart);
            callbacks.wallGrab.OnClimbStop.AddListener(OnClimbStop);

            callbacks.wallGrab.OnSlideStart.AddListener(OnSlideStart);
            callbacks.wallGrab.OnSlideStop.AddListener(OnSlideStop);

            callbacks.attack.OnAttack.AddListener(OnAttack);
            callbacks.attack.OnHeavyAttackStart.AddListener(OnHeavyAttackStart);
            callbacks.attack.OnHeavyAttackEnd.AddListener(OnHeavyAttackEnd);
            callbacks.attack.OnAirAttack.AddListener(OnAirAttack);
            callbacks.attack.OnWallAttack.AddListener(OnWallAttack);
        }

        // Walk
        public void OnStartWalk()
        {
            effects.PlayVFX("Walk");
        }
        public void OnStopWalk()
        {
            effects.StopVFX("Walk");
        }

        // Collision Callbacks
        public void OnGroundHit()
        {
            if (controller.isAttacking) return;
            if (!controller.onSteepSlop)
            {
                controller.anim.PlayAnimation("Landing", 0);
            }

            effects.SpawnVFX("Land", controller.feetPosition);
        }
        public void OnWallHit()
        {
            effects.SpawnVFX("Wall Hit", controller.wallHitPoint);
        }

        // Dash Callbacks
        public void OnStartDash()
        {
            effects.PlayVFX("Dash");
        }
        public void OnEndDash()
        {
            effects.StopVFX("Dash");
        }
        public void OnDashReady()
        {

        }
        // Slide & Climb
        public void OnSlideStart()
        {
            effects.PlayVFX("Slide");
        }
        public void OnSlideStop()
        {
            effects.StopVFX("Slide");
        }

        public void OnClimbStart()
        {
            effects.PlayVFX("Climb");
        }
        public void OnClimbStop()
        {
            effects.StopVFX("Climb");
        }

        // Jump Callbacks
        public void OnGroundJump()
        {
            anim.PlayAnimation("Ground Jump", 0);
            effects.SpawnVFX("Jump", controller.feetPosition);
            effects.PlaySFX("Jump");
        }
        public void OnDoubleJump()
        {
            anim.PlayAnimation("Double Jump", 0);
            effects.SpawnVFX("Jump", controller.feetPosition);
            effects.PlaySFX("Jump");
        }
        public void OnWallJump()
        {
            anim.PlayAnimation("Wall Jump", 0);
            effects.SpawnVFX("Wall Jump", controller.feetPosition);
        }

        public void OnDashJump()
        {
            anim.PlayAnimation("Wall Jump", 0);
            effects.SpawnVFX("Wall Jump", controller.feetPosition);
        }

        // Attack Callbacks
        private void StartAttack()
        {
            controller.isAttacking = true;
        }
        private void EndAttack()
        {
            controller.isAttacking = false;
        }
        public void OnAttack()
        {
            StartAttack();
            anim.PlayAnimation("Attack", 0, EndAttack);
        }

        public void OnHeavyAttackStart()
        {
            effects.PlayVFX("HeavyAttack");
        }

        public void OnHeavyAttackEnd()
        {
            StartAttack();
            effects.StopVFX("HeavyAttack");
            anim.PlayAnimation("Heavy Attack", 0, EndAttack);
        }

        public void OnAirAttack()
        {
            StartAttack();
            anim.PlayAnimation("Air Attack", 0, EndAttack);
        }

        public void OnWallAttack()
        {
            StartAttack();
            controller.isAttacking = true;
            anim.PlayAnimation("Wall Attack", 0, EndAttack);
        }
    }
}

