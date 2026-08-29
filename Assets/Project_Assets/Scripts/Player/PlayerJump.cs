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
        [SerializeField] private float fallGravityMultiplier = 2.5f;
        [SerializeField] private float lowJumpGravityMultiplier = 2f;

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
        private bool wasAttacking = false;
        private bool isJumpButtonHeld = false;

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
            ApplyCustomGravity();
        }

        private void ApplyCustomGravity()
        {
            if (rb == null || blackboard == null) return;
            if (blackboard.IsGrounded) return;

            if (rb.linearVelocity.y < 0)
            {
                // Falling down: fall faster for a heavier, less floaty feel
                rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallGravityMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (rb.linearVelocity.y > 0 && !isJumpButtonHeld)
            {
                // Moving up but button released: cut the jump short
                rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpGravityMultiplier - 1) * Time.fixedDeltaTime;
            }
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

            bool isAttackingNow = blackboard.IsAttacking;
            if (wasAttacking && !isAttackingNow && !blackboard.IsGrounded)
            {
                // An aerial attack just finished. Resume the correct aerial animation.
                if (rb.linearVelocity.y > 0)
                {
                    PlayAnimation(jumpStartTransition);
                }
                else
                {
                    currentJumpState = JumpState.Falling;
                    PlayAnimation(jumpFallTransition);
                }
            }
            wasAttacking = isAttackingNow;

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
                    if (!blackboard.IsAttacking)
                    {
                        PlayAnimation(jumpFallTransition);
                    }
                }
            }
            else
            {
                // We are grounded
                if (currentJumpState == JumpState.Ascending || currentJumpState == JumpState.Falling)
                {
                    // Only allow landing if we actually left the ground first
                    if (hasLeftGround)
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
                    else
                    {
                        // We are grounded but never left the ground, safely reset
                        currentJumpState = JumpState.None;
                        blackboard.IsJumping = false;
                        blackboard.IsLanding = false;
                    }
                }
                else if (currentJumpState == JumpState.Landing)
                {
                    // If the landing animation was interrupted by an attack or dodge, clean up state
                    if (blackboard.IsAttacking || blackboard.IsDodging)
                    {
                        currentJumpState = JumpState.None;
                        blackboard.IsJumping = false;
                        blackboard.IsLanding = false;
                    }
                }
            }
        }

        // Called via Unity Events from PlayerInput
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                isJumpButtonHeld = true;
                TryJump();
            }
            else if (context.canceled)
            {
                isJumpButtonHeld = false;
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
                                if (!blackboard.IsAttacking)
                                {
                                    PlayAnimation(jumpFallTransition);
                                }
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
