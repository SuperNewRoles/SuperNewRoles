using System.IO;

namespace SuperNewRoles.CustomCosmetics;

public class CustomCosmeticsSaver
{
    public static string CurrentHat2Id { get; private set; } = string.Empty;
    public static string CurrentVisor2Id { get; private set; } = string.Empty;
    public static void Save()
    {
        DirectoryInfo directoryInfo = new(SuperNewRolesPlugin.BaseDirectory + "/SaveData");
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }
        FileInfo fileInfo = new(Path.Combine(directoryInfo.FullName, "CustomCosmetics.data"));
        using FileStream fileStream = fileInfo.Create();
        using BinaryWriter binaryWriter = new(fileStream);
        binaryWriter.Write(CurrentHat2Id);
        binaryWriter.Write(CurrentVisor2Id);
    }
    public static void Load()
    {
        DirectoryInfo directoryInfo = new(SuperNewRolesPlugin.BaseDirectory + "/SaveData");
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }
        FileInfo fileInfo = new(Path.Combine(directoryInfo.FullName, "CustomCosmetics.data"));
        if (!fileInfo.Exists)
        {
            return;
        }
        using FileStream fileStream = fileInfo.OpenRead();
        using BinaryReader binaryReader = new(fileStream);
        CurrentHat2Id = binaryReader.ReadString();
        CurrentVisor2Id = binaryReader.ReadString();
    }
    public static void SetHat2Id(string hatId)
    {
        CurrentHat2Id = hatId;
        Save();
    }
    public static void SetVisor2Id(string visorId)
    {
        CurrentVisor2Id = visorId;
        Save();
    }
}
