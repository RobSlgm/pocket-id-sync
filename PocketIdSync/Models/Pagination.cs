namespace PocketIdSync.Models;

sealed class Pagination
{
    public int? CurrentPage { get; set; }
    public int? ItemsPerPage { get; set; }
    public int? TotalItems { get; set; }
    public int? TotalPages { get; set; }
}
