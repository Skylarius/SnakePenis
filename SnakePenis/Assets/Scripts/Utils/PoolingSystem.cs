using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingSystem<T> where T: class, new()
{
    private List<T> PooledObjects;
    private T Template;

    private int reusedObj = 0, newObj = 0;
    public string Name = "Pool";

    public PoolingSystem()
    {
        PooledObjects = new List<T>();
    }

    public PoolingSystem(T template) : base()
    {
        Template = template;
    }

    private T CreateNewPooledObject()
    {
        if (typeof(T) == typeof(GameObject))
        {
            GameObject newPooledObject = Object.Instantiate(Template as GameObject);
            return newPooledObject as T;
        }
        return new T();
    }

    public T GetObject()
    {
        T pooledObject;
        if (PooledObjects == null)
        {
            ++newObj;
            PooledObjects = new List<T>();
            pooledObject = CreateNewPooledObject();
        }
        else if (PooledObjects.Count == 0)
        {
            ++newObj;
            pooledObject = CreateNewPooledObject();
        } else
        {
            pooledObject = PooledObjects[0];
            PooledObjects.RemoveAt(0);
            ++reusedObj;
        }
        Debug.Log("'" + Name + "': " + newObj + " new, " + reusedObj + " reused Objects.");
        return pooledObject;
    }

    public void DisableObject(T pooledObject)
    {
        if (typeof(T) == typeof(GameObject))
        {
            (pooledObject as GameObject).SetActive(false);
        }
        PooledObjects.Add(pooledObject);
    }

    public void StorePooledObjectList(List<T> listOfObjects)
    {
        if (typeof(T) == typeof(GameObject))
        {
            foreach (T pooledObject in listOfObjects)
            {
               (pooledObject as GameObject).SetActive(false);
            }
        }
        PooledObjects.AddRange(listOfObjects);
    }
}
