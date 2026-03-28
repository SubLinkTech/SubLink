$version = "3.4.6";

$currentDir = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName;

. dotnet.exe restore $currentDir
. dotnet.exe publish -c Release -r win-x64 $currentDir/SubLink.References/SubLink.References.csproj /p:Version=$version /p:SkipInvalidConfigurations=true;
. dotnet.exe publish -c Release -r win-x64 $currentDir/SubLink/SubLink.csproj /p:Version=$version /p:SkipInvalidConfigurations=true /p:PublishSingleFile=true /p:PublishReadyToRun=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true --self-contained true
. dotnet.exe publish -c Release -r win-x64 $currentDir/SubLink.Twitch/SubLink.Twitch.csproj /p:Version=$version /p:SkipInvalidConfigurations=true
. dotnet.exe publish -c Release -r win-x64 $currentDir/SubLink.Kick/SubLink.Kick.csproj /p:Version=$version /p:SkipInvalidConfigurations=true
. dotnet.exe publish -c Release -r win-x64 $currentDir/SubLink.Streampad/SubLink.Streampad.csproj /p:Version=$version /p:SkipInvalidConfigurations=true
. dotnet.exe publish -c Release -r win-x64 $currentDir/SubLink.StreamElements/SubLink.StreamElements.csproj /p:Version=$version /p:SkipInvalidConfigurations=true
. dotnet.exe publish -c Release -r win-x64 $currentDir/SubLink.Fansly/SubLink.Fansly.csproj /p:Version=$version /p:SkipInvalidConfigurations=true
. dotnet.exe publish -c Release -r win-x64 $currentDir/SubLink.OBS/SubLink.OBS.csproj /p:Version=$version /p:SkipInvalidConfigurations=true
. dotnet.exe publish -c Release -r win-x64 $currentDir/SubLink.OpenShock/SubLink.OpenShock.csproj /p:Version=$version /p:SkipInvalidConfigurations=true
. dotnet.exe publish -c Release -r win-x64 $currentDir/SubLink.Discord/SubLink.Discord.csproj /p:Version=$version /p:SkipInvalidConfigurations=true

New-Item build-$version -ItemType directory;
Copy-Item -Path "SubLink\bin\Release\net8.0\win-x64\publish\SubLink.exe" -Destination build-$version;
Copy-Item -Path "SubLink\SubLink.cs" -Destination "build-$($version)";
Copy-Item -Path "SubLink\Platforms\" -Destination "build-$($version)" -Recurse;
Copy-Item -Path "SubLink\Settings\" -Destination "build-$($version)" -Recurse;
Copy-Item -Path "SubLink.Twitch\bin\Release\net8.0\win-x64\publish\SubLink.Twitch.dll" -Destination "build-$($version)\Platforms";
Copy-Item -Path "SubLink.Kick\bin\Release\net8.0\win-x64\publish\SubLink.Kick.dll" -Destination "build-$($version)\Platforms";
Copy-Item -Path "SubLink.Streampad\bin\Release\net8.0\win-x64\publish\SubLink.Streampad.dll" -Destination "build-$($version)\Platforms";
Copy-Item -Path "SubLink.StreamElements\bin\Release\net8.0\win-x64\publish\SubLink.StreamElements.dll" -Destination "build-$($version)\Platforms";
Copy-Item -Path "SubLink.Fansly\bin\Release\net8.0\win-x64\publish\SubLink.Fansly.dll" -Destination "build-$($version)\Platforms";
Copy-Item -Path "SubLink.OBS\bin\Release\net8.0\win-x64\publish\SubLink.OBS.dll" -Destination "build-$($version)\Platforms";
Copy-Item -Path "SubLink.OpenShock\bin\Release\net8.0\win-x64\publish\SubLink.OpenShock.dll" -Destination "build-$($version)\Platforms";
Copy-Item -Path "SubLink.Discord\bin\Release\net8.0\win-x64\publish\SubLink.Discord.dll" -Destination "build-$($version)\Platforms";

if (-not (Test-Path -Path "builds")) {
    New-Item -Path "builds" -ItemType Directory;
}

if (Test-Path builds\SubLink-$version.zip) {
    Remove-Item builds\SubLink-$version.zip;
}

Compress-Archive -Path "build-$version\*" -destinationpath "builds\SubLink-$version.zip" -compressionlevel optimal;
Remove-Item build-$version -Recurse;
