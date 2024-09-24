using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;

namespace VEOController
{

    [Serializable]
    public class PlayerAnimations
    {

        // Private
        private string currentState;
        private float lockTime = 0f;
        private bool locked = false;
        private Dictionary<string, int> HashedAnimations;
        private AnimationClip[] clips;
        private Coroutine WaitAnimation = null;

        //All animations in the animator will be added automatically to the animations list
        public List<string> animationClips;
        public Animator animator;
        public Controller controller;

        // Private Functions
        public void PopulateClips()
        {
            if (animator == null) { return; }

            clips = animator.runtimeAnimatorController.animationClips;
            animationClips = new List<string>();

            foreach (var clip in clips)
            {
                if (animationClips.Contains(clip.name)) { return; }

                animationClips.Add(clip.name);
            }

            animationClips.Sort();

            GenerateHashedClips();
        }
        private void GenerateHashedClips()
        {
            HashedAnimations = new Dictionary<string, int>();

            foreach (var clip in clips)
            {
                if (HashedAnimations.ContainsKey(clip.name)) { return; }

                HashedAnimations.Add(clip.name, Animator.StringToHash(clip.name));
            }
        }
        private AnimationClip GetClip(string name)
        {
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == name)
                {
                    return clip;
                }
            }

            return null;
        }
        private IEnumerator WaitForAnimation(Action onFinish)
        {
            locked = true;

            while (lockTime > 0)
            {
                lockTime -= Time.deltaTime;
                yield return null;
            }

            lockTime = 0;
            locked = false;

            onFinish?.Invoke();
        }



        // Public Functions
        public void UpdateAnimations()
        {
            if (!controller.enableAnimations) { return; }

            if (locked) { return; }

            // The Important First
            // Always put the animation that you want it to override other animations on top

            switch (controller.state.currentState)
            {
                case StatesHandler.State.Idle:
                    PlayLoopAnimation("Idle", 0);
                    break;
                case StatesHandler.State.Walking:
                    PlayLoopAnimation("Walking", 0);
                    break;
                case StatesHandler.State.Running:
                    PlayLoopAnimation("Running", 0);
                    break;
                case StatesHandler.State.Sliding:
                    PlayLoopAnimation("Wall Sliding", 0);
                    break;
                case StatesHandler.State.Jumping:
                    PlayLoopAnimation("Ground Jump", 0);
                    break;
                case StatesHandler.State.Falling:
                    PlayLoopAnimation("Falling", 0);
                    break;
                case StatesHandler.State.Dashing:
                    PlayLoopAnimation("Dashing", 0);
                    break;
                case StatesHandler.State.ClimbingUp:
                    PlayLoopAnimation("Climbing Down", 0);
                    break;
                case StatesHandler.State.ClimbingDown:
                    PlayLoopAnimation("Climbing Down", 0);
                    break;
                case StatesHandler.State.WallHanging:
                    PlayLoopAnimation("Wall Grabing", 0);
                    break;
                case StatesHandler.State.PrepareToWallJump:
                    PlayLoopAnimation("Wall Sliding", 0);
                    break;
            }
        }
        public void PlayAnimation(string animation, float transitionTime = 0f, Action onFinish = null)
        {
            if (!controller.enableAnimations) { return; }

            if (animation == currentState) { return; }

            if (!animationClips.Contains(animation))
            {
                Debug.LogError("The animation (" + animation + "). Does not exist in the animator");
                return;
            }


            lockTime = GetClip(animation).length;

            if (transitionTime > lockTime)
            {
                Debug.LogError("animation transition time can't be longer than the clip (" + animation + ") duration");
                return;
            }

            if (WaitAnimation != null)
                controller.StopCoroutine(WaitAnimation);

            WaitAnimation = controller.StartCoroutine(WaitForAnimation(onFinish));

            animator.CrossFade(HashedAnimations[animation], transitionTime, 0);
            currentState = animation;
        }
        public void PlayLoopAnimation(string animation, float transitionTime = 0)
        {
            if (!controller.enableAnimations) { return; }

            if (locked || animation == currentState) { return; }

            if (!animationClips.Contains(animation))
            {
                Debug.LogError("The animation (" + animation + "). Does not exist in the animator");
                return;
            }


            animator.CrossFade(HashedAnimations[animation], transitionTime, 0);
            currentState = animation;
        }
        public void StopLoopAnimation(string animation)
        {
            if (!controller.enableAnimations) { return; }

            if (animation != currentState) { return; }

            if (!animationClips.Contains(animation))
            {
                Debug.LogError("The animation (" + animation + "). Does not exist in the animator");
                return;
            }

            locked = false;
        }

        // Public Helprs
        public void SetSpeed(string paramater, float speed, float dampTime = 0, float deltaTime = 0)
        {
            if (locked) { return; }

            animator.SetFloat(paramater, speed, dampTime, deltaTime);
            lockTime /= speed;
        }
        public void Stop()
        {
            animator.enabled = false;
        }
        public void Resume()
        {
            animator.enabled = true;
        }

    }
}