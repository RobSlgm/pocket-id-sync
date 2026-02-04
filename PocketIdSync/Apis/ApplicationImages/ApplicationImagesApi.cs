namespace PocketIdSync.Apis.ApplicationImages;

sealed class ApplicationImagesApi(PocketIdClient PocketId)
{
    public BackgroundImageApi BackgroundImage { get { return new BackgroundImageApi(PocketId); } }
    public LogoApi Logo(LogoThemeMode theme) => new(PocketId, theme);
    public EmailLogoApi EmailLogo { get { return new EmailLogoApi(PocketId); } }
    public FavIconApi FavIcon { get { return new FavIconApi(PocketId); } }
    public DefaultProfilePictureApi DefaultProfilePicture { get { return new DefaultProfilePictureApi(PocketId); } }
}
