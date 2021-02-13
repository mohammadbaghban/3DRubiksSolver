using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Kociemba;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Solver : MonoBehaviour
{
    private static string[] indexToFace = new[] {"F", "R", "B", "L", "U", "D"};
    public string facelets = "";
    private string solution = "";
    public GameObject solveBtn;
    public Text solveBtnText;
    public GameObject CamSet;
    public Text solutionText;
    public bool solve = true;
    public GameObject cube;
    public GameObject guideCube;

    // Start is called before the first frame update
    void Start()
    {
       // Automate.moveList = StringToList("R U L' D B2");
       if (Application.platform == RuntimePlatform.Android)
       {
           Tools.pathToTablesPrefix = "jar:file://";
           Tools.pathToTables = "/Tables/";
       }

    }

    public void SetFacelets(string facelets)
    {
        this.facelets = facelets;
    }

    public void Solve(Cell[] cells)
    {
        

        string cube54 = CellsToCube54(cells);
        Debug.Log(cube54);
        string info = "";
        solution = Search.solution(Cube54ToKociemba(cube54), out info);
        Debug.Log(info);
        Debug.Log(Cube54ToKociemba(cube54));
        Debug.Log(solution);
        CamSet.SetActive(false);
        solveBtn.SetActive(true);
        solutionText.gameObject.SetActive(true);
        cube.SetActive(true);
        guideCube.SetActive(false);
        solutionText.text = solution;

        if (solution.Contains("Error"))
        {
            solve = false;
            solveBtnText.text = "Reset";
        }
    }

    public void SolveButtonClicked()
    {

        if (solve)
        {
            Automate.moveList = StringToList(solution);
            solveBtnText.text = "Reset";
            solve = false;
        }
        else
        {
            Automate.moveList = new List<string>();
            SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex ) ;
        }
    }

    public static string CellsToCube54(Cell[] cells)
    {
        string cube54 = "";
        for (int i = 0; i < 54; i++)
        {
            Debug.Log(i + " " + cells[i].face);
            cube54 += indexToFace[cells[i].face];
        }
        return cube54;
    }
    
    List<string> StringToList(string solution)
    {
        List<string> solutionList = new List<string>(solution.Split(new string[] { " " }, System.StringSplitOptions.RemoveEmptyEntries));
        return solutionList;
    }
    
     public static string Cube54ToKociemba(string cube54)
        {
            string faceletCube = "";
            
            faceletCube += cube54[36]; //U
            faceletCube += cube54[39];
            faceletCube += cube54[42];
            faceletCube += cube54[37];
            faceletCube += cube54[40];
            faceletCube += cube54[43];
            faceletCube += cube54[38];
            faceletCube += cube54[41];
            faceletCube += cube54[44];
            
            faceletCube += cube54[15]; //R
            faceletCube += cube54[16];
            faceletCube += cube54[17];
            faceletCube += cube54[12];
            faceletCube += cube54[13];
            faceletCube += cube54[14];
            faceletCube += cube54[9];
            faceletCube += cube54[10];
            faceletCube += cube54[11];
            
            faceletCube += cube54[6]; //F
            faceletCube += cube54[7];
            faceletCube += cube54[8];
            faceletCube += cube54[3];
            faceletCube += cube54[4];
            faceletCube += cube54[5];
            faceletCube += cube54[0];
            faceletCube += cube54[1];
            faceletCube += cube54[2];
            
            faceletCube += cube54[53]; //D
            faceletCube += cube54[50];
            faceletCube += cube54[47];
            faceletCube += cube54[52];
            faceletCube += cube54[49];
            faceletCube += cube54[46];
            faceletCube += cube54[51];
            faceletCube += cube54[48];
            faceletCube += cube54[45];
            
            faceletCube += cube54[33]; //L
            faceletCube += cube54[34];
            faceletCube += cube54[35];
            faceletCube += cube54[30];
            faceletCube += cube54[31];
            faceletCube += cube54[32];
            faceletCube += cube54[27];
            faceletCube += cube54[28];
            faceletCube += cube54[29];
            
            faceletCube += cube54[24]; //B
            faceletCube += cube54[25];
            faceletCube += cube54[26];
            faceletCube += cube54[21];
            faceletCube += cube54[22];
            faceletCube += cube54[23];
            faceletCube += cube54[18];
            faceletCube += cube54[19];
            faceletCube += cube54[20];

            return faceletCube;
        }

     private void Update()
     {
         if (Input.GetKey ("escape")) {
             Application.Quit();
         }
     }
}
