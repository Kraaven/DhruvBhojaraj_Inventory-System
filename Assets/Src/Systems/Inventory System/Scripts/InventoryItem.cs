using System;
using DG.Tweening;
using UnityEngine;

public class InventoryItem : MonoBehaviour
{
    [Header("Settings")]
    public ItemType itemType = ItemType.Single;
    public int itemID = 0;

    public Bounds objectBounds;
    public BaseGrabInteractable grabInteractableReference;
    public Rigidbody rigidbodyReference;
    private Tween currentTween;
    private float tweenDuration = 0.3f;

    public InventorySlot ASSINGNED_SLOT;

    
    [HideInInspector] public Vector3 originalScale;
    void Start()
    {
        objectBounds = CalculateRendererBounds();
        this.TryGetComponent(out grabInteractableReference);
        this.TryGetComponent(out rigidbodyReference);
        originalScale = transform.localScale;
    }
    
    private Bounds CalculateRendererBounds()
    {
        var originalRotation = transform.rotation;
        transform.rotation = Quaternion.identity;
        
        MeshRenderer[] colliders = this.GetComponentsInChildren<MeshRenderer>();
        if (colliders == null || colliders.Length == 0) { return new Bounds(this.transform.position, Vector3.zero); }
        
        Bounds combinedBounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
        {
            combinedBounds.Encapsulate(colliders[i].bounds);
        }
        
        transform.rotation = originalRotation;
        
        return combinedBounds;
        
    }

    public void Shrink(float targetScaleFactor)
    {
        var targetScale = originalScale * targetScaleFactor;
        currentTween?.Kill();
        currentTween = transform.DOScale(targetScale, tweenDuration).SetEase(Ease.OutBack);
        currentTween.onComplete += () =>
        {
            print($"Shrink Scale Complete:\nOriginal : {originalScale}\nTarget : {targetScale}\nTweened Result : {transform.localScale}");
            transform.localScale = targetScale;
        };
    }

    public void ResetScale()
    {
        currentTween?.Kill();
        currentTween = transform.DOScale(originalScale, tweenDuration).SetEase(Ease.OutBack);
        currentTween.onComplete += () =>
        {
            transform.localScale = originalScale;
        };
    }
    
    public enum ItemType
    {
        Single,
        Accumulative
    }
}


