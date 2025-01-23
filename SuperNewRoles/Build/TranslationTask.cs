using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;
using System.Text;

namespace SuperNewRoles.Build
{
    public class TranslationTask : Task
    {
        [Required]
        public ITaskItem[] TranslationFiles { get; set; }

        [Required]
        public string OutputPath { get; set; }

        public override bool Execute()
        {
            var sb = new StringBuilder();
            foreach (var file in TranslationFiles)
            {
                var langCode = Path.GetFileNameWithoutExtension(file.ItemSpec);
                sb.AppendLine($"        [\"{langCode}\"] = new Dictionary<string, string>");
                sb.AppendLine("        {");

                var lines = File.ReadAllLines(file.ItemSpec);
                for (int i = 1; i < lines.Length; i++) // Skip header
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length >= 2)
                    {
                        sb.AppendLine($"            [\"{parts[0].Trim()}\"] = \"{parts[1].Trim()}\",");
                    }
                }

                sb.AppendLine("        },");
            }

            var template = File.ReadAllText("ModTranslate.cs.template");
            var output = template.Replace("// {TRANSLATION_DATA}", sb.ToString());
            File.WriteAllText(OutputPath, output);

            return true;
        }
    }
}