using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "BubbleAnimationSettings", menuName = "Bubble Shooter/Animation Settings")]
    public class BubbleAnimationSettings : ScriptableObject
    {
        [Header("Neighbor Push Animation")]
        [Tooltip("Длительность смещения в секундах")]
        public float pushDuration = 0.1f;

        [Tooltip("Длительность возврата в секундах (если 0, то равна pushDuration)")]
        public float returnDuration = 0.1f;

        [Tooltip("Сила смещения (доля от расстояния между центрами)")]
        [Range(0f, 0.5f)]
        public float pushStrength = 0.15f;

        [Tooltip("Easing для движения туда")]
        public DG.Tweening.Ease pushEase = DG.Tweening.Ease.OutQuad;

        [Tooltip("Easing для возврата")]
        public DG.Tweening.Ease returnEase = DG.Tweening.Ease.InQuad;

        [Tooltip("Задержка перед началом возврата")]
        public float returnDelay = 0.05f;

        public float secondWaveStrengthMultiplier = 0.5f;
        public float secondWaveDelay = 0.05f;
    }
}