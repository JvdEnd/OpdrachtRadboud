using System;
using System.Collections.Generic;

namespace RadboudOpdracht
{
    //OpenReadingFrameCalculator
    //The program should return the longest open reading frame for each sequence, the length of this reading frame, and the best match to a human protein sequence in Uniprot.
    public class Program
    {
        public static void Main()
        {
            var orfCalc = new OpenReadingFrameCalculator();
            var protMatcher = new ProteinMatcher();
            var sequences = FileInput();
            var orfs = orfCalc.SearchOrf(sequences);
            var allOrfs = protMatcher.SelectBestHumanMatches(orfs);

            foreach(var orf in allOrfs)
            {
                Console.WriteLine(orf);
            }
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static List<string> FileInput()
        {
            //Program expects the file to end with .fasta 
            Console.WriteLine("Please provide a file or press q to exit");
            var fileLocation = Console.ReadLine();
            if(fileLocation.ToLower() == "q")
            {
                Environment.Exit(0);
            }
            if (!fileLocation.ToLower().Contains(".fasta"))
            {
                Console.WriteLine("File is not in the correct format, please give a file in .fasta format");
                FileInput();
            }
            var sequences = new FileReader().Readfile(fileLocation);
            if (sequences == null)
            {
                FileInput();
            }
            return sequences;
        }
    }
}
