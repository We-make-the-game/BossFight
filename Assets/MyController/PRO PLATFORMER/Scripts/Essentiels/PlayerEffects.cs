using System.Collections.Generic;
using UnityEngine;
using System;

namespace VEOController
{
    [Serializable]
    public class PlayerEffects
    {
        public bool enable = true;
        public bool debug = true;
        public AudioSource audioSource;

        [Serializable]
        public class Effect
        {
            public string tag;
            public ParticleSystem VFX;
            public AudioClip SFX;
            [Range(-3, 3)] public float pitch = 1;
            [Range(0, 1)] public float volume = 1;

            string name => tag;
        }
        [SerializeField] public List<Effect> effects = new List<Effect>();

        private Effect GetEffect(string name)
        {
            foreach (var item in effects)
            {
                if (item.tag.Equals(name))
                    return item;
            }

            if (debug)
                Debug.LogWarning("The effect with the tag ''" + name + "'' does not exist. \nPlease create it from the Effect tab");

            return null;
        }

        public ParticleSystem GetVFX(string tag)
        {
            Effect effect = GetEffect(tag);

            if (effect != null && effect.VFX != null)
            {
                return effect.VFX;
            }
            else if (debug && effect != null && effect.VFX == null)
            {
                Debug.LogWarning("Failed to get VFX. \nThe VFX with the tag ''" + tag + "'' is not set.");
            }

            return null;
        }
        public AudioClip GetSFX(string tag)
        {
            Effect effect = GetEffect(tag);

            if (effect != null && effect.SFX != null)
            {
                return effect.SFX;
            }
            else if (debug && effect != null && effect.SFX == null)
            {
                Debug.LogWarning("Failed to get SFX. \nThe SFX with the tag ''" + tag + "'' is not set.");
            }

            return null;
        }

        public void PlayVFX(string tag)
        {
            if (!enable) { return; }

            Effect effect = GetEffect(tag);

            if (effect != null && effect.VFX != null)
            {
                effect.VFX.gameObject.SetActive(true);
                effect.VFX.Play();
            }

            else if (debug && effect != null && effect.VFX == null)
                Debug.LogWarning("Failed to play VFX. \nThe VFX with the tag ''" + tag + "'' is not set.");

        }
        public void StopVFX(string tag)
        {
            if (!enable) { return; }

            Effect effect = GetEffect(tag);

            if (effect != null && effect.VFX != null)
                effect.VFX.Stop();

            else if (debug && effect != null && effect.VFX == null)
                Debug.LogWarning("Failed to stop VFX. \nThe VFX with the tag ''" + tag + "'' is not set.");


        }

        public void SpawnVFX(string tag, Vector2 position)
        {
            if (!enable) { return; }

            Effect effect = GetEffect(tag);

            if (effect != null && effect.VFX != null)
            {
                // Better to use object pooling for better performance
                UnityEngine.Object.Instantiate(effect.VFX, position, Quaternion.identity);
            }
            else if (debug && effect.VFX == null)
            {
                Debug.LogWarning("Failed to spawn VFX. \nThe VFX with the tag ''" + tag + "'' is not set.");
            }
        }

        public void PlaySFX(String tag, bool loop = false)
        {
            if (!enable) { return; }

            Effect effect = GetEffect(tag);

            if (effect != null && effect.SFX != null)
            {
                audioSource.clip = effect.SFX;
                audioSource.loop = loop;
                audioSource.volume = effect.volume;
                audioSource.pitch = effect.pitch;
                audioSource.Play();
            }
        }
        public void StopSFX()
        {
            if (audioSource.clip == null) { return; }

            audioSource.Stop();
        }

    }
}