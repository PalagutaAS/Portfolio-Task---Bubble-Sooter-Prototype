using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "BubbleDataSettings", menuName = "Bubble/ColorData")]
    public class BubbleDataSettings : ScriptableObject
    {
        public ColorData[] _colors;
        [SerializeField] private int _scoreByPop = 10;
        [SerializeField, Range(0.1f, 0.5f)] private float _rate = 0.2f;

        public int ScoreByPop => _scoreByPop;
        public float Rate => _rate;

        private Dictionary<BubbleColor, ColorData> _dictionary = new ();

        public void OnEnable()
        {
            foreach (ColorData colorData in _colors)
            {
                _dictionary.Add(colorData.ColorName, colorData);
            }
        }

        public ColorData GetData(BubbleColor type)
        {
            if (_dictionary.TryGetValue(type, out ColorData colorData))
            {
                return colorData;
            }
            return new ColorData();
        }
    }
}

[System.Serializable]
public class ColorData
{
    public BubbleColor ColorName;
    public Color ColorValue;
}