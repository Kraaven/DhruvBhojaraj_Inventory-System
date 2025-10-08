using System;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class InventoryMainBox : MonoBehaviour
{
    private InventorySlot inventorySlot;

    public void Awake()
    {
        this.GetComponentInChildren<GridSpawner>().Init();
        
        print("Grid Spawned");
        inventorySlot = GetComponentInChildren<InventorySlot>();
        print("Slot Assigned");
    }

    private void OnTriggerEnter(Collider other)
    {
        BaseGrabInteractable item = other.gameObject.GetComponentInChildren<BaseGrabInteractable>();

        if (item == null || inventorySlot == null)
            return;
        
        BoxCollider slotRenderer = inventorySlot.GetComponent<BoxCollider>();

        if (item == null || slotRenderer == null) return;

        Bounds itemBounds = item.GetCombinedRendererBounds();
        Bounds slotBounds = slotRenderer.bounds;
        
        float scaleFactor = GetFitScaleFactor(itemBounds.size, slotBounds.size);
        
        Vector3 targetScale = item.originalScale * scaleFactor;
        item.Shrink(targetScale);
    }

    private void OnTriggerExit(Collider other)
    {
        BaseGrabInteractable item = other.GetComponent<BaseGrabInteractable>();
        if (item == null) return;
        
        item.ResetScale();
    }
    private float GetFitScaleFactor(Vector3 itemSize, Vector3 slotSize)
    {
        float xRatio = slotSize.x / itemSize.x;
        float yRatio = slotSize.y / itemSize.y;
        float zRatio = slotSize.z / itemSize.z;
        
        float scaleFactor = Mathf.Min(xRatio, yRatio, zRatio) * 0.9f;
        // return Mathf.Clamp(scaleFactor, 0.05f, 10f);
        return scaleFactor;
    }
}
