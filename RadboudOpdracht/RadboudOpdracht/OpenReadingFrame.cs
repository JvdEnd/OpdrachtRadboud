using System;
using System.Collections.Generic;
using System.Text;

namespace RadboudOpdracht
{
    public class OpenReadingFrame
    {
        public string Sequence { get; set; }
        public int Length { get; set; }
        public string Protein { get; set; }

        public override string ToString()
        {
            return $"Sequence: {Sequence} {Environment.NewLine} Length: {Length} {Environment.NewLine} Protein: {Protein}";
        }
    }
}
