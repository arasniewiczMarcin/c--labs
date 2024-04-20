using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyApp
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Nie podano argumentu na wejscie");
                return;
            }
            DirectoryInfo directory = new DirectoryInfo(args[0]);
            displayFiles(directory, 0);
           
            DateTime oldestFileDate = directory.findOldestDate(DateTime.Today);
            Console.WriteLine();

            Console.WriteLine($"Najstarszy plik: {oldestFileDate}");

            Console.WriteLine();

            SortedDictionary<string, long> content = getFilesDictionary(directory);
            serialize(content);
            deserialize();

        }
        static SortedDictionary<string, long> getFilesDictionary(DirectoryInfo directory)
        {
            Comparator comparator = new Comparator();
            SortedDictionary<string, long> dict = new SortedDictionary<string, long>(comparator);

            foreach(var element in directory.GetFiles())
            {
                dict.Add(element.Name, element.Length);
            }
            foreach (var element in directory.GetDirectories())
            {
                dict.Add(element.Name, element.GetFiles().Length + element.GetDirectories().Length);
            }
            return dict;
        }
        static string getAttributes(this FileSystemInfo file)
        {
            string attribute = "";
            if ((file.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                attribute += "r";
            else
                attribute += "-";
            if ((file.Attributes & FileAttributes.Archive) == FileAttributes.Archive)
                attribute += "a";
            else
                attribute += "-";
            if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                attribute += "h";
            else
                attribute += "-";
            if ((file.Attributes & FileAttributes.System) == FileAttributes.System)
                attribute += "s";
            else
                attribute += "-";

            return attribute;
        }
        static void displayFiles(DirectoryInfo directory, int indentationLevel)
        {
            try
            {
                for (int i = 0; i < indentationLevel; i++)
                {
                    Console.Write("\t");
                }
                int filesInDirectoryNumber = directory.GetFiles().Length + directory.GetDirectories().Length;
                Console.WriteLine($"{directory.Name} ({filesInDirectoryNumber}) {directory.getAttributes()}");
                
                indentationLevel++;
                foreach (var file in directory.GetFiles())
                {
                    for(int i = 0; i < indentationLevel; i++)
                    {
                        Console.Write("\t");
                    }
                    Console.WriteLine($"{file.Name} {file.Length} bajtów {file.getAttributes()}");
                }
                foreach(var dir in directory.GetDirectories())
                {
                    displayFiles(dir, indentationLevel);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        static DateTime findOldestDate(this DirectoryInfo directory, DateTime date)
        {
           
            foreach (var file in directory.GetFiles())
            {
                if (file.LastWriteTime < date)
                {
                    date = file.LastWriteTime;
                }
            }

            foreach (var dir in directory.GetDirectories())
            {
                return findOldestDate(dir, date);
            }
            return date;
           

        }

        [Serializable]
        class Comparator : IComparer<string>
        {
            public int Compare(string a, string b)
            {
                return a.CompareTo(b);
            }
        }

        static void serialize(SortedDictionary<string, long> dict)
        {
            using (FileStream fs = new FileStream("collection.bin", FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, dict);
            }

        }
        static void deserialize()
        {
            using (FileStream fs = new FileStream("collection.bin", FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                SortedDictionary<string, long> dict = (SortedDictionary<string, long>)formatter.Deserialize(fs);
                foreach(KeyValuePair<string, long> element in dict)
                {
                    Console.WriteLine($"{element.Key} -> {element.Value}");
                }
            }
        }
    }
}