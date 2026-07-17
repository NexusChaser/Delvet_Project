using UnityEngine;
using CrunchStreet.Core;

namespace CrunchStreet.Enemies
{
    public class EnemyHealth : HealthBase
    {
        [Header("Enemy Data")]
        [SerializeField] private EnemyStatsSO enemyStats;

        private void Start()
        {
            if (enemyStats != null)
            {
                // Aquí aplicaremos después el multiplicador global multijugador.
                // Por ejemplo: float multiplier = GameManager.Instance.PlayerCount;
                float multiplier = 1f; 
                Initialize(enemyStats.BaseMaxHealth * multiplier);
            }
            else
            {
                Debug.LogWarning("EnemyStatsSO is missing!", this);
                Initialize(50f);
            }
        }

        protected override void Die()
        {
            Debug.Log($"Enemy {gameObject.name} died! Drop loot, add points, play particle, destroy.");
            // Implementar lógica de muerte de enemigo
            Destroy(gameObject);
        }
    }
}
