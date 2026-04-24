using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Knot.Editor.Utility;
using Knot.Include.Attributes;
using UnityEditor;
using UnityEngine;

namespace Knot.Editor.Construct
{
    /// <summary>
    /// 脚本模板生成器，用于创建新的指令脚本文件
    /// </summary>
    [Version("1.0.0")]
    public class ScriptTemplateGenerator
    {
        private string fileStorePath =
            "./ScriptTemplateGenerator.json";
        [JsonInclude] private string templateFile;
        [JsonInclude] private string folderPath;

        private string template;
        private string scriptName;
        
        private void ReadTemplate()
        {
            GetPath();

            template = File.ReadAllText(templateFile);

            template = template.Replace("_InstrParamTemplate_", $"{scriptName}Param");
            template = template.Replace("_InstrExecuteTemplate_", $"{scriptName}");
        }

        private void GetPath()
        {
            string fileStoreJson = File.ReadAllText(fileStorePath);
            var scriptTemplateGenerator = JsonSerializer.Deserialize<ScriptTemplateGenerator>(fileStoreJson);

            this.templateFile = scriptTemplateGenerator.templateFile;
            this.folderPath = scriptTemplateGenerator.folderPath;
        }

        /// <summary>
        /// 创建脚本文件
        /// </summary>
        /// <param name="scriptName">脚本名称</param>
        /// <returns>是否创建成功</returns>
        public bool CreateScript(string scriptName)
        {
            ValidationResult result = ClassNameValidator.ValidateClassName(scriptName);
            if (!result.IsValid)
            {
                foreach (string error in result.Errors)
                    Debug.Log($"[ScriptTemplateGenerator]  - {error}");
                return false;
            }

            this.scriptName = scriptName;
            ReadTemplate();
            string scriptPath = $"{folderPath}{this.scriptName}.cs";

            try
            {
                if (File.Exists(scriptPath))
                {
                    Debug.LogWarning($"[ScriptTemplateGenerator]文件 {this.scriptName}.cs 已存在！");
                    return false;
                }

                File.WriteAllText(scriptPath, template);

                AssetDatabase.Refresh();

                if (File.Exists(scriptPath))
                {
                    Debug.Log($"[ScriptTemplateGenerator]{this.scriptName}.cs 文件创建成功！");
                    return true;
                }
                else
                {
                    Debug.Log($"[ScriptTemplateGenerator]文件创建后检查失败");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[ScriptTemplateGenerator]创建文件失败: {ex.Message}");
                return false;
            }
        }
    }
}
