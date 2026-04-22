using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Trajectory Settings", menuName = "Game Tools/Trajectory Settings", order = 2)]
    public class TrajectorySettings : ScriptableObject
    {
        public int maxPoints = 50;
        [Range(1, 3)] public int step = 1;
        public float minDistanceBetweenDots = 0.4f;
        public float scaleLastDot = 0.4f;
    }
}