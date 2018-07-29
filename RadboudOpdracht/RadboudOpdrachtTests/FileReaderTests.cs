using Xunit;
using RadboudOpdracht;

namespace RadboudOpdrachtTests
{
    public class FileReaderTests
    {
        FileReader _fileReader;

        public FileReaderTests()
        {
            _fileReader = new FileReader();
        }

        //Given: An empty fasta file
        //When: it is read by the program
        //Then: the reader returns nothing        
        [Fact]
        public void EmptyFileThrowsError()
        {
            var reader = _fileReader.Readfile(@"D:\Documents\OpdrachtRadboud\RadboudOpdracht\RadboudOpdrachtTests\VeryEmpty.fasta");
            Assert.Null(reader);
        }

        //Given: A file that cannot by found by the program
        //When: it is read by the program
        //Then: the reader returns nothing
        [Fact]
        public void FileNotFoundThrowsError()
        {
            var reader = _fileReader.Readfile(@"D:\Documents\OpdrachtRadboud\RadboudOpdracht\RadboudOpdrachtTests\VeryEmptyy.fasta");
            Assert.Null(reader);
        }

        //Given: A file that can be found and contains 3 sequences
        //When: it is read by the program
        //Then: a list of 3 sequences is returned
        [Fact]
        public void ReadingFileReturnsSequences()
        {
            var sequences = _fileReader.Readfile(@"D:\Documents\OpdrachtRadboud\RadboudOpdracht\RadboudOpdrachtTests\sequences.fasta");
            Assert.NotEmpty(sequences);
            Assert.Equal(3, sequences.Count);
        }
    }
}
