using UnityEngine;
using CrunchStreet.Core;
using Animancer;

namespace CrunchStreet.Player
{
    public class PlayerHealth : HealthBase
    {
        [Header("Player Data")]
        [SerializeField] private GlobalPlayerProfileSO playerProfile;

        [Header("References")]
        [SerializeField] private AnimancerComponent animancer;
        [SerializeField] private CharacterBlackboard blackboard;
        [SerializeField] private EntityIdentifier entityIdentifier;

        [Header("Animations")]
        [SerializeField] private ClipTransition deathTransition;

        private bool isDead = false;

        private void Start()
        {
            if (playerProfile != null)
            {
                Initialize(playerProfile.MaxHealth);
            }
            else
            {
                Debug.LogWarning("PlayerProfileSO is missing! Initializing with default 100 health.", this);
                Initialize(100f);
            }

            if (animancer == null) animancer = GetComponentInChildren<AnimancerComponent>();
            if (blackboard == null) blackboard = GetComponent<CharacterBlackboard>();
            if (entityIdentifier == null) entityIdentifier = GetComponent<EntityIdentifier>();
        }

        private void Update()
        {
            // Placeholder: Test death
            if (!isDead && Input.GetKeyDown(KeyCode.H))
            {
                TakeDamage(CurrentHealth > 0 ? CurrentHealth : 1f, gameObject);
            }
        }

        protected override void Die()
        {
            if (isDead) return;
            isDead = true;

            // 1. Evitar que los enemigos detecten al jugador
            if (entityIdentifier != null)
            {
                entityIdentifier.IsDetectable = false;
            }

            // 2. Bloquear todo tipo de movimiento y salto
            if (blackboard != null)
            {
                blackboard.CanMove = false;
                blackboard.IsDead = true;
            }

            // 3. Reproducir la animación de muerte
            if (animancer != null && deathTransition != null)
            {
                var state = animancer.Layers[0].Play(deathTransition);
                if (state != null)
                {
                    state.Events.OnEnd = () => 
                    {
                        Debug.Log("La animacion de muerte del jugador ha terminado.");
                        state.Events.OnEnd = null;
                    };
                }
            }
            else
            {
                Debug.Log("La animacion de muerte del jugador ha terminado. (No hay ClipTransition asignado)");
            }
        }
    }
}
