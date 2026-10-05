using System;
using System.Threading.Tasks;

namespace Chapter6Generics;

public class AnalyzerTest
{
    public async Task SaveUser()
    {
        await Task.Delay(100);
    }

    public async Task RunTestAsync()
    {
        try
        {
            await SaveUser();
        }
        catch (Exception)
        {
        }
    }
}