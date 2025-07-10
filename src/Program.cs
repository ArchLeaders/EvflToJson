using BfevLibrary;
using System.Text;

if (args.Length <= 0) {
    Log("Drag and drop any BFEV or JSON file(s) to convert: ", ConsoleColor.Cyan, inline: true);
    string? input = Console.ReadLine();

    while (string.IsNullOrEmpty(input) || ParseArgs(input).Any(x => !File.Exists(x) && !x.StartsWith('-'))) {
        Log("Invalid input(s). Please specify valid files.\n", ConsoleColor.DarkRed);
        Log("Drag and drop any BFEV or JSON file(s) to convert: ", ConsoleColor.Cyan, inline: true);
        input = Console.ReadLine();
        Console.WriteLine();
    }

    args = ParseArgs(input).ToArray();
}

string[] bfevFiles = args.Where(File.Exists).ToArray();

if (bfevFiles.Length <= 0) {
    Log("Could not find any valid files in the provided args", ConsoleColor.DarkRed);
}

bool compact = args.Any(x => x is "-c" or "--compact");
foreach (string file in bfevFiles) {
    try {
        if (Path.GetExtension(file) == ".json") {
            BfevFile bfev = BfevFile.FromJson(File.ReadAllText(file));
            string ext = bfev.Flowcharts.Count > 0 ? "bfevfl" : "bfevtm";
            bfev.ToBinary(Path.Combine(Path.GetDirectoryName(file) ?? "", $"{Path.GetFileNameWithoutExtension(file)}.{ext}"));
            Log($"Successfully converted '{file}' to {ext.ToUpper()}", ConsoleColor.Green);
        }
        else {
            BfevFile bfev = BfevFile.FromBinary(file);
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(file) ?? "", $"{Path.GetFileNameWithoutExtension(file)}.json"), bfev.ToJson(format: !compact));
            Log($"Successfully converted '{file}' to JSON (--compact {compact})", ConsoleColor.Green);
        }
    }
    catch (Exception ex) {
        Log($"Failed to convert '{file}'\n{ex}", ConsoleColor.DarkRed);
    }
}

return;

static IEnumerable<string> ParseArgs(string args)
{
    List<string> argList = [];

    StringBuilder arg = new();
    bool inQt = false;

    foreach (char @char in args) {
        if (@char == '\"') {
            inQt = !inQt;
        }
        else if (!inQt && @char == ' ') {
            argList.Add(arg.ToString());
            arg.Clear();
        }
        else {
            arg.Append(@char);
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