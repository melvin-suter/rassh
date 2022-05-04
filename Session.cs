using System.IO;

class Session {
    public static void create(int val_port, List<string> val_redirections, int val_ttl, bool val_verbose, bool val_dryrun){
        int currentTimeStamp = Helper.getEpoch();
        int expirationTimeStamp = currentTimeStamp + (val_ttl * 24);
        string username = "rassh" + currentTimeStamp;
        string homePath = Config.homeBaseFolder + "/" + username;
        string password = Helper.getRandomString(Config.passwordLength);

        Helper.log("Checking on home base path " + Config.homeBaseFolder, Helper.LogLevel.verbose);
        if(!Directory.Exists(Config.homeBaseFolder)){
            Helper.log("Creating home base path", Helper.LogLevel.verbose);
            Directory.CreateDirectory(Config.homeBaseFolder);
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
        foreach(string cmd in Config.allowedCommands){
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



        Console.WriteLine("Username:  " + username);
        Console.WriteLine("Password:  " + password);
        Console.WriteLine("Homepath:  " + homePath);
        Console.WriteLine("Expires:   " + expirationTimeStamp.ToString());
        Console.WriteLine("Port:      " + val_port.ToString());
        Console.WriteLine("Redirects: ");
        foreach(string q in val_redirections){
            Console.WriteLine("  - " + q);
        }
    }



    public static void delete(string val_username, bool val_dryrun, bool val_verbose){
        string homePath = Config.homeBaseFolder + "/" + val_username;

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
        
        foreach(string homeFolder in Directory.GetDirectories(Config.homeBaseFolder)){
            string username = Path.GetFileName(homeFolder);
            string expirationTimeStamp = Shell.runOutput("bash", "-c \"grep rassh1651690869 /etc/passwd | awk -F':' '{print $5}'\"");
            string expirationDate = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(expirationTimeStamp)).ToString("s");

            Console.WriteLine(username + "  " + expirationDate);
        }
    }

}