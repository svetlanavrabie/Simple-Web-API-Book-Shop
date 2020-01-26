using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoBookAPI.Dtos;
using DemoBookAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DemoBookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : Controller
    {
        private ICountryRepository _countryRepository;
        public CountriesController(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }


        //api/countries/
        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type=typeof(IEnumerable<CountryDto>))]
        public IActionResult GetCountries()
        {
            var countries = _countryRepository.GetCountries().ToList();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var countriesDto = new List<CountryDto>();

            foreach (var country in countries)
            {
                countriesDto.Add(new CountryDto
                {
                    Id = country.Id,
                    Name= country.Name
                }); 
                
            }
            return Ok(countriesDto);
        }


        //api/countries/countryId
        [HttpGet("{countryId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(CountryDto))]
        public IActionResult GetCountry(int countryId)
        {
            if (!_countryRepository.CountryExists(countryId))
            {
                return NotFound();
            }

            var country = _countryRepository.GetCountry(countryId);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var countriyDto = new CountryDto()
            {
                Id = country.Id,
                Name = country.Name
            };

          
            return Ok(countriyDto);
        }

        //api/countries/authors/authorId
        [HttpGet("authors/{authorId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(CountryDto))]
        public IActionResult GetCountryOfAnAuthor(int authorId)
        {
            var country = _countryRepository.GetCountryofAnAuthor(authorId);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var countryDto = new CountryDto()
            {
                Id = country.Id,
                Name = country.Name
            };


            return Ok(countryDto);

        }
    }
}