using UnityEngine;

namespace CrunchStreet.Combat
{
    public class BasicAttackHitbox : HitboxOverlap
    {
        [Header("Attack Data")]
        [SerializeField] private BasicAttackSO attackSO;

        protected override IAttackData GetAttackData() => attackSO;
    }
}
