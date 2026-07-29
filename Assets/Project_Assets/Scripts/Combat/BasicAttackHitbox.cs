using UnityEngine;

namespace CrunchStreet.Combat
{
    public class BasicAttackHitbox : HitboxOverlap
    {
        [Header("Attack Data")]
        [SerializeField] private BasicAttackSO attackSO;

        protected override IAttackData GetAttackData() => attackSO;

        private void Awake()
        {
            if (attackSO != null && attackSO.Animations != null)
            {
                foreach (var anim in attackSO.Animations)
                {
                    if (anim != null)
                    {
                        anim.Events.SetShouldNotModifyReason(null);
                        anim.Events.SetCallback("EnableHitbox", EnableHitbox);
                        anim.Events.SetCallback("DisableHitbox", DisableHitbox);
                    }
                }
            }
        }
    }
}
