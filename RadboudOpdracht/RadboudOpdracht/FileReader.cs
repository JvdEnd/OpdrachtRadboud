using System;
using System.Collections.Generic;
using System.IO;

namespace RadboudOpdracht
{
    public class FileReader
    {
        public List<string> Readfile(string fileLocation)
        {
            List<string> allSequences = new List<string>();
            try
            {
                using (var reader = new StreamReader(fileLocation))
                {
                    if (reader.ReadLine() == null)
                    {
                        Console.WriteLine("File is empty");
                        return null;
                    }
                    ProcessFile(allSequences, reader);
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File could not be found, please try again");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error has occured", e);
                Console.WriteLine("Press any key to quit");
                Console.ReadKey();
                Environment.Exit(0);
            }
            return allSequences;
        }

        private static void ProcessFile(List<string> allSequences, StreamReader reader)
        {
            var sequence = string.Empty;
            while (true)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    allSequences.Add(sequence);
                    break;
                }
                if (!line.StartsWith(">"))
                {
                    sequence += line;
                }
                else
                {
                    allSequences.Add(sequence);
                }
            }
        }
    }
}

