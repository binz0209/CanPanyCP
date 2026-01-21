using System.Text.RegularExpressions;
using CanPany.Application.Interfaces.Services;

namespace CanPany.Application.Common.SemanticSearch;

/// <summary>
/// Lightweight, dependency-free embedding based on hashing trick (bag-of-words).
/// Not as strong as LLM embeddings, but provides reasonable semantic-ish matching
/// for skills/titles/keywords and works offline.
/// </summary>
public sealed class HashingTextEmbeddingService : ITextEmbeddingService
{
    private readonly int _dims;

    public HashingTextEmbeddingService(int dims = 256)
    {
        if (dims <= 0) throw new ArgumentOutOfRangeException(nameof(dims));
        _dims = dims;
    }

    public List<double> Embed(string text)
    {
        var vec = new double[_dims];
        if (string.IsNullOrWhiteSpace(text))
            return vec.ToList();

        foreach (var token in Tokenize(text))
        {
            var h = StableHash(token);
            var idx = (int)((uint)h % (uint)_dims);
            // Use signed hashing to reduce collisions bias
            var sign = ((h & 1) == 0) ? 1.0 : -1.0;
            vec[idx] += sign;
        }

        return VectorMath.Normalize(vec).ToList();
    }

    private static IEnumerable<string> Tokenize(string text)
    {
        // Lowercase; keep letters/digits; split by non-word chars.
        foreach (Match m in Regex.Matches(text.ToLowerInvariant(), @"[\p{L}\p{N}]+"))
        {
            var tok = m.Value;
            if (tok.Length >= 2)
                yield return tok;
        }
    }

    // FNV-1a 32-bit (stable across processes)
    private static int StableHash(string s)
    {
        unchecked
        {
            const int fnvOffset = unchecked((int)2166136261);
            const int fnvPrime = 16777619;
            var hash = fnvOffset;
            foreach (var c in s)
            {
                hash ^= c;
                hash *= fnvPrime;
            }
            return hash;
        }
    }
}



