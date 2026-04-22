using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Physical Simulate Settings", menuName = "Game Tools/Physical Simulate Settings", order = 1)]
    public class PhysicalSimulateSettings : ScriptableObject
    {
        public float predictDeltaTime = 0.02f;
        public float radiusBubble = 0.2f;
        public float gravity = 20f;
    }
}