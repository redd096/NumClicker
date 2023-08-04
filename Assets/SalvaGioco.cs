
using UnityEngine;
using System;
using System.IO;
#if !UNITY_WSA
using System.Runtime.Serialization.Formatters.Binary;
#endif

[Serializable]
public class PlayerStatistics
{
    public int punteggio;
    public int punteggioComplessivo;
    public int ID;
    public string nome;
    public int speedIndex;
    public int speed_cost;
    public int colorIndex;
    public int color_cost;
}

public class SalvaGioco : MonoBehaviour
{
    private static void SaveGame(PlayerStatistics saveGame, string name)
    {

#if UNITY_WSA

        string json = JsonUtility.ToJson(saveGame);
        File.WriteAllText(GetSavePath(name), json);
#else
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.Create(GetSavePath(name));

        formatter.Serialize(saveFile, saveGame);

        saveFile.Close();
#endif

    }

    private static PlayerStatistics LoadGame(string name)
    {
        if (!DoesSaveGameExist(name))
        {
            return null;
        }

#if UNITY_WSA

        PlayerStatistics ps = new PlayerStatistics();

        string json = File.ReadAllText(GetSavePath(name));
        JsonUtility.FromJsonOverwrite(json, ps);

        return ps;
#else
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.Open(GetSavePath(name), FileMode.Open);

        PlayerStatistics ps = (PlayerStatistics)formatter.Deserialize(saveFile);

        saveFile.Close();

        return ps;
#endif

    }

    private static bool DoesSaveGameExist(string name)
    {
        return File.Exists(GetSavePath(name));
    }

    private static string GetSavePath(string name)
    {
        string format = ".binary";
#if UNITY_WSA
        format = ".json";
#endif
        return Path.Combine(Application.persistentDataPath, name + format);
    }

    private static void WriteData(PlayerStatistics ps, ControlloGioco cg, MenuScript ms)
    {
        cg.Punteggio = ps.punteggio;
        cg.PunteggioComplessivo = ps.punteggioComplessivo;
        cg.AggiungiPunteggio(0);

        string Nome = ps.nome;
        if (Nome == "")
            Nome = "guest";

        ms.Refresh(ps.ID, ps.nome, ps.speedIndex, ps.speed_cost, ps.colorIndex, ps.color_cost);
    }

    private static void ResetData(ControlloGioco cg, MenuScript ms)
    {
        cg.Punteggio = 0;
        cg.PunteggioComplessivo = 0;
        cg.AggiungiPunteggio(0);

        ms.Refresh(0, "guest", 0, 0, 0, 0);
    }

    public static void SaveData(ControlloGioco cg, MenuScript ms)
    {
        PlayerStatistics LocalCopyOfData = new PlayerStatistics();

        LocalCopyOfData.punteggio = cg.Punteggio;
        LocalCopyOfData.punteggioComplessivo = cg.PunteggioComplessivo;
        LocalCopyOfData.ID = ms.ID;
        LocalCopyOfData.nome = ms.Name;
        LocalCopyOfData.speedIndex = ms.speedIndex;
        LocalCopyOfData.speed_cost = ms.speed_cost;
        LocalCopyOfData.colorIndex = ms.colorIndex;
        LocalCopyOfData.color_cost = ms.color_cost;

        SaveGame(LocalCopyOfData, "NumClickerOfficial");
    }

    public static void LoadData(ControlloGioco cg, MenuScript ms)
    {
        PlayerStatistics LocalCopyOfData = LoadGame("NumClickerOfficial");

        if (LocalCopyOfData != null)
            WriteData(LocalCopyOfData, cg, ms);
        else
            ResetData(cg, ms);
    }

    public static void DeleteData(string name)
    {
        File.Delete(GetSavePath(name));
    }
}
