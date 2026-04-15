using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleFlightAnimator : IBubbleFlightAnimator
{
    public IEnumerator AnimateFlight(Bubble bubble, ShotResult shotResult)
    {
        List<Vector2> path = shotResult.trajectory;
        if (path.Count < 2)
        {
            yield break;
        }

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 start = path[i - 1];
            Vector2 end = path[i];
            float duration = shotResult.duration / path.Count;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                bubble.transform.position = Vector2.Lerp(start, end, t);
                yield return null;
            }
            bubble.transform.position = end;
        }
    }
}

public interface IBubbleFlightAnimator
{
    IEnumerator AnimateFlight(Bubble bubble, ShotResult shotResult);
}