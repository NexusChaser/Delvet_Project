using UnityEngine;
using UnityEngine.InputSystem;
using Animancer;

namespace CrunchStreet.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CharacterBlackboard))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private CharacterBlackboard blackboard;
        [SerializeField] private AnimancerComponent animancer;

        [Header("Animations")]
        [SerializeField] private AnimationClip idleClip;
        [SerializeField] private AnimationClip walkClip;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float autoRotationSpeed = 25f;

        [Header("Movement Direction Mapping")]
        [SerializeField] private bool swapAxes = true; // Swaps X and Z axes (useful if level is aligned with Z)
        [SerializeField] private Vector2 inputScale = new Vector2(1f, 1f); // Multiply axes to invert them (e.g. -1 to invert)

        private Vector2 moveInput;
        private Quaternion targetRotation;
        private bool hasTargetRotation = false;

        private void Awake()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
                if (rb == null)
                {
                    Debug.LogError("Rigidbody reference is missing on " + gameObject.name, this);
                }
            }

            if (blackboard == null)
            {
                blackboard = GetComponent<CharacterBlackboard>();
                if (blackboard == null)
                {
                    Debug.LogError("CharacterBlackboard reference is missing on " + gameObject.name, this);
                }
            }

            if (animancer == null)
            {
                animancer = GetComponentInChildren<AnimancerComponent>();
                if (animancer == null)
                {
                    Debug.LogError("AnimancerComponent reference is missing on " + gameObject.name, this);
                }
            }

            if (rb != null)
            {
                targetRotation = rb.rotation;
            }
        }

        // Called via Unity Events from PlayerInput
        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        private void FixedUpdate()
        {
            if (blackboard == null || rb == null) return;

            if (!blackboard.CanMove)
            {
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                if (blackboard.IsGrounded)
                {
                    PlayAnimation(idleClip);
                }
                return;
            }

            // Map input to world direction
            float xInput = moveInput.x * inputScale.x;
            float yInput = moveInput.y * inputScale.y;

            Vector3 moveDir;
            if (swapAxes)
            {
                moveDir = new Vector3(yInput, 0f, xInput).normalized;
            }
            else
            {
                moveDir = new Vector3(xInput, 0f, yInput).normalized;
            }

            // Apply Movement
            Vector3 targetVelocity = moveDir * moveSpeed;
            targetVelocity.y = rb.linearVelocity.y; // Keep vertical velocity
            rb.linearVelocity = targetVelocity;

            // Apply Rotation
            if (moveDir != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(moveDir);
                hasTargetRotation = true;
                if (blackboard.IsGrounded)
                {
                    PlayAnimation(walkClip);
                }
            }
            else
            {
                if (blackboard.IsGrounded)
                {
                    PlayAnimation(idleClip);
                }
            }

            if (hasTargetRotation)
            {
                float speed = moveDir != Vector3.zero ? rotationSpeed : autoRotationSpeed;
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, speed * Time.fixedDeltaTime);
                if (Quaternion.Angle(rb.rotation, targetRotation) < 1f)
                {
                    rb.rotation = targetRotation;
                }
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
