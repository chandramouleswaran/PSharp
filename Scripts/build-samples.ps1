param(
    [string]$dotnet="dotnet",
    [string]$msbuild="msbuild",
    [ValidateSet("Debug","Release")]
    [string]$configuration="Release"
)

Import-Module $PSScriptRoot\powershell\common.psm1

$samples_dir = "$PSScriptRoot\..\Samples"
$projects = "BoundedAsync\BoundedAsync.PSharpLanguage\BoundedAsync.PSharpLanguage.csproj",
    "BoundedAsync\BoundedAsync.PSharpLibrary\BoundedAsync.PSharpLibrary.csproj",
    "CacheCoherence\CacheCoherence.PSharpLanguage\CacheCoherence.PSharpLanguage.csproj",
    "CacheCoherence\CacheCoherence.PSharpLibrary\CacheCoherence.PSharpLibrary.csproj",
    "ChainReplication\ChainReplication.PSharpLibrary\ChainReplication.PSharpLibrary.csproj",
    "Chord\Chord.PSharpLibrary\Chord.PSharpLibrary.csproj",
    "FailureDetector\FailureDetector.PSharpLanguage\FailureDetector.PSharpLanguage.csproj",
    "FailureDetector\FailureDetector.PSharpLibrary\FailureDetector.PSharpLibrary.csproj",
    "MultiPaxos\MultiPaxos.PSharpLanguage\MultiPaxos.PSharpLanguage.csproj",
    "MultiPaxos\MultiPaxos.PSharpLibrary\MultiPaxos.PSharpLibrary.csproj",
    "PingPong\PingPong.CustomLogging\PingPong.CustomLogging.csproj",
    "PingPong\PingPong.MixedMode\PingPong.MixedMode.csproj",
    "PingPong\PingPong.PSharpLanguage\PingPong.PSharpLanguage.csproj",
    "PingPong\PingPong.PSharpLanguage.AsyncAwait\PingPong.PSharpLanguage.AsyncAwait.csproj",
    "PingPong\PingPong.PSharpLibrary\PingPong.PSharpLibrary.csproj",
    "PingPong\PingPong.PSharpLibrary.AsyncAwait\PingPong.PSharpLibrary.AsyncAwait.csproj",
    "Raft\Raft.PSharpLanguage\Raft.PSharpLanguage.csproj",
    "Raft\Raft.PSharpLibrary\Raft.PSharpLibrary.csproj",
    "ReplicatingStorage\ReplicatingStorage.PSharpLanguage\ReplicatingStorage.PSharpLanguage.csproj",
    "ReplicatingStorage\ReplicatingStorage.PSharpLibrary\ReplicatingStorage.PSharpLibrary.csproj",
    "TwoPhaseCommit\TwoPhaseCommit.PSharpLibrary\TwoPhaseCommit.PSharpLibrary.csproj",
    "Timers\TimerSample\TimerSample.csproj"

Write-Comment -prefix "." -text "Building P# samples" -color "yellow"
Write-Comment -prefix "..." -text "Configuration: $configuration" -color "white"
foreach ($project in $projects) {
    Invoke-DotnetRestore -dotnet $dotnet -project $samples_dir\$project
    New-MSBuildProject -msbuild $msbuild -project $samples_dir\$project -configuration $configuration
}

Write-Comment -prefix "." -text "Successfully built P# samples" -color "green"
