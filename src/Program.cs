using System.CommandLine;

// Load Config
Config.load();

/***************
* Setup Arguments
***************/

// Base
var rootCommand = new RootCommand();


// Options
var opt_verbose = new Option<bool>(name:"--verbose", description: "VERBOSITY ENSUES"){IsHidden = true};
opt_verbose.AddAlias("-v");
var opt_dryrun = new Option<bool>(name:"--dryrun", description: "Please don't destroy my system");
var opt_port = new Option<int>(name:"--port", description: "Specify SSH port to use",getDefaultValue: () => 22);
var opt_redirections = new Option<List<string>>(name:"--redirect", getDefaultValue: () => new List<string>(){"22:localhost:50022"} , description: "Specify a local port to redirect (Format local:ip:gate i.E. 22:localhost:50022) can be use multiple times");
var opt_username = new Option<string>(name:"--username", description: "Specify a username"){IsRequired = true};
opt_username.AddAlias("-u");
var opt_ttl = new Option<int>(name:"--ttl", description: "Specify the time-to-live for this rassh session in hours",getDefaultValue: () => 24);


rootCommand.AddGlobalOption(opt_verbose);
rootCommand.AddGlobalOption(opt_dryrun);

/***************
*   ActionSet
***************/

// Create Command
var cmd_create = new Command(name: "create", description: "Creates a new rassh session");
cmd_create.Add(opt_port);
cmd_create.Add(opt_ttl);
cmd_create.Add(opt_redirections);
cmd_create.SetHandler(
    (int val_port, List<string> val_redirections, int val_ttl, bool val_verbose, bool val_dryrun) =>
        ActionSet.create(val_port, val_redirections, val_ttl, val_verbose, val_dryrun)
,opt_port, opt_redirections, opt_ttl, opt_verbose, opt_dryrun);
rootCommand.Add(cmd_create);

// Delete Command
var cmd_delete = new Command(name: "delete", description: "Delete a rassh session");
cmd_delete.Add(opt_username);
cmd_delete.SetHandler(
    (string val_username, bool val_dryrun, bool val_verbose) =>
        ActionSet.delete(val_username, val_dryrun, val_verbose)
,opt_username, opt_dryrun, opt_verbose);
rootCommand.Add(cmd_delete);

// List Command
var cmd_list = new Command(name: "list", description: "Lists all configured rassh sessions");
cmd_list.SetHandler(
    (bool val_dryrun, bool val_verbose) =>
        ActionSet.list(val_dryrun, val_verbose)
, opt_dryrun, opt_verbose);
rootCommand.Add(cmd_list);

// Config Command
var cmd_config = new Command(name: "config", description: "Show the currently used config");
cmd_config.SetHandler(
    () =>
        ActionSet.config()
);
rootCommand.Add(cmd_config);

// Cleanup Command
var cmd_cleanup = new Command(name: "cleanup", description: "Cleans up all expired sessions");
cmd_cleanup.SetHandler(
    (bool val_dryrun, bool val_verbose) =>
        ActionSet.cleanup(val_dryrun, val_verbose)
, opt_dryrun, opt_verbose);
rootCommand.Add(cmd_cleanup);



// RUN
await rootCommand.InvokeAsync(args);
























/*


// Global Parameters
var opt_version = new Option<bool>(name:"--version", description: "Show installed version");
opt_version.AddAlias("-V");
//rootCommand.AddGlobalOption(opt_version);


var rootCommand = new RootCommand();
var sub1Command = new Command("-v", "Show installed version");
var sub1aCommand = new Command("sub1a", "Second level subcommand");

var messageOption = new Option<string>
    (name:"--message", description: "An option whose argument is parsed as a string.",getDefaultValue: () => "default");
messageOption.AddAlias("-m");

rootCommand.AddGlobalOption(messageOption);

rootCommand.Add(sub1Command);
rootCommand.Add(sub1aCommand);

rootCommand.SetHandler((string messageOptionValue) =>{
    Console.WriteLine(messageOptionValue);
},messageOption);
sub1Command.SetHandler(() =>{
    Console.WriteLine("Version " + Helper.getVersion());
});
sub1aCommand.SetHandler((string messageOptionValue) =>{
    Console.WriteLine("sub1a " +messageOptionValue);
},messageOption);

await rootCommand.InvokeAsync(args);
*/