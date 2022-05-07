using System.Diagnostics;

public class Shell {

    public static int run(string command, string arguments){
        var procInf = new ProcessStartInfo(command,arguments);
        var proc = Process.Start(procInf);
        proc.WaitForExit();
        return proc.ExitCode;
    }

    public static string runOutput(string command, string arguments){
        var procInf = new ProcessStartInfo(command,arguments);
        procInf.UseShellExecute = false;
        procInf.RedirectStandardOutput = true;
        procInf.CreateNoWindow = true;
        var proc = Process.Start(procInf);
        proc.WaitForExit();
        return proc.StandardOutput.ReadToEnd();
    }
}