namespace OpenAISearchScenarios.Services;
public class SearchService
{
    /// <summary>
    /// Open AI Client
    /// </summary>
    private readonly OpenAIClient _openAIClient;

    /// <summary>
    /// Cached dataframe
    /// </summary>
    private DataFrame<ProcessedDataRow> dataframe = null;

    /// <summary>
    /// Cached document embeddings.
    /// </summary>
    private HashSet<EmbeddingResult> documentEmbeddings = null;

    /// <summary>
    /// Implements the instance of <see cref="SearchService"/> class
    /// </summary>
    public SearchService()
    {
        _openAIClient = new OpenAIClient("<openAI-Azure lab endpoint url>");
    }

    /// <summary>
    /// Generates response of the incoming query using Generative AI.
    /// </summary>
    /// <param name="query">Incoming query</param>
    /// <returns>SearchResult</returns>
    public async Task<SearchResults> Search(string query)
    {
        if (dataframe == null)
        {
            dataframe = this._openAIClient.LoadProcessedCsv("DevTools-documentation-processed1.csv");
        }

        if (documentEmbeddings == null)
        {
            documentEmbeddings = await this._openAIClient.ComputeDocEmbeddings(dataframe);
        }

        var response = await this._openAIClient.AnswerQueryWithContext(query, dataframe, documentEmbeddings, false);

        return new SearchResults(
            response.RelatedDocuments.Select(x => new SearchResult(x.Item1, x.Item2)),
            response.Answer);
    }

    record SearchDocument (string? metadata_storage_name, string content);

    public record SearchResult (string? Title, string Content);

    public record SearchResults(IEnumerable<SearchResult> Documents, string? CrossDocumentSummary = null);
}
