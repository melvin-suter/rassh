# rassh

The basic idea is simple. You can create a remote-access user with a command as simple as `rassh create`.
You'll receive a url that (if setup correctely) you can give to a customer. If he runs the script under said url, he will connect to a restricted user over ssh and redirect his local ports (as specified by you) and you can reverse-tunnel onto the customers machine. The user will than be deleted after a set period of time.

How to use:
```
Usage:
  rassh [command] [options]

Options:
  --dryrun        Please don't destroy my system
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  create             Creates a new rassh session
  delete <username>  Delete a rassh session
  list               Lists all configured rassh sessions
  config             Show the currently used config
  cleanup            Cleans up all expired sessions
```

## Config file

You can create a yaml file here:
- `/etc/rassh/config.yml`
- `./rassh.yml`

It can have these options:

```yaml
# Where the users home will point to
homeBaseFolder: /var/lib/rassh/home
# Where the remote access scripts available through the link will be placed
accessScriptFolder: /var/lib/rassh/scripts
# Under which hostname your machine is accassible to the "public" (the remote machine)
publicAccessName: HOSTNAME
# Under which URL your machine/the scripts are accassible to the "public" (the remote machine)
scriptBaseURL: https://HOSTNAME/rassh
# The length of the generated passwords for the remote access users
passwordLength: 8
# Default Time-To-Live (in hours) for a remote user
defaultTTL: 24
# Default ssh port to use if not specified
defaultSSHPort: 22
# Which port-redirections should be used by default
defaultPortRedirects:
- "22:localhost:50022"
- "443:localhost:50443"
- "80:localhost:50080"
# Which commands ADDITIONALY to the default commands should the remote access user be allowed to run
allowedCommands:
- /bin/hostname
- /bin/figlet
- /bin/clear
```

## Nginx Config

Here is a very basic example config

```
server {
    listen          443 ssl default_server;
    server_name     _;
    location /rassh {
        root            /var/lib/rassh/scripts;
        autoindex on;
    }

}

```

## Examples

```bash
[root@rasshserver rassh]# rassh create
... Creating user rassh1651958500
... Setting password
... Creating .bashrc
... Enabling commands for user
... Setting owner
... Making .bashrc immutable
... Creating access script

curl https://rasshserver.example.com/rassh/rassh1651958500.sh | bash

[root@rasshserver rassh]# rassh list
Username         Date
====================================
rassh1651958500  2022-05-08T21:21:40

[root@rasshserver rassh]# cat /var/lib/rassh/scripts/rassh1651958500.sh
#!/bin/bash
test -n "$(command -v dnf)" && sudo dnf install -y -q sshpass
test -n "$(command -v apt-get)" && sudo apt-get install -y sshpass
sshpass -p '2558804f' ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -lrassh1651958500 -p22 -R 22:localhost:50022@rasshserver.example.com

```