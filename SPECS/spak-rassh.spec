Summary: Installs the rassh tool
Name: spak-rassh
Version: 0.1.5
Release: 1
License: GPL
URL: https://github.com/melvin-suter/rassh
Group: System
Packager: SuterDEV 
Requires: figlet
BuildRoot: $HOME/rpmbuild
Source:     %{expand:%%(pwd)}

# Build with the following syntax:
# rpmbuild --target noarch -bb utils.spec

%description
Installs the rassh tool

%prep
# Prepare dir structure and copy files into rpm tree
echo "BUILDROOT = $RPM_BUILD_ROOT"
mkdir -p $RPM_BUILD_ROOT/usr/sbin $RPM_BUILD_ROOT/etc/cron.d
cp %{SOURCEURL0}/src/publish/rassh $RPM_BUILD_ROOT/usr/sbin/rassh
cp %{SOURCEURL0}/package-data/cron.d $RPM_BUILD_ROOT/etc/cron.d/rassh
exit

%files
# Set file permissions of added stuff
%attr(0755, root, root) /usr/sbin/*
%attr(0644, root, root) /etc/cron.d/rassh

%pre
# nothing to do pre-installation

%post
# do stuff post-installation (after copying files)

%postun
# do stuff post-uninstallation (after deleting files)

%clean
rm -rf $RPM_BUILD_ROOT/etc

%changelog

* Sat May 07 2022 SuterDEV <packages@suter.dev>
  v0.1.5
  - Maybe a WIP?
  - Improvements :)
  - deletes scripts and stuff

* Sat May 07 2022 SuterDEV <packages@suter.dev>
  v0.1.4
  - Still a WIP but getting there
  - creates the scripts now
  - can cleanup after itself

* Sat May 07 2022 SuterDEV <packages@suter.dev>
  v0.1.3
  - Still a WIP
  - Can now handle config files
  - Improvements

* Sat Apr 30 2022 SuterDEV <packages@suter.dev>
  v0.1.0
  - This is a work in progress
