using System.IO;

namespace SuperNewRoles.CustomCosmetics;

public class CustomCosmeticsSaver
{
    public static string CurrentHat1Id { get; private set; } = string.Empty;
    public static string CurrentHat2Id { get; private set; } = string.Empty;
    public static string CurrentVisor1Id { get; private set; } = string.Empty;
    public static string CurrentVisor2Id { get; private set; } = string.Empty;
    public static void Save()
    {
        DirectoryInfo directoryInfo = new("./SuperNewRolesNext/SaveData");
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }
        FileInfo fileInfo = new(Path.Combine(directoryInfo.FullName, "CustomCosmetics.data"));
        using FileStream fileStream = fileInfo.Create();
        using BinaryWriter binaryWriter = new(fileStream);
        binaryWriter.Write(CurrentHat1Id);
        binaryWriter.Write(CurrentHat2Id);
        binaryWriter.Write(CurrentVisor1Id);
        binaryWriter.Write(CurrentVisor2Id);
    }
    public static void Load()
    {
        DirectoryInfo directoryInfo = new("./SuperNewRolesNext/SaveData");
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
        CurrentHat1Id = binaryReader.ReadString();
        CurrentHat2Id = binaryReader.ReadString();
        CurrentVisor1Id = binaryReader.ReadString();
        CurrentVisor2Id = binaryReader.ReadString();
    }
    public static void SetHat1Id(string hatId)
    {
        CurrentHat1Id = hatId;
        Save();
    }
    public static void SetHat2Id(string hatId)
    {
        CurrentHat2Id = hatId;
        Save();
    }
    public static void SetVisor1Id(string visorId)
    {
        CurrentVisor1Id = visorId;
        Save();
    }
    public static void SetVisor2Id(string visorId)
    {
        CurrentVisor2Id = visorId;
        Save();
    }

}
