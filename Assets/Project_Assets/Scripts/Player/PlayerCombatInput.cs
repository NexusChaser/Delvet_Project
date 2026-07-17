using UnityEngine;
using UnityEngine.InputSystem;

namespace CrunchStreet.Player
{
    [RequireComponent(typeof(PlayerCombat))]
    public class PlayerCombatInput : MonoBehaviour
    {
        private PlayerCombat playerCombat;

        private void Awake()
        {
            playerCombat = GetComponent<PlayerCombat>();
        }

        // Se invoca desde el UnityEvent del componente PlayerInput (New Input System)
        // o mapeándolo por código.
        public void OnLightAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                playerCombat.ExecuteInput(Combat.CombatInput.Light);
            }
        }

        public void OnHeavyAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                playerCombat.ExecuteInput(Combat.CombatInput.Heavy);
            }
        }
    }
}
