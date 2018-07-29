using Xunit;
using RadboudOpdracht;
using System.Collections.Generic;

namespace RadboudOpdrachtTests
{
    public class OrfCalculatorTests
    {
        OpenReadingFrameCalculator _calculator;
        readonly string _seq;
        readonly string _orf;
        readonly string _hiddenOrf;

        public OrfCalculatorTests()
        {
            _calculator = new OpenReadingFrameCalculator();
            _seq = "ATGC";
            _orf = "ATGCCGGGCTACTAG";
            _hiddenOrf = "CCCATGCCGGGCTACTAGGGG";
        }
   
        [Fact]
        public void ReverseTest()
        {
            var reverse = _calculator.Reverse(_seq);
            Assert.Equal("CGTA", reverse);
        }

        [Fact]
        public void ComplementTest()
        {
            var comp = _calculator.Complement(_seq);
            Assert.Equal("TACG", comp);
        }

        [Fact]
        public void ReverseComplementTest()
        {
            var reverse = _calculator.ReverseCompSeq(_seq);
            Assert.Equal("GCAT", reverse);
        }

        [Fact]
        public void FindLongestOrfTest()
        {
            var longorf = new OpenReadingFrame { Length = 14 };
            var orfs = new List<OpenReadingFrame>{
               new OpenReadingFrame{Length = 3},
               new OpenReadingFrame{Length = 7},
               new OpenReadingFrame{Length = 8},
               new OpenReadingFrame{Length = 10}
            };
            orfs.Add(longorf);

            var longest = _calculator.FindLongestOrf(orfs);
            Assert.Equal(longorf, longest);
        }

        [Fact]
        public void SelectOrfTest()
        {
            var selectedOrf = _calculator.SelectOrf(_orf,0,15);
            Assert.Equal(15, selectedOrf.Length);
            Assert.Equal("ATGCCGGGCTACTAG", selectedOrf.Sequence);
        }

        [Fact]
        public void FindOrfTest()
        {
            var foundOrf = _calculator.FindOrf(_hiddenOrf, 0);
            Assert.Equal(15, foundOrf[0].Length);
            Assert.Equal("ATGCCGGGCTACTAG", foundOrf[0].Sequence);
        }

        [Fact]
        public void SearchOrfTest()
        {
            var sequences = new List<string> { _orf, _hiddenOrf };
            var allOrfs = _calculator.SearchOrf(sequences);
            Assert.Equal(2, allOrfs.Count);
        }
    }
}
