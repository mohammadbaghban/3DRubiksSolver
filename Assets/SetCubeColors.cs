using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class SetCubeColors : MonoBehaviour
{
    public Color[] colors = new Color[6];
    public Dictionary<char, int> kociembaFaceToIndex = new Dictionary<char, int>();

    public GameObject Cube;
    //public Cell[] cells;
    
    public List<GameObject> front;
    public List<GameObject> right;
    public List<GameObject> back;
    public List<GameObject> left;
    public List<GameObject> up;
    public List<GameObject> down;

    public List<GameObject> allCells;

    public Material frontMaterial;
    public Material rightMaterial;
    public Material backMaterial;
    public Material leftMaterial;
    public Material upMaterial;
    public Material downtMaterial;

    public void SetColors(Color[] colors, string kociembaCube54)
    {
        Cube.SetActive(true);

        for (int i = 0; i < 54; i++)
        {
            allCells[i].GetComponent<Renderer>().material.color = colors[kociembaFaceToIndex[kociembaCube54[i]]];
        }
        
        foreach (var gameobject in front)
        {
            gameobject.GetComponent<Renderer>().material = frontMaterial;
        }
        
        foreach (var gameobject in right)
        {
            gameobject.GetComponent<Renderer>().material = rightMaterial;
        }
        
        foreach (var gameobject in back)
        {
            gameobject.GetComponent<Renderer>().material = backMaterial;
        }
        
        foreach (var gameobject in left)
        {
            gameobject.GetComponent<Renderer>().material = leftMaterial;
        }
        
        foreach (var gameobject in up)
        {
            gameobject.GetComponent<Renderer>().material = upMaterial;
        }
        
        foreach (var gameobject in down)
        {
            gameobject.GetComponent<Renderer>().material = downtMaterial;
        }
    }

    // public void SetCells(Cell[] cells)
    // {
    //     this.cells = cells;
    // }
    // Start is called before the first frame update
    void Start()
    {
        kociembaFaceToIndex['U'] = 4;
        kociembaFaceToIndex['R'] = 1;
        kociembaFaceToIndex['F'] = 0;
        kociembaFaceToIndex['D'] = 5;
        kociembaFaceToIndex['L'] = 3;
        kociembaFaceToIndex['B'] = 2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
