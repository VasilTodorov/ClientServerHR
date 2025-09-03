using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClientServerHR.Repositories
{
    public interface ICountryRepository
    {
        IEnumerable<Country> AllCountries { get; }
        Country? GetCountryById(int countryId);
        Country? GetCountryByName(string countryName);
        public IEnumerable<SelectListItem> CountryOptions { get;}
        public void AddCountry(Country country);

        public void Update(Country country);

        public void Delete(int id);
    }
}
