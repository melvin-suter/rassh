using System.IO;

class ActionSet {
    public static void create(int val_port, List<string> val_redirections, int val_ttl, bool val_verbose, bool val_dryrun){
        int currentTimeStamp = Helper.getEpoch();
        int expirationTimeStamp = currentTimeStamp + (Config.config.defaultTTL * 3600);
        string username = "rassh" + currentTimeStamp;
        string homePath = Config.config.homeBaseFolder + "/" + username;
        string password = Helper.getRandomString(Config.config.passwordLength);

        Helper.log("Checking on rbash", Helper.LogLevel.verbose);
        if(!File.Exists("/bin/rbash")){
            Helper.log("Creating rbash", Helper.LogLevel.verbose);
            Shell.run("ln", "/bin/bash /bin/rbash");
        }

        Helper.log("Creating user " + username);
        Shell.run("useradd","--create-home --home-dir " + homePath + " --comment " + expirationTimeStamp + " --shell /bin/rbash " + username);
    
        Helper.log("Setting password");
        Shell.run("bash","-c \"echo '"+password+"' | passwd --stdin " + username + "\"");

        Helper.log("Creating .bashrc");
        Shell.run("bash","-c \"echo 'PATH=~/bin' >> " + homePath + "/.bashrc ; echo 'export PATH' >> " + homePath + "/.bashrc\"");
        Shell.run("bash","-c \"echo 'clear' >> " + homePath + "/.bashrc; echo 'figlet rassh' >> " + homePath + "/.bashrc\"");

        Helper.log("Enabling commands for user");
        Shell.run("bash","-c \"mkdir -p "+homePath+"/bin\"");
      
        foreach(string cmd in Config.config.allowedCommands){
            Shell.run("bash","-c \"ln -s "+cmd+" "+homePath+"/bin/"+Path.GetFileName(cmd)+"\"");
        }

        Helper.log("Setting owner");
        Shell.run("bash","-c \"chown "+username+":"+username+" -R " + homePath + "\"");

        Helper.log("Making .bashrc immutable");
        Shell.run("bash","-c \"chattr +i "+homePath+"/.bashrc\"");

        Helper.log("Creating access script");
        string scriptContent = "";
        scriptContent += "#!/bin/bash\n";
        scriptContent +=  "test -n \"$(command -v dnf)\" && sudo dnf install -y -q sshpass";
        scriptContent +=  "test -n \"$(command -v apt-get)\" && sudo apt-get install -y sshpass";
        scriptContent += "sshpass -p '"+password+"' ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -l"+username+" -p"+val_port.ToString();
        foreach(string red in val_redirections){
            scriptContent += " -R " + red;
        }
        scriptContent += " " +Config.config.publicAccessName;
        System.IO.File.WriteAllText(Config.config.accessScriptFolder +"/" + username +".sh",scriptContent);

        Console.WriteLine("\ncurl " + Config.config.scriptBaseURL + "/" + username + ".sh | bash");
    }



    public static void delete(string val_username, bool val_dryrun, bool val_verbose){
        string homePath = Config.config.homeBaseFolder + "/" + val_username;

        Helper.log("Removing immutable attribute");
        Shell.run("bash","-c \"chattr -i "+homePath+"/.bashrc\"");

        Helper.log("Removing user");
        Shell.run("bash","-c \"userdel --remove "+val_username+"\"");

        Helper.log("Removing user home if needed");
        Shell.run("bash","-c \"test -d "+homePath+" && rm -rf "+homePath+"\"");

        Helper.log("Removing script if needed");
        string scriptPath = Config.config.accessScriptFolder +"/" + val_username +".sh";
        Shell.run("bash","-c \"test -d "+scriptPath+" && rm -rf "+scriptPath+"\"");

        
    }

    public static void list(bool val_dryrun, bool val_verbose){
        Console.WriteLine("Username         Date");
        Console.WriteLine("====================================");
        
        foreach(string homeFolder in Directory.GetDirectories(Config.config.homeBaseFolder)){
            string username = Path.GetFileName(homeFolder);
            string expirationTimeStamp = Shell.runOutput("bash", "-c \"grep "+username+" /etc/passwd | awk -F':' '{print $5}'\"", true);
            string expirationDate = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(expirationTimeStamp)).ToString("s");

            Console.WriteLine(username + "  " + expirationDate);
        }
    }

    public static void config(){
        Console.WriteLine("Config            Value");
        Console.WriteLine("====================================");
        Console.WriteLine("Home Dir:             "+Config.config.homeBaseFolder);
        Console.WriteLine("Access Script Folder: "+Config.config.accessScriptFolder);
        Console.WriteLine("Access Script URL:    "+Config.config.scriptBaseURL);
        Console.WriteLine("Password Length:      "+Config.config.passwordLength);
        Console.WriteLine("Allowed Commands:     "+String.Join(", ",Config.config.allowedCommands));
    }

    public static void cleanup(bool val_dryrun, bool val_verbose){

        Helper.log("Running cleanup",Helper.LogLevel.verbose);

        foreach(string homeFolder in Directory.GetDirectories(Config.config.homeBaseFolder)){
            Helper.log("Working on " + homeFolder,Helper.LogLevel.verbose);
            string username = Path.GetFileName(homeFolder);
            string expirationTimeStamp = Shell.runOutput("bash", "-c \"grep "+username+" /etc/passwd | awk -F':' '{print $5}'\"");
            var expirationDate = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(expirationTimeStamp));
            Helper.log("Username is " + username,Helper.LogLevel.verbose);
            Helper.log("Expiration Date is " + expirationDate.ToString("s"),Helper.LogLevel.verbose);

            if(expirationDate < DateTimeOffset.Now){
                Console.WriteLine("Cleaning up user " + username);
                delete(username,val_dryrun,val_verbose);
            }
        }
    }

}