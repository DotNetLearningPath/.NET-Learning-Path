using System.Threading.Tasks;

namespace CodeGuidelines.Analyzers;

public class AnalyzerTest
{
    public async Task SaveUser()
    {
    await Task.Delay(100);
    }
}
