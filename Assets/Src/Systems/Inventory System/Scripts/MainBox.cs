using System;
using UnityEngine;
using DG.Tweening;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class InventoryMainBox : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputActionReference ToggleInventoryAction;
    [SerializeField] private Transform interactiveElementContainer;

    private InventorySlot inventorySlotSample;
    private GridSpawner gridSpawner;
    private Camera mainCamera;
    private Transform xrOrigin;

    private bool inventoryIsOpen;
    private readonly Vector3 hiddenPosition = new Vector3(0, -100f, 0); // under player instead of far away

    private void Awake()
    {
        gridSpawner = GetComponentInChildren<GridSpawner>();
        gridSpawner.Init();

        inventorySlotSample = GetComponentInChildren<InventorySlot>();

        // Hide below ground (not off to infinity)
        transform.position = hiddenPosition;

        ToggleInventoryAction.action.performed += ToggleInventorySystem;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        xrOrigin = FindAnyObjectByType<XROrigin>()?.transform;
    }

    private void ToggleInventorySystem(InputAction.CallbackContext context)
    {
        inventoryIsOpen = !inventoryIsOpen;

        if (inventoryIsOpen)
            OpenInventory();
        else
            CloseInventory();
    }

    private void OpenInventory()
    {
        if (xrOrigin == null || mainCamera == null) return;
        
        Vector3 position = xrOrigin.position + xrOrigin.forward * 1.0f;
        position.y = mainCamera.transform.position.y;
        transform.position = position;

        
        Vector3 lookDir = (mainCamera.transform.position - transform.position).normalized;
        lookDir.y = 0;
        Quaternion lookRot = Quaternion.LookRotation(-lookDir, Vector3.up);
        transform.rotation = Quaternion.Euler(-90f, lookRot.eulerAngles.y, -90f);
        transform.RotateAround(transform.position, Vector3.up, 180);
        
        transform.localScale = Vector3.one * 2;
        Sequence openSequence = DOTween.Sequence();
        openSequence.Append(transform.DORotate(new Vector3(0, 180, 0), 1.25f, RotateMode.WorldAxisAdd).SetEase(Ease.OutSine))
                    .Join(transform.DOScale(Vector3.one * 20, 1.25f).SetEase(Ease.OutSine))
                    .Join(transform.DOMoveY(transform.position.y + 0.25f, 1.25f).SetEase(Ease.OutBack))
                    .OnComplete(() => gridSpawner.inventorySlots.ForEach(slot => slot.UnPack(interactiveElementContainer)));
    }

    private void CloseInventory()
    {
        gridSpawner.inventorySlots.ForEach(slot => slot.Pack());

        Sequence closeSequence = DOTween.Sequence();
        closeSequence.Append(transform.DOScale(Vector3.one * 0.5f, 0.75f).SetEase(Ease.OutCubic))
                     .OnComplete(() =>
                     {
                         transform.position = hiddenPosition;
                     });
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out InventoryItem item))
        {
            float scaleFactor = GetFitScaleFactor(item.objectBounds.size, inventorySlotSample.slotCollider.bounds.size);
            item.Shrink(scaleFactor);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out InventoryItem item))
            item.ResetScale();
    }

    private float GetFitScaleFactor(Vector3 itemSize, Vector3 slotSize)
    {
        float scaleFactor = Mathf.Min(slotSize.x / itemSize.x, slotSize.y / itemSize.y, slotSize.z / itemSize.z);
        return scaleFactor;
    }
}
