using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CSLM.Config
{
    internal class ConfigManager
    {
        internal Config _config;
        private CSLM _log;
        internal String _configPath = $"{AppContext.BaseDirectory}\\config.json";

        internal ConfigManager(CSLM log, string configPath = "") 
        {
            _log = log;

            if (configPath != "") 
            { 
                _configPath = configPath;
                _config = LoadConfigFile(_configPath);
                return;
            }
            
            _config = InitializeConfigObject();

        }

        private Config InitializeConfigObject()
        {
            return new Config();
        }

        private static bool IfConfigExists(string path)
        {
            return File.Exists(path);
        }

        internal string ReadConfigFile(string path)
        {
            string json = string.Empty;
            try
            {
                json = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                _log.DebugWriteLog("Error",$"[ConfigManager:ReadConfigFile] Error reading configuration file: {ex.Message}");
            }
            return json;
        }

        internal void WriteConfigFile(string json)
        {
            try
            {
                File.WriteAllText(_configPath, json);
                _log.DebugWriteLog("INFO", "[ConfigManager:WriteConfigFile] Configuration file written successfully");
                Console.WriteLine($"[ConfigManager:WriteConfigFile] Writing content: {json}");
                Console.WriteLine($"[ConfigManager:WriteConfigFile] Configuration file written successfully to {_configPath}");
            }
            catch (Exception ex)
            {
                _log.DebugWriteLog("Error",$"[ConfigManager:WriteConfigFile] Error writing configuration file: {ex.Message}");
            }
        }

        internal Config DeserializeConfig(string config)
        {
            try
            {
                return JsonConvert.DeserializeObject<Config>(config);
            }
            catch (Exception ex)
            {
                _log.DebugWriteLog("ERROR", $"[ConfigManager:DeserializeConfig] Failed to parse config: {ex.Message}");
                _log.DebugWriteLog("ERROR", $"[ConfigManager:DeserializeConfig] Returning default config object");
                Console.WriteLine($"[ConfigManager:DeserializeConfig] Failed to parse config: {ex.Message}");
                return new Config();
            }
        }

        internal string SerializeConfig(Config conf)
        {
            Console.WriteLine($"[ConfigManager:SerializeConfig] Serializing config object to JSON");
            Console.WriteLine($"[ConfigManager:SerializeConfig] Content: {JsonConvert.SerializeObject(conf, Formatting.Indented)}");
            return JsonConvert.SerializeObject(conf, Formatting.Indented);
        }

        internal void SaveConfigFile(Config conf)
        {
            _log.DebugWriteLog("INFO", $"[ConfigManager:SaveConfigFile] Saving configuration file to disk: {conf}");
            Console.WriteLine($"[ConfigManager:SaveConfigFile] Saving configuration file to disk: {conf}");
            Console.WriteLine($"[ConfigManager:SaveConfigFile] Content: {conf.logType}");
            WriteConfigFile(SerializeConfig(conf));
        }

        internal Config LoadConfigFile(string path)
        {
            if (IfConfigExists(path))
            {
                return DeserializeConfig(ReadConfigFile(path));
            }
            else
            {
                _log.DebugWriteLog("WARNING", "[ConfigManager:LoadConfigFile] Configuration file not found, creating default config file");
                Console.WriteLine("[ConfigManager:LoadConfigFile] Configuration file not found, creating default config file");
                Config conf = InitializeConfigObject();
                _log.DebugWriteLog("INFO", "[ConfigManager:LoadConfigFile] Default configuration file created");
                Console.WriteLine("[ConfigManager:LoadConfigFile] Default configuration file created");
                SaveConfigFile(conf);
                _log.DebugWriteLog("INFO", "[ConfigManager:LoadConfigFile] Default configuration file saved to disk");
                Console.WriteLine("[ConfigManager:LoadConfigFile] Default configuration file saved to disk");
                return conf;
            }
        }
    }

}

