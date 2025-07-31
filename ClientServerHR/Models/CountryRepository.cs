
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClientServerHR.Models
{
    public class CountryRepository : ICountryRepository
    {
        private readonly ClientServerHRDbContext _clientServerHRDbContext;

        public CountryRepository(ClientServerHRDbContext clientServerHRDbContext)
        {
            _clientServerHRDbContext = clientServerHRDbContext;
        }
        public IEnumerable<Country> AllCountries
        {
            get
            {
                return _clientServerHRDbContext.Countries;
            }
        }

        public IEnumerable<SelectListItem> CountryOptions 
        { 
            get 
            {
                return _clientServerHRDbContext.Countries
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.CountryId.ToString(),
                    Text = c.Name
                }).ToList();
            }
             
        }

        public void AddCountry(Country country)
        {
            _clientServerHRDbContext.Countries.Add(country);
            _clientServerHRDbContext.SaveChanges();            
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Country? GetCountryById(int countryId)
        {
            throw new NotImplementedException();
        }

        public Country? GetCountryByName(string countryName)
        {
            throw new NotImplementedException();
        }

        public void Update(Country country)
        {
            throw new NotImplementedException();
        }
    }
}
