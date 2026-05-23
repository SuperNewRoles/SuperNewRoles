using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Modules;

public sealed class PresetSnapshot
{
    public int? SourcePresetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, byte> Options { get; set; } = new();
    public List<RoleOptionSnapshot> RoleOptions { get; set; } = new();
    public bool HasExclusivitySettingsSection { get; set; }
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
    public const int MaxArchiveFileBytes = 32 * 1024 * 1024;
    private const int MaxOptionsEntryBytes = 1024 * 1024;
    private const int MaxPresetEntryBytes = 4 * 1024 * 1024;

    public static byte[] ReadArchiveFileBytes(string path)
    {
        var fileInfo = new FileInfo(path);
        if (!fileInfo.Exists)
            throw new FileNotFoundException("Preset archive file was not found.", path);
        if (fileInfo.Length > MaxArchiveFileBytes)
            throw new PresetImportExportException("Preset archive file is too large.");

        return File.ReadAllBytes(path);
    }

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
            ArchiveOptionData optionsData = ReadOptionsData(ReadEntryBytes(optionsEntry, MaxOptionsEntryBytes, "Options.data"));
            var presetDataBySourceId = ReadPresetEntries(archive);
            var requestedPresets = GetRequestedPresets(optionsData, presetDataBySourceId);
            ValidateRequestedPresetData(requestedPresets, presetDataBySourceId);

            storage.EnsureStorageExists();
            var importPlans = BuildImportPlans(
                requestedPresets,
                presetDataBySourceId,
                presetNames,
                storage.GetExistingPresetDataIds());
            var updatedPresetNames = BuildUpdatedPresetNames(presetNames, importPlans);

            CommitImportPlans(storage, currentVersion, currentPreset, updatedPresetNames, importPlans);
            ReplacePresetNames(presetNames, updatedPresetNames);

            return new PresetImportResult(importPlans.Select(plan => plan.TargetPresetId).ToList());
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
                presetDataBySourceId[presetId] = ReadEntryBytes(entry, MaxPresetEntryBytes, $"PresetOptions_{presetId}.data");
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

    private static byte[] ReadEntryBytes(ZipArchiveEntry entry, int maxBytes, string entryName)
    {
        if (entry.Length > maxBytes)
            throw new PresetImportExportException($"{entryName} is too large.");

        using var stream = entry.Open();
        using var memoryStream = new MemoryStream();
        byte[] buffer = new byte[8192];
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            if (memoryStream.Length + read > maxBytes)
                throw new PresetImportExportException($"{entryName} is too large.");
            memoryStream.Write(buffer, 0, read);
        }

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
        int presetNameCount = PresetRawDataLimits.ReadCount(reader, PresetRawDataLimits.MaxPresetNames, "preset name count");

        var presetNames = new Dictionary<int, string>();
        for (int i = 0; i < presetNameCount; i++)
        {
            int presetId = reader.ReadInt32();
            string name = PresetRawDataLimits.ReadPresetName(reader);
            if (presetId >= 0)
                presetNames[presetId] = name ?? string.Empty;
        }

        return new ArchiveOptionData(currentPreset, presetNames);
    }

    private static void ValidatePresetRawData(int sourcePresetId, byte[] data)
    {
        if (data == null || data.Length < 6)
            throw new PresetImportExportException($"PresetOptions_{sourcePresetId}.data is too short.");

        try
        {
            using var memoryStream = new MemoryStream(data, writable: false);
            using var reader = new BinaryReader(memoryStream);
            if (!ValidateChecksum(reader))
                throw new PresetImportExportException($"PresetOptions_{sourcePresetId}.data checksum is invalid.");

            ReadOptionsForValidation(reader);
            if (memoryStream.Position < memoryStream.Length)
                ReadRoleOptionsForValidation(reader);
            if (memoryStream.Position < memoryStream.Length)
                ReadExclusivitySettingsForValidation(reader);
            if (memoryStream.Position < memoryStream.Length)
                PresetRawDataTailReader.Read(reader, memoryStream.Length, out _, out _, out _);
        }
        catch (PresetImportExportException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PresetImportExportException($"PresetOptions_{sourcePresetId}.data is invalid.", ex);
        }
    }

    private static List<ArchivePresetReference> GetRequestedPresets(
        ArchiveOptionData optionsData,
        IReadOnlyDictionary<int, byte[]> presetDataBySourceId)
    {
        var requestedPresets = optionsData.PresetNames.Keys
            .Concat(presetDataBySourceId.Keys)
            .Where(presetId => presetId >= 0)
            .Distinct()
            .OrderBy(presetId => presetId)
            .Select(presetId => new ArchivePresetReference(
                presetId,
                optionsData.PresetNames.TryGetValue(presetId, out string name)
                    ? name
                    : GetExportPresetName(string.Empty, presetId)))
            .ToList();

        if (requestedPresets.Count == 0)
            throw new PresetImportExportException("Preset archive has no presets.");

        return requestedPresets;
    }

    private static void ValidateRequestedPresetData(
        IEnumerable<ArchivePresetReference> requestedPresets,
        IReadOnlyDictionary<int, byte[]> presetDataBySourceId)
    {
        foreach (var preset in requestedPresets)
        {
            if (!presetDataBySourceId.TryGetValue(preset.SourcePresetId, out byte[] presetData))
                throw new PresetImportExportException($"Preset archive is missing PresetOptions_{preset.SourcePresetId}.data.");
            ValidatePresetRawData(preset.SourcePresetId, presetData);
        }
    }

    private static List<ArchivePresetImportPlan> BuildImportPlans(
        IEnumerable<ArchivePresetReference> requestedPresets,
        IReadOnlyDictionary<int, byte[]> presetDataBySourceId,
        IDictionary<int, string> presetNames,
        IEnumerable<int> existingPresetDataIds)
    {
        var usedIds = new HashSet<int>(presetNames.Keys.Where(id => id >= 0));
        foreach (int presetId in existingPresetDataIds.Where(id => id >= 0))
            usedIds.Add(presetId);

        var usedNames = new HashSet<string>(
            presetNames.Values
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(NormalizePresetNameKey),
            StringComparer.OrdinalIgnoreCase);
        long nextPresetId = usedIds.Count == 0 ? 0 : (long)usedIds.Max() + 1;
        var importPlans = new List<ArchivePresetImportPlan>();

        foreach (var preset in requestedPresets)
        {
            int targetPresetId = GetNextImportedPresetId(usedIds, ref nextPresetId);
            string targetName = GetUniquePresetName(preset.Name, usedNames);
            usedIds.Add(targetPresetId);
            importPlans.Add(new ArchivePresetImportPlan(
                targetPresetId,
                targetName,
                presetDataBySourceId[preset.SourcePresetId]));
        }

        return importPlans;
    }

    private static Dictionary<int, string> BuildUpdatedPresetNames(
        IDictionary<int, string> currentPresetNames,
        IEnumerable<ArchivePresetImportPlan> importPlans)
    {
        var updatedPresetNames = new Dictionary<int, string>(currentPresetNames);
        foreach (var plan in importPlans)
            updatedPresetNames[plan.TargetPresetId] = plan.Name;
        PresetRawDataLimits.ValidatePresetNamesForWrite(updatedPresetNames);
        return updatedPresetNames;
    }

    private static void CommitImportPlans(
        IOptionStorage storage,
        byte currentVersion,
        int currentPreset,
        IReadOnlyDictionary<int, string> updatedPresetNames,
        IReadOnlyCollection<ArchivePresetImportPlan> importPlans)
    {
        var savedPresetIds = new List<int>();
        try
        {
            foreach (var plan in importPlans)
            {
                storage.SavePresetRawData(plan.TargetPresetId, plan.PresetData);
                savedPresetIds.Add(plan.TargetPresetId);
            }

            storage.SaveOptionData(currentVersion, currentPreset, updatedPresetNames);
        }
        catch
        {
            RollBackSavedPresets(storage, savedPresetIds);
            throw;
        }
    }

    private static void RollBackSavedPresets(IOptionStorage storage, IEnumerable<int> savedPresetIds)
    {
        foreach (int savedPresetId in savedPresetIds)
        {
            try
            {
                storage.DeletePresetRawData(savedPresetId);
            }
            catch (Exception rollbackEx)
            {
                Logger.Warning($"Failed to roll back imported preset {savedPresetId}: {rollbackEx.Message}");
            }
        }
    }

    private static void ReplacePresetNames(
        IDictionary<int, string> targetPresetNames,
        IReadOnlyDictionary<int, string> updatedPresetNames)
    {
        targetPresetNames.Clear();
        foreach (var pair in updatedPresetNames)
            targetPresetNames[pair.Key] = pair.Value;
    }

    private static void ReadOptionsForValidation(BinaryReader reader)
    {
        int optionCount = PresetRawDataLimits.ReadCount(reader, PresetRawDataLimits.MaxOptions, "option count");
        for (int i = 0; i < optionCount; i++)
        {
            _ = PresetRawDataLimits.ReadOptionId(reader);
            _ = reader.ReadByte();
        }
    }

    private static void ReadRoleOptionsForValidation(BinaryReader reader)
    {
        int count = PresetRawDataLimits.ReadCount(reader, PresetRawDataLimits.MaxRoleOptions, "role option count");
        for (int i = 0; i < count; i++)
        {
            _ = PresetRawDataLimits.ReadRoleId(reader);
            _ = reader.ReadByte();
            _ = reader.ReadInt32();
        }
    }

    private static void ReadExclusivitySettingsForValidation(BinaryReader reader)
    {
        int count = PresetRawDataLimits.ReadCount(reader, PresetRawDataLimits.MaxExclusivitySettings, "exclusivity setting count");
        for (int i = 0; i < count; i++)
        {
            _ = reader.ReadInt32();
            int roleCount = PresetRawDataLimits.ReadCount(reader, PresetRawDataLimits.MaxExclusivityRoles, "exclusivity role count");
            for (int j = 0; j < roleCount; j++)
                _ = PresetRawDataLimits.ReadRoleId(reader, "exclusivity role id");
        }
    }

    private static bool ValidateChecksum(BinaryReader reader)
    {
        int random = reader.ReadByte();
        int checksum = reader.ReadByte();
        return random * random == checksum;
    }

    private static int GetNextImportedPresetId(ISet<int> usedIds, ref long nextPresetId)
    {
        long maxPresetCount = CustomOptionSaver.MaxPresetCount;
        if (nextPresetId < 0 || nextPresetId >= maxPresetCount)
            nextPresetId = 0;

        long scanned = 0;
        while (scanned < maxPresetCount)
        {
            int candidate = (int)nextPresetId;
            nextPresetId++;
            scanned++;
            if (nextPresetId >= maxPresetCount)
                nextPresetId = 0;
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

    private sealed class ArchivePresetReference
    {
        public int SourcePresetId { get; }
        public string Name { get; }

        public ArchivePresetReference(int sourcePresetId, string name)
        {
            SourcePresetId = sourcePresetId;
            Name = name;
        }
    }

    private sealed class ArchivePresetImportPlan
    {
        public int TargetPresetId { get; }
        public string Name { get; }
        public byte[] PresetData { get; }

        public ArchivePresetImportPlan(int targetPresetId, string name, byte[] presetData)
        {
            TargetPresetId = targetPresetId;
            Name = name;
            PresetData = presetData;
        }
    }
}

public static partial class CustomOptionSaver
{
    public static byte[] ExportCurrentPresetArchive()
    {
        return ExportSelectedPresetsArchive(new[] { CurrentPreset });
    }

    public static byte[] ExportAllPresetsArchive()
    {
        var presetIds = PresetNames.Keys.Concat(new[] { CurrentPreset });
        return ExportSelectedPresetsArchive(presetIds);
    }

    public static byte[] ExportSelectedPresetsArchive(IEnumerable<int> presetIds)
    {
        Save();
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

internal static class PresetRawDataLimits
{
    public const int MaxPresetNames = 256;
    public const int MaxPresetNameBytes = 1024;
    public const int MaxPresetNameLength = 128;
    public const int MaxOptionIdBytes = 1024;
    public const int MaxOptionIdLength = 256;
    public const int MaxRoleIdBytes = 256;
    public const int MaxRoleIdLength = 128;
    public const int MaxCategoryNameBytes = 512;
    public const int MaxCategoryNameLength = 128;
    public const int MaxOptions = 4096;
    public const int MaxRoleOptions = 256;
    public const int MaxExclusivitySettings = 256;
    public const int MaxExclusivityRoles = 256;
    public const int MaxModifierRoleOptions = 256;
    public const int MaxModifierAssignFilterRoles = 256;
    public const int MaxGhostRoleOptions = 256;
    public const int MaxCategoryAssignFilters = 128;
    public const int MaxCategoryAssignFilterRoles = 256;

    public static string ReadPresetName(BinaryReader reader)
        => ReadLimitedString(reader, MaxPresetNameBytes, MaxPresetNameLength, "preset name");

    public static string ReadOptionId(BinaryReader reader)
        => ReadLimitedString(reader, MaxOptionIdBytes, MaxOptionIdLength, "option id");

    public static string ReadRoleId(BinaryReader reader, string fieldName = "role id")
        => ReadLimitedString(reader, MaxRoleIdBytes, MaxRoleIdLength, fieldName);

    public static string ReadCategoryName(BinaryReader reader)
        => ReadLimitedString(reader, MaxCategoryNameBytes, MaxCategoryNameLength, "category name");

    public static void ValidatePresetNamesForWrite(IReadOnlyDictionary<int, string> presetNames)
    {
        if (presetNames == null)
            throw new InvalidDataException("Preset name list is null.");
        if (presetNames.Count > MaxPresetNames)
            throw new InvalidDataException("Preset data preset name count is invalid.");

        foreach (var pair in presetNames)
            ValidatePresetNameForWrite(pair.Value);
    }

    public static bool IsPresetNameWithinLimits(string value)
    {
        string name = value ?? string.Empty;
        return name.Length <= MaxPresetNameLength
            && Encoding.UTF8.GetByteCount(name) <= MaxPresetNameBytes;
    }

    private static void ValidatePresetNameForWrite(string value)
    {
        if (!IsPresetNameWithinLimits(value))
            throw new InvalidDataException("Preset data preset name is too long.");
    }

    public static int ReadCount(BinaryReader reader, int maxCount, string fieldName)
    {
        int count = reader.ReadInt32();
        if (count < 0 || count > maxCount)
            throw new InvalidDataException($"Preset data {fieldName} is invalid.");
        return count;
    }

    private static string ReadLimitedString(BinaryReader reader, int maxBytes, int maxLength, string fieldName)
    {
        int byteCount = Read7BitEncodedInt(reader, fieldName);
        if (byteCount < 0 || byteCount > maxBytes)
            throw new InvalidDataException($"Preset data {fieldName} is too long.");

        byte[] bytes = reader.ReadBytes(byteCount);
        if (bytes.Length != byteCount)
            throw new EndOfStreamException();

        string value = Encoding.UTF8.GetString(bytes);
        if (value.Length > maxLength)
            throw new InvalidDataException($"Preset data {fieldName} is too long.");
        return value;
    }

    private static int Read7BitEncodedInt(BinaryReader reader, string fieldName)
    {
        int count = 0;
        int shift = 0;
        while (shift != 35)
        {
            byte b = reader.ReadByte();
            count |= (b & 0x7F) << shift;
            shift += 7;
            if ((b & 0x80) == 0)
                return count;
        }

        throw new InvalidDataException($"Preset data {fieldName} length is invalid.");
    }
}

internal static class PresetRawDataTailReader
{
    private const int MaxParseBranchAttempts = 4096;

    public static void Read(
        BinaryReader reader,
        long streamLength,
        out List<ModifierRoleOptionSnapshot> modifierRoleOptions,
        out List<GhostRoleOptionSnapshot> ghostRoleOptions,
        out List<CategoryAssignFilterSnapshot> categoryAssignFilters)
    {
        long tailStart = reader.BaseStream.Position;
        if (tailStart > streamLength)
            throw new PresetImportExportException("Preset data tail position is invalid.");

        if (streamLength - tailStart > int.MaxValue)
            throw new PresetImportExportException("Preset data tail is too large.");

        byte[] tail = reader.ReadBytes((int)(streamLength - tailStart));
        if (TryRead(tail, out modifierRoleOptions, out ghostRoleOptions, out categoryAssignFilters))
            return;

        throw new PresetImportExportException("Preset data modifier/ghost/category sections are invalid.");
    }

    private static bool TryRead(
        byte[] tail,
        out List<ModifierRoleOptionSnapshot> modifierRoleOptions,
        out List<GhostRoleOptionSnapshot> ghostRoleOptions,
        out List<CategoryAssignFilterSnapshot> categoryAssignFilters)
    {
        modifierRoleOptions = new List<ModifierRoleOptionSnapshot>();
        ghostRoleOptions = new List<GhostRoleOptionSnapshot>();
        categoryAssignFilters = new List<CategoryAssignFilterSnapshot>();

        try
        {
            using var memoryStream = new MemoryStream(tail, writable: false);
            using var reader = new BinaryReader(memoryStream);
            int modifierCount = PresetRawDataLimits.ReadCount(reader, PresetRawDataLimits.MaxModifierRoleOptions, "modifier role option count");
            int parseBranchAttempts = 0;
            if (TryReadModifierRoleOptions(
                reader,
                modifierCount,
                new List<ModifierRoleOptionSnapshot>(modifierCount),
                ref parseBranchAttempts,
                out modifierRoleOptions,
                out ghostRoleOptions,
                out categoryAssignFilters))
            {
                return true;
            }
        }
        catch (Exception ex) when (IsReadFailure(ex))
        {
        }

        modifierRoleOptions = new List<ModifierRoleOptionSnapshot>();
        ghostRoleOptions = new List<GhostRoleOptionSnapshot>();
        categoryAssignFilters = new List<CategoryAssignFilterSnapshot>();
        return false;
    }

    private static bool TryReadModifierRoleOptions(
        BinaryReader reader,
        int remainingCount,
        List<ModifierRoleOptionSnapshot> snapshots,
        ref int parseBranchAttempts,
        out List<ModifierRoleOptionSnapshot> modifierRoleOptions,
        out List<GhostRoleOptionSnapshot> ghostRoleOptions,
        out List<CategoryAssignFilterSnapshot> categoryAssignFilters)
    {
        modifierRoleOptions = new List<ModifierRoleOptionSnapshot>();
        ghostRoleOptions = new List<GhostRoleOptionSnapshot>();
        categoryAssignFilters = new List<CategoryAssignFilterSnapshot>();

        if (remainingCount == 0)
        {
            if (!TryReadRemainingSections(reader, out ghostRoleOptions, out categoryAssignFilters))
                return false;

            modifierRoleOptions = snapshots;
            return true;
        }

        long snapshotStart = reader.BaseStream.Position;
        ModifierRoleOptionSnapshot snapshot;
        try
        {
            snapshot = ReadModifierRoleOptionWithoutAssignFilters(reader);
        }
        catch (Exception ex) when (IsReadFailure(ex))
        {
            return false;
        }

        var branchOrder = ModifierHasAssignFilter(snapshot.ModifierRoleId)
            ? new[] { true, false }
            : new[] { false, true };

        foreach (bool readAssignFilterPayload in branchOrder)
        {
            parseBranchAttempts++;
            if (parseBranchAttempts > MaxParseBranchAttempts)
                return false;

            reader.BaseStream.Position = snapshotStart;
            int snapshotCountBeforeBranch = snapshots.Count;
            try
            {
                var candidate = ReadModifierRoleOptionWithoutAssignFilters(reader);
                if (readAssignFilterPayload)
                    ReadAssignFilterRoles(reader, candidate.AssignFilterRoles);

                snapshots.Add(candidate);
                if (TryReadModifierRoleOptions(
                    reader,
                    remainingCount - 1,
                    snapshots,
                    ref parseBranchAttempts,
                    out modifierRoleOptions,
                    out ghostRoleOptions,
                    out categoryAssignFilters))
                {
                    return true;
                }

                snapshots.RemoveAt(snapshots.Count - 1);
            }
            catch (Exception ex) when (IsReadFailure(ex))
            {
                while (snapshots.Count > snapshotCountBeforeBranch)
                    snapshots.RemoveAt(snapshots.Count - 1);
            }
        }

        reader.BaseStream.Position = snapshotStart;
        return false;
    }

    private static bool TryReadRemainingSections(
        BinaryReader reader,
        out List<GhostRoleOptionSnapshot> ghostRoleOptions,
        out List<CategoryAssignFilterSnapshot> categoryAssignFilters)
    {
        ghostRoleOptions = new List<GhostRoleOptionSnapshot>();
        categoryAssignFilters = new List<CategoryAssignFilterSnapshot>();

        try
        {
            if (reader.BaseStream.Position == reader.BaseStream.Length)
                return true;

            ghostRoleOptions = ReadGhostRoleOptions(reader);
            if (reader.BaseStream.Position == reader.BaseStream.Length)
                return true;

            categoryAssignFilters = ReadCategoryAssignFilters(reader);
            return reader.BaseStream.Position == reader.BaseStream.Length;
        }
        catch (Exception ex) when (IsReadFailure(ex))
        {
            ghostRoleOptions = new List<GhostRoleOptionSnapshot>();
            categoryAssignFilters = new List<CategoryAssignFilterSnapshot>();
            return false;
        }
    }

    private static ModifierRoleOptionSnapshot ReadModifierRoleOptionWithoutAssignFilters(BinaryReader reader)
    {
        return new ModifierRoleOptionSnapshot
        {
            ModifierRoleId = PresetRawDataLimits.ReadRoleId(reader, "modifier role id"),
            NumberOfCrews = reader.ReadByte(),
            Percentage = reader.ReadInt32(),
            MaxImpostors = reader.ReadInt32(),
            ImpostorChance = reader.ReadInt32(),
            MaxNeutrals = reader.ReadInt32(),
            NeutralChance = reader.ReadInt32(),
            MaxCrewmates = reader.ReadInt32(),
            CrewmateChance = reader.ReadInt32()
        };
    }

    private static void ReadAssignFilterRoles(BinaryReader reader, List<string> roles)
    {
        int assignFilterCount = PresetRawDataLimits.ReadCount(reader, PresetRawDataLimits.MaxModifierAssignFilterRoles, "modifier assign filter count");
        for (int i = 0; i < assignFilterCount; i++)
            roles.Add(PresetRawDataLimits.ReadRoleId(reader, "modifier assign filter role id"));
    }

    private static List<GhostRoleOptionSnapshot> ReadGhostRoleOptions(BinaryReader reader)
    {
        int count = PresetRawDataLimits.ReadCount(reader, PresetRawDataLimits.MaxGhostRoleOptions, "ghost role option count");
        var snapshots = new List<GhostRoleOptionSnapshot>(count);
        for (int i = 0; i < count; i++)
        {
            snapshots.Add(new GhostRoleOptionSnapshot
            {
                GhostRoleId = PresetRawDataLimits.ReadRoleId(reader, "ghost role id"),
                NumberOfCrews = reader.ReadByte(),
                Percentage = reader.ReadInt32()
            });
        }

        return snapshots;
    }

    private static List<CategoryAssignFilterSnapshot> ReadCategoryAssignFilters(BinaryReader reader)
    {
        int count = PresetRawDataLimits.ReadCount(reader, PresetRawDataLimits.MaxCategoryAssignFilters, "category assign filter count");
        var snapshots = new List<CategoryAssignFilterSnapshot>(count);
        for (int i = 0; i < count; i++)
        {
            string categoryName = PresetRawDataLimits.ReadCategoryName(reader);
            int roleCount = PresetRawDataLimits.ReadCount(reader, PresetRawDataLimits.MaxCategoryAssignFilterRoles, "category assign filter role count");
            var roles = new List<string>(roleCount);
            for (int j = 0; j < roleCount; j++)
                roles.Add(PresetRawDataLimits.ReadRoleId(reader, "category assign filter role id"));

            snapshots.Add(new CategoryAssignFilterSnapshot
            {
                CategoryName = categoryName,
                Roles = roles
            });
        }

        return snapshots;
    }

    private static bool ModifierHasAssignFilter(string modifierRoleId)
    {
        if (!Enum.TryParse<ModifierRoleId>(modifierRoleId, out var parsedModifierRoleId))
            return false;
        return CustomRoleManager.AllModifiers
            .FirstOrDefault(role => role.ModifierRole == parsedModifierRoleId)
            ?.AssignFilter == true;
    }

    private static bool IsReadFailure(Exception ex)
        => ex is EndOfStreamException
            || ex is IOException
            || ex is ArgumentException
            || ex is InvalidDataException;
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

    public void DeletePresetRawData(int preset)
    {
        lock (FileLocker)
        {
            string fileName = GetPresetFileName(preset);
            if (File.Exists(fileName))
                File.Delete(fileName);
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
            {
                snapshot.HasExclusivitySettingsSection = true;
                snapshot.ExclusivitySettings = ReadExclusivitySettingSnapshots(reader);
            }
            if (fileStream.Position < fileStream.Length)
            {
                PresetRawDataTailReader.Read(
                    reader,
                    fileStream.Length,
                    out var modifierRoleOptions,
                    out var ghostRoleOptions,
                    out var categoryAssignFilters);
                snapshot.ModifierRoleOptions = modifierRoleOptions;
                snapshot.GhostRoleOptions = ghostRoleOptions;
                snapshot.CategoryAssignFilters = categoryAssignFilters;
            }

            return (true, snapshot);
        }
    }

    public void SavePresetSnapshot(int preset, PresetSnapshot snapshot)
    {
        lock (FileLocker)
        {
            EnsureStorageExists();
            string fileName = GetPresetFileName(preset);
            using var fileStream = new FileStream(fileName, FileMode.Create);
            using var writer = new BinaryWriter(fileStream);

            WriteChecksum(writer);
            WriteOptionSnapshots(writer, snapshot.Options);
            WriteRoleOptionSnapshots(writer, snapshot.RoleOptions);
            WriteOptionalPresetSections(writer, snapshot);
        }
    }

    private string GetPresetFileName(int preset)
        => $"{_presetFileNameBase}{preset}.data";

    private static List<RoleOptionSnapshot> ReadRoleOptionSnapshots(BinaryReader reader)
    {
        int count = PresetRawDataLimits.ReadCount(reader, PresetRawDataLimits.MaxRoleOptions, "role option count");
        var snapshots = new List<RoleOptionSnapshot>(count);
        for (int i = 0; i < count; i++)
        {
            snapshots.Add(new RoleOptionSnapshot
            {
                RoleId = PresetRawDataLimits.ReadRoleId(reader),
                NumberOfCrews = reader.ReadByte(),
                Percentage = reader.ReadInt32()
            });
        }
        return snapshots;
    }

    private static List<ExclusivitySettingSnapshot> ReadExclusivitySettingSnapshots(BinaryReader reader)
    {
        int count = PresetRawDataLimits.ReadCount(reader, PresetRawDataLimits.MaxExclusivitySettings, "exclusivity setting count");
        var snapshots = new List<ExclusivitySettingSnapshot>(count);
        for (int i = 0; i < count; i++)
        {
            int maxAssign = reader.ReadInt32();
            int roleCount = PresetRawDataLimits.ReadCount(reader, PresetRawDataLimits.MaxExclusivityRoles, "exclusivity role count");
            var roles = new List<string>(roleCount);
            for (int j = 0; j < roleCount; j++)
                roles.Add(PresetRawDataLimits.ReadRoleId(reader, "exclusivity role id"));

            snapshots.Add(new ExclusivitySettingSnapshot
            {
                MaxAssign = maxAssign,
                Roles = roles
            });
        }
        return snapshots;
    }

    private static void WriteOptionalPresetSections(BinaryWriter writer, PresetSnapshot snapshot)
    {
        if (!ShouldWriteExclusivitySection(snapshot))
            return;

        WriteExclusivitySettingSnapshots(writer, snapshot.ExclusivitySettings);
        WriteModifierRoleOptionSnapshots(writer, snapshot.ModifierRoleOptions);
        WriteGhostRoleOptionSnapshots(writer, snapshot.GhostRoleOptions);
        WriteCategoryAssignFilterSnapshots(writer, snapshot.CategoryAssignFilters);
    }

    private static bool ShouldWriteExclusivitySection(PresetSnapshot snapshot)
    {
        // Tail sections are serialized after exclusivity, so tail data still needs the preceding block.
        return snapshot.HasExclusivitySettingsSection
            || snapshot.ExclusivitySettings.Count > 0
            || HasTailSections(snapshot);
    }

    private static bool HasTailSections(PresetSnapshot snapshot)
        => snapshot.ModifierRoleOptions.Count > 0
            || snapshot.GhostRoleOptions.Count > 0
            || snapshot.CategoryAssignFilters.Count > 0;

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
        if (snapshot.HasExclusivitySettingsSection)
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
