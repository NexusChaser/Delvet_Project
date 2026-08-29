using UnityEngine;

namespace CrunchStreet.Combat
{
    public class ComboHitbox : HitboxOverlap
    {
        [Header("Combo Data")]
        [SerializeField] private ComboSequenceSO comboSO;

        protected override IAttackData GetAttackData() => comboSO;

        private void Awake()
        {
            if (comboSO != null && comboSO.FinalAnimation != null)
            {
                comboSO.FinalAnimation.Events.SetShouldNotModifyReason(null);
                comboSO.FinalAnimation.Events.SetCallback("EnableHitbox", EnableHitbox);
                comboSO.FinalAnimation.Events.SetCallback("DisableHitbox", DisableHitbox);
            }
        }
    }
}
