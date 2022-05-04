public class Helper {
    private static Random random = new Random();


    public static string getVersion(){
        return String.Join('.',System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.').Take(3));
    }

    public static int getEpoch(){
        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        return Convert.ToInt32(t.TotalSeconds);
    }

    public static void log(string message, LogLevel level = LogLevel.info){
        Console.WriteLine("... " + message);
    }

    public static string getRandomString(int length, string chars="abcdef0123456789"){
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public enum LogLevel {
        info,
        verbose
    }
}