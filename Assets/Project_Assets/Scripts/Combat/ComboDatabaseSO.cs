using System.Collections.Generic;
using UnityEngine;

namespace CrunchStreet.Combat
{
    [CreateAssetMenu(fileName = "ComboDatabase", menuName = "CrunchStreet/Combat/Combo Database")]
    public class ComboDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<ComboSequenceSO> allCombos = new List<ComboSequenceSO>();
        [SerializeField] private List<BasicAttackSO> allBasicAttacks = new List<BasicAttackSO>();

        public List<ComboSequenceSO> AllCombos => allCombos;
        public List<BasicAttackSO> AllBasicAttacks => allBasicAttacks;
    }
}
