using UnityEngine;

namespace CrunchStreet.Enemies
{
    [CreateAssetMenu(fileName = "NewEnemyStats", menuName = "CrunchStreet/Enemies/Enemy Stats")]
    public class EnemyStatsSO : ScriptableObject
    {
        [Header("Base Stats")]
        public float BaseMaxHealth = 50f;
        public float BaseDamage = 10f;
        public float MovementSpeed = 3f;

        [Header("Visuals")]
        public Material materialVariation;
    }
}
