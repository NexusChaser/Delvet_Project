using UnityEngine;

namespace CrunchStreet.Core
{
    public abstract class HealthBase : MonoBehaviour, IDamageable
    {
        [Header("Settings")]
        [SerializeField] protected bool isInvincible = false;

        [Header("Runtime Data (Do not edit directly)")]
        [SerializeField] protected float currentHealth;
        protected float maxHealth;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsInvincible { get => isInvincible; set => isInvincible = value; }

        public virtual void Initialize(float startingMaxHealth)
        {
            maxHealth = startingMaxHealth;
            currentHealth = maxHealth;
        }

        public virtual void TakeDamage(float amount, GameObject instigator)
        {
            if (currentHealth <= 0) return;

            if (!isInvincible)
            {
                currentHealth -= amount;
            }
            
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }

        protected abstract void Die();
    }
}
