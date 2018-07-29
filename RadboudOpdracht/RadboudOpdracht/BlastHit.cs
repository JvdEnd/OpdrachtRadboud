using System;
using System.Collections.Generic;
using System.Text;

namespace RadboudOpdracht
{
    public class BlastHit
    {
        public string Def { get; set; }
        public string Accession { get; set; }
        public int Identity { get; set; }
        public int AlignmentLenght { get; set; }
        public int IdentityCalculated { get; set; }

        internal void CalculateIdentity()
        {
            IdentityCalculated = (Identity / AlignmentLenght) * 100;
        }
    }
}
