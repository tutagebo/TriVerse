using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class NotesPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultCapacity = 100;
    [SerializeField] private int maxSize = 150;

    private IObjectPool<Notes> pool;

    public void Initialize(Notes prefab, Vector3 position)
    {
        pool = new ObjectPool<Notes>(
            createFunc: () =>
            {
                Notes instance = Instantiate(prefab, position, Quaternion.identity, transform);
                instance.gameObject.SetActive(false);
                return instance;
            },
            actionOnGet: (notes) =>
            {
                notes.gameObject.SetActive(true);
            },
            actionOnRelease: (notes) =>
            {
                notes.Reset();
                notes.gameObject.SetActive(false);
            },
            actionOnDestroy: (notes) =>
            {
                Destroy(notes.gameObject);
            },
            collectionCheck: collectionCheck,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }
    public Notes Get()
    {
        return pool.Get();
    }
    public void Release(Notes notes)
    {
        pool.Release(notes);
    }
    public void Prewarm(int count)
    {
        var tmp = new List<Notes>(count);
        for (int i = 0; i < count; i++) tmp.Add(pool.Get());
        foreach (var n in tmp) pool.Release(n);
    }
}
