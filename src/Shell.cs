using System.Diagnostics;

public class Shell {

    public static int run(string command, string arguments, bool ignoreDryrun =  false){
        if(Helper.val_dryrun && !ignoreDryrun){
            Helper.log(command + " " + arguments, Helper.LogLevel.dryrun);
            return 0;
        } else {
            var procInf = new ProcessStartInfo(command,arguments);
            procInf.RedirectStandardOutput = true;
            var proc = Process.Start(procInf);
            proc.WaitForExit();
            return proc.ExitCode;
        }
    }

    public static string runOutput(string command, string arguments, bool ignoreDryrun =  false){
        if(Helper.val_dryrun && !ignoreDryrun){
            Helper.log(command + " " + arguments, Helper.LogLevel.dryrun);
            return "";
        } else {
            var procInf = new ProcessStartInfo(command,arguments);
            procInf.UseShellExecute = false;
            procInf.RedirectStandardOutput = true;
            procInf.CreateNoWindow = true;
            var proc = Process.Start(procInf);
            proc.WaitForExit();
            return proc.StandardOutput.ReadToEnd();
        }
    }

    public static void checkOnFolders(){
        // Only try if in root/sudo mode
        if(Helper.getuid() == 0){
            Helper.log("Checking on home base path " + Config.config.homeBaseFolder, Helper.LogLevel.verbose);
            if(!Directory.Exists(Config.config.homeBaseFolder)){
                Helper.log("Creating home base path", Helper.LogLevel.verbose);
                Directory.CreateDirectory(Config.config.homeBaseFolder);
            }

            Helper.log("Checking on access script path " + Config.config.homeBaseFolder, Helper.LogLevel.verbose);
            if(!System.IO.Directory.Exists(Config.config.accessScriptFolder)){
                Helper.log("Creating access script path", Helper.LogLevel.verbose);
                System.IO.Directory.CreateDirectory(Config.config.accessScriptFolder);
            }
        }
    }
}