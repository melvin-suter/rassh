public class Helper {

    [System.Runtime.InteropServices.DllImport("libc")]
    public static extern uint getuid();


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
        Console.WriteLine("... " + message);
    }

    public static string getRandomString(int length, string chars="abcdef0123456789"){
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static string GetFQDN()
    {
        string domainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
        string hostName = System.Net.Dns.GetHostName();

        domainName = "." + domainName;
        if(!hostName.EndsWith(domainName))  // if hostname does not already include domain name
        {
            hostName += domainName;   // add the domain name part
        }

        return hostName;                    // return the fully qualified name
    }

    public enum LogLevel {
        info,
        verbose
    }
}