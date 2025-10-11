using System;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class InventoryMainBox : MonoBehaviour
{
    private InventorySlot inventorySlotSample;

    public void Awake()
    {
        this.GetComponentInChildren<GridSpawner>().Init();
        inventorySlotSample = GetComponentInChildren<InventorySlot>();
        
        print("Inventory Slots Created");
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!other.TryGetComponent(out InventoryItem inventoryItem)) return;

        Bounds itemBounds = inventoryItem.objectBounds;
        Bounds slotBounds = inventorySlotSample.slotCollider.bounds;
        
        float scaleFactor = GetFitScaleFactor(itemBounds.size, slotBounds.size);
        inventoryItem.Shrink(scaleFactor);
    }

    private void OnTriggerExit(Collider other)
    {
        if(!other.TryGetComponent(out InventoryItem inventoryItem)) return;
        inventoryItem.ResetScale();
    }
    private float GetFitScaleFactor(Vector3 itemSize, Vector3 slotSize)
    {
        float xRatio = slotSize.x / itemSize.x;
        float yRatio = slotSize.y / itemSize.y;
        float zRatio = slotSize.z / itemSize.z;
        
        float scaleFactor = Mathf.Min(xRatio, yRatio, zRatio) * 1;
        return scaleFactor;
    }
}
