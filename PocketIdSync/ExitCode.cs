
namespace PocketIdSync;

static class ExitCode
{
    /// <summary>
    /// Success, operation finished without error
    /// </summary>
    public const int Success = 0;
    /// <summary>
    /// Operation failed
    /// </summary>
    public const int GeneralError = 1;
    /// <summary>
    /// The request contains invalid information (user input wrong)
    /// </summary>
    public const int BadRequest = 2;
    /// <summary>
    /// User is not authorized
    /// </summary>
    public const int Unauthorized = 7;
    /// <summary>
    /// Configuration is invalid
    /// </summary>
    public const int InvalidConfiguration = 8;
    /// <summary>
    /// Operation failed with unhandled fatal error
    /// </summary>
    public const int FatalError = 9;
}
