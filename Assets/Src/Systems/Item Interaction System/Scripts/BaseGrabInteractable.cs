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
    private Rigidbody rigidbody;
    protected override void Awake()
    {
        base.Awake();
        outline = GetComponent<Outline>();
        outline.OutlineWidth = 3f;
        outline.OutlineColor = Color.bisque;
        outline.enabled = false;

        this.trackScale = false;
        this.gameObject.tag = "Interactable";
        rigidbody = this.GetComponent<Rigidbody>();

        useDynamicAttach = true;
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

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        rigidbody.isKinematic = false;
    }
}