using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
        static Cell[] _cells = new Cell[54];
        private static ColorItem[] differences = new ColorItem[324]; //54 * 6 = 324
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
            }
            
            string cube54 = "";
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    for (int k = 0; k < 6; k++)
                    {
                        differences[i * 54 + j * 6 + k] = new ColorItem(i, j, k, ColorDifference(k, AverageColor(_colors[i][j])));
                    }
                }
            }

            
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    _cells[i * 9 + j] = new Cell(i, j);
                }
            }
            ClassifyColors();
            GameObject.Find("SetCubeColors").GetComponent<SetCubeColors>().SetColors(centersAverage, Solver.Cube54ToKociemba(Solver.CellsToCube54(_cells)));
            GameObject.Find("Solver").GetComponent<Solver>().Solve(_cells);
        }

        static double ColorDifference(int faceIndex, Color color)
        {
            return  Math.Pow(color.r - centersAverage[faceIndex].r, 2) +
                    Math.Pow(color.g - centersAverage[faceIndex].g, 2) +
                    Math.Pow(color.b - centersAverage[faceIndex].b, 2);
        }
        
        public static Color AverageColor(Color[] colors)
        {
            int numberOfValidPixels = 0;
            float r = 0, g = 0, b = 0;
            for (int j = 0; j < colors.Length; j++)
            {
                float h, s, v;
                Color.RGBToHSV(colors[j], out h, out s, out v);

                if (v > 30f / 255) // ignore dark pixels
                {
                    r += colors[j].r;
                    g += colors[j].g;
                    b += colors[j].b;
                    numberOfValidPixels++;
                }
            }
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

        public static void AddFaceColor(Color[][] colors, int index)
        {
            _colors[index] = colors;
        }

        public static void AddFaceColor(Color[][] colors)
        {
            AddFaceColor(colors, _faceIndex);
            _faceIndex++;
            if (_faceIndex == 6)
            {
                ColorDetection();
                _faceIndex = 0;
                assignedCellsToEachFace = new[] {0, 0, 0, 0, 0, 0};
            }
        }

        public static Color[] GetCenterColors()
        {
            return centersAverage;
        }

        public static Cell[] GetCells()
        {
            return _cells;
        }
    }

    public class Cell
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