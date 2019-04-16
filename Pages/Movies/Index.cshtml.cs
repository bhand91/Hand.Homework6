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

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex)
        {
            //specifies sort order. Line 42 = if sort order is null, then titles will be sorted by decending
            CurrentSort = sortOrder;
            NameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            DateSort = sortOrder == "Date" ? "date_desc" : "Date";
            

            if(searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            //holds search
            CurrentFilter = searchString;

            var movies = _context.Movie.Include(m => m.Reviews).Select(m => m);
            
            //ToUpper helps remove case sensitivity
            if (!string.IsNullOrEmpty(SearchString))
            {
                movies = movies.Where(m => m.Title.ToUpper().Contains(searchString.ToUpper()) || m.Genre.Contains(searchString));
            }

            IQueryable<string> genreQuery = from m in _context.Movie
                                                        orderby m.Genre
                                                        select m.Genre;

            if (!string.IsNullOrEmpty(MovieGenre))
            {
                movies = movies.Where(x => x.Genre == MovieGenre);
            }

            

            Genres = new SelectList(await genreQuery.Distinct().ToListAsync());

            //creates a switch statement to specify how each option will react on the page.
            switch (sortOrder)
            {
                case "name_desc":
                    movies = movies.OrderByDescending( m => m.Title);
                    break;
                case "Date" :
                    movies = movies.OrderBy(m => m.ReleaseDate);
                    break;
                case "date_desc" :
                    movies = movies.OrderByDescending(m => m.ReleaseDate);
                    break;
                default:
                    movies = movies.OrderBy(m => m.Title);
                    break;
            }

            int pageSize = 10;
            Movie = await PaginatedList<Movie>.CreateAsync(
                movies.AsNoTracking(), pageIndex ?? 1, pageSize);
            
            
        }
        
    }
}
