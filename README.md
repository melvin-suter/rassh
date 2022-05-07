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