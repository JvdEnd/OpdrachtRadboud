using RadboudOpdracht;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Xunit;

namespace RadboudOpdrachtTests
{
    public class BlastTests
    {
        BlastRequest _blast;
        string _orf = "gtaattagatccgccaatttcacagacaatactaaaatcataatagtacagctgaatgaatctgtacaaattaattgtacaagacccaacaacaatacaagaaaaagtataaatatagga";
        string _human = "ATGGGAAAAATCAGCAGTCTTCCAACCCAATTATTTAAGTGCTGCTTTTGTGATTTCTTGAAGGTGA" +
            "AGATGCACACCATGTCCTCCTCGCATCTCTTCTACCTGGCGCTGTGCCTGCTCACCTTCACCAGCTCTGCCACGGCT" +
            "GGACCGGAGACGCTCTGCGGGGCTGAGCTGGTGGATGCTCTTCAGTTCGTGTGTGGAGACAGGGGCTTTTATTTCAA" +
            "CAAGCCCACAGGGTATGGCTCCAGCAGTCGGAGGGCGCCTCAGACAGGCATCGTGGATGAGTGCTGCTTCCGGAGCT" +
            "GTGATCTAAGGAGGCTGGAGATGTATTGCGCACCCCTCAAGCCTGCCAAGTCAGCTCGCTCTGTCCGTGCCCAGCGC" +
            "CACACCGACATGCCCAAGACCCAGAAGGAAGTACATTTGAAGAACGCAAGTAGAGGGAGTGCAGGAAACAAGAACTA" +
            "CAGGATGTAG";
        public BlastTests() {
            _blast = new BlastRequest();

        }

        [Fact]
        public async void BlastTest()
        {
            var xml = await _blast.ExecuteBlast(new CancellationToken(), _orf);
            var blastHit = _blast.ParseBlastXml(xml);
            Assert.Equal("Z29296", blastHit.Accession);
        }


        //Not working because of not selecting the right blast result
        [Fact]
        public async void HumanBlastTest()
        {
            var xml = await _blast.ExecuteBlast(new CancellationToken(), _human);
            var blastHit = _blast.ParseBlastXml(xml);
            Assert.Equal("NM_000618", blastHit.Accession);
        }
    }
}
