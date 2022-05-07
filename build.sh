#!/bin/bash
SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

version="$(cat $SCRIPT_DIR/VERSION)"
releaseDir=$SCRIPT_DIR/releases/v$version

echo "###### Updating Versions to $version"
sed -i "s;<Version>.*</Version>;<Version>$version</Version>;" $SCRIPT_DIR/src/rassh.csproj
sed -i "s;Version: .*;Version: $version;" $SCRIPT_DIR/SPECS/spak-rassh.spec
fullVersion="$version-$(cat SPECS/spak-rassh.spec | grep "Release" | awk '{print $2}')"

echo "###### Building Changelog"
cat $SCRIPT_DIR/SPECS/spak-rassh.spec  | grep -Pzo "%changelog(.*\n)*" | tail -n+2 > $SCRIPT_DIR/CHANGELOG

echo "###### Preparing release folder"
sed -i "s;!releases/.*;!releases/v$version/*;" .gitignore
rm -rf $releaseDir
mkdir -p $releaseDir

echo "###### Publishing dotnet binaries"
cd $SCRIPT_DIR/src
dotnet publish -c Release -o publish -p:PublishReadyToRun=true -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true
cp $SCRIPT_DIR/src/publish/rassh $releaseDir

echo "###### Building .rpm"
mkdir -p $HOME/rpmbuild/{BUILD,BUILDROOT,RPMS,SOURCES,SPECS,SRPMS}
cd $SCRIPT_DIR
rpmbuild --target x86_64 -bb SPECS/spak-rassh.spec
mv $HOME/rpmbuild/RPMS/x86_64/spak-rassh-$fullVersion.x86_64.rpm $releaseDir/
rm -rf $HOME/rpmbuild/{BUILD,BUILDROOT}