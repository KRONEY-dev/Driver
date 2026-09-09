using UnityEngine;

namespace Driver.Gameplay.Characters
{
    public class CharacterProxy : MonoBehaviour
    {
        public ICharacter Character => characterObject as ICharacter;

        [SerializeField] private MonoBehaviour characterObject;
    }
}