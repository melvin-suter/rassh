public class Config {

    public static string homeBaseFolder = "/var/lib/rassh";
    public static int passwordLength = 8;
    public static List<string> allowedCommands = new List<string>(){"/bin/hostname"};
}