using UnityEngine;
using UnityEngine.InputSystem;
using Animancer;

namespace CrunchStreet.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CharacterBlackboard))]
    public class PlayerJump : MonoBehaviour
    {
        private enum JumpState { None, Ascending, Falling, Landing }

        [Header("References")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private CharacterBlackboard blackboard;
        [SerializeField] private AnimancerComponent animancer;
        [SerializeField] private Transform groundPivot;

        [Header("Animations")]
        [SerializeField] private ClipTransition jumpStartTransition;
        [SerializeField] private ClipTransition jumpFallTransition;
        [SerializeField] private ClipTransition jumpLandTransition;

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

        private JumpState currentJumpState = JumpState.None;
        private bool hasLeftGround = false;

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
                groundPivot = transform.Find("GroundPivot");
                if (groundPivot == null)
                {
                    groundPivot = transform;
                    Debug.LogError("GroundPivot transform reference is missing on " + gameObject.name + ". Falling back to self.", this);
                }
            }
        }

        private void FixedUpdate()
        {
            UpdateGroundedState();
            UpdateJumpState();
        }

        private void UpdateGroundedState()
        {
            if (blackboard == null || groundPivot == null) return;

            Vector3 origin = groundPivot.position + groundCheckOffset;
            bool grounded = Physics.CheckSphere(origin, groundCheckRadius, groundLayer);
            
            blackboard.IsGrounded = grounded;
        }

        private void UpdateJumpState()
        {
            if (blackboard == null || rb == null || animancer == null) return;
            if (blackboard.IsDead) return;

            if (!blackboard.IsGrounded)
            {
                // We are in the air
                if (currentJumpState == JumpState.Ascending)
                {
                    // We just left the ground after jumping
                    hasLeftGround = true;
                }
                else if (currentJumpState == JumpState.None)
                {
                    // We walked off an edge without jumping - go straight to falling
                    hasLeftGround = true;
                    blackboard.IsJumping = true;
                    currentJumpState = JumpState.Falling;
                    PlayAnimation(jumpFallTransition);
                }
            }
            else
            {
                // We are grounded
                // Only allow landing if we actually left the ground first
                if (hasLeftGround && currentJumpState == JumpState.Falling)
                {
                    currentJumpState = JumpState.Landing;
                    blackboard.IsLanding = true;
                    blackboard.CanMove = false; // Block movement during landing
                    hasLeftGround = false;
                    
                    if (jumpLandTransition != null)
                    {
                        var state = animancer.Layers[0].Play(jumpLandTransition);
                        if (state != null)
                        {
                            state.Events.OnEnd = () => 
                            {
                                currentJumpState = JumpState.None;
                                blackboard.IsJumping = false;
                                blackboard.IsLanding = false;
                                blackboard.CanMove = true; // Restore movement
                                state.Events.OnEnd = null;
                            };
                        }
                        else
                        {
                            currentJumpState = JumpState.None;
                            blackboard.IsLanding = false;
                            blackboard.CanMove = true;
                        }
                    }
                    else
                    {
                        currentJumpState = JumpState.None;
                        blackboard.IsJumping = false;
                        blackboard.IsLanding = false;
                        blackboard.CanMove = true;
                    }
                }
            }
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
            if (blackboard.IsDead) return;

            if (blackboard.IsGrounded && !blackboard.IsAttacking && !blackboard.IsDodging && currentJumpState == JumpState.None)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                
                hasLeftGround = false;
                blackboard.IsJumping = true;
                currentJumpState = JumpState.Ascending;
                
                if (animancer != null && jumpStartTransition != null)
                {
                    var state = animancer.Layers[0].Play(jumpStartTransition);
                    if (state != null)
                    {
                        state.Events.OnEnd = () =>
                        {
                            if (currentJumpState == JumpState.Ascending)
                            {
                                currentJumpState = JumpState.Falling;
                                PlayAnimation(jumpFallTransition);
                            }
                            state.Events.OnEnd = null;
                        };
                    }
                }
            }
        }

        private void PlayAnimation(ClipTransition transition)
        {
            if (animancer != null && transition != null)
            {
                animancer.Layers[0].Play(transition);
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
            
            Gizmos.color = isGrounded ? groundedColor : airColor;
            
            Vector3 origin = pivot.position + groundCheckOffset;

            Gizmos.DrawSphere(pivot.position, 0.05f);
            Gizmos.DrawLine(pivot.position, origin);
            Gizmos.DrawWireSphere(origin, groundCheckRadius);
        }
    }
}
