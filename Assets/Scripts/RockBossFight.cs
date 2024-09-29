using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VEOController
{
    public class RockBossFight : MonoBehaviour
    {
        public GameObject player; // Player
        public Image healthBar; // Boss Health Bar
        private Animator RockAni; // Boss Animator
        public float attackRange = 20f; 
        public float attackCooldown = 2f;
        public float moveSpeed = 5f; 
        private bool isMoving = false;
        private bool isAttacking = false;
        private bool isDead = false; 
        private float preAttackTime = 0f;
        private float maxHealth = 100f;
        private float health;
        private Controller playerCtrl;
        private CombatFunctions playerCombat;

        // Start is called before the first frame update
        void Start()
        {
            health = maxHealth;
            RockAni = GetComponent<Animator>();
            playerCtrl = player.GetComponent<Controller>();
            playerCombat = player.GetComponent<CombatFunctions>();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateHealthBar(health);
            UpdateMovement();
            if (!isMoving && Time.time - preAttackTime > attackCooldown)
            {
                AttackPlayer();
            }
        }

        void UpdateHealthBar(float health)
        {
            if (healthBar != null)
            {
                healthBar.fillAmount = health / maxHealth;
            }
        }

        void UpdateMovement()
        {
            float distanceToPlayer = player.transform.position.x - transform.position.x;
            if (player.transform.position.x < transform.position.x)
                transform.localScale = new Vector3(-2.5f, 2.5f, 2.5f); // move left
            else
                transform.localScale = new Vector3(2.5f, 2.5f, 2.5f); // move right

            if (Mathf.Abs(distanceToPlayer) > attackRange)
            {
                isMoving = true;
                transform.position += new Vector3(moveSpeed * Time.deltaTime * distanceToPlayer / Mathf.Abs(distanceToPlayer), 0, 0);
            }
            else
            {
                isMoving = false;
            }
            
            RockAni.SetBool("isWalking", isMoving);
        }

        void AttackPlayer()
        {
            preAttackTime = Time.time;
            RockAni.SetTrigger("Attack1");
            StartCoroutine(WaitCoroutine(0.03f));
            float distanceToPlayer = player.transform.position.x - transform.position.x;
            if (Mathf.Abs(distanceToPlayer) <= attackRange)
            {
                playerCombat.TakeDamage(10);
            }
        }

        // Boss受到攻击
        public void TakeHit()
        {
            if (!isDead)
            {
                //animator.Play("RockTakeHit");
                // 根据游戏设计的需求，添加受伤逻辑，比如减少Boss的生命值等
            }
        }

        public void Die()
        {
            if (!isDead)
            {
                isDead = true;
                //animator.Play("RockDeath");
            }
        }

        private IEnumerator WaitCoroutine(float time)
        {
            yield return new WaitForSecondsRealtime(time);
        }
    }
}
