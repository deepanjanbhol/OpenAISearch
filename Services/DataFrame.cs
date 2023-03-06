using CsvHelper.Configuration.Attributes;

namespace OpenAISearchScenarios.Services
{
    public class DataFrame<T> where T : class
    {
        public List<T> Rows { get; set; }
    }

    public class ProcessedDataRow
    {
        [Name("title")]
        public string Title { get; set; }

        [Name("heading")]
        public string Heading { get; set; }

        [Name("content")]
        public string Content { get; set; }

        [Name("tokens")]
        public int Tokens { get; set; }
    }
}
