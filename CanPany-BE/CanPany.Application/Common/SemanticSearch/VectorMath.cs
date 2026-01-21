namespace CanPany.Application.Common.SemanticSearch;

public static class VectorMath
{
    public static double CosineSimilarity(IReadOnlyList<double> a, IReadOnlyList<double> b)
    {
        if (a.Count == 0 || b.Count == 0) return 0;
        if (a.Count != b.Count) return 0;

        double dot = 0, na = 0, nb = 0;
        for (int i = 0; i < a.Count; i++)
        {
            dot += a[i] * b[i];
            na += a[i] * a[i];
            nb += b[i] * b[i];
        }

        var denom = Math.Sqrt(na) * Math.Sqrt(nb);
        return denom <= 0 ? 0 : dot / denom;
    }

    public static double[] Normalize(double[] v)
    {
        double n2 = 0;
        for (int i = 0; i < v.Length; i++) n2 += v[i] * v[i];
        var n = Math.Sqrt(n2);
        if (n <= 0) return v;
        for (int i = 0; i < v.Length; i++) v[i] /= n;
        return v;
    }
}



