using UnityEngine;

public class GameLoop : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("Awake"); 
    }
    
    public void Constructor(IBubbleGridStorage bubbleStorage, IBubbleNeighborFinder neighborFinder, IBubbleMatchFinder matchFinder)
    {
    }

    private void Start()
    {
        Debug.Log("Start"); 
    }
    
}