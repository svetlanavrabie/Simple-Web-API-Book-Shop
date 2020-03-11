using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoBookAPI.Services
{
    public class CountryRepository : ICountryRepository
    {
        private BookDbContext _countryContext;

        public CountryRepository(BookDbContext countryContext)
        {
            _countryContext = countryContext;
        }

        public ICollection<Author> GetAuthorsFromACountry(int countryId)
        {
            return _countryContext.Authors.Where(c => c.Country.Id == countryId).ToList();
        }

        public ICollection<Country> GetCountries()
        {
            return _countryContext.Countries.OrderBy(n => n.Name).ToList();
        }

        public Country GetCountry(int countryId)
        {
            return _countryContext.Countries.Where(c=>c.Id==countryId).FirstOrDefault();
        }

        public Country GetCountryofAnAuthor(int authorId)
        {
            return _countryContext.Authors.Where(a => a.Id == authorId).Select(c => c.Country).FirstOrDefault();
        }

        public bool CountryExists(int countryId)
        {
            return _countryContext.Countries.Any(c => c.Id == countryId);
        }

        public bool IsDublicateCountryName(int countryId, string countryName)
        {
            var country = _countryContext.Countries.Where(c => c.Name.Trim().ToUpper() == countryName.Trim().ToUpper() && c.Id != countryId).FirstOrDefault();

            return country == null ? false : true;
        }
    }
}
