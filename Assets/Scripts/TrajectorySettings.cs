using UnityEngine;

[CreateAssetMenu(fileName = "Trajectory Settings", menuName = "Game Tools/Trajectory Settings", order = 1)]
public class TrajectorySettings : ScriptableObject
{
    public float aimLineDt = 0.02f;
    public float predictDt = 0.02f;
    public float radiusBubble = 0.2f;
    public float gravity = 20f;
}