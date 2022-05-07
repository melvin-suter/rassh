using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

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
var opt_username = new Argument<string>(name:"username", description: "Specify a username");
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


/***************
*   Invoke
***************/

var cliBuilder = new CommandLineBuilder(rootCommand);
cliBuilder.UseDefaults();

// Add Middleware to handle stuff globaly
cliBuilder.AddMiddleware(async (context, next) => {
    // Prepare Logging function
    Helper.val_dryrun = context.ParseResult.GetValueForOption(opt_dryrun);
    Helper.val_verbose = context.ParseResult.GetValueForOption(opt_verbose);

    // Check if run as root/sudo, as long as not config/list command
    if(context.ParseResult.CommandResult.Command != cmd_list && context.ParseResult.CommandResult.Command != cmd_config){
        Helper.checkRoot();
    }
        
    // Create folders if possible
    Shell.checkOnFolders();

    next(context);
});

var parser = cliBuilder.Build();
await parser.InvokeAsync(args);
