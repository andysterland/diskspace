using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Transactions;

namespace Sizer
{
    class Program
    {
        public class Options
        {
            [Option('d',"depth",Required = false, Default = 3, HelpText = "Depth to recurse.")]
            public int Depth { get; set; }

            [Option('o', "output", Required = false, Default = @"data.json", HelpText = "File to write to output.")]
            public string OutputPath { get; set; }

            [Option('p', "path", Required = false, Default = @"c:\", HelpText = "Path to search from.")]
            public string RootPath { get; set; }
            [Option('m', "min-size", Required = false, Default = 100, HelpText = "Exclude any folders less than this size.")]
            public int MinSize { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       SizeData[] data = Calculate(o.RootPath, o.Depth);

                       List<Object[]> filteredData = new List<Object[]>();

                       Dictionary<string, int> ids = new Dictionary<string, int>();

                       Object[] header = new Object[] {
                           "Path",
                           "Parent",
                           "Size"
                       };

                       filteredData.Add(header);

                       foreach(SizeData d in data)
                       {
                           if (d.value > o.MinSize)
                           {
                               string id = d.id;
                               if (ids.ContainsKey(d.id))
                               {
                                   id += $"({ids[d.id]})";
                                   ids[d.id]++;
                               }
                               ids.Add(id, 1);

                               Object[] row = new Object[] {
                                   id,
                                   d.parent,
                                   d.value
                               };

                               filteredData.Add(row);
                           }
                       }

                       string jsonData = JsonSerializer.Serialize<Object[]>(filteredData.ToArray());

                       Console.WriteLine($"Writting output to {o.OutputPath}");
                       File.WriteAllText(o.OutputPath, jsonData);
                   });
        }

        static int MaxDepth = 0;
        static int CurrentId = 0;
        static List<SizeData> Data = new List<SizeData>();
        static SizeData[] Calculate(string rootFolder, int maxDepth)
        {
            if(!Directory.Exists(rootFolder))
            {
                Console.WriteLine($"No such folder {rootFolder}.");
            }
            MaxDepth = maxDepth;

            CalculateFolderSize(rootFolder, 0, null);

            return Data.ToArray();
        }

        static float CalculateFolderSize(string folder, int currentDepth, string? parent)
        {
            SizeData folderData = new SizeData();
            //folderData.id = new DirectoryInfo(folder).Name;
            folderData.id = folder;
            folderData.parent = parent;

            float folderSize = 0.0f;
            try
            {
                if (!Directory.Exists(folder))
                {
                    return folderSize;
                }
                else
                {
                    try
                    {
                        foreach (string file in Directory.GetFiles(folder))
                        {
                            if (File.Exists(file))
                            {
                                FileInfo finfo = new FileInfo(file);
                                folderSize += finfo.Length;
                            }
                        }

                        if (currentDepth < MaxDepth)
                        {
                            string parentInScope = folderData.id; 
                            foreach (string dir in Directory.GetDirectories(folder))
                            {
                                folderSize += CalculateFolderSize(dir, currentDepth + 1, parentInScope);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unable to calculate folder size: {0}", e.Message);
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Unable to calculate folder size: {0}", e.Message);
            }

            // Round to 1mb
            folderData.value = (int) Math.Round(folderSize / 1024 / 1024);

            Data.Add(folderData);

            return folderSize;
        }
    }
}
