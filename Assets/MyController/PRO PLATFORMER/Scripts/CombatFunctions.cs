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
            combat.lastDamageTime = Time.time;
            float preHealth = combat.health;
            combat.health = Mathf.Max(0, combat.health - damage);
            UpdateHealthBar(preHealth/Combat.maxHealth, combat.health/Combat.maxHealth);
            if (combat.health == 0)
            {
                Die();
            }
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
    }
}