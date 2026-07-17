using UnityEngine;

namespace CrunchStreet.Core
{
    public enum EntityType
    {
        Player,
        Enemy,
        Throwable,
        Environment
    }

    /// <summary>
    /// Pure identifier component for quick faction/type checking without heavy logic.
    /// </summary>
    public class EntityIdentifier : MonoBehaviour
    {
        [SerializeField] private EntityType entityType = EntityType.Environment;
        [SerializeField] private Transform pivot;
        [SerializeField] private bool isDetectable = true;
        
        public EntityType Type => entityType;

        /// <summary>
        /// Punto central para cálculos de mira o distancia (ej. el pecho).
        /// Si no se asigna, devuelve el transform base.
        /// </summary>
        public Transform Pivot => pivot != null ? pivot : transform;

        /// <summary>
        /// Si es false, los demás sistemas deberían ignorar a esta entidad (como si fuera invisible).
        /// </summary>
        public bool IsDetectable { get => isDetectable; set => isDetectable = value; }
    }
}
