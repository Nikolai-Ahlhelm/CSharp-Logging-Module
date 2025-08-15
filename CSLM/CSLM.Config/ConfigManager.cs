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
        Config _config;
        CSLM _log;
        String _configPath = $"{AppContext.BaseDirectory}\\config.json";

        internal ConfigManager(CSLM log) 
        {
            _log = log;
            _config = InitializeConfigObject();

            // Load the configuration from the file if it exists
            if (IfConfigExists(_configPath))
            {
                try
                {
                    string json = File.ReadAllText(_configPath);
                    _config = JsonConvert.DeserializeObject<Config>(json);
                    _log.Log("Configuration loaded successfully.", "ConfigManager", "INFO");
                }
                catch (Exception ex)
                {
                    _log.Log($"Error loading configuration: {ex.Message}", "ConfigManager", "ERROR");
                }
            }
            else
            {
                _log.Log("Configuration file not found, using default settings.", "ConfigManager", "WARN");
            }
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
                json = File.ReadAllText(_configPath);
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
            }
            catch (Exception ex)
            {
                _log.DebugWriteLog("Error",$"[ConfigManager:WriteConfigFile] Error writing configuration file: {ex.Message}");
            }
        }


      





    }

}

