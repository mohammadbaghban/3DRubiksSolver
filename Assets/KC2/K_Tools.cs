using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

namespace Kociemba
{
    public class Tools
    {
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Check if the cube string s represents a solvable cube.
        // 0: Cube is solvable
        // -1: There is not exactly one facelet of each colour
        // -2: Not all 12 edges exist exactly once
        // -3: Flip error: One edge has to be flipped
        // -4: Not all corners exist exactly once
        // -5: Twist error: One corner has to be twisted
        // -6: Parity error: Two corners or two edges have to be exchanged
        // 
        /// <summary>
        /// Check if the cube definition string s represents a solvable cube.
        /// </summary>
        /// <param name="s"> is the cube definition string , see <seealso cref="Facelet"/> </param>
        /// <returns> 0: Cube is solvable<br>
        ///         -1: There is not exactly one facelet of each colour<br>
        ///         -2: Not all 12 edges exist exactly once<br>
        ///         -3: Flip error: One edge has to be flipped<br>
        ///         -4: Not all 8 corners exist exactly once<br>
        ///         -5: Twist error: One corner has to be twisted<br>
        ///         -6: Parity error: Two corners or two edges have to be exchanged </returns>
        ///
        public static string pathToTablesPrefix = "";
        public static string pathToTables = @"/StreamingAssets/Tables/";

        public static int verify(string s)
        {
            int[] count = new int[6];
            try
            {
                for (int i = 0; i < 54; i++)
                {
                    count[(int)CubeColor.Parse(typeof(CubeColor), i.ToString())]++;
                }
            }
            catch (Exception)
            {
                return -1;
            }

            for (int i = 0; i < 6; i++)
            {
                if (count[i] != 9)
                {
                    return -1;
                }
            }

            FaceCube fc = new FaceCube(s);
            CubieCube cc = fc.toCubieCube();

            return cc.verify();
        }

        /// <summary>
        /// Generates a random cube. </summary>
        /// <returns> A random cube in the string representation. Each cube of the cube space has the same probability. </returns>
        public static string randomCube()
        {
            CubieCube cc = new CubieCube();
            Random gen = new Random();
            cc.setFlip((short)gen.Next(CoordCube.N_FLIP));
            cc.setTwist((short)gen.Next(CoordCube.N_TWIST));
            do
            {
                cc.setURFtoDLB(gen.Next(CoordCube.N_URFtoDLB));
                cc.setURtoBR(gen.Next(CoordCube.N_URtoBR));
            } while ((cc.edgeParity() ^ cc.cornerParity()) != 0);
            FaceCube fc = cc.toFaceCube();
            return fc.to_fc_String();
        }


        // https://stackoverflow.com/questions/7742519/c-sharp-export-write-multidimension-array-to-file-csv-or-whatever
        // Kristian Fenn: https://stackoverflow.com/users/989539/kristian-fenn

        public static void SerializeTable(string filename, short[,] array)
        {
            //EnsureFolder(Application.streamingAssetsPath + pathToTables);
            BinaryFormatter bf = new BinaryFormatter();
            WWW www = new WWW(Application.streamingAssetsPath + pathToTables + filename);
            while (!www.isDone)
            {
                Debug.Log("s0");
            }

            byte[] bytes = www.bytes;
            MemoryStream s = new MemoryStream(bytes);
            // Stream s = File.Open(Application.streamingAssetsPath + pathToTables + filename, FileMode.Create);
            bf.Serialize(s, array);
            s.Close();
        }

        public static short[,] DeserializeTable(string filename)
        {
            string str = "";
            byte[] bytes;
            
            Debug.Log(Application.streamingAssetsPath + pathToTables + filename);
            //EnsureFolder(pathToTablesPrefix + Application.dataPath + pathToTables);
            // if (Application.platform == RuntimePlatform.Android)
            // {
                WWW www = new WWW(Application.streamingAssetsPath + pathToTables + filename);
                while (!www.isDone)
                {
                    Debug.Log("s1");
                }

                bytes = www.bytes;
            // }
            // Stream s = File.Open(pathToTablesPrefix + Application.dataPath + pathToTables + filename, FileMode.Open);
            MemoryStream s = new MemoryStream(bytes);
            BinaryFormatter bf = new BinaryFormatter();
            short[,] array = (short[,])bf.Deserialize(s);
            s.Close();
            return array;
        }

        public static void SerializeSbyteArray(string filename, sbyte[] array)
        {
            //EnsureFolder(pathToTablesPrefix + Application.dataPath + pathToTables);
            BinaryFormatter bf = new BinaryFormatter();
            WWW www = new WWW(Application.streamingAssetsPath + pathToTables + filename);
            while (!www.isDone)
            {
                Debug.Log("s2");

            }
            byte[] bytes = www.bytes;
            MemoryStream s = new MemoryStream(bytes);

            bf.Serialize(s, array);
            s.Close();
        }

        public static sbyte[] DeserializeSbyteArray(string filename)
        {
            //EnsureFolder(pathToTablesPrefix + Application.dataPath + pathToTables);
            WWW www = new WWW(Application.streamingAssetsPath + pathToTables + filename);
            while (!www.isDone)
            {
                Debug.Log("s3");

            }
            byte[] bytes = www.bytes;
            MemoryStream s = new MemoryStream(bytes);

            BinaryFormatter bf = new BinaryFormatter();
            sbyte[] array = (sbyte[])bf.Deserialize(s);
            s.Close();
            return array;
        }

        // https://stackoverflow.com/questions/3695163/filestream-and-creating-folders
        // Joe: https://stackoverflow.com/users/13087/joe

        static void EnsureFolder(string path)
        {
            string directoryName = Path.GetDirectoryName(path);
            // If path is a file name only, directory name will be an empty string
            if (directoryName.Length > 0)
            {
                // Create all directories on the path that don't already exist
                Directory.CreateDirectory(directoryName);
            }
        }

        public static String fromScramble(string s) {
            int[] arr = new int[s.Length];
            int j = 0;
            int axis = -1;
            for (int i = 0, length = s.Length; i < length; i++) {
                switch (s[i]) {
                    case 'U':   axis = 0;   break;
                    case 'R':   axis = 3;   break;
                    case 'F':   axis = 6;   break;
                    case 'D':   axis = 9;   break;
                    case 'L':   axis = 12;  break;
                    case 'B':   axis = 15;  break;
                    case ' ':
                        if (axis != -1) {
                            arr[j++] = axis;
                        }
                        axis = -1;
                        break;
                    case '2':   axis++; break;
                    case '\'':  axis += 2; break;
                    default:    continue;
                }

            }
            if (axis != -1) arr[j++] = axis;
            int[] ret = new int[j];
            while (--j >= 0) {
                ret[j] = arr[j];
            }
            return fromScramble(ret);
        }
        
        public static string fromScramble(int[] scramble) {
            CubieCube c1 = new CubieCube();
            CubieCube c2 = new CubieCube();
            CubieCube tmp;
            for (int i = 0; i < scramble.Length; i++) {
                c1.cornerMultiply(CubieCube.moveCube[scramble[i]]);
                c2.cornerMultiply(CubieCube.moveCube[scramble[i]]);
                tmp = c1; c1 = c2; c2 = tmp;
            }
            return c1.toFaceCube().to_fc_String();
        }
    }    
}
