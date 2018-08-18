$hintPathPattern = @"
<hintpath>(\d|\w|\s|\.|\\)*packages
"@

$solutionPath = Get-Location
$solutionPath = Join-Path -Path $solutionPath -ChildPath "src"
echo  "running in solution dir ${solutionPath}"

ls -Recurse -include *.csproj, *.sln, *.fsproj, *.vbproj |
  foreach {
    $dir =Split-Path $_.FullName
    Set-Location -Path $dir
    $command = "nuget install -SolutionDirectory ${solutionPath}"
    echo  "${dir}"
    echo  "${command}"
    iex $command	    
}