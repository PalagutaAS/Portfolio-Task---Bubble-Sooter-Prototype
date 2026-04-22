using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Bubble Shooter Settings", menuName = "Shooter Settings", order = 0)]
    public class BubbleShooterSettings : ScriptableObject
    {
        [Header("Настройки натягивания")]
        public float maxDragRadius = 2f;
        public float minSpeed = 5f;
        public float maxSpeed = 30f;
    }
}