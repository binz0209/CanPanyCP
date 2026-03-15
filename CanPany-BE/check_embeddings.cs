using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

class Program
{
    static async Task Main()
    {
        var client = new HttpClient();
        try {
            var res = await client.GetStringAsync("http://localhost:5033/api/jobs");
            var json = JsonDocument.Parse(res);
            var jobs = json.RootElement.GetProperty("data").GetProperty("jobs");
            Console.WriteLine($"Total jobs: {jobs.GetArrayLength()}");
            
            int embeddedCount = 0;
            foreach(var job in jobs.EnumerateArray()) {
                if (job.TryGetProperty("skillEmbedding", out var emb) && emb.ValueKind == JsonValueKind.Array && emb.GetArrayLength() > 0) {
                    embeddedCount++;
                }
            }
            Console.WriteLine($"Jobs with SkillEmbedding: {embeddedCount}");
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }
}
