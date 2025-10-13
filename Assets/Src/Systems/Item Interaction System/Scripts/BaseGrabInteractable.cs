using DG.Tweening;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

[RequireComponent(typeof(Outline))]
public class BaseGrabInteractable : XRGrabInteractable
{
    private Outline outlineReference;
    private Rigidbody rigidbodyReference;
    protected override void Awake()
    {
        base.Awake();
        outlineReference = GetComponent<Outline>();
        outlineReference.OutlineWidth = 3f;
        outlineReference.OutlineColor = Color.bisque;
        outlineReference.enabled = false;

        this.trackScale = false;
        this.gameObject.tag = "Interactable";
        rigidbodyReference = this.GetComponent<Rigidbody>();

        useDynamicAttach = true;
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        outlineReference.enabled = true;
    }
    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        outlineReference.enabled = false;
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        rigidbodyReference.isKinematic = false;
    }
}