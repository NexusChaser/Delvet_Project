using UnityEngine;
using CrunchStreet.Player;

namespace CrunchStreet.Combat
{
    public abstract class ComboListener : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerCombat playerCombat;

        private void OnEnable()
        {
            if (playerCombat != null)
            {
                playerCombat.OnAttackExecuted += HandleAttackExecuted;
            }
        }

        private void OnDisable()
        {
            if (playerCombat != null)
            {
                playerCombat.OnAttackExecuted -= HandleAttackExecuted;
            }
        }

        private void HandleAttackExecuted(IAttackData attackData)
        {
            switch (this)
            {
                case BasicAttackListener basicListener:
                    if (attackData is BasicAttackSO basicSO && basicSO == basicListener.TargetAttack)
                    {
                        OnExecuteAttack();
                    }
                    break;
                case ComboSequenceListener comboListener:
                    if (attackData is ComboSequenceSO comboSO && comboSO == comboListener.TargetCombo)
                    {
                        OnExecuteAttack();
                    }
                    break;
            }
        }

        protected abstract void OnExecuteAttack();
    }

    public abstract class BasicAttackListener : ComboListener
    {
        [Header("Target Attack")]
        [SerializeField] private BasicAttackSO targetAttack;

        public BasicAttackSO TargetAttack => targetAttack;
    }

    public abstract class ComboSequenceListener : ComboListener
    {
        [Header("Target Combo")]
        [SerializeField] private ComboSequenceSO targetCombo;

        public ComboSequenceSO TargetCombo => targetCombo;
    }
}
