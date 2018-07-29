using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace RadboudOpdracht
{
    public class OpenReadingFrameCalculator
    {
        private List<string> _startCodon = new List<string> { "ATG", "TTG", "GTG", "CTG" };
        private List<string> _stopCodon = new List<string> { "TAA", "TAG", "TGA" };

        /// <summary>
        /// Searches for ORFS in all sequences
        /// </summary>
        /// <returns>Returns ORF with longest orf per sequence</returns>
        public List<OpenReadingFrame> SearchOrf(List<string> sequences)
        {
            var longestOrfs = new List<OpenReadingFrame>();
            foreach (var sequence in sequences)
            {
                var allOrfsInSeq = new List<OpenReadingFrame>();
                for (var i = 0; i <= 2; i++)
                {
                    var rev = ReverseCompSeq(sequence);
                    var orf = FindOrf(sequence, i);
                    var revorf = FindOrf(rev, i);
                    allOrfsInSeq.AddRange(orf);
                    allOrfsInSeq.AddRange(revorf);
                }
                if(allOrfsInSeq.Count() != 0)
                {
                    longestOrfs.Add(FindLongestOrf(allOrfsInSeq));
                }
            }
            return longestOrfs;
        }

        /// <summary>
        /// Tries to find ORF
        /// </summary>
        /// <returns>Returns all found ORFS</returns>
        public List<OpenReadingFrame> FindOrf(string sequence, int start)
        {
            var orfs = new List<OpenReadingFrame>();
            var codon = string.Empty;
            var currentStart = 0;
            var inOrf = false;
            for (var i = start; i <= sequence.Length; i++)
            {
                if (codon.Length == 3)
                {
                    if (_startCodon.Contains(codon) && inOrf == false)
                    {
                        currentStart = i - 3;
                        inOrf = true;
                    }
                    if (_stopCodon.Contains(codon) && inOrf == true)
                    {
                        inOrf = false;
                        orfs.Add(SelectOrf(sequence, currentStart, i));
                    }
                    codon = string.Empty;
                }
                if(i != sequence.Length)
                {
                    codon += sequence[i];
                }
                
            }
            return orfs;
        }

        /// <summary>
        /// Calculates ORF
        /// </summary>
        /// <returns>orf</returns>
        public OpenReadingFrame SelectOrf(string sequence, int start, int end)
        {
            var seq = sequence.Substring(start, end-start);
            var orf = new OpenReadingFrame
            {
                Sequence = seq,
                Length = seq.Length
            };
            return orf;
        }

        /// <summary>
        /// Finds longest ORF
        /// </summary>
        /// <returns>Longest ORF</returns>
        public OpenReadingFrame FindLongestOrf(List<OpenReadingFrame> orf)
        {
            return orf.OrderByDescending(x => x.Length).First();
        }

        /// <summary>
        /// calculates reverse complement of sequence
        /// </summary>
        /// <returns>reverse complement</returns>
        public string ReverseCompSeq(string sequence)
        {
            var reverse = Reverse(sequence);
            return Complement(reverse);
        }

        /// <summary>
        /// calculates complement of sequence
        /// </summary>
        /// <returns>complement of sequence</returns>
        public string Complement(string reverse)
        {
            string comp = string.Empty;
            foreach (var s in reverse)
            {
                switch (s)
                {
                    case 'G':
                        comp += "C";
                        break;
                    case 'C':
                        comp += "G";
                        break;
                    case 'T':
                        comp += "A";
                        break;
                    case 'A':
                        comp += "T";
                        break;
                    case 'N':
                        comp += "N";
                        break;
                    default:
                        throw new Exception("Nucleotide not recognised");
                }
            }
            return comp;
        }

        /// <summary>
        /// Reverses sequence
        /// </summary>
        /// <returns>reverse sequence</returns>
        public string Reverse(string s)
        {
            var charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
