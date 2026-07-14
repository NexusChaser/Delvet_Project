using UnityEngine;

namespace CrunchStreet.Player
{
    public class CharacterBlackboard : MonoBehaviour
    {
        public bool CanMove = true;
        public bool IsGrounded = true;
        public bool IsAttacking = false;
        public bool IsDodging = false;
    }
}
