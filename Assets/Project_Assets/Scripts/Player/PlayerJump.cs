using UnityEngine;
using UnityEngine.InputSystem;
using Animancer;

namespace CrunchStreet.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CharacterBlackboard))]
    public class PlayerJump : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private CharacterBlackboard blackboard;
        [SerializeField] private AnimancerComponent animancer;
        [SerializeField] private Transform groundPivot;

        [Header("Animations")]
        [SerializeField] private AnimationClip jumpClip;

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 6f;

        [Header("Ground Check Settings")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Vector3 groundCheckOffset = Vector3.zero;
        [SerializeField] private float groundCheckRadius = 0.3f;

        [Header("Gizmos Settings")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private bool showAlways = true;
        [SerializeField] private Color airColor = Color.green;
        [SerializeField] private Color groundedColor = Color.red;

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

            if (groundPivot == null)
            {
                // Try to find a child named GroundPivot
                groundPivot = transform.Find("GroundPivot");
                if (groundPivot == null)
                {
                    // Fallback to transform itself so the script doesn't crash
                    groundPivot = transform;
                    Debug.LogError("GroundPivot transform reference is missing on " + gameObject.name + ". Falling back to self.", this);
                }
            }
        }

        private void FixedUpdate()
        {
            UpdateGroundedState();
        }

        private void UpdateGroundedState()
        {
            if (blackboard == null || groundPivot == null) return;

            Vector3 origin = groundPivot.position + groundCheckOffset;
            bool grounded = Physics.CheckSphere(origin, groundCheckRadius, groundLayer);
            
            blackboard.IsGrounded = grounded;
        }

        // Called via Unity Events from PlayerInput
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                TryJump();
            }
        }

        private void TryJump()
        {
            if (blackboard == null || rb == null) return;

            if (blackboard.IsGrounded && !blackboard.IsAttacking && !blackboard.IsDodging)
            {
                // Set y velocity to 0 first to ensure consistent jump height
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                
                PlayAnimation(jumpClip);
            }
        }

        private void PlayAnimation(AnimationClip clip)
        {
            if (animancer != null && clip != null)
            {
                // Play the jump animation on Layer 0 (Base Layer)
                animancer.Layers[0].Play(clip, 0.1f);
            }
        }

        private void OnDrawGizmos()
        {
            if (showGizmos && showAlways)
            {
                DrawGizmosImplementation();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (showGizmos && !showAlways)
            {
                DrawGizmosImplementation();
            }
        }

        private void DrawGizmosImplementation()
        {
            Transform pivot = groundPivot != null ? groundPivot : transform;
            bool isGrounded = blackboard != null && blackboard.IsGrounded;
            
            // 1. Set color based on ground detection
            Gizmos.color = isGrounded ? groundedColor : airColor;
            
            Vector3 origin = pivot.position + groundCheckOffset;

            // 2. Draw small filled sphere at groundPivot
            Gizmos.DrawSphere(pivot.position, 0.05f);

            // 3. Draw line from pivot to the center of the check sphere
            Gizmos.DrawLine(pivot.position, origin);

            // 4. Draw ground check wire sphere
            Gizmos.DrawWireSphere(origin, groundCheckRadius);
        }
    }
}
