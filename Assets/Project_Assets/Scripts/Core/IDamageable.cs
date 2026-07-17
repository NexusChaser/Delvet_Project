using UnityEngine;

namespace CrunchStreet.Core
{
    public interface IDamageable
    {
        void TakeDamage(float amount, GameObject instigator);
    }
}
