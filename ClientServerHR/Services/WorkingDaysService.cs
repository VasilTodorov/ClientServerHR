using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class WorkingDaysService
{
    private static readonly HttpClient client = new HttpClient();
    private const string apiKey = "0ujgF1FUKULubdqvDyg3Pg==EfyEQBVf67KqBeoK";

    public async Task<int> GetWorkingDaysThisMonth(string country, int month)
    {        
        string url = $"https://api.api-ninjas.com/v1/workingdays?country={country}&month={month}";        

        return await Calculate(url);
    }
    public async Task<int> GetWorkingDaysThisMonth(string country, int month, int year)
    {
        string url = $"https://api.api-ninjas.com/v1/workingdays?country={country}&month={month}&year={year}";

        return await Calculate(url);
    }
    private async Task<int> Calculate(string url)
    {
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(json);
            return (int)obj["num_working_days"];
        }
        else
        {
            throw new Exception($"API Error: {response.StatusCode}");
        }
    }
}
