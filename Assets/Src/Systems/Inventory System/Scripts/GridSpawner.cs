using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    [Header("Object to Spawn")]
    public GameObject prefab;

    [Header("Grid Settings")]
    public Vector2Int xRange = new Vector2Int(-2, 2);
    public Vector2Int yRange = new Vector2Int(-3, 3);
    public int spacing = 2;
    
    void Start()
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab not assigned!");
            return;
        }

        SpawnGrid();
    }

    void SpawnGrid()
    {
        for (int y = yRange.x; y <= yRange.y; y += spacing)
        {
            for (int x = xRange.x; x <= xRange.y; x += spacing)
            {
                Vector3 localPosition = new Vector3(x, y, 0);
                GameObject obj = Instantiate(prefab, transform);
                obj.transform.localPosition = localPosition;
            }
        }
    }
}