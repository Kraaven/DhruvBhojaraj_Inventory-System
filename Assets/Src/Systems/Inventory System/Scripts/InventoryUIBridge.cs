using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryUIBridge : MonoBehaviour
{
    private VisualElement root;

    private List<VisualElement> UISlots;
    public static InventoryUIBridge instance;

    private void Awake()
    {
        if(instance == null) instance = this;
    }

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        UISlots = root.Query<VisualElement>(className: "inventory-slot__root").ToList();
    }

    public void HoverSlot(int slotID)
    {
        UISlots[slotID].AddToClassList("hover");
    }
    
    public void UnHoverSlot(int slotID)
    {
        UISlots[slotID].RemoveFromClassList("hover");
    }

    public void ActiveSlot(int slotID)
    {
        UISlots[slotID].AddToClassList("active");
    }

    public void UnActiveSlot(int slotID)
    {
        UISlots[slotID].RemoveFromClassList("active");
    }

}
