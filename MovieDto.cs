namespace Изпит
{
    public class MovieDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? PosterUrl { get; set; }
        public string? TrailerLink { get; set; }
        public bool? IsWatched { get; set; }
    }
}