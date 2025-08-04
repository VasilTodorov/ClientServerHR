using Newtonsoft.Json.Linq;

public class WorkingDaysService
{
    private static readonly HttpClient client = new HttpClient();
    private readonly string apiKey;
    
    public WorkingDaysService()
    {
        apiKey = Environment.GetEnvironmentVariable("WORKING_DAYS_API_KEY")
            ?? throw new InvalidOperationException("API key is missing. Set WORKING_DAYS_API_KEY environment variable.");
    }
    public int GetWorkingDaysThisMonth(string country, int month, int year)
    {
        string url = $"https://api.api-ninjas.com/v1/workingdays?country={country}&month={month}&year={year}";
        return Calculate(url);
    }
    public int GetWorkingDaysThisMonth(string country, int month)
    {
        string url = $"https://api.api-ninjas.com/v1/workingdays?country={country}&month={month}";
        return Calculate(url);
    }
    private int Calculate(string url)
    {
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = client.GetAsync(url).Result;  
        if (response.IsSuccessStatusCode)
        {
            string json = response.Content.ReadAsStringAsync().Result;
            var obj = JObject.Parse(json);
            return (int)obj["num_working_days"]!;
        }
        else
        {
            throw new Exception($"API Error: {response.StatusCode}");
        }
    }
}