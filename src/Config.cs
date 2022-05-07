using YamlDotNet.Serialization;

public class Config {
    public static Config config = new Config();

    public string homeBaseFolder = "/var/lib/rassh/home";
    public string accessScriptFolder = "/var/lib/rassh/scripts";
    public string publicAccessName = Helper.GetFQDN();
    public string scriptBaseURL = "https://" + Helper.GetFQDN() + "/rassh";
    public int passwordLength = 8;
    public int defaultTTL = (24); // in hours
    public List<string> allowedCommands = new List<string>(){"/bin/hostname"};

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
        if(newConf.allowedCommands != null){
            this.allowedCommands = newConf.allowedCommands;
        }
    }
}