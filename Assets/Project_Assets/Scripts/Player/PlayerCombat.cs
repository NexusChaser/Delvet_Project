using System;
using System.Collections.Generic;
using UnityEngine;
using CrunchStreet.Combat;
using Animancer;

namespace CrunchStreet.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterBlackboard blackboard;
        [SerializeField] private GlobalPlayerProfileSO playerProfile;
        [SerializeField] private AnimancerComponent animancer;
        private PlayerMovement playerMovement;
        
        public float RuntimeDamage { get; private set; }

        // This event broadcasts whenever an attack is executed, allowing ComboListeners to react.
        public event Action<IAttackData> OnAttackExecuted;
        
        // This event broadcasts when an attack completely finishes or is abruptly interrupted.
        public event Action OnAttackEnded;

        private List<CombatInput> currentSequence = new List<CombatInput>();
        private AnimancerState currentAttackState;
        private Coroutine attackRoutine;
        
        public bool CanChainAttack { get; private set; } = true;
        public bool isBufferWindowOpen { get; private set; } = false;
        private CombatInput? bufferedInput = null;
        private CombatInput? bufferedNextAttack = null;

        private void Start()
        {
            playerMovement = GetComponent<PlayerMovement>();
            if (playerProfile != null)
            {
                RuntimeDamage = playerProfile.BaseDamage;
            }
        }

        public void ExecuteInput(CombatInput input)
        {
            if (blackboard == null || blackboard.IsDead || playerProfile == null || playerProfile.Moveset == null) return;
            
            if (!CanChainAttack)
            {
                if (isBufferWindowOpen)
                {
                    // Overwrite so only the LAST input pressed in the valid window is executed
                    bufferedInput = input;
                }
                return;
            }

            currentSequence.Add(input);
            bufferedInput = null;

            // Check if current sequence matches any combo
            ComboSequenceSO matchedCombo = null;
            bool isPartialMatch = false;

            bool isAirborne = !blackboard.IsGrounded;

            foreach (var combo in playerProfile.Moveset.UnlockedCombos)
            {
                if (combo.IsAerial != isAirborne) continue;

                if (IsExactMatch(combo.Sequence))
                {
                    matchedCombo = combo;
                    break;
                }
                else if (IsPartialMatch(combo.Sequence))
                {
                    isPartialMatch = true;
                }
            }

            if (matchedCombo != null)
            {
                PlayCombo(matchedCombo);
                currentSequence.Clear(); // Reset sequence after combo finishes
            }
            else if (isPartialMatch)
            {
                // Still building a combo. Keep it in the sequence and play the basic attack.
                PlayBasicAttack(input);
            }
            else
            {
                // Invalid combo! Not even a partial match.
                currentSequence.RemoveAt(currentSequence.Count - 1);

                if (blackboard.IsAttacking)
                {
                    // If we are currently attacking, don't interrupt. Buffer it for when the attack completely ends.
                    bufferedNextAttack = input;
                }
                else
                {
                    // Not attacking, just play it as a basic attack
                    PlayBasicAttack(input);
                }
            }
        }

        private bool IsExactMatch(CombatInput[] comboSequence)
        {
            if (comboSequence.Length != currentSequence.Count) return false;

            for (int i = 0; i < comboSequence.Length; i++)
            {
                if (comboSequence[i] != currentSequence[i]) return false;
            }
            return true;
        }

        private bool IsPartialMatch(CombatInput[] comboSequence)
        {
            if (currentSequence.Count >= comboSequence.Length) return false;

            for (int i = 0; i < currentSequence.Count; i++)
            {
                if (comboSequence[i] != currentSequence[i]) return false;
            }
            return true;
        }

        private void PlayBasicAttack(CombatInput input)
        {
            bool isAirborne = !blackboard.IsGrounded;
            BasicAttackSO matchedAttack = null;

            foreach (var atk in playerProfile.Moveset.UnlockedBasicAttacks)
            {
                if (atk.InputType == input && atk.IsAerial == isAirborne)
                {
                    matchedAttack = atk;
                    break;
                }
            }

            if (matchedAttack != null)
            {
                var clip = matchedAttack.GetRandomAnimation();
                if (clip != null)
                {
                    if (playerMovement != null)
                    {
                        playerMovement.FaceInputDirectionInstant();
                    }
                    currentAttackState = animancer.Layers[0].Play(clip);
                    currentAttackState.Time = 0f; // Force rewind
                    blackboard.IsAttacking = true;
                    blackboard.CanMove = false;
                    CanChainAttack = false;
                    isBufferWindowOpen = false;
                    bufferedInput = null;

                    OnAttackExecuted?.Invoke(matchedAttack);

                    if (attackRoutine != null)
                    {
                        StopCoroutine(attackRoutine);
                        OnAttackEnded?.Invoke(); // Force disable previous hitboxes
                    }
                    attackRoutine = StartCoroutine(ResetAttackRoutine(currentAttackState));
                }
            }
            else
            {
                // Input did not match any unlocked basic attacks
                currentSequence.RemoveAt(currentSequence.Count - 1);
            }
        }

        private void PlayCombo(ComboSequenceSO combo)
        {
            if (combo.FinalAnimation != null)
            {
                if (playerMovement != null)
                {
                    playerMovement.FaceInputDirectionInstant();
                }
                currentAttackState = animancer.Layers[0].Play(combo.FinalAnimation);
                currentAttackState.Time = 0f; // Force rewind
                blackboard.IsAttacking = true;
                blackboard.CanMove = false;
                CanChainAttack = false;
                isBufferWindowOpen = false;
                bufferedInput = null;

                OnAttackExecuted?.Invoke(combo);

                if (attackRoutine != null)
                {
                    StopCoroutine(attackRoutine);
                    OnAttackEnded?.Invoke(); // Force disable previous hitboxes
                }
                attackRoutine = StartCoroutine(ResetAttackRoutine(currentAttackState));
            }
        }

        private System.Collections.IEnumerator ResetAttackRoutine(AnimancerState state)
        {
            yield return state; // Espera a que termine la animación de Animancer

            if (currentAttackState == state)
            {
                blackboard.IsAttacking = false;
                blackboard.CanMove = true;
                CanChainAttack = true;
                isBufferWindowOpen = false;
                bufferedInput = null;
                currentSequence.Clear();
                
                OnAttackEnded?.Invoke();

                if (bufferedNextAttack.HasValue)
                {
                    CombatInput next = bufferedNextAttack.Value;
                    bufferedNextAttack = null;
                    ExecuteInput(next);
                }
            }
        }

        public void OpenBufferWindow()
        {
            isBufferWindowOpen = true;
        }

        public void EnableChaining()
        {
            CanChainAttack = true;
            isBufferWindowOpen = false;
            if (bufferedInput.HasValue)
            {
                CombatInput nextInput = bufferedInput.Value;
                bufferedInput = null;
                ExecuteInput(nextInput);
            }
        }
    }
}
