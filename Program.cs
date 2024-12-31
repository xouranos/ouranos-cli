using System.Buffers.Text;
using System.Text;


try
{
    // Preprocess the command line arguments
    var argList = new List<string>(args);

    var optionList = new List<string>();
    while ((argList.Any()) && (argList[0].StartsWith('-')))
    {
        optionList.Add(argList[0]);
        argList.RemoveAt(0);
    }

    string command = string.Empty;
    if (argList.Any())
    {
        command = argList.First();
        argList.RemoveAt(0);
    }

    var commandArgList = new List<string>(argList);

    // Display help if required.
    if (optionList.Contains("-help") || optionList.Contains("--help") || string.IsNullOrWhiteSpace(command))
    {
        var builder = new StringBuilder();
        builder.AppendLine("Usage:");
        builder.AppendLine(" dotnet run <Marscoin..Cli/Marscoin.Cli.dll> [options] <command> [arguments]");
        builder.AppendLine();
        builder.AppendLine("Command line arguments:");
        builder.AppendLine();
        builder.AppendLine("[options]                          Options for the CLI (optional) - e.g. -help, -rpcuser, see below.");
        builder.AppendLine("[command]                          Name of RPC method.");
        builder.AppendLine("[arguments]                        Argument by position (RPC) or Name = Value pairs (API) (optional).");
        builder.AppendLine();
        builder.AppendLine("Options:");
        builder.AppendLine("-help                              This help message");
        builder.AppendLine("-rpcconnect=<ip>                   Send commands to node running on <ip> (default: 127.0.0.1)");
        builder.AppendLine("-rpcport=<port>                    Connect to JSON-RPC on <port> (default for Stratis: 26174 or default for Bitcoin: 8332)");
        builder.AppendLine("-rpcuser=<user>                    Username for JSON-RPC connections");
        builder.AppendLine("-rpcpassword=<pw>                  Password for JSON-RPC connections");
        builder.AppendLine();
        builder.AppendLine("Examples:");
        builder.AppendLine();
        builder.AppendLine("dotnet run Marscoin -testnet GET Wallet/history WalletName=testwallet - Lists all the historical transactions of the wallet called 'testwallet' on the stratis test network.");
        builder.AppendLine("dotnet run Marscoin -rpcuser=admin -rpcpassword=123456 -rpcconnect=127.0.0.3 -rpcport=26174 getinfo - Displays general information about the Stratis node on the 127.0.0.3:26174, authenticating with the RPC specified user.");
        builder.AppendLine("dotnet run Marscoin -rpcuser=admin -rpcpassword=123456 getbalance - Displays the current balance of the opened wallet on the 127.0.0.1:8332 node, authenticating with the RPC specified user.");
        Console.WriteLine(builder);
        Console.ReadKey();
        return;
    }

    
    // API calls require both the contoller name and the method name separated by "/".
    // If this is not an API call then assume it is an RPC call.
    // Process RPC call.
    try
    {
        var ip = optionList.First(a => a.StartsWith("-rpcconnect=")).Replace("-rpcconnect=", "");
        var port= optionList.First(a => a.StartsWith("-rpcport=")).Replace("-rpcport=", "");
        var nodeEndPoint = ip + ":" + port;
        var RpcUser = optionList.First(a => a.StartsWith("-rpcuser=")).Replace("-rpcuser=", "");
        var RpcPassword = optionList.First(a => a.StartsWith("-rpcpassword=")).Replace("-rpcpassword=", "");
        // Find the binding to 127.0.0.1 or the first available. The logic in RPC settings ensures there will be at least 1.
        var rpcUri = new Uri($"http://{nodeEndPoint}");

        Console.WriteLine($"Connecting to the following RPC node: http://{RpcUser}:{RpcPassword}@{rpcUri.Authority}.");

        // Initialize the RPC client with the configured or passed userid, password and endpoint.
        var http = new HttpClient();
        http.BaseAddress = rpcUri;
        http.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(RpcUser + ":" + RpcPassword)));
        // Execute the RPC command
        Console.WriteLine($"Sending RPC command '{command} {string.Join(" ", commandArgList)}' to '{rpcUri}'.");
        var postString = "{\"method\":\"" + command + "\",\"params\":[\"" + string.Join("\",\"", commandArgList) + "\"]}";
        var response = await http.PostAsync(rpcUri, new StringContent(postString, Encoding.UTF8, "application/json"));
        var result = await response.Content.ReadAsStringAsync();
        // Return the result as a string to the console.
        Console.WriteLine(result);

    }
    catch (Exception err)
    {
        Console.WriteLine(err.Message);
    }

}
catch (Exception err)
{
    // Report any errors to the console.
    Console.WriteLine(err.Message);
}