using CinemaTicket.Model;
using CinemaTicket.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaTicket.Config;

namespace CinemaTicket.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController(ApplicationDbContext context) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyCollection<Movie>>> List(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Movies.AsQueryable();
        
        if (!includeInactive)
        {
            query = query.Where(m => m.IsActive);
        }
        
        var movies = await query.OrderBy(m => m.Title).ToListAsync(cancellationToken);
        return Ok(movies);
    }

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<Movie>> GetById(long id, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies.FindAsync(new object[] { id }, cancellationToken);
        
        if (movie == null)
        {
            return NotFound(new { message = $"Movie with ID {id} not found" });
        }
        
        return Ok(movie);
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<ActionResult<Movie>> Create([FromBody] Movie movie, CancellationToken cancellationToken)
    {
        if (movie.Id != 0)
        {
            return BadRequest(new { message = "ID should not be provided when creating a movie" });
        }
        
        _context.Movies.Add(movie);
        await _context.SaveChangesAsync(cancellationToken);
        
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<ActionResult<Movie>> Update(long id, [FromBody] Movie movie, CancellationToken cancellationToken)
    {
        if (id != movie.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }
        
        var existingMovie = await _context.Movies.FindAsync(new object[] { id }, cancellationToken);
        if (existingMovie == null)
        {
            return NotFound(new { message = $"Movie with ID {id} not found" });
        }
        
        existingMovie.Title = movie.Title;
        existingMovie.Description = movie.Description;
        existingMovie.DurationMinutes = movie.DurationMinutes;
        existingMovie.Genre = movie.Genre;
        existingMovie.Rating = movie.Rating;
        existingMovie.Director = movie.Director;
        existingMovie.Cast = movie.Cast;
        existingMovie.PosterUrl = movie.PosterUrl;
        existingMovie.TrailerUrl = movie.TrailerUrl;
        existingMovie.ReleaseDate = movie.ReleaseDate;
        existingMovie.IsActive = movie.IsActive;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok(existingMovie);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies.FindAsync(new object[] { id }, cancellationToken);
        
        if (movie == null)
        {
            return NotFound(new { message = $"Movie with ID {id} not found" });
        }
        
        // Check if movie is used in any screenings
        var hasScreenings = await _context.Screenings.AnyAsync(s => s.MovieId == id, cancellationToken);
        if (hasScreenings)
        {
            return BadRequest(new { message = "Cannot delete movie that has associated screenings" });
        }
        
        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync(cancellationToken);
        
        return NoContent();
    }
}
