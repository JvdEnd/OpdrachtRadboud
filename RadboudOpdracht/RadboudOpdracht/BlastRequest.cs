using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace RadboudOpdracht
{
    public class BlastRequest
    {
        const string BlastEndPoint = @"https://www.ncbi.nlm.nih.gov/blast/Blast.cgi";
        HttpClient client = new HttpClient { BaseAddress = new Uri("https://www.ncbi.nlm.nih.gov/blast/Blast.cgi") };
        readonly CancellationToken _cancel = new CancellationToken();

        internal async Task<string> BlastAsync(string sequence)
        {
            var blastXml = await ExecuteBlast(_cancel, sequence);
            return ParseBlastXml(blastXml).Accession;
        }

        /// <summary>
        /// Parses the xml returned from blast, only included the bare minimum and takes the first blast result
        /// </summary>
        /// <returns>Returns first blast hit</returns>
        public BlastHit ParseBlastXml(Stream blastXml)
        {
            BlastHit hit = null;
            try
            {
                var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(blastXml);
                    using (XmlReader r = XmlReader.Create(sr, settings))
                    {
                        string curElement = string.Empty;
                        while (r.Read())
                        {
                            SelectHit(ref hit, r, ref curElement);
                            if (hit?.Accession != null && hit?.Def != null)
                            {
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    if (sr != null)
                    {
                        sr.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return hit;
        }

        /// <summary>
        /// Select nodes with the wanted information
        /// </summary>
        /// <returns>Returns blast hit object, current element</returns>
        private void SelectHit(ref BlastHit hit, XmlReader r, ref string curElement)
        {
            switch (r.NodeType)
            {
                case XmlNodeType.Element:
                    curElement = r.Name;
                    switch (curElement)
                    {
                        case "Hit":
                            hit = new BlastHit();
                            break;
                    }
                    break;
                case XmlNodeType.Text:
                    if (curElement.StartsWith("Hit_", StringComparison.OrdinalIgnoreCase))
                    {
                        ParseHit(curElement, r.Value, hit);
                    }
                    break;

            }
        }

        /// <summary>
        /// Fills the blast hit object
        /// </summary>
        /// <returns>Returns blast hit object</returns>
        private void ParseHit(string element, string value, BlastHit curHit)
        {
            switch (element)
            {
                case "Hit_num":
                    break;
                case "Hit_id":
                    break;
                case "Hit_def":
                    curHit.Def = value;
                    break;
                case "Hit_accession":
                    curHit.Accession = value;
                    break;
                case "Hit_len":
                    break;
                case "Hit_hsps":
                    break;
                default:
                    throw new FormatException($"Unknown element {element}");
            }
        }

        /// <summary>
        /// Excute blast
        /// </summary>
        /// <returns>blast output in XML format</returns>
        public async Task<Stream> ExecuteBlast(CancellationToken token, string sequence)
        {
            Match match = await RequestBlastInfo(token, sequence);
            string rid = match.Groups[1].Value;

            // Wait until time to completion has passed, then start polling
            if (Int32.TryParse(match.Groups[2].Value, out int seconds))
            {
                await Task.Delay(seconds * 1000, token);
            }

            Regex statusExpr = new Regex(@"QBlastInfoBegin\s+Status=(\w+)");

            //Wait for ncbi to finish blasting
            while (true)
            {
                string resultResponse = await client.GetStringAsync(
                    string.Format($"{BlastEndPoint}?CMD=Get&FORMAT_OBJECT=SearchInfo&RID={rid}"));
                var statusMatch = statusExpr.Matches(resultResponse);
                if (statusMatch.Count == 1)
                {
                    string state = statusMatch[0].Groups[1].Value;
                    if (state == "FAILED")
                        throw new Exception("Search " + rid + " failed;");
                    if (state == "UNKNOWN")
                        throw new OperationCanceledException("Search " + rid + " expired.");
                    if (state == "READY")
                    {
                        Regex hasHitsExpr = new Regex(@"QBlastInfoBegin\s+ThereAreHits=yes");
                        if (!hasHitsExpr.IsMatch(resultResponse))
                        {
                            return null;
                        }
                        break;
                    }
                }

                //Try again
                await Task.Delay(2000, token);

                //Check at the end so we get at least one attempt
                token.ThrowIfCancellationRequested();
            }

            // Retrieve the response.
            return await client.GetStreamAsync(
                string.Format("{0}?CMD=Get&FORMAT_TYPE=XML&RID={1}", BlastEndPoint, rid));
        }

        /// <summary>
        /// Sends initial request to ncbi
        /// </summary>
        /// <returns>response</returns>
        private async Task<Match> RequestBlastInfo(CancellationToken token, string sequence)
        {
            var content = CreateRequest(sequence);

            var result = await client.PostAsync(BlastEndPoint, content, token);
            result.EnsureSuccessStatusCode();

            string InitialResponse = await result.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(InitialResponse))
            {
                throw new HttpRequestException("No data returned from ncbi");
            }

            return ParseResponseInfo(InitialResponse);
        }

        /// <summary>
        /// Parses the response info from blast
        /// </summary>
        /// <returns>response</returns>
        private static Match ParseResponseInfo(string response)
        {
            Regex ridExpr = new Regex(@"QBlastInfoBegin\s+RID = (\w+)\s+RTOE = (\w+)");
            var matches = ridExpr.Matches(response);
            if (matches.Count != 1
                || matches[0].Groups.Count != 3)
                throw new HttpRequestException("Unrecognized format returned, no Request Id located");

            var match = matches[0];
            return match;
        }

        /// <summary>
        /// Build the HTTP content for the request based on the parameters
        /// </summary>
        /// <returns>The request</returns>
        public HttpContent CreateRequest(string sequence)
        {
            if (sequence == null)
                throw new ArgumentException("Missing sequence argument");

            var data = new List<KeyValuePair<string, string>> { CreateKVP("CMD", "Put") };
            data.Add(CreateKVP("PROGRAM", "blastn"));
            data.Add(CreateKVP("DATABASE", "nt"));
            data.Add(CreateKVP("QUERY", sequence));
            //data.Add(CreateKVP("ORGANISM", "9606"));

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
