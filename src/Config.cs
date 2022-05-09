using YamlDotNet.Serialization;

public class Config {
    public static Config config = new Config();

    public string homeBaseFolder = "/var/lib/rassh/home";
    public string accessScriptFolder = "/var/lib/rassh/scripts";
    public string publicAccessName = System.Net.Dns.GetHostName();
    public string scriptBaseURL = "https://" + System.Net.Dns.GetHostName() + "/rassh";
    public int passwordLength = 8;
    public int defaultTTL = 24; // in hours
    public int defaultSSHPort = 22;
    public List<string> defaultPortRedirects = new List<string>(){"22:localhost:50022","443:localhost:50443","80:localhost:50080"};
    public List<string> allowedCommands = new List<string>(){"/bin/hostname","/bin/figlet","/bin/clear"};

    public static void load(){
        Config newConf = new Config();

        // TODO: do better :)

        string filePath = "/etc/rassh/config.yml";
        if(System.IO.File.Exists(filePath)){
            var deserializer = new DeserializerBuilder().Build();
            Config etcConf = deserializer.Deserialize<Config>(System.IO.File.ReadAllText(filePath));
            newConf.appendData(etcConf);
        }

        filePath = "rassh.yml";
        if(System.IO.File.Exists(filePath)){
            var deserializer = new DeserializerBuilder().Build();
            Config pwdConf = deserializer.Deserialize<Config>(System.IO.File.ReadAllText(filePath));
            newConf.appendData(pwdConf);
        }

        // TODO: add ENV vars
        config = newConf;
    }

    public void appendData(Config newConf){
        if(!String.IsNullOrWhiteSpace(newConf.homeBaseFolder)){
            this.homeBaseFolder = newConf.homeBaseFolder;
        }
        if(!String.IsNullOrWhiteSpace(newConf.accessScriptFolder)){
            this.accessScriptFolder = newConf.accessScriptFolder;
        }
        if(!String.IsNullOrWhiteSpace(newConf.publicAccessName)){
            this.publicAccessName = newConf.publicAccessName;
        }
        if(!String.IsNullOrWhiteSpace(newConf.scriptBaseURL)){
            this.scriptBaseURL = newConf.scriptBaseURL;
        }
        if(newConf.passwordLength > 0){
            this.passwordLength = newConf.passwordLength;
        }
        if(newConf.defaultTTL > 0){
            this.defaultTTL = newConf.defaultTTL;
        }
        if(newConf.defaultSSHPort > 0){
            this.defaultSSHPort = newConf.defaultSSHPort;
        }
        if(newConf.allowedCommands != null){
            this.allowedCommands.AddRange(newConf.allowedCommands);
        }
        if(newConf.defaultPortRedirects != null){
            this.defaultPortRedirects = newConf.defaultPortRedirects;
        }
    }
}