using System;
using System.Collections.Generic;
using System.IO;
using Kociemba;
using UnityEngine;

namespace DefaultNamespace
{
    public class ColorDetector
    {
        private static int _faceIndex = 0;
        private static Color[][][] _colors = new Color[6][][]; // 6 faces, 9 cells, n pixels
        static Color[][] centers = new Color[6][]; // F, R, B, L, U, D
        static Color[] centersAverage = new Color[6];
        private static string[] indexToFace = new[] {"F", "R", "B", "L", "U", "D"};
        static double[][][] colorDifferences = new double[6][][]; // 6 * 9 * 6
        static Cell[] _cells = new Cell[54];
        static Cell[][] twoDCells = new Cell[6][]; // 6 * 9
        private static ColorItem[] differences = new ColorItem[324]; //54 * 6 = 324
        static string[][] cube = new string[6][];
        private static int[] assignedCellsToEachFace = new[] {0, 0, 0, 0, 0, 0};

        static void ColorDetection()
        {
            for (int i = 0; i < 6; i++)
            {
                centers[i] = _colors[i][4]; // Center cell index is 4
            }
            for (int i = 0; i < 6; i++)
            {
                centersAverage[i] = AverageColor(centers[i]);
                Debug.Log("CA:: " + centersAverage[i].r * 255 + " " +  centersAverage[i].g * 255 + " " + centersAverage[i].b * 255);
            }

            string cube54 = "";
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    // _cells[i * 9 + j] = new Cell(i, j);
                    // ClassifyColor(AverageColor(_colors[i][j]), i * 9 + j);
                    for (int k = 0; k < 6; k++)
                    {
                        differences[i * 54 + j * 6 + k] = new ColorItem(i, j, k, ColorDifference(k, AverageColor(_colors[i][j])));
                    }
                }
            }

            for (int i = 0; i < 324; i++)
            {
                Debug.Log("differences " + differences[i].assignedFaceIndex + " " + differences[i].colorDifference);

            }
            
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    _cells[i * 9 + j] = new Cell(i, j);
                }
            }
            ClassifyColors();

            for (int i = 0; i < 54; i++)
            {
                cube54 += indexToFace[_cells[i].face];
            }
            Debug.Log(cube54);
            
            string info = "";
            string solution = Search.solution(Cube54ToKociemba(cube54), out info);
            Debug.Log(info);
            Debug.Log(Cube54ToKociemba(cube54));
            Debug.Log(solution);
        }

        static double ColorDifference(int faceIndex, Color color)
        {
            return  Math.Pow(color.r - centersAverage[faceIndex].r, 2) +
                    Math.Pow(color.g - centersAverage[faceIndex].g, 2) +
                    Math.Pow(color.b - centersAverage[faceIndex].b, 2);
        }
        static Color AverageColor(Color[] colors)
        {
            int numberOfValidPixels = 0;
            float r = 0, g = 0, b = 0;
            for (int j = 0; j < colors.Length; j++)
            {
                float h, s, v;
                Color.RGBToHSV(colors[j], out h, out s, out v);
                    
                r += colors[j].r;
                g += colors[j].g;
                b += colors[j].b;
                numberOfValidPixels++;
            }
            Debug.Log("AVG: " + 255 * r / numberOfValidPixels + " " + 255 * g / numberOfValidPixels + " " + 255 * b / numberOfValidPixels);
            return new Color(r / numberOfValidPixels, g / numberOfValidPixels, b / numberOfValidPixels);
        }

        static void ClassifyColor(Color color, int cellIndex)
        {
            for (int i = 0; i < 6; i++)
            {
                _cells[cellIndex].differences[i] =
                    Math.Pow(color.r - centersAverage[i].r, 2) +
                    Math.Pow(color.g - centersAverage[i].g, 2) +
                    Math.Pow(color.b - centersAverage[i].b, 2);
            }
        }

        static void ClassifyColors()
        {
            SortColorItems();
            for (int i = 0; i < differences.Length; i++)
            {
                if (_cells[differences[i].faceIndexInPhoto * 9 + differences[i].cellIndexInPhoto].face == -1 &&
                    assignedCellsToEachFace[differences[i].assignedFaceIndex] < 9)
                {
                    _cells[differences[i].faceIndexInPhoto * 9 + differences[i].cellIndexInPhoto].face =
                        differences[i].assignedFaceIndex;
                    assignedCellsToEachFace[differences[i].assignedFaceIndex]++;
                }
            }
        }

        static void SortColorItems()
        {
            ColorItem temp;
            for (int j = 0; j <= differences.Length - 2; j++) {
                for (int i = 0; i <= differences.Length - 2; i++) {
                    if (differences[i].colorDifference > differences[i + 1].colorDifference)
                    {
                        temp = differences[i + 1];
                        differences[i + 1] = differences[i];
                        differences[i] = temp;
                    }
                }
            }
        }

        public static void AddFaceColor(Color[][] colors, int numberOfPixels, int index)
        {
            _colors[index] = colors;

            // for (int i = 0; i < 9; i++)
            // {
            //     Texture2D photo1 = new Texture2D(numberOfPixels, numberOfPixels);
            //     photo1.SetPixels(colors[i]);
            //     photo1.Apply();
            //     byte[] bytes1 = photo1.EncodeToPNG();
            //     //Write out the PNG. Of course you have to substitute your_path for something sensible
            //     File.WriteAllBytes(Application.dataPath + "/photo" + index + "" + i + ".png", bytes1);
            // }
        }

        public static void AddFaceColor(Color[][] colors, int numberOfPixels)
        {
            AddFaceColor(colors, numberOfPixels, _faceIndex);
            _faceIndex++;
            if (_faceIndex == 6)
            {
                _faceIndex = 0;
                ColorDetection();
            }
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
    }

    class Cell
    {
        public int faceIndex;
        public int cellIndex;
        public double[] differences = new double[6];
        public int face = -1;

        public Cell(int faceIndex, int cellIndex)
        {
            this.faceIndex = faceIndex;
            this.cellIndex = cellIndex;
        }
    }

    class ColorItem
    {

        public int faceIndexInPhoto;
        public int cellIndexInPhoto;
        public double colorDifference;
        public int assignedFaceIndex;

        public ColorItem(int faceIndexInPhoto, int cellIndexInPhoto, int assignedFaceIndex, double colorDifference)
        {
            this.assignedFaceIndex = assignedFaceIndex;
            this.colorDifference = colorDifference;
            this.faceIndexInPhoto = faceIndexInPhoto;
            this.cellIndexInPhoto = cellIndexInPhoto;
        }
    }
}