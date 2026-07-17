using UnityEngine;
using CrunchStreet.Core;
using Animancer;

namespace CrunchStreet.Combat
{
    [CreateAssetMenu(fileName = "NewBasicAttack", menuName = "CrunchStreet/Combat/Basic Attack")]
    public class BasicAttackSO : ScriptableObject, IAttackData
    {
        [Header("Input")]
        [SerializeField] private CombatInput inputType;

        [Header("Animation Variants")]
        [SerializeField] private ClipTransition[] animations;

        [Header("Stats")]
        [SerializeField] private float damageMultiplier = 1f;

        public CombatInput InputType => inputType;
        public ClipTransition[] Animations => animations;
        public float DamageMultiplier => damageMultiplier;

        public ClipTransition GetRandomAnimation()
        {
            if (animations == null || animations.Length == 0) return null;
            return animations[Random.Range(0, animations.Length)];
        }
    }
}
