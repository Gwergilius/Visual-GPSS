<#
.SYNOPSIS
    Sends one test log event per level to a local Seq server.

.DESCRIPTION
    Posts CLEF-formatted events to Seq's raw ingestion endpoint
    (see docker-compose.yml). Serilog/Seq have no "Trace" level name;
    "Verbose" is the equivalent and is used here for the TRACE case.

.PARAMETER SeqUrl
    Base URL of the Seq server. Defaults to the local docker-compose instance.
#>

param(
    [string]$SeqUrl = "http://localhost:5341"
)

$endpoint = "$SeqUrl/api/events/raw"

$events = @(
    @{ Level = "Verbose";     Label = "TRACE";   Message = "Trace test message: verbose diagnostic detail" }
    @{ Level = "Debug";       Label = "DEBUG";   Message = "Debug test message: diagnostic detail for developers" }
    @{ Level = "Information"; Label = "INFO";    Message = "Info test message: normal application flow" }
    @{ Level = "Warning";     Label = "WARNING"; Message = "Warning test message: unexpected but recoverable condition" }
    @{ Level = "Error";       Label = "ERROR";   Message = "Error test message: operation failed" }
    @{ Level = "Fatal";       Label = "FATAL";   Message = "Fatal test message: application cannot continue" }
)

$lines = foreach ($evt in $events) {
    $clefEvent = [ordered]@{
        "@t"   = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")
        "@mt"  = $evt.Message
        "@l"   = $evt.Level
        Source = "test-seq-logs.ps1"
    }
    $clefEvent | ConvertTo-Json -Compress
}

$body = $lines -join "`n"

Invoke-RestMethod -Uri $endpoint -Method Post -Body $body -ContentType "application/vnd.serilog.clef"

Write-Host "Sent $($events.Count) test log events to $endpoint (levels: $($events.Label -join ', '))"
