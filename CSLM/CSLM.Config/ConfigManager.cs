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

            if (configPath != "") { _configPath = configPath; }
            
            _config = LoadConfig(_configPath);
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

        internal Config DeserializeConfig(string config)
        {
            return JsonConvert.DeserializeObject<Config>(config);
        }

        internal Config LoadConfig(string path)
        {
            if (IfConfigExists(path))
            {
                return DeserializeConfig(ReadConfigFile(path));
            }
            else
            {
                return InitializeConfigObject();
            }
        }







    }

}

