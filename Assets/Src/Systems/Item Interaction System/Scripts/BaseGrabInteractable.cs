using DG.Tweening;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

[RequireComponent(typeof(Outline))]
public class BaseGrabInteractable : XRGrabInteractable
{
    private Outline outline;
    [HideInInspector] public Vector3 originalScale;
    private Tween currentTween;
    private Rigidbody rigidbody;
    protected override void Awake()
    {
        base.Awake();
        outline = GetComponent<Outline>();
        outline.OutlineWidth = 2f;
        outline.OutlineColor = Color.bisque;
        outline.enabled = false;
        
        originalScale = transform.localScale;

        this.trackScale = false;
        this.gameObject.tag = "Interactable";
        rigidbody = this.GetComponent<Rigidbody>();
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        outline.enabled = true;
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        outline.enabled = false;
    }
    
    public void Shrink(Vector3 targetScale, float duration = 0.3f)
    {
        currentTween?.Kill();
        currentTween = transform.DOScale(targetScale, duration).SetEase(Ease.OutBack);
    }
    
    public void ResetScale(float duration = 0.3f)
    {
        currentTween?.Kill();
        currentTween = transform.DOScale(originalScale, duration).SetEase(Ease.OutBack);
    }
    
    public Bounds GetCombinedRendererBounds()
    {
        Renderer[] renderers = this.GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0) { return new Bounds(this.transform.position, Vector3.zero); }

        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combinedBounds.Encapsulate(renderers[i].bounds);
        }

        return combinedBounds;
    }

    public void DisableMovement()
    {
        rigidbody.isKinematic = true;
    }
}
