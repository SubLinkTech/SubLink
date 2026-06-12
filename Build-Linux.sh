#!/bin/bash

VERSION="3.5.0"
CURRENT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

dotnet restore "$CURRENT_DIR"

dotnet publish -c Release -r linux-x64 "$CURRENT_DIR/SubLink.References/SubLink.References.csproj" /p:Version="$VERSION" /p:SkipInvalidConfigurations=true
dotnet publish -c Release -r linux-x64 "$CURRENT_DIR/SubLink/SubLink.csproj" /p:Version="$VERSION" /p:SkipInvalidConfigurations=true /p:PublishSingleFile=true /p:PublishReadyToRun=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true --self-contained true
dotnet publish -c Release -r linux-x64 "$CURRENT_DIR/SubLink.Twitch/SubLink.Twitch.csproj" /p:Version="$VERSION" /p:SkipInvalidConfigurations=true
dotnet publish -c Release -r linux-x64 "$CURRENT_DIR/SubLink.Kick/SubLink.Kick.csproj" /p:Version="$VERSION" /p:SkipInvalidConfigurations=true
dotnet publish -c Release -r linux-x64 "$CURRENT_DIR/SubLink.Streampad/SubLink.Streampad.csproj" /p:Version="$VERSION" /p:SkipInvalidConfigurations=true
dotnet publish -c Release -r linux-x64 "$CURRENT_DIR/SubLink.StreamElements/SubLink.StreamElements.csproj" /p:Version="$VERSION" /p:SkipInvalidConfigurations=true
dotnet publish -c Release -r linux-x64 "$CURRENT_DIR/SubLink.Fansly/SubLink.Fansly.csproj" /p:Version="$VERSION" /p:SkipInvalidConfigurations=true
dotnet publish -c Release -r linux-x64 "$CURRENT_DIR/SubLink.OBS/SubLink.OBS.csproj" /p:Version="$VERSION" /p:SkipInvalidConfigurations=true
dotnet publish -c Release -r linux-x64 "$CURRENT_DIR/SubLink.OpenShock/SubLink.OpenShock.csproj" /p:Version="$VERSION" /p:SkipInvalidConfigurations=true
dotnet publish -c Release -r linux-x64 "$CURRENT_DIR/SubLink.Discord/SubLink.Discord.csproj" /p:Version="$VERSION" /p:SkipInvalidConfigurations=true
dotnet publish -c Release -r linux-x64 "$CURRENT_DIR/SubLink.Joystick/SubLink.Joystick.csproj" /p:Version="$VERSION" /p:SkipInvalidConfigurations=true

BUILD_DIR="build-$VERSION"
mkdir -p "$BUILD_DIR"

cp "SubLink/bin/Release/net8.0/linux-x64/publish/SubLink" "$BUILD_DIR/"
cp "SubLink/SubLink.cs" "$BUILD_DIR/"
cp -r "SubLink/Platforms/" "$BUILD_DIR/"
cp -r "SubLink/Settings/" "$BUILD_DIR/"

cp "SubLink.Twitch/bin/Release/net8.0/linux-x64/publish/SubLink.Twitch.dll" "$BUILD_DIR/Platforms/"
cp "SubLink.Kick/bin/Release/net8.0/linux-x64/publish/SubLink.Kick.dll" "$BUILD_DIR/Platforms/"
cp "SubLink.Streampad/bin/Release/net8.0/linux-x64/publish/SubLink.Streampad.dll" "$BUILD_DIR/Platforms/"
cp "SubLink.StreamElements/bin/Release/net8.0/linux-x64/publish/SubLink.StreamElements.dll" "$BUILD_DIR/Platforms/"
cp "SubLink.Fansly/bin/Release/net8.0/linux-x64/publish/SubLink.Fansly.dll" "$BUILD_DIR/Platforms/"
cp "SubLink.OBS/bin/Release/net8.0/linux-x64/publish/SubLink.OBS.dll" "$BUILD_DIR/Platforms/"
cp "SubLink.OpenShock/bin/Release/net8.0/linux-x64/publish/SubLink.OpenShock.dll" "$BUILD_DIR/Platforms/"
cp "SubLink.Discord/bin/Release/net8.0/linux-x64/publish/SubLink.Discord.dll" "$BUILD_DIR/Platforms/"
cp "SubLink.Joystick/bin/Release/net8.0/linux-x64/publish/SubLink.Joystick.dll" "$BUILD_DIR/Platforms/"

mkdir -p builds

if [ -f "builds/SubLink-$VERSION-linux-x64.zip" ]; then
    rm "builds/SubLink-$VERSION-linux-x64.zip"
fi

zip -r -9 "builds/SubLink-$VERSION-linux-x64.zip" "$BUILD_DIR/"

rm -rf "$BUILD_DIR"
