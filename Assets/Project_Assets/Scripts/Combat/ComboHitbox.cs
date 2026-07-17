using UnityEngine;

namespace CrunchStreet.Combat
{
    public class ComboHitbox : HitboxOverlap
    {
        [Header("Combo Data")]
        [SerializeField] private ComboSequenceSO comboSO;

        protected override IAttackData GetAttackData() => comboSO;
    }
}
