using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandInventory : MonoBehaviour
{
    [Header("Card References")]
    [SerializeField] private List<GameObject> cards = new List<GameObject>();
    
    [Header("Attach Points")]
    [SerializeField] private Transform wristAttachPoint;
    [SerializeField] private Transform palmAttachPoint;
    [SerializeField] private Transform openAttachPoint;
    
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float staggerDelay = 0.05f;
    [SerializeField] private Ease animationEase = Ease.OutCubic;
    [SerializeField] private PathType pathType = PathType.CatmullRom;
    [SerializeField] private int pathResolution = 10;
    
    [Header("Card Stack Settings")]
    [SerializeField] private float stackYOffset = 0.01f;
    
    [Header("Fan Settings")]
    [SerializeField] private float fanSpread = 45f;
    [SerializeField] private float cardWidth = 0.063f;
    [SerializeField] private float cardOverlap = 0.75f;
    [SerializeField] private float fanTilt = 15f;
    
    private bool isOpen = false;
    private List<Tween> activeTweens = new List<Tween>();

    private void Start()
    {
        MoveCardsToStackInstant();
    }

    private void MoveCardsToStackInstant()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null && wristAttachPoint != null)
            {
                cards[i].transform.position = wristAttachPoint.TransformPoint(new Vector3(0, i * stackYOffset, 0));
                cards[i].transform.rotation = wristAttachPoint.rotation;
            }
        }
    }

    public void Open()
    {
        if (isOpen) return;
        
        KillAllAnimations();
        isOpen = true;
        
        for (int i = 0; i < cards.Count; i++)
        {
            AnimateCardToOpen(cards[i], i);
        }
    }

    public void Close()
    {
        if (!isOpen) return;
        
        KillAllAnimations();
        isOpen = false;
        
        for (int i = 0; i < cards.Count; i++)
        {
            AnimateCardToWrist(cards[i], i);
        }
    }

    private void AnimateCardToOpen(GameObject card, int index)
    {
        if (card == null || openAttachPoint == null || palmAttachPoint == null || wristAttachPoint == null) return;
        
        Vector3 startPos = card.transform.position;
        Vector3 palmPos = palmAttachPoint.position;
        Vector3 fanLocalPos = CalculateFanPosition(index, cards.Count);
        Vector3 endPos = openAttachPoint.TransformPoint(fanLocalPos);
        
        Vector3[] path = new Vector3[] { startPos, palmPos, endPos };
        
        Tween moveTween = card.transform.DOPath(path, animationDuration, pathType, PathMode.Full3D, pathResolution)
            .SetEase(animationEase)
            .SetDelay(index * staggerDelay);
        
        Quaternion fanLocalRot = CalculateFanRotation(index, cards.Count);
        Quaternion fanWorldRot = openAttachPoint.rotation * fanLocalRot;
        
        Tween rotateTween = card.transform.DORotateQuaternion(fanWorldRot, animationDuration)
            .SetEase(animationEase)
            .SetDelay(index * staggerDelay);

        Tween scaleUp = card.transform.DOScale(Vector3.one * 2, animationDuration);
        
        activeTweens.Add(moveTween);
        activeTweens.Add(rotateTween);
        activeTweens.Add(scaleUp);
    }

    private void AnimateCardToWrist(GameObject card, int index)
    {
        if (card == null || wristAttachPoint == null || palmAttachPoint == null) return;
        
        Vector3 startPos = card.transform.position;
        Vector3 palmPos = palmAttachPoint.position;
        Vector3 stackLocalPos = new Vector3(0, index * stackYOffset, 0);
        Vector3 endPos = wristAttachPoint.TransformPoint(stackLocalPos);
        
        Vector3[] path = new Vector3[] { startPos, palmPos, endPos };
        
        Tween moveTween = card.transform.DOPath(path, animationDuration, pathType, PathMode.Full3D, pathResolution)
            .SetEase(animationEase)
            .SetDelay((cards.Count - 1 - index) * staggerDelay);
        
        Tween rotateTween = card.transform.DORotateQuaternion(wristAttachPoint.rotation, animationDuration)
            .SetEase(animationEase)
            .SetDelay((cards.Count - 1 - index) * staggerDelay);
        
        Tween ScaleDown = card.transform.DOScale(Vector3.one, animationDuration);
        
        activeTweens.Add(moveTween);
        activeTweens.Add(rotateTween);
        activeTweens.Add(ScaleDown);
    }

    private Vector3 CalculateFanPosition(int index, int totalCards)
    {
        if (totalCards <= 1)
            return Vector3.zero;
        
        float effectiveCardWidth = cardWidth * (1f - cardOverlap);
        
        float totalWidth = effectiveCardWidth * (totalCards - 1);
        
        float step = fanSpread / (totalCards - 1);
        float angle = -fanSpread / 2f + (step * index);
        float angleRad = angle * Mathf.Deg2Rad;
        
        float xOffset = -totalWidth / 2f + (index * effectiveCardWidth);

        float radius = totalWidth * 1.5f;
        float y = -Mathf.Sqrt(Mathf.Max(0, radius * radius - xOffset * xOffset)) + radius;
        
        y *= 0.2f;
        
        return new Vector3(xOffset, y, 0);
    }

    private Quaternion CalculateFanRotation(int index, int totalCards)
    {
        if (totalCards <= 1)
            return Quaternion.identity;
        
        float step = fanSpread / (totalCards - 1);
        float zAngle = -fanSpread / 2f + (step * index);
        
        return Quaternion.Euler(fanTilt, 0, zAngle);
    }

    public void AddCard(GameObject newCard)
    {
        if (newCard == null) return;
        
        cards.Add(newCard);
        
        if (isOpen)
        {
            AnimateCardToOpen(newCard, cards.Count - 1);
            RefreshFanPositions();
        }
        else
        {
            int stackIndex = cards.Count - 1;
            Vector3 stackPos = wristAttachPoint.TransformPoint(new Vector3(0, stackIndex * stackYOffset + 0.5f, 0));
            newCard.transform.position = stackPos;
            newCard.transform.rotation = wristAttachPoint.rotation;

            Vector3 finalStackPos = wristAttachPoint.TransformPoint(new Vector3(0, stackIndex * stackYOffset, 0));
            Tween addTween = newCard.transform.DOMove(finalStackPos, animationDuration * 0.3f).SetEase(Ease.OutBounce);
            activeTweens.Add(addTween);
        }
    }

    public void RemoveCard(GameObject cardToRemove)
    {
        if (cardToRemove == null || !cards.Contains(cardToRemove)) return;
        
        int removedIndex = cards.IndexOf(cardToRemove);
        cards.Remove(cardToRemove);
        
        Tween removeTween = cardToRemove.transform.DOScale(0f, animationDuration * 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() => 
            {
                if (cardToRemove != null)
                    Destroy(cardToRemove);
            });
        
        activeTweens.Add(removeTween);
        
        if (isOpen)
        {
            RefreshFanPositions();
        }
        else
        {
            for (int i = removedIndex; i < cards.Count; i++)
            {
                Vector3 newStackPos = wristAttachPoint.TransformPoint(new Vector3(0, i * stackYOffset, 0));
                Tween stackTween = cards[i].transform.DOMove(newStackPos, animationDuration * 0.3f).SetEase(animationEase);
                activeTweens.Add(stackTween);
            }
        }
    }

    public void RemoveCardAtIndex(int index)
    {
        if (index < 0 || index >= cards.Count) return;
        RemoveCard(cards[index]);
    }

    private void RefreshFanPositions()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == null) continue;
            
            Vector3 fanLocalPos = CalculateFanPosition(i, cards.Count);
            Vector3 fanWorldPos = openAttachPoint.TransformPoint(fanLocalPos);
            
            Quaternion fanLocalRot = CalculateFanRotation(i, cards.Count);
            Quaternion fanWorldRot = openAttachPoint.rotation * fanLocalRot;
            
            Tween moveTween = cards[i].transform.DOMove(fanWorldPos, animationDuration * 0.5f).SetEase(animationEase);
            Tween rotateTween = cards[i].transform.DORotateQuaternion(fanWorldRot, animationDuration * 0.5f).SetEase(animationEase);
            
            activeTweens.Add(moveTween);
            activeTweens.Add(rotateTween);
        }
    }

    private void KillAllAnimations()
    {
        foreach (var tween in activeTweens)
        {
            if (tween != null && tween.IsActive())
                tween.Kill();
        }
        activeTweens.Clear();
    }

    private void OnDestroy()
    {
        KillAllAnimations();
    }

    
    public bool OPENINVENTORY;
    private void Update()
    {
        if (OPENINVENTORY)
        {
            OPENINVENTORY = false;
            Open();
        }
    }
}