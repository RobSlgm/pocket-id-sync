namespace PocketIdSync.Models;

sealed partial class ApiCreateDto
{
    public required string Name { get; set; }
    public required string Resource { get; set; }
}

sealed partial class ApiUpdateDto
{
    public required string Name { get; set; }
}
