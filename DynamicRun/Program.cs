using DynamicRun.Builder;
using System;
using System.IO;
using System.Reactive.Linq;
namespace DynamicRun;

class Program
{
    static void Main()
    {
        var watcherSourcesDirectoryPath = Path.Combine(Environment.CurrentDirectory, "Sources");

        Console.WriteLine($"Running from: {Environment.CurrentDirectory}");
        Console.WriteLine($"Sources from: {watcherSourcesDirectoryPath}");
        Console.WriteLine("Modify the sources to re-compile and run it again!");

        var compiler = new Compiler();
        var runner = new Runner();

        if (!Directory.Exists(watcherSourcesDirectoryPath))
        {
            Directory.CreateDirectory(watcherSourcesDirectoryPath);
        }

        using var watcher = new ObservableFileSystemWatcher
                                    (
                                        (c) =>
                                        {
                                            c.Path = watcherSourcesDirectoryPath;
                                        }
                                    );
        var changes = watcher
                            .Changed
                            .Throttle
                                (TimeSpan.FromSeconds(.5))
                            //.Where
                            //    (c => c.FullPath.EndsWith(@"DynamicProgram.cs"))
                            .Select
                                (c => c.FullPath)
                            ;
        changes
            .Subscribe
                (
                    (filePath) =>
                    {
                        Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<");
                        Console.ForegroundColor = ConsoleColor.Green;
                        try
                        {
                            Console.WriteLine (filePath);
                            runner
                                .Execute
                                    (
                                        compiler.Compile(filePath)
                                        , new[] { "France" }
                                    );
                            
                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"{e}");
                        }
                        Console.ResetColor();
                        Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>");

                    }
                );

        watcher.Start();

        var sourcesDirectoryPath = @"..\..\..\sources";

        var files = Directory
                        .GetFiles
                            (
                                sourcesDirectoryPath
                                , "*.cs"
                            );

        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var filePath = Path.Combine(watcherSourcesDirectoryPath, fileName);
            File.Copy(file, filePath, true);
        }
        Console.WriteLine("Press any key to exit!");
        Console.ReadLine();
    }
}
