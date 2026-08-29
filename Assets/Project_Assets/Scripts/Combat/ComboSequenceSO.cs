using UnityEngine;
using Animancer;

namespace CrunchStreet.Combat
{
    [CreateAssetMenu(fileName = "NewComboSequence", menuName = "CrunchStreet/Combat/Combo Sequence")]
    public class ComboSequenceSO : ScriptableObject, IAttackData
    {
        [Header("Combo Requirements")]
        [SerializeField] private CombatInput[] sequence;
        [SerializeField] private bool isAerial = false;

        [Header("Final Animation")]
        [SerializeField] private ClipTransition finalAnimation;

        [Header("Stats")]
        [SerializeField] private float damageMultiplier = 1.5f;

        public CombatInput[] Sequence => sequence;
        public bool IsAerial => isAerial;
        public ClipTransition FinalAnimation => finalAnimation;
        public float DamageMultiplier => damageMultiplier;
    }
}
