using UnityEngine;
using CrunchStreet.Core;
using CrunchStreet.Player;

namespace CrunchStreet.Combat
{
    public enum HitboxShape
    {
        Cube,
        Sphere
    }

    public abstract class HitboxOverlap : MonoBehaviour
    {
        [Header("Targeting")]
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private EntityType targetType;

        [Header("Shape Settings")]
        [SerializeField] private HitboxShape shape = HitboxShape.Cube;

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugLogs = false;
        [SerializeField] private string logPrefix = "[BasicAttack]";

        [Header("Gizmo Colors")]
        [SerializeField] private Color colorInactive = new Color(0f, 1f, 0f, 0.3f); // Verde
        [SerializeField] private Color colorDetecting = new Color(0f, 0f, 1f, 0.3f); // Azul
        [SerializeField] private Color colorHit = new Color(1f, 0f, 0f, 0.5f); // Rojo

        [Header("References")]
        [SerializeField] private PlayerCombat playerCombat;

        private bool isDetecting = false;
        private bool hasHit = false;

        // Abstract method for derived classes to supply their specific attack data
        protected abstract IAttackData GetAttackData();

        protected virtual void Start()
        {
            if (playerCombat != null)
            {
                playerCombat.OnAttackEnded += ForceDisableHitbox;
            }
        }

        protected virtual void OnDestroy()
        {
            if (playerCombat != null)
            {
                playerCombat.OnAttackEnded -= ForceDisableHitbox;
            }
        }

        private void ForceDisableHitbox()
        {
            if (isDetecting)
            {
                isDetecting = false;
                hasHit = false;
                if (showDebugLogs)
                {
                    Debug.Log($"{logPrefix} Hitbox forcefully disabled due to attack end/interruption.");
                }
            }
        }

        // Called via Animancer Event
        public void EnableHitbox()
        {
            isDetecting = true;
            hasHit = false;
            
            if (playerCombat != null)
            {
                playerCombat.OpenBufferWindow();
            }
            
            if (showDebugLogs)
            {
                Debug.Log($"{logPrefix} Hitbox Enabled. Starting detection...");
            }
        }

        public void DisableHitbox()
        {
            if (showDebugLogs && isDetecting && !hasHit)
            {
                Debug.Log($"{logPrefix} Hitbox Disabled. Did not hit any targets.");
            }

            isDetecting = false;
            hasHit = false;

            if (playerCombat != null)
            {
                playerCombat.EnableChaining();
            }
        }

        private void FixedUpdate()
        {
            if (!isDetecting || hasHit) return;

            Collider[] hits;
            if (shape == HitboxShape.Cube)
            {
                hits = Physics.OverlapBox(transform.position, transform.localScale / 2f, transform.rotation, targetLayer);
            }
            else
            {
                float radius = Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z) / 2f;
                hits = Physics.OverlapSphere(transform.position, radius, targetLayer);
            }
            
            if (hits.Length > 0)
            {
                IDamageable closestTarget = null;
                float closestDistance = float.MaxValue;
                Vector3 playerPos = transform.root.position;

                int validTargetsCount = 0;

                foreach (Collider hit in hits)
                {
                    EntityIdentifier entity = hit.GetComponentInParent<EntityIdentifier>();
                    
                    if (entity != null && entity.IsDetectable && entity.Type == targetType)
                    {
                        IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                        if (damageable != null)
                        {
                            validTargetsCount++;
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

                if (showDebugLogs && validTargetsCount > 0)
                {
                    Debug.Log($"{logPrefix} Detected {hits.Length} colliders, {validTargetsCount} are valid targets.");
                }

                if (closestTarget != null)
                {
                    if (showDebugLogs)
                    {
                        MonoBehaviour targetMB = closestTarget as MonoBehaviour;
                        string targetName = targetMB != null ? targetMB.gameObject.name : "UnknownTarget";
                        Debug.Log($"{logPrefix} Prioritized target: {targetName} at distance {closestDistance}. Applying damage!");
                    }

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
            
            Color currentColor;
            if (hasHit)
            {
                currentColor = colorHit;
            }
            else if (isDetecting)
            {
                currentColor = colorDetecting;
            }
            else
            {
                currentColor = colorInactive;
            }

            Gizmos.color = currentColor;

            if (shape == HitboxShape.Cube)
            {
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
                
                // Wireframe con color sólido sin transparencia
                Gizmos.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1f);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
            else if (shape == HitboxShape.Sphere)
            {
                Gizmos.DrawSphere(Vector3.zero, 0.5f);
                
                // Wireframe con color sólido sin transparencia
                Gizmos.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1f);
                Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
            }
        }
    }
}
