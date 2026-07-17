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
        
        public float RuntimeDamage { get; private set; }

        // This event broadcasts whenever an attack is executed, allowing ComboListeners to react.
        public event Action<IAttackData> OnAttackExecuted;

        private List<CombatInput> currentSequence = new List<CombatInput>();
        private AnimancerState currentAttackState;

        private void Start()
        {
            if (playerProfile != null)
            {
                RuntimeDamage = playerProfile.BaseDamage;
            }
        }

        public void ExecuteInput(CombatInput input)
        {
            if (blackboard == null || blackboard.IsDead || playerProfile == null || playerProfile.Moveset == null) return;
            currentSequence.Add(input);

            // Check if current sequence matches any combo
            ComboSequenceSO matchedCombo = null;
            foreach (var combo in playerProfile.Moveset.UnlockedCombos)
            {
                if (IsSequenceMatch(combo.Sequence))
                {
                    matchedCombo = combo;
                    break;
                }
            }

            if (matchedCombo != null)
            {
                PlayCombo(matchedCombo);
                currentSequence.Clear(); // Reset sequence after combo finishes
            }
            else
            {
                PlayBasicAttack(input);
            }
        }

        private bool IsSequenceMatch(CombatInput[] comboSequence)
        {
            if (comboSequence.Length != currentSequence.Count) return false;

            for (int i = 0; i < comboSequence.Length; i++)
            {
                if (comboSequence[i] != currentSequence[i]) return false;
            }
            return true;
        }

        private void PlayBasicAttack(CombatInput input)
        {
            BasicAttackSO matchedAttack = null;
            foreach (var atk in playerProfile.Moveset.UnlockedBasicAttacks)
            {
                if (atk.InputType == input)
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
                    if (currentAttackState != null)
                    {
                        currentAttackState.Events.OnEnd = null;
                    }

                    currentAttackState = animancer.Layers[0].Play(clip);
                    blackboard.IsAttacking = true;
                    blackboard.CanMove = false;

                    OnAttackExecuted?.Invoke(matchedAttack);

                    // Reset sequence if the animation completely finishes without new input
                    currentAttackState.Events.OnEnd = () => 
                    {
                        blackboard.IsAttacking = false;
                        blackboard.CanMove = true;
                        currentSequence.Clear();
                        currentAttackState.Events.OnEnd = null;
                    };
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
                if (currentAttackState != null)
                {
                    currentAttackState.Events.OnEnd = null;
                }

                currentAttackState = animancer.Layers[0].Play(combo.FinalAnimation);
                blackboard.IsAttacking = true;
                blackboard.CanMove = false;

                OnAttackExecuted?.Invoke(combo);

                currentAttackState.Events.OnEnd = () => 
                {
                    blackboard.IsAttacking = false;
                    blackboard.CanMove = true;
                    currentSequence.Clear();
                    currentAttackState.Events.OnEnd = null;
                };
            }
        }
    }
}
