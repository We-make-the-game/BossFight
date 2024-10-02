using System.Collections;
using UnityEngine;

namespace VEOController
{
    public class CombatFunctions : MonoBehaviour
    {
        private Controller controller;
        private Combat combat;

        private void Awake()
        {
            controller = GetComponent<Controller>();
            combat = controller.combat;
            UpdateHealthBar(combat.health/Combat.maxHealth);
        }

        public void TakeDamage(float damage)
        {
            if (Time.time - combat.lastDamageTime < combat.damageCoolDownTime)
            {
                return;
            }
            Debug.Log("Player TakeDamage");
            // update lastDamageTime
            combat.lastDamageTime = Time.time;
            // update health
            float preHealth = combat.health;
            combat.health = Mathf.Max(0, combat.health - damage);
            UpdateHealthBar(preHealth/Combat.maxHealth, combat.health/Combat.maxHealth);
            // if die
            if (combat.health == 0)
            {
                Die();
                return;
            }
            // damage effect
            OnHit();
        }

        // public void Attack()
        // {
        //     Debug.Log("Attack");
        // }

        // public void Block()
        // {
        //     Debug.Log("Block");
        // }

        // public void Dodge()
        // {
        //     Debug.Log("Dodge");
        // }

        public void Die()
        {
            Debug.Log("Die");
            controller.callbacks.attack.Death?.Invoke();
        }

        private void UpdateHealthBar(float health)
        {
            if (combat.healthBar != null)
            {
                combat.healthBar.fillAmount = health;
            }
        }

        private void UpdateHealthBar(float startFill = 0, float endFill = 0)
        {
            if (combat.healthBar != null)
            {
                StartCoroutine(AnimateFillAmount(startFill, endFill, 1f));
            }
        }

        // Coroutine：fillAmount chaging animation
        private IEnumerator AnimateFillAmount(float startFill, float endFill, float animationDuration)
        {
            float elapsedTime = 0f;
            combat.healthBar.fillAmount = startFill;
            while (elapsedTime < animationDuration)
            {
                combat.healthBar.fillAmount = Mathf.Lerp(startFill, endFill, elapsedTime / animationDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            combat.healthBar.fillAmount = endFill;
        }

        private void OnHit()
        {
            StartCoroutine(FlashCoroutine());
            //controller.callbacks.attack.OnTakeHit?.Invoke();
        }

        private IEnumerator FlashCoroutine()
        {
            float elapsedTime = 0;
            float flashDuration = 1.0f;
            float flashInterval = 0.1f;
            var spriteRenderer = controller.rotation.skin.GetComponent<SpriteRenderer>();
            while (elapsedTime < flashDuration)
            {
                Color color = spriteRenderer.color;
                // change alpha value
                color.a = color.a == 1f ? 0.3f : 1f;
                spriteRenderer.color = color;
                // wait for interval time
                yield return new WaitForSeconds(flashInterval);
                elapsedTime += flashInterval;
            }

            // make sure the sprite is visible
            Color resetColor = spriteRenderer.color;
            resetColor.a = 1f;
            spriteRenderer.color = resetColor;
        }

    }
}