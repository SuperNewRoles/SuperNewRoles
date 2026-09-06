using SuperNewRoles.RpcWeaver;

if (args.Length == 3 && args[0] == "--harmony")
{
    var result = HarmonyRegistryWeaver.Weave(args[1], args[2]);
    Console.WriteLine($"[HarmonyRegistry] classes={result.Classes}, prepared={result.Prepared}: {args[1]}");
    return;
}
if (args.Length is < 1 or > 2)
    throw new ArgumentException("Usage: SuperNewRoles.RpcWeaver <assembly.dll> [reference-paths.txt]");
Console.WriteLine($"[RpcWeaver] {RpcAssemblyWeaver.Weave(args[0], args.Length == 2 ? args[1] : null)} RPC methods woven: {args[0]}");
