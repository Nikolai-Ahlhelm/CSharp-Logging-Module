# Load DLL
Add-Type -Path ".\CSLM.dll"

# Arguments: logFileName, logFilePath, allowedTypes[List[string]], logType, printToConsole, timestampFormat

# CSLM-Instanz erstellen
<#$log = [CSLM.CSLM]::new(
    "TEST-log-%hh%-%m%-%ss%.txt",   # logFileName
    ".\",                           # logFilePath
    "DEBUG",                        # logType
    $true,                          # printToConsole
    "dd-MM-yyyy HH:mm:ss.fff"       # timestampFormat
) #>

$log = [CSLM.CSLM]::new($true)


# Test log entries
$log.Entry("Info", "Logging Mode: $($logger.LogType)")
$log.Entry("Info", "Log Path: $($logger.LogFileFullPath)")
$log.Entry("Info", "Info Test Message")
$log.Entry("DEBUG", "Debug Test Message")
$log.Entry("w", "Warning Test Message")
$log.Entry("w", "🔥 You can use emojis to make your logs more interesting")
$log.Entry("crit", "Critical Test Message")
$log.Entry("Error", "Error Test Message")

# new more convenient methods
$log.Info("Info function test")
$log.Debug("Debug function test")
$log.Warn("Warning function test")
$log.Crit("Critical function test")
$log.Error("Error function test")

pause


# Note: 
# Executiontime for example.ps1
# 1.1.0: 12 ms
# 1.2.0: 2 ms