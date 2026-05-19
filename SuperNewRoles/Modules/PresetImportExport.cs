using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Modules;

public sealed class PresetSnapshot
{
    public int? SourcePresetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, byte> Options { get; set; } = new();
    public List<RoleOptionSnapshot> RoleOptions { get; set; } = new();
    public List<ExclusivitySettingSnapshot> ExclusivitySettings { get; set; } = new();
    public List<ModifierRoleOptionSnapshot> ModifierRoleOptions { get; set; } = new();
    public List<GhostRoleOptionSnapshot> GhostRoleOptions { get; set; } = new();
    public List<CategoryAssignFilterSnapshot> CategoryAssignFilters { get; set; } = new();
}

public sealed class RoleOptionSnapshot
{
    public string RoleId { get; set; } = string.Empty;
    public byte NumberOfCrews { get; set; }
    public int Percentage { get; set; }
}

public sealed class ExclusivitySettingSnapshot
{
    public int MaxAssign { get; set; }
    public List<string> Roles { get; set; } = new();
}

public sealed class ModifierRoleOptionSnapshot
{
    public string ModifierRoleId { get; set; } = string.Empty;
    public byte NumberOfCrews { get; set; }
    public int Percentage { get; set; }
    public int MaxImpostors { get; set; }
    public int ImpostorChance { get; set; }
    public int MaxNeutrals { get; set; }
    public int NeutralChance { get; set; }
    public int MaxCrewmates { get; set; }
    public int CrewmateChance { get; set; }
    public List<string> AssignFilterRoles { get; set; } = new();
}

public sealed class GhostRoleOptionSnapshot
{
    public string GhostRoleId { get; set; } = string.Empty;
    public byte NumberOfCrews { get; set; }
    public int Percentage { get; set; }
}

public sealed class CategoryAssignFilterSnapshot
{
    public string CategoryName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

public sealed class PresetImportResult
{
    public IReadOnlyList<int> ImportedPresetIds { get; }
    public int ImportedCount => ImportedPresetIds.Count;

    public PresetImportResult(IReadOnlyList<int> importedPresetIds)
    {
        ImportedPresetIds = importedPresetIds;
    }
}

public sealed class PresetImportExportException : Exception
{
    public PresetImportExportException(string message) : base(message)
    {
    }

    public PresetImportExportException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public static class PresetImportExportService
{
    public const string ArchiveExtension = "snrpresets";
    public const string OptionsArchivePath = "SuperNewRolesNext/SaveData/Options.data";
    public const string PresetArchivePathPrefix = "SuperNewRolesNext/SaveData/PresetOptions_";
    public const string PresetArchivePathSuffix = ".data";

    public static byte[] ExportPresetArchive(
        IOptionStorage storage,
        IEnumerable<int> presetIds,
        Func<int, string> getPresetName,
        int currentPreset,
        byte currentVersion)
    {
        storage.EnsureStorageExists();
        var presetDataById = new SortedDictionary<int, byte[]>();
        foreach (int presetId in presetIds.Distinct().OrderBy(id => id))
        {
            if (presetId < 0)
                continue;

            var (success, data) = storage.LoadPresetRawData(presetId);
            if (!success)
                continue;

            presetDataById[presetId] = data;
        }

        if (presetDataById.Count == 0)
            throw new PresetImportExportException("No preset data could be exported.");

        var archiveNames = presetDataById.Keys.ToDictionary(
            presetId => presetId,
            presetId => GetExportPresetName(getPresetName(presetId), presetId));
        int archiveCurrentPreset = presetDataById.ContainsKey(currentPreset)
            ? currentPreset
            : presetDataById.Keys.First();
        byte[] optionsData = storage.BuildOptionDataBytes(currentVersion, archiveCurrentPreset, archiveNames);

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteArchiveEntry(archive, OptionsArchivePath, optionsData);
            foreach (var pair in presetDataById)
                WriteArchiveEntry(archive, BuildPresetArchivePath(pair.Key), pair.Value);
        }

        return memoryStream.ToArray();
    }

    public static PresetImportResult ImportPresetsArchive(
        byte[] archiveBytes,
        IOptionStorage storage,
        IDictionary<int, string> presetNames,
        int currentPreset,
        byte currentVersion)
    {
        if (archiveBytes == null || archiveBytes.Length == 0)
            throw new PresetImportExportException("Preset archive is empty.");

        try
        {
            using var memoryStream = new MemoryStream(archiveBytes, writable: false);
            using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
            var optionsEntry = FindOptionsEntry(archive)
                ?? throw new PresetImportExportException("Preset archive does not contain Options.data.");
            ArchiveOptionData optionsData = ReadOptionsData(ReadEntryBytes(optionsEntry));
            var presetDataBySourceId = ReadPresetEntries(archive);
            var requestedPresets = optionsData.PresetNames
                .Where(pair => pair.Key >= 0)
                .OrderBy(pair => pair.Key)
                .ToList();

            if (requestedPresets.Count == 0)
                throw new PresetImportExportException("Preset archive has no presets.");

            foreach (var pair in requestedPresets)
            {
                if (!presetDataBySourceId.ContainsKey(pair.Key))
                    throw new PresetImportExportException($"Preset archive is missing PresetOptions_{pair.Key}.data.");
            }

            storage.EnsureStorageExists();
            var usedIds = new HashSet<int>(presetNames.Keys.Where(id => id >= 0));
            foreach (int presetId in storage.GetExistingPresetDataIds().Where(id => id >= 0))
                usedIds.Add(presetId);

            var usedNames = new HashSet<string>(
                presetNames.Values
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(NormalizePresetNameKey),
                StringComparer.OrdinalIgnoreCase);
            long nextPresetId = usedIds.Count == 0 ? 0 : (long)usedIds.Max() + 1;
            var importedIds = new List<int>();

            foreach (var pair in requestedPresets)
            {
                int targetPresetId = GetNextImportedPresetId(usedIds, ref nextPresetId);
                string targetName = GetUniquePresetName(pair.Value, usedNames);

                storage.SavePresetRawData(targetPresetId, presetDataBySourceId[pair.Key]);
                presetNames[targetPresetId] = targetName;
                usedIds.Add(targetPresetId);
                importedIds.Add(targetPresetId);
            }

            storage.SaveOptionData(currentVersion, currentPreset, new Dictionary<int, string>(presetNames));
            return new PresetImportResult(importedIds);
        }
        catch (PresetImportExportException)
        {
            throw;
        }
        catch (InvalidDataException ex)
        {
            throw new PresetImportExportException("Invalid preset archive.", ex);
        }
        catch (Exception ex)
        {
            throw new PresetImportExportException("Failed to import preset archive.", ex);
        }
    }

    private static void WriteArchiveEntry(ZipArchive archive, string path, byte[] data)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
        using var stream = entry.Open();
        stream.Write(data, 0, data.Length);
    }

    private static string BuildPresetArchivePath(int presetId)
        => $"{PresetArchivePathPrefix}{presetId}{PresetArchivePathSuffix}";

    private static string GetExportPresetName(string name, int presetId)
        => string.IsNullOrWhiteSpace(name) ? $"Preset {presetId + 1}" : name.Trim();

    private static ZipArchiveEntry FindOptionsEntry(ZipArchive archive)
        => archive.Entries.FirstOrDefault(entry => IsOptionsArchivePath(entry.FullName));

    private static bool IsOptionsArchivePath(string path)
    {
        string normalized = NormalizeArchivePath(path);
        return normalized == "supernewrolesnext/savedata/options.data"
            || normalized.EndsWith("/savedata/options.data", StringComparison.Ordinal);
    }

    private static Dictionary<int, byte[]> ReadPresetEntries(ZipArchive archive)
    {
        var presetDataBySourceId = new Dictionary<int, byte[]>();
        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
                continue;
            if (!TryReadPresetId(entry.FullName, out int presetId))
                continue;
            if (!presetDataBySourceId.ContainsKey(presetId))
                presetDataBySourceId[presetId] = ReadEntryBytes(entry);
        }

        return presetDataBySourceId;
    }

    private static bool TryReadPresetId(string path, out int presetId)
    {
        presetId = 0;
        string normalized = NormalizeArchivePath(path);
        const string marker = "/savedata/presetoptions_";
        int markerIndex = normalized.LastIndexOf(marker, StringComparison.Ordinal);
        if (markerIndex < 0)
            return false;

        string idAndSuffix = normalized[(markerIndex + marker.Length)..];
        if (!idAndSuffix.EndsWith(".data", StringComparison.Ordinal))
            return false;

        string idText = idAndSuffix[..^".data".Length];
        return int.TryParse(idText, out presetId) && presetId >= 0;
    }

    private static string NormalizeArchivePath(string path)
        => (path ?? string.Empty).Replace('\\', '/').TrimStart('/').ToLowerInvariant();

    private static byte[] ReadEntryBytes(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    private static ArchiveOptionData ReadOptionsData(byte[] data)
    {
        if (data.Length < 11)
            throw new PresetImportExportException("Options.data is too short.");

        using var memoryStream = new MemoryStream(data, writable: false);
        using var reader = new BinaryReader(memoryStream);
        _ = reader.ReadByte();
        if (!ValidateChecksum(reader))
            throw new PresetImportExportException("Options.data checksum is invalid.");

        int currentPreset = reader.ReadInt32();
        int presetNameCount = reader.ReadInt32();
        if (presetNameCount < 0)
            throw new PresetImportExportException("Options.data preset name count is invalid.");

        var presetNames = new Dictionary<int, string>();
        for (int i = 0; i < presetNameCount; i++)
        {
            int presetId = reader.ReadInt32();
            string name = reader.ReadString();
            if (presetId >= 0)
                presetNames[presetId] = name ?? string.Empty;
        }

        if (presetNames.Count == 0)
            throw new PresetImportExportException("Options.data has no presets.");

        return new ArchiveOptionData(currentPreset, presetNames);
    }

    private static bool ValidateChecksum(BinaryReader reader)
    {
        int random = reader.ReadByte();
        int checksum = reader.ReadByte();
        return random * random == checksum;
    }

    private static int GetNextImportedPresetId(ISet<int> usedIds, ref long nextPresetId)
    {
        if (nextPresetId < 0)
            nextPresetId = 0;

        while (nextPresetId <= int.MaxValue)
        {
            int candidate = (int)nextPresetId;
            nextPresetId++;
            if (!usedIds.Contains(candidate))
                return candidate;
        }

        throw new PresetImportExportException("No free preset slot available.");
    }

    private static string GetUniquePresetName(string sourceName, ISet<string> usedNameKeys)
    {
        string baseName = string.IsNullOrWhiteSpace(sourceName) ? "Imported Preset" : sourceName.Trim();
        string candidate = baseName;
        int suffix = 2;
        while (!usedNameKeys.Add(NormalizePresetNameKey(candidate)))
        {
            candidate = $"{baseName} ({suffix})";
            suffix++;
        }

        return candidate;
    }

    private static string NormalizePresetNameKey(string name)
        => (name ?? string.Empty).Trim();

    private sealed class ArchiveOptionData
    {
        public int CurrentPreset { get; }
        public Dictionary<int, string> PresetNames { get; }

        public ArchiveOptionData(int currentPreset, Dictionary<int, string> presetNames)
        {
            CurrentPreset = currentPreset;
            PresetNames = presetNames;
        }
    }
}

public static partial class CustomOptionSaver
{
    public static byte[] ExportCurrentPresetArchive()
    {
        Save();
        return PresetImportExportService.ExportPresetArchive(
            Storage,
            new[] { CurrentPreset },
            GetPresetName,
            CurrentPreset,
            CurrentVersion);
    }

    public static byte[] ExportAllPresetsArchive()
    {
        Save();
        var presetIds = PresetNames.Keys.Concat(new[] { CurrentPreset });
        return PresetImportExportService.ExportPresetArchive(
            Storage,
            presetIds,
            GetPresetName,
            CurrentPreset,
            CurrentVersion);
    }

    public static PresetImportResult ImportPresetsArchive(byte[] archiveBytes)
        => PresetImportExportService.ImportPresetsArchive(
            archiveBytes,
            Storage,
            presetNames,
            CurrentPreset,
            CurrentVersion);
}

public partial class FileOptionStorage
{
    public bool PresetDataExists(int preset)
    {
        lock (FileLocker)
        {
            return File.Exists(GetPresetFileName(preset));
        }
    }

    public (bool success, byte[] data) LoadPresetRawData(int preset)
    {
        lock (FileLocker)
        {
            string fileName = GetPresetFileName(preset);
            if (!File.Exists(fileName))
                return (false, Array.Empty<byte>());

            return (true, File.ReadAllBytes(fileName));
        }
    }

    public void SavePresetRawData(int preset, byte[] data)
    {
        lock (FileLocker)
        {
            EnsureStorageExists();
            File.WriteAllBytes(GetPresetFileName(preset), data ?? Array.Empty<byte>());
        }
    }

    public IReadOnlyCollection<int> GetExistingPresetDataIds()
    {
        lock (FileLocker)
        {
            if (!_directory.Exists)
                return Array.Empty<int>();

            string presetFilePrefix = Path.GetFileName(_presetFileNameBase);
            var presetIds = new List<int>();
            foreach (var file in _directory.EnumerateFiles($"{presetFilePrefix}*.data"))
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
                if (!fileNameWithoutExtension.StartsWith(presetFilePrefix, StringComparison.Ordinal))
                    continue;

                string idText = fileNameWithoutExtension[presetFilePrefix.Length..];
                if (int.TryParse(idText, out int presetId) && presetId >= 0)
                    presetIds.Add(presetId);
            }

            return presetIds;
        }
    }

    public (bool success, PresetSnapshot snapshot) LoadPresetSnapshot(int preset)
    {
        lock (FileLocker)
        {
            string fileName = GetPresetFileName(preset);
            if (!File.Exists(fileName))
                return (false, new PresetSnapshot());

            using var fileStream = new FileStream(fileName, FileMode.Open);
            using var reader = new BinaryReader(fileStream);

            if (!ValidateChecksum(reader))
                return (false, new PresetSnapshot());

            var snapshot = new PresetSnapshot
            {
                SourcePresetId = preset,
                Options = ReadOptions(reader)
            };

            if (fileStream.Position < fileStream.Length)
                snapshot.RoleOptions = ReadRoleOptionSnapshots(reader);
            if (fileStream.Position < fileStream.Length)
                snapshot.ExclusivitySettings = ReadExclusivitySettingSnapshots(reader);
            if (fileStream.Position < fileStream.Length)
                snapshot.ModifierRoleOptions = ReadModifierRoleOptionSnapshots(reader);
            if (fileStream.Position < fileStream.Length)
                snapshot.GhostRoleOptions = ReadGhostRoleOptionSnapshots(reader);
            if (fileStream.Position < fileStream.Length)
                snapshot.CategoryAssignFilters = ReadCategoryAssignFilterSnapshots(reader);

            return (true, snapshot);
        }
    }

    public void SavePresetSnapshot(int preset, PresetSnapshot snapshot)
    {
        lock (FileLocker)
        {
            string fileName = GetPresetFileName(preset);
            using var fileStream = new FileStream(fileName, FileMode.Create);
            using var writer = new BinaryWriter(fileStream);

            WriteChecksum(writer);
            WriteOptionSnapshots(writer, snapshot.Options);
            WriteRoleOptionSnapshots(writer, snapshot.RoleOptions);
            WriteExclusivitySettingSnapshots(writer, snapshot.ExclusivitySettings);
            WriteModifierRoleOptionSnapshots(writer, snapshot.ModifierRoleOptions);
            WriteGhostRoleOptionSnapshots(writer, snapshot.GhostRoleOptions);
            WriteCategoryAssignFilterSnapshots(writer, snapshot.CategoryAssignFilters);
        }
    }

    private string GetPresetFileName(int preset)
        => $"{_presetFileNameBase}{preset}.data";

    private static List<RoleOptionSnapshot> ReadRoleOptionSnapshots(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        var snapshots = new List<RoleOptionSnapshot>(count);
        for (int i = 0; i < count; i++)
        {
            snapshots.Add(new RoleOptionSnapshot
            {
                RoleId = reader.ReadString(),
                NumberOfCrews = reader.ReadByte(),
                Percentage = reader.ReadInt32()
            });
        }
        return snapshots;
    }

    private static List<ExclusivitySettingSnapshot> ReadExclusivitySettingSnapshots(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        var snapshots = new List<ExclusivitySettingSnapshot>(count);
        for (int i = 0; i < count; i++)
        {
            int maxAssign = reader.ReadInt32();
            int roleCount = reader.ReadInt32();
            var roles = new List<string>(roleCount);
            for (int j = 0; j < roleCount; j++)
                roles.Add(reader.ReadString());

            snapshots.Add(new ExclusivitySettingSnapshot
            {
                MaxAssign = maxAssign,
                Roles = roles
            });
        }
        return snapshots;
    }

    private static List<ModifierRoleOptionSnapshot> ReadModifierRoleOptionSnapshots(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        var snapshots = new List<ModifierRoleOptionSnapshot>(count);
        for (int i = 0; i < count; i++)
        {
            string modifierRoleId = reader.ReadString();
            var snapshot = new ModifierRoleOptionSnapshot
            {
                ModifierRoleId = modifierRoleId,
                NumberOfCrews = reader.ReadByte(),
                Percentage = reader.ReadInt32(),
                MaxImpostors = reader.ReadInt32(),
                ImpostorChance = reader.ReadInt32(),
                MaxNeutrals = reader.ReadInt32(),
                NeutralChance = reader.ReadInt32(),
                MaxCrewmates = reader.ReadInt32(),
                CrewmateChance = reader.ReadInt32()
            };

            if (ModifierHasAssignFilter(modifierRoleId))
            {
                int assignFilterCount = reader.ReadInt32();
                for (int j = 0; j < assignFilterCount; j++)
                    snapshot.AssignFilterRoles.Add(reader.ReadString());
            }

            snapshots.Add(snapshot);
        }
        return snapshots;
    }

    private static List<GhostRoleOptionSnapshot> ReadGhostRoleOptionSnapshots(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        var snapshots = new List<GhostRoleOptionSnapshot>(count);
        for (int i = 0; i < count; i++)
        {
            snapshots.Add(new GhostRoleOptionSnapshot
            {
                GhostRoleId = reader.ReadString(),
                NumberOfCrews = reader.ReadByte(),
                Percentage = reader.ReadInt32()
            });
        }
        return snapshots;
    }

    private static List<CategoryAssignFilterSnapshot> ReadCategoryAssignFilterSnapshots(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        var snapshots = new List<CategoryAssignFilterSnapshot>(count);
        for (int i = 0; i < count; i++)
        {
            string categoryName = reader.ReadString();
            int roleCount = reader.ReadInt32();
            var roles = new List<string>(roleCount);
            for (int j = 0; j < roleCount; j++)
                roles.Add(reader.ReadString());

            snapshots.Add(new CategoryAssignFilterSnapshot
            {
                CategoryName = categoryName,
                Roles = roles
            });
        }
        return snapshots;
    }

    private static void WriteOptionSnapshots(BinaryWriter writer, Dictionary<string, byte> options)
    {
        writer.Write(options.Count);
        foreach (var pair in options)
        {
            writer.Write(pair.Key);
            writer.Write(pair.Value);
        }
    }

    private static void WriteRoleOptionSnapshots(BinaryWriter writer, IReadOnlyCollection<RoleOptionSnapshot> snapshots)
    {
        writer.Write(snapshots.Count);
        foreach (var snapshot in snapshots)
        {
            writer.Write(snapshot.RoleId);
            writer.Write(snapshot.NumberOfCrews);
            writer.Write(snapshot.Percentage);
        }
    }

    private static void WriteExclusivitySettingSnapshots(BinaryWriter writer, IReadOnlyCollection<ExclusivitySettingSnapshot> snapshots)
    {
        writer.Write(snapshots.Count);
        foreach (var snapshot in snapshots)
        {
            writer.Write(snapshot.MaxAssign);
            writer.Write(snapshot.Roles.Count);
            foreach (string role in snapshot.Roles)
                writer.Write(role);
        }
    }

    private static void WriteModifierRoleOptionSnapshots(BinaryWriter writer, IReadOnlyCollection<ModifierRoleOptionSnapshot> snapshots)
    {
        writer.Write(snapshots.Count);
        foreach (var snapshot in snapshots)
        {
            writer.Write(snapshot.ModifierRoleId);
            writer.Write(snapshot.NumberOfCrews);
            writer.Write(snapshot.Percentage);
            writer.Write(snapshot.MaxImpostors);
            writer.Write(snapshot.ImpostorChance);
            writer.Write(snapshot.MaxNeutrals);
            writer.Write(snapshot.NeutralChance);
            writer.Write(snapshot.MaxCrewmates);
            writer.Write(snapshot.CrewmateChance);

            if (ModifierHasAssignFilter(snapshot.ModifierRoleId))
            {
                writer.Write(snapshot.AssignFilterRoles.Count);
                foreach (string role in snapshot.AssignFilterRoles)
                    writer.Write(role);
            }
        }
    }

    private static void WriteGhostRoleOptionSnapshots(BinaryWriter writer, IReadOnlyCollection<GhostRoleOptionSnapshot> snapshots)
    {
        writer.Write(snapshots.Count);
        foreach (var snapshot in snapshots)
        {
            writer.Write(snapshot.GhostRoleId);
            writer.Write(snapshot.NumberOfCrews);
            writer.Write(snapshot.Percentage);
        }
    }

    private static void WriteCategoryAssignFilterSnapshots(BinaryWriter writer, IReadOnlyCollection<CategoryAssignFilterSnapshot> snapshots)
    {
        writer.Write(snapshots.Count);
        foreach (var snapshot in snapshots)
        {
            writer.Write(snapshot.CategoryName);
            writer.Write(snapshot.Roles.Count);
            foreach (string role in snapshot.Roles)
                writer.Write(role);
        }
    }

    private static bool ModifierHasAssignFilter(string modifierRoleId)
    {
        if (!Enum.TryParse<ModifierRoleId>(modifierRoleId, out var parsedModifierRoleId))
            return false;
        return CustomRoleManager.AllModifiers
            .FirstOrDefault(role => role.ModifierRole == parsedModifierRoleId)
            ?.AssignFilter == true;
    }
}

internal static class PresetSnapshotRuntimeApplier
{
    public static void Apply(PresetSnapshot snapshot)
    {
        ApplyRoleOptions(snapshot.RoleOptions);
        ApplyExclusivitySettings(snapshot.ExclusivitySettings);
        ApplyModifierRoleOptions(snapshot.ModifierRoleOptions);
        ApplyGhostRoleOptions(snapshot.GhostRoleOptions);
        ApplyCategoryAssignFilters(snapshot.CategoryAssignFilters);
    }

    private static void ApplyRoleOptions(IEnumerable<RoleOptionSnapshot> snapshots)
    {
        if (RoleOptionManager.RoleOptions == null)
            return;

        foreach (var snapshot in snapshots)
        {
            if (!Enum.TryParse<RoleId>(snapshot.RoleId, out var roleId))
                continue;

            var roleOption = RoleOptionManager.RoleOptions.FirstOrDefault(option => option.RoleId == roleId);
            if (roleOption == null)
                continue;

            roleOption.NumberOfCrews = snapshot.NumberOfCrews;
            roleOption.Percentage = snapshot.Percentage;
        }
    }

    private static void ApplyExclusivitySettings(IEnumerable<ExclusivitySettingSnapshot> snapshots)
    {
        RoleOptionManager.ClearLocalExclusivitySettings();
        foreach (var snapshot in snapshots)
        {
            string[] validRoles = snapshot.Roles
                .Where(role => Enum.TryParse<RoleId>(role, out _))
                .ToArray();
            RoleOptionManager.AddLocalExclusivitySetting(snapshot.MaxAssign, validRoles);
        }
    }

    private static void ApplyModifierRoleOptions(IEnumerable<ModifierRoleOptionSnapshot> snapshots)
    {
        if (RoleOptionManager.ModifierRoleOptions == null)
            return;

        foreach (var snapshot in snapshots)
        {
            if (!Enum.TryParse<ModifierRoleId>(snapshot.ModifierRoleId, out var modifierRoleId))
                continue;

            var modifierRoleOption = RoleOptionManager.ModifierRoleOptions.FirstOrDefault(option => option.ModifierRoleId == modifierRoleId);
            if (modifierRoleOption == null)
                continue;

            modifierRoleOption.NumberOfCrews = snapshot.NumberOfCrews;
            modifierRoleOption.Percentage = snapshot.Percentage;
            modifierRoleOption.MaxImpostors = snapshot.MaxImpostors;
            modifierRoleOption.ImpostorChance = snapshot.ImpostorChance;
            modifierRoleOption.MaxNeutrals = snapshot.MaxNeutrals;
            modifierRoleOption.NeutralChance = snapshot.NeutralChance;
            modifierRoleOption.MaxCrewmates = snapshot.MaxCrewmates;
            modifierRoleOption.CrewmateChance = snapshot.CrewmateChance;

            modifierRoleOption.AssignFilterList = snapshot.AssignFilterRoles
                .Where(role => Enum.TryParse<RoleId>(role, out _))
                .Select(Enum.Parse<RoleId>)
                .ToList();
        }
    }

    private static void ApplyGhostRoleOptions(IEnumerable<GhostRoleOptionSnapshot> snapshots)
    {
        if (RoleOptionManager.GhostRoleOptions == null)
            return;

        foreach (var snapshot in snapshots)
        {
            if (!Enum.TryParse<GhostRoleId>(snapshot.GhostRoleId, out var ghostRoleId))
                continue;

            var ghostRoleOption = RoleOptionManager.GhostRoleOptions.FirstOrDefault(option => option.RoleId == ghostRoleId);
            if (ghostRoleOption == null)
                continue;

            ghostRoleOption.NumberOfCrews = snapshot.NumberOfCrews;
            ghostRoleOption.Percentage = snapshot.Percentage;
        }
    }

    private static void ApplyCategoryAssignFilters(IEnumerable<CategoryAssignFilterSnapshot> snapshots)
    {
        foreach (var snapshot in snapshots)
        {
            var category = CustomOptionManager.OptionCategories.FirstOrDefault(
                optionCategory => optionCategory.Name == snapshot.CategoryName && optionCategory.HasModifierAssignFilter);
            if (category == null)
                continue;

            category.ModifierAssignFilter = snapshot.Roles
                .Where(role => Enum.TryParse<RoleId>(role, out _))
                .Select(Enum.Parse<RoleId>)
                .ToList();
        }
    }
}
