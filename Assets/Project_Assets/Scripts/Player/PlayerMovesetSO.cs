using System.Collections.Generic;
using UnityEngine;
using CrunchStreet.Combat;

namespace CrunchStreet.Player
{
    [CreateAssetMenu(fileName = "NewPlayerMoveset", menuName = "CrunchStreet/Player/Moveset")]
    public class PlayerMovesetSO : ScriptableObject
    {
        [Header("Database Reference")]
        [SerializeField] private ComboDatabaseSO database;

        [Header("Unlocked Moves")]
        [SerializeField] private List<BasicAttackSO> unlockedBasicAttacks = new List<BasicAttackSO>();
        [SerializeField] private List<ComboSequenceSO> unlockedCombos = new List<ComboSequenceSO>();

        public ComboDatabaseSO Database => database;
        public List<BasicAttackSO> UnlockedBasicAttacks => unlockedBasicAttacks;
        public List<ComboSequenceSO> UnlockedCombos => unlockedCombos;

        public void UnlockBasicAttack(BasicAttackSO attack)
        {
            if (!unlockedBasicAttacks.Contains(attack))
            {
                unlockedBasicAttacks.Add(attack);
            }
        }

        public void UnlockCombo(ComboSequenceSO combo)
        {
            if (!unlockedCombos.Contains(combo))
            {
                unlockedCombos.Add(combo);
            }
        }
    }
}
