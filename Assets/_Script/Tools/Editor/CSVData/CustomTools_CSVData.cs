using UnityEditor;
using UnityEngine;

public partial class CustomTools
{
    [MenuItem("Custom Tools/CSV Data/Generate All Parsers")]
    public static void GenerateAllParser()
    {
        CSVDataEditor.GenerateAllParser();
    }
    [MenuItem("Custom Tools/CSV Data/Generate All CSV to game data")]
    public static void GenerateAllCSVToGameData()
    {
        CSVDataEditor.GenerateAllIngameData();
    }
}
