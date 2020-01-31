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
    public class CategoriesController : Controller
    {
        private ICategoryRepository _categoryRepository;
        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }


        //api/categories/
        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CategoryDto>))]
        public IActionResult GetCategories()
        {
            var categories = _categoryRepository.GetCategories().ToList();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoriesDto = new List<CategoryDto>();

            foreach (var category in categories)
            {
                categoriesDto.Add(new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name
                });

            }
            return Ok(categoriesDto);
        }


        //api/categories/categoryId
        [HttpGet("{categoryId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(CategoryDto))]
        public IActionResult GetCategory(int categoryId)
        {
            if (!_categoryRepository.CategoryExists(categoryId))
            {
                return NotFound();
            }

            var category = _categoryRepository.GetCategory(categoryId);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoryDto = new CategoryDto()
            {
                Id = category.Id,
                Name = category.Name
            };


            return Ok(categoryDto);
        }


        //api/categories/books/bookId
        [HttpGet("books/{bookId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CategoryDto>))]
        public IActionResult GetAllCategoriesForABook(int bookId)
        {
            //to do - if the book exists

            var categories = _categoryRepository.GetAllCategoriesForABook(bookId);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoriesDto = new List<CategoryDto>();

            foreach (var category in categories)
            {
                categoriesDto.Add(new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name
                });

            }

            return Ok(categoriesDto);

        }


        //api/categories/categoryId/books

        [HttpGet("{categoryId}/books")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<BookDto>))]
        public IActionResult GetAllBooksForCategory(int categoryId)
        {
            if (!_categoryRepository.CategoryExists(categoryId))
            {
                return NotFound();
            }


            var books = _categoryRepository.GetBookForCategory(categoryId);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var booksDto = new List<BookDto>();

            foreach (var book in books)
            {
                booksDto.Add(new BookDto
                {
                    Id = book.Id,
                    Title = book.Title,
                    Isbn = book.Isbn,
                    DatePublished= book.DatePublished
                });

            }

            return Ok(booksDto);
        }

    }
}