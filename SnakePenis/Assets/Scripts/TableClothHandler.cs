using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableClothHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject TableClothPlane;
    public List<Texture> TableClothPatterns;
    void Start()
    {
        TableClothPlane.GetComponent<Renderer>().material.mainTexture = TableClothPatterns[Random.Range(0, TableClothPatterns.Count)];
    }

}
