namespace OpenAISearchScenarios.Services
{
    public class AnswerQueryWithContextResponse
    {
        public string Answer { get; set; }

        public List<Tuple<string, string>> RelatedDocuments { get; set; }
    }
}
