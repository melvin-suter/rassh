using System.IO;

class ActionSet {
    public static void create(int val_port, List<string> val_redirections, int val_ttl, bool val_verbose, bool val_dryrun){
        Helper.checkRoot();

        int currentTimeStamp = Helper.getEpoch();
        int expirationTimeStamp = currentTimeStamp + (Config.config.defaultTTL * 3600);
        string username = "rassh" + currentTimeStamp;
        string homePath = Config.config.homeBaseFolder + "/" + username;
        string password = Helper.getRandomString(Config.config.passwordLength);

        Helper.log("Checking on home base path " + Config.config.homeBaseFolder, Helper.LogLevel.verbose);
        if(!Directory.Exists(Config.config.homeBaseFolder)){
            Helper.log("Creating home base path", Helper.LogLevel.verbose);
            Directory.CreateDirectory(Config.config.homeBaseFolder);
        }

        Helper.log("Checking on rbash", Helper.LogLevel.verbose);
        if(!File.Exists("/bin/rbash")){
            Helper.log("Creating rbash", Helper.LogLevel.verbose);
            Shell.run("ln", "/bin/bash /bin/rbash");
        }

        Helper.log("Creating user " + username);
        if(!val_dryrun){
            Shell.run("useradd","--create-home --home-dir " + homePath + " --comment " + expirationTimeStamp + " --shell /bin/rbash " + username);
        } else {
            Helper.log("DRYRUN: useradd --create-home --home-dir " + homePath + " --comment " + expirationTimeStamp + " --shell /bin/rbash " + username);
        }

        Helper.log("Setting password");
        if(!val_dryrun){
            Shell.run("bash","-c \"echo '"+password+"' | passwd --stdin " + username + "\"");
        } else {
            Helper.log("DRYRUN: bash -c \"echo '"+password+"' | passwd --stdin " + username + "\"");
        }

        Helper.log("Creating .bashrc");
        if(!val_dryrun){
            Shell.run("bash","-c \"echo 'PATH=~/bin' >> " + homePath + "/.bashrc ; echo 'export PATH' >> " + homePath + "/.bashrc ; chattr +i "+homePath+"/.bashrc\"");
        } else {
            Helper.log("DRYRUN: bash -c \"echo 'PATH=~/bin' >> " + homePath + "/.bashrc ; echo 'export PATH' >> " + homePath + "/.bashrc ; chattr +i "+homePath+"/.bashrc\"");
        }

        Helper.log("Enabling commands for user");
        if(!val_dryrun){
            Shell.run("bash","-c \"mkdir -p "+homePath+"/bin\"");
        } else {
            Helper.log("DRYRUN: bash -c \"mkdir -p "+homePath+"/bin\"");
        }
        foreach(string cmd in Config.config.allowedCommands){
            if(!val_dryrun){
                Shell.run("bash","-c \"ln -s "+cmd+" "+homePath+"/bin/"+Path.GetFileName(cmd)+"\"");
            } else {
                Helper.log("DRYRUN: bash -c \"ln -s "+cmd+" "+homePath+"/bin/"+Path.GetFileName(cmd)+"\"");
            }
        }

        Helper.log("Setting owner");
        if(!val_dryrun){
            Shell.run("bash","-c \"chown "+username+":"+username+" -R " + homePath + "\"");
        } else {
            Helper.log("DRYRUN: bash -c \"chown "+username+":"+username+" -R " + homePath + "\"");
        }

        Helper.log("Creating access script");
        if(!System.IO.Directory.Exists(Config.config.accessScriptFolder)){
            Helper.log("Creating access script path", Helper.LogLevel.verbose);
            System.IO.Directory.CreateDirectory(Config.config.accessScriptFolder);
        }
        string scriptContent = "";
        scriptContent += "#!/bin/bash\n";
        scriptContent += "ssh -l"+username+" -p"+val_port.ToString();
        foreach(string red in val_redirections){
            scriptContent += " -R " + red;
        }
        scriptContent += " " +Config.config.publicAccessName;
        System.IO.File.WriteAllText(Config.config.accessScriptFolder +"/" + username +".sh",scriptContent);
        Console.WriteLine("curl " + Config.config.scriptBaseURL + "/" + username + ".sh | bash");
    }



    public static void delete(string val_username, bool val_dryrun, bool val_verbose){
        Helper.checkRoot();

        string homePath = Config.config.homeBaseFolder + "/" + val_username;

        Helper.log("Removing immutable attribute");
        if(!val_dryrun){
            Shell.run("bash","-c \"chattr -i "+homePath+"/.bashrc\"");
        } else {
            Helper.log("DRYRUN: bash -c \"chattr -i "+homePath+"/.bashrc\"");
        }

        Helper.log("Removing user");
        if(!val_dryrun){
            Shell.run("bash","-c \"userdel --remove "+val_username+"\"");
        } else {
            Helper.log("DRYRUN: bash -c \"userdel --remove "+val_username+"\"");
        }

        Helper.log("Removing user home if needed");
        if(!val_dryrun){
            Shell.run("bash","-c \"test -d "+homePath+" && rm -rf "+homePath+"\"");
        } else {
            Helper.log("DRYRUN: bash -c \"test -d "+homePath+" && rm -rf "+homePath+"\"");
        }
    }

    public static void list(bool val_dryrun, bool val_verbose){
        Console.WriteLine("Username         Date");
        Console.WriteLine("====================================");
        
        foreach(string homeFolder in Directory.GetDirectories(Config.config.homeBaseFolder)){
            string username = Path.GetFileName(homeFolder);
            string expirationTimeStamp = Shell.runOutput("bash", "-c \"grep "+username+" /etc/passwd | awk -F':' '{print $5}'\"");
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
        Helper.checkRoot();
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