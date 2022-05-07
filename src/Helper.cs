public class Helper {

    [System.Runtime.InteropServices.DllImport("libc")]
    public static extern uint getuid();
    public static bool val_verbose = false;
    public static bool val_dryrun = false;


    private static Random random = new Random();

    public static void checkRoot() {
        // Stop if not root
        if(getuid() != 0){
            Console.WriteLine("Please run as root/sudo");
            Environment.Exit(1);
        }
    }


    public static string getVersion(){
        return String.Join('.',System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.').Take(3));
    }

    public static int getEpoch(){
        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        return Convert.ToInt32(t.TotalSeconds);
    }

    public static void log(string message, LogLevel level = LogLevel.info){
        // TODO: Do something with the log level
        switch(level){
            case LogLevel.verbose:
                if(val_verbose){
                    Console.WriteLine("DEBUG ... " + message);
                }
                break;
            case LogLevel.dryrun:
                if(val_dryrun){
                    Console.WriteLine("DRYRUN ... " + message);
                }
                break;
            default:
                Console.WriteLine("... " + message);
                break;
        }
    }

    public static string getRandomString(int length, string chars="abcdef0123456789"){
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public enum LogLevel {
        info,
        verbose,
        dryrun
    }
}