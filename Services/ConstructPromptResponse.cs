namespace OpenAISearchScenarios.Services
{
    public class ConstructPromptResponse
    {
        public string Prompt { get; set; }

        public List<Tuple<string, string>> RelatedDocuments { get; set; }
    }
}
