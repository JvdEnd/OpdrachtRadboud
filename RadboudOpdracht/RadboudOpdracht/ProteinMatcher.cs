using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;

namespace RadboudOpdracht
{
    public class ProteinMatcher
    {
        const string UniEndPoint = @"https://www.uniprot.org/uniprot/";
        HttpClient client = new HttpClient();//?query=Z29296&sort=score
        readonly CancellationToken _cancel = new CancellationToken();
        BlastRequest _blast = new BlastRequest();

        /// <summary>
        /// Blasts and then queries Uniprot
        /// </summary>
        /// <returns>Orf with added protein or no match</returns>
        public List<OpenReadingFrame> SelectBestHumanMatches(List<OpenReadingFrame> orfs)
        {
            foreach(var orf in orfs)
            {
                string accession = _blast.BlastAsync(orf.Sequence).GetAwaiter().GetResult();
                orf.Protein = FindInUniprot(accession).GetAwaiter().GetResult();
            }
            return orfs;
        }

        /// <summary>
        /// Query Uniprot and parse response
        /// </summary>
        /// <returns>Human protein or no match</returns>
        public async Task<string> FindInUniprot(string accession)
        {
           var searchXml = await RequestUniProtInfo(_cancel, accession);
            if(searchXml == "No data returned from uniprot")
            {
                return "No Match found";
            }
            return GetProteinSequence(searchXml);
        }

        /// <summary>
        /// Sends initial request to ncbi
        /// </summary>
        /// <returns>response</returns>
        private async Task<string> RequestUniProtInfo(CancellationToken token, string sequence)
        {
            var content = CreateRequest(sequence);

            var result = await client.PostAsync(UniEndPoint, content, token);
            result.EnsureSuccessStatusCode();

            string searchXml = await result.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(searchXml))
            {
                return "No data returned from uniprot";
            }
            return searchXml;
        }

        /// <summary>
        /// Get the protein sequence from the uniprot XML
        /// </summary>
        /// <returns>The protein</returns>
        private string GetProteinSequence(string searchXml)
        {
            string protein = null;
            try
            {
                XDocument xmlDoc = XDocument.Parse(searchXml);
                var elements = xmlDoc.Root.Elements();
                var sequenceElement = elements.Select(x => x.Element("{http://uniprot.org/uniprot}sequence")).FirstOrDefault();
                var proteinFromXml = sequenceElement.Value.Trim();
                protein = Regex.Replace(proteinFromXml, @"\t|\n|\r", "");
            }
            catch (Exception e)
            {
                throw e;
            }
            return protein ?? "No human protein found";
        }

        /// <summary>
        /// Build the HTTP content for the request based on the parameters
        /// </summary>
        /// <returns>The request</returns>
        public HttpContent CreateRequest(string accession)
        {
            if (accession == null)
                throw new ArgumentException("Missing accesion number");

            var data = new List<KeyValuePair<string, string>> { CreateKVP("query", $"{accession}+AND+organism:9606") }; 
            data.Add(CreateKVP("sort", "score"));
            data.Add(CreateKVP("format", "xml"));
            return new FormUrlEncodedContent(data);
        }

        /// <summary>
        /// Creates a KeyValuePair for the request
        /// </summary>
        /// <returns>The KeyValue</returns>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        KeyValuePair<string, string> CreateKVP(string key, string value)
        {
            return new KeyValuePair<string, string>(key, value);
        }
    }
}
