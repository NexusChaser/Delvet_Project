using UnityEngine;
using UnityEngine.InputSystem;
using Animancer;

namespace CrunchStreet.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CharacterBlackboard))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float rotationSpeed = 10f;

        [Header("Animations")]
        public AnimancerComponent animancer;
        public AnimationClip idleClip;
        public AnimationClip walkClip;
        
        private Rigidbody rb;
        private CharacterBlackboard blackboard;
        private Vector2 moveInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            blackboard = GetComponent<CharacterBlackboard>();
            
            if (animancer == null)
            {
                // Try to find it in children (like the Model)
                animancer = GetComponentInChildren<AnimancerComponent>();
            }
        }

        // Called via Unity Events from PlayerInput
        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        private void FixedUpdate()
        {
            if (!blackboard.CanMove)
            {
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                PlayAnimation(idleClip);
                return;
            }

            Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            // Apply Movement
            Vector3 targetVelocity = moveDir * moveSpeed;
            targetVelocity.y = rb.linearVelocity.y; // Keep vertical velocity
            rb.linearVelocity = targetVelocity;

            // Apply Rotation
            if (moveDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                PlayAnimation(walkClip);
            }
            else
            {
                PlayAnimation(idleClip);
            }
        }
        
        private void PlayAnimation(AnimationClip clip)
        {
            if (animancer != null && clip != null)
            {
                // Play the clip on Layer 0 (Base Layer)
                animancer.Layers[0].Play(clip, 0.25f);
            }
        }
    }
}
