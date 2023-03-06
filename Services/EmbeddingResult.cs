namespace OpenAISearchScenarios.Services
{
    public class EmbeddingResult : IEquatable<EmbeddingResult>
    {
        public List<float> Embedding { get; set; }

        public string Title { get; set; }

        public string Heading { get; set; }

        public string EmbeddingResultKey => $"{Title}{Heading}";

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;

            return this.Equals((EmbeddingResult)obj);
        }

        public bool Equals(EmbeddingResult? other)
        {
            if (other == null) return false;
            return string.Equals(this.EmbeddingResultKey, other.EmbeddingResultKey);
        }

        public override int GetHashCode()
        {
            return this.EmbeddingResultKey.GetHashCode();
        }
    }
}
