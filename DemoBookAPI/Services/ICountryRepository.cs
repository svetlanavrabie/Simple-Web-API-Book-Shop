﻿using System.Collections.Generic;

namespace DemoBookAPI.Services
{
    public interface ICountryRepository
    {
        ICollection<Country> GetCountries();

        Country GetCountry(int countryId);

        Country GetCountryofAnAuthor(int authorId);

        ICollection<Author> GetAuthorsFromACountry(int countryId);

        bool CountryExists(int countryId);

        bool IsDublicateCountryName(int countryId, string countryName);

        bool CreateCountry(Country country);

        bool UpdateCountry(Country country);

        bool DeleteCountry(Country country);

        bool Save();

    }
}
