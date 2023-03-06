using CsvHelper;
using Newtonsoft.Json;
using System.Text;

namespace OpenAISearchScenarios.Services
{
    /// <summary>
    /// Class to interact with Azure Open AI Api's.
    /// </summary>
    public class OpenAIClient
    {
        private const int MAX_SECTION_LEN = 500;

        private const string SEPARATOR = "\n*";

        /// <summary>
        /// encoding for text-davinci-003
        /// </summary>
        private const string ENCODING = "gpt2"; 

        private const int separator_len = 3;

        private readonly string BaseUrl;

        /// <summary>
        /// Initializes a new instance of <see cref="OpenAIClient"/> class.
        /// </summary>
        /// <param name="baseUrl">Base url of Open Azure lab endpoint</param>
        public OpenAIClient(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public async Task<string> GetCompletionApiResponseAsync(string deploymentName, string prompt, int max_tokens = 20, double temperature = 0.8)
        {
            string url = BaseUrl + "/openai/deployments/" + deploymentName + "/completions?api-version=2022-12-01";

            var payload = new
            {
                prompt = prompt,
                max_tokens = max_tokens,
                temperature = temperature
            };

            string api_key = "<api-key>";
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("api-key", api_key);
                request.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

                var content = new StringContent(JsonConvert.SerializeObject(payload));

                var response = await client.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();

                return responseBody;
            }
        }

        /// <summary>
        /// Performs the embedding of the provided text
        /// </summary>
        /// <param name="text">Incoming text</param>
        /// <returns>Vector containing the List of float</returns>
        public async Task<List<float>> GetEmbedding(string text)
        {
            var url = BaseUrl + "/openai/deployments/deployment-d58013de8b464e908fd40963d9889fc3/embeddings?api-version=2022-12-01";
            string api_key = "<api-key>";
            var payload = new { input = text };
            var httpClient = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("api-key", api_key);
            request.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            var embeddingVector = JsonConvert.DeserializeObject<dynamic>(responseBody);
            return embeddingVector.data[0].embedding.ToObject<List<float>>();
        }

        /// <summary>
        /// Print the first 5 documents which are similar to the provided query.
        /// </summary>
        /// <param name="query">Incoming query</param>
        /// <param name="documentEmbeddings">Document Embeddings</param>
        /// <returns>Corresponding task</returns>
        public async Task PrintOrderDocumentSectionsByQuerySimilarity(string query, HashSet<EmbeddingResult> documentEmbeddings)
        {
            var querySimilarity = await OrderDocumentSectionsByQuerySimilarity(query, documentEmbeddings);

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(querySimilarity);
            }
        }

        /// <summary>
        /// Create an embedding for each row in the list using the OpenAI Embeddings API.
        /// Return a dictionary that maps between each embedding vector and the index of the row that it corresponds to.
        /// </summary>
        /// <param name="df">DataFrame</param>
        /// <returns>List of embedding result for the docs in dataframe</returns>
        public async Task<HashSet<EmbeddingResult>> ComputeDocEmbeddings(DataFrame<ProcessedDataRow> df)
        {
            var embeddings = new HashSet<EmbeddingResult>();
            foreach (var (idx, row) in df.Rows.WithIndex())
            {
                var embedding = await GetEmbedding(row.Content);
                embeddings.Add(new EmbeddingResult { Title = row.Title, Heading = row.Heading, Embedding = embedding });
            }

            return embeddings;
        }

        //private HashSet<EmbeddingResult> LoadEmbeddings(string fname)
        //{
        //    using (var reader = new StreamReader(fname))
        //    using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
        //    {
        //        var records = csv.GetRecords<dynamic>();
        //        var max_dim = records.First().Dictionary.Keys.Where(k => k != "title" && k != "heading").Max(k => int.Parse(k));
        //        return records.ToDictionary(
        //            r => ((string)r.title, (string)r.heading),
        //            r => Enumerable.Range(0, max_dim + 1).Select(i => (float)r[i.ToString()]).ToList()
        //        );
        //    }
        //}

        /// <summary>
        /// Determine similarity of two vectors.
        /// Perform cosine similarity of the two vectors.
        /// </summary>
        /// <param name="x">Vector x</param>
        /// <param name="y">Vector y</param>
        /// <returns>Similarity</returns>
        private float VectorSimilarity(List<float> x, List<float> y)
        {
            return x.Zip(y, (a, b) => a * b).Sum();
        }

        /// <summary>
        /// Order document sections by similarity to query.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="contexts"></param>
        /// <returns>List of similarity sorted by most similar document</returns>
        private async Task<List<DocumentSimilarity>> OrderDocumentSectionsByQuerySimilarity(string query, HashSet<EmbeddingResult> contexts)
        {
            var query_embedding = await GetEmbedding(query);

            var document_similarities = contexts.Select(kvp => new DocumentSimilarity { Heading = kvp.Heading, Title = kvp.Title, Similarity = VectorSimilarity(query_embedding, kvp.Embedding) })
                                                 .OrderByDescending(pair => pair.Similarity)
                                                 .ToList();

            return document_similarities;
        }

        /// <summary>
        /// Load preprocessed csv into Dataframe
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public DataFrame<ProcessedDataRow> LoadProcessedCsv(string filePath)
        {
            List<ProcessedDataRow> records;

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
            {
                records = csv.GetRecords<ProcessedDataRow>().ToList();
            }

            return new DataFrame<ProcessedDataRow> { Rows = records };
        }

        /// <summary>
        /// Answers the incoming query by injecting the right context into the prompt.
        /// </summary>
        /// <param name="query">Incoming query</param>
        /// <param name="df">Dataframe</param>
        /// <param name="context_embeddings">context embeddings</param>
        /// <param name="showPrompt">Whether to show prompt</param>
        /// <returns></returns>
        public async Task<AnswerQueryWithContextResponse> AnswerQueryWithContext(
            string query, 
            DataFrame<ProcessedDataRow> df,
            HashSet<EmbeddingResult> context_embeddings, 
            bool showPrompt = false)
        {
            var response = await ConstructPrompt(query, context_embeddings, df);

            if (showPrompt)
            {
                Console.WriteLine(response.Prompt);
            }

            var openAICompletionResponse = await GetCompletionApiResponseAsync("text-davinci-003", prompt: response.Prompt, 300, 0);
            var openAICompletionResponseJson = JsonConvert.DeserializeObject<dynamic>(openAICompletionResponse);
            return new AnswerQueryWithContextResponse
            {
                Answer = openAICompletionResponseJson.choices[0].text.ToString(),
                RelatedDocuments = response.RelatedDocuments
            };
        }

        /// <summary>
        /// Construct prompt with the context embeddings.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="context_embeddings"></param>
        /// <param name="df"></param>
        /// <returns></returns>
        private async Task<ConstructPromptResponse> ConstructPrompt(string query, HashSet<EmbeddingResult> context_embeddings, DataFrame<ProcessedDataRow> df)
        {
            var most_relevant_document_sections = await OrderDocumentSectionsByQuerySimilarity(query, context_embeddings);

            var relatedDocs = new List<Tuple<string, string>>();
            var chosen_sections = new List<string>();
            var chosen_sections_len = 0;
            var chosen_sections_indexes = new List<string>();

            foreach (var doc in most_relevant_document_sections)
            {
                // Add contexts until we run out of space.
                var document_section = df.Rows.First(x => x.Title == doc.Title && x.Heading == doc.Heading);

                chosen_sections_len += document_section.Tokens + separator_len;
                relatedDocs.Add(new Tuple<string, string>(document_section.Title, document_section.Heading));
                if (chosen_sections_len > MAX_SECTION_LEN)
                {
                    break;
                }

                chosen_sections.Add(SEPARATOR + document_section.Content.Replace("\n", " "));
                chosen_sections_indexes.Add(document_section.Title + " | " + document_section.Heading);
            }

            // Useful diagnostic information
            Console.WriteLine($"Selected {chosen_sections.Count} document sections:");
            Console.WriteLine(string.Join("\n", chosen_sections_indexes));

            var header = @"Answer the question as truthfully as possible using the provided context, and if the answer is not contained within the text below, say ""I don't know."". 
            Context:
            ";

            return new ConstructPromptResponse
            {
                Prompt = header + string.Join("", chosen_sections) + "\n\n Q: " + query + "\n A:",
                RelatedDocuments = relatedDocs
            };
        }
    }

    public static class Extensions
    {
        public static IEnumerable<(int, T)> WithIndex<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (index, item));
        }
    }
}
