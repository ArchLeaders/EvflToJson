using BfevLibrary;
using System.Text;

string[] bfevFiles;

if (args.Length > 0) {
    bfevFiles = args.Where(File.Exists).ToArray();
}
else {
    Log("Drag and drop any BFEV or JSON file(s) to convert: ", ConsoleColor.Cyan, inline: true);
    string? input = Console.ReadLine();

    while (string.IsNullOrEmpty(input) || ParseArgs(input ?? "").Where(x => !File.Exists(x) && !x.StartsWith('-')).Any()) {
        Log("Invalid input(s). Please specify valid files.\n", ConsoleColor.DarkRed);
        Log("Drag and drop any BFEV or JSON file(s) to convert: ", ConsoleColor.Cyan, inline: true);
        input = Console.ReadLine();
        Console.WriteLine();
    }

    args = ParseArgs(input!).ToArray();
    bfevFiles = args.Where(File.Exists).ToArray();
}

if (bfevFiles.Length <= 0) {
    Log("Could not find any valid files in the provided args", ConsoleColor.DarkRed);
}

bool compact = args.Where(x => x == "-c" || x == "--compact").Any();
foreach (var file in bfevFiles) {
    try {
        if (Path.GetExtension(file) == ".json") {
            BfevFile bfev = BfevFile.FromJson(File.ReadAllText(file));
            string ext = bfev.Flowcharts.Count > 0 ? "bfevfl" : "bfevtm";
            bfev.ToBinary(Path.Combine(Path.GetDirectoryName(file) ?? "", $"{Path.GetFileNameWithoutExtension(file)}.{ext}"));
            Log($"Successfully converted '{file}' to {ext.ToUpper()}", ConsoleColor.Green);
        }
        else {
            BfevFile bfev = new(file);
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(file) ?? "", $"{Path.GetFileNameWithoutExtension(file)}.json"), bfev.ToJson(format: !compact));
            Log($"Successfully converted '{file}' to JSON (--compact {compact})", ConsoleColor.Green);
        }
    }
    catch (Exception ex) {
        Log($"Failed to convert '{file}'\n{ex}", ConsoleColor.DarkRed);
    }
}

Log("\nPress enter to continue. . .");
Console.ReadLine();

static IEnumerable<string> ParseArgs(string args)
{
    List<string> argList = new();

    StringBuilder arg = new();
    bool inQt = false;

    for (int i = 0; i < args.Length; i++) {
        char _char = args[i];
        if (_char == '\"') {
            inQt = !inQt;
        }
        else if (!inQt && _char == ' ') {
            argList.Add(arg.ToString());
            arg.Clear();
        }
        else {
            arg.Append(_char);
        }
    }

    argList.Add(arg.ToString());
    return argList;
}

static void Log(string message, ConsoleColor? color = null, bool inline = false)
{
    if (color != null) {
        Console.ForegroundColor = (ConsoleColor)color;
    }

    if (inline) {
        Console.Write(message);
    }
    else {
        Console.WriteLine(message);
    }
    Console.ResetColor();
}