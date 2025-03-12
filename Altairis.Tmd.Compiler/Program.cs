Console.WriteLine($"Altairis TMD (Tutorial Markdown) Compiler version {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine("Copyrigth (c) Michal A. Valasek - Altairis, 2019-2025");
Console.WriteLine();
NConsoler.Consolery.Run(typeof(Altairis.Tmd.Compiler.Commands), args);