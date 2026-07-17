using UnityEngine;
using CrunchStreet.Core;
using CrunchStreet.Player;

namespace CrunchStreet.Combat
{
    public abstract class HitboxOverlap : MonoBehaviour
    {
        [Header("Targeting")]
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private EntityType targetType;

        [Header("References")]
        [SerializeField] private PlayerCombat playerCombat;

        private bool isDetecting = false;
        private bool hasHit = false;

        // Abstract method for derived classes to supply their specific attack data
        protected abstract IAttackData GetAttackData();

        // Called via Animancer Event
        public void EnableHitbox()
        {
            isDetecting = true;
            hasHit = false;
        }

        // Called via Animancer Event
        public void DisableHitbox()
        {
            isDetecting = false;
            hasHit = false;
        }

        private void FixedUpdate()
        {
            if (!isDetecting || hasHit) return;

            Collider[] hits = Physics.OverlapBox(transform.position, transform.localScale / 2f, transform.rotation, targetLayer);
            
            if (hits.Length > 0)
            {
                IDamageable closestTarget = null;
                float closestDistance = float.MaxValue;
                Vector3 playerPos = transform.root.position;

                foreach (Collider hit in hits)
                {
                    EntityIdentifier entity = hit.GetComponentInParent<EntityIdentifier>();
                    
                    if (entity != null && entity.IsDetectable && entity.Type == targetType)
                    {
                        IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                        if (damageable != null)
                        {
                            Vector3 targetPivotPos = entity.Pivot != null ? entity.Pivot.position : hit.transform.position;
                            float distance = Vector3.Distance(playerPos, targetPivotPos);

                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestTarget = damageable;
                            }
                        }
                    }
                }

                if (closestTarget != null)
                {
                    float baseDamage = playerCombat != null ? playerCombat.RuntimeDamage : 0f;
                    IAttackData attackData = GetAttackData();
                    float damageMultiplier = attackData != null ? attackData.DamageMultiplier : 1f;
                    
                    float finalDamage = baseDamage * damageMultiplier;
                    closestTarget.TakeDamage(finalDamage, transform.root.gameObject);
                    hasHit = true;
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            
            if (isDetecting && !hasHit)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            }
            else
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            }

            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }
    }
}
