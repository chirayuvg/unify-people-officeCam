namespace Unify.People.RulesEngine
{
    public static class EmbeddingComparer
    {
        public static double CosineSimilarity(double[] vectorA, double[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                throw new ArgumentException("Vectors must be of same length.");

            double dotProduct = 0.0;
            double normA = 0.0;
            double normB = 0.0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                normA += Math.Pow(vectorA[i], 2);
                normB += Math.Pow(vectorB[i], 2);
            }

            normA = Math.Sqrt(normA);
            normB = Math.Sqrt(normB);

            if (normA == 0 || normB == 0)
                return 0; // Avoid division by zero

            return dotProduct / (normA * normB);
        }
    }
}
