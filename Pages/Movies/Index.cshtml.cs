using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MovieReviews.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MovieReviews.Pages.Movies
{
    public class IndexModel : PageModel
    {
        private readonly MovieReviews.Models.MovieDbContext _context;

        public IndexModel(MovieReviews.Models.MovieDbContext context)
        {
            _context = context;
        }

        public PaginatedList<Movie> Movie { get;set; }
        
        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }
        // Requires using Microsoft.AspNetCore.Mvc.Rendering;
        public SelectList Genres { get; set; }
        [BindProperty(SupportsGet = true)]
        public string MovieGenre { get; set; }
        public string NameSort {get; set;}

        public string DateSort {get; set;}

        public string CurrentFilter {get; set;}

        public string CurrentSort {get; set;}

        public async Task OnGetAsync(string sortOrder, string CurrentFilter, string SearchString, int? pageIndex)
        {
            CurrentSort = sortOrder;
            NameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            DateSort = sortOrder == "Date" ? "date_desc" : "Date";
            if (SearchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                SearchString = CurrentFilter;
            }

            CurrentFilter = SearchString;

            IQueryable<Movie> movieSort = from m in _context.Movie
                                        select m;

            switch (sortOrder)
            {
                case "name_desc":
                    movieSort = movieSort.OrderByDescending( m => m.Title);
                    break;
                case "Date" :
                    movieSort = movieSort.OrderBy(m => m.ReleaseDate);
                    break;
                case "date_desc" :
                    movieSort = movieSort.OrderByDescending(m => m.ReleaseDate);
                    break;
                default:
                    movieSort = movieSort.OrderBy(m => m.Title);
                    break;
            }

            int pageSize = 3;
            Movie = await PaginatedList<Movie>.CreateAsync(
                movieSort.AsNoTracking(), pageIndex ?? 1, pageSize);
        
            // Use LINQ to get list of genres.
            IQueryable<string> genreQuery = from m in _context.Movie
                                            orderby m.Genre
                                            select m.Genre;

            //var movies = from m in _context.Movie
            //            select m;
            // var movies = _context.Movie.Include(m => m.Reviews).Select(m => new {
            //     ID = m.ID,
            //     Title = m.Title,
            //     ReleaseDate = m.ReleaseDate,
            //     Genre = m.Genre,
            //     Price = m.Price,
            //     Rating = m.Rating,
            //     Review = m.Reviews.Average(r => r.Score)            
            // });
            // IQueryable<Movie> movies = _context.Movie.Include(m => m.Reviews);
            // var movies = (from m in _context.Movie
            //     select m).Include("Reviews");
            
            // Use .Include() to bring in the reviews
            var movies = _context.Movie.Include(m => m.Reviews).Select(m => m);

            if (!string.IsNullOrEmpty(SearchString))
            {
                movies = movies.Where(s => s.Title.Contains(SearchString));
            }
            
            if (!string.IsNullOrEmpty(MovieGenre))
            {
                movies = movies.Where(x => x.Genre == MovieGenre);
            }

            Genres = new SelectList(await genreQuery.Distinct().ToListAsync());
            
        }
    }
}
