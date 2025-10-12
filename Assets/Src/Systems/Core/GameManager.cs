using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Transform Interactables;

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);
    }
    
    
}
