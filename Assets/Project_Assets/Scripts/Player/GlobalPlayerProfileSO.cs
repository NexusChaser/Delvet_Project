using UnityEngine;

namespace CrunchStreet.Player
{
    [CreateAssetMenu(fileName = "GlobalPlayerProfile", menuName = "CrunchStreet/Player/Global Player Profile")]
    public class GlobalPlayerProfileSO : ScriptableObject
    {
        [Header("Persistent Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float baseDamage = 10f;
        
        [Header("Moveset")]
        [SerializeField] private PlayerMovesetSO moveset;

        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float BaseDamage => baseDamage;
        public PlayerMovesetSO Moveset => moveset;
    }
}
