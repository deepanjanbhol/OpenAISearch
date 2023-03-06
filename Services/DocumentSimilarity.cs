namespace OpenAISearchScenarios.Services
{
    public class DocumentSimilarity : IEquatable<DocumentSimilarity>
    {
        public float Similarity { get; set; }

        public string Title { get; set; }

        public string Heading { get; set; }

        public string DocumentSimilarityKey => $"{Title}{Heading}";

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;

            return this.Equals((DocumentSimilarity)obj);
        }

        public bool Equals(DocumentSimilarity? other)
        {
            if (other == null) return false;
            return string.Equals(this.DocumentSimilarityKey, other.DocumentSimilarityKey);
        }

        public override int GetHashCode()
        {
            return this.DocumentSimilarityKey.GetHashCode();
        }
    }
}
