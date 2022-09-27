using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableClothHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject TableClothPlane;
    public List<Texture> TableClothPatterns;
    private Texture selectedPattern;
    void Start()
    {
        selectedPattern = TableClothPatterns[Random.Range(0, TableClothPatterns.Count)];
        TableClothPlane.GetComponent<Renderer>().material.mainTexture = selectedPattern;
    }

    public void AddPatternToGeneratedPlayground()
    {
        List<Tile> GeneratedTiles = GameGodSingleton.PlaygroundGenerator.GeneratedTiles;
        foreach (Tile t in GeneratedTiles)
        {
            t.SetBoardTexture(selectedPattern);
        }
    }

}
