using RadboudOpdracht;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace RadboudOpdrachtTests
{
    public class ProteinMatcherTests
    {
        ProteinMatcher _matcher;

        public ProteinMatcherTests()
        {
            _matcher = new ProteinMatcher();
        }

        [Fact]
        public async void FindInUniProtTest()
        {
            string corSeq = "MTGYTPDEKLRLQQLRELRRRWLKDQELSPREPVLPPQKMGPMEKFWNKFLENK" +
                             "SPWRKMVHGVYKKSIFVFTHVLVPVWIIHYYMKYHVSEKPYGIVEKKSRIFPGDTILETGEVIPPM" +
                             "KEFPDQHH";
            var seq = await _matcher.FindInUniprot("AF035840");
            Assert.Equal(corSeq, seq);
        }
    }
}
