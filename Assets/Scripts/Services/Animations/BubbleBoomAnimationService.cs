using DG.Tweening;
using UnityEngine;

public interface IBubbleBoomAnimationService
{
    void AnimateBoom(Transform bubble);
}

public class BubbleBoomAnimationService : IBubbleBoomAnimationService
{
    // private readonly AnimationBoomSettings _settings;
    public BubbleBoomAnimationService() //need settings 
    {
        //_settings = _settings;
    }

    public void AnimateBoom(Transform transform)
    {
       
        Sequence destroySequence = DOTween.Sequence();

        destroySequence
            .Append(transform.DOScale(transform.localScale * 1.25f, 0.05f).SetEase(Ease.OutQuad))
            .Append(transform.DOScale(0f, 0.15f).SetEase(Ease.InBack))
            .OnComplete(() =>
            {
                transform.DOKill();
                Object.Destroy(transform.gameObject);
            })
            .Play();
    }
}


