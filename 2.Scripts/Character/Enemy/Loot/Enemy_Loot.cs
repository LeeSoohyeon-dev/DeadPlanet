using UnityEngine;

public class Enemy_Loot : MonoBehaviour
{
    [SerializeField] private GameObject item;

    public void DropItems()
    {
        if (item != null)
            CreateItem(item);
    }

    private void CreateItem(GameObject itemPrefab)
    {
        GameObject newItem = ObjectPool.instance.GetObject(itemPrefab, transform);

        newItem.transform.position = transform.position + Vector3.up;
    }
}
