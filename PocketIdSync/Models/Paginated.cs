namespace PocketIdSync.Models;

sealed class Paginated<T>
{
    public T[] Data { get; set; } = [];
    public Pagination? Pagination { get; set; }
}
