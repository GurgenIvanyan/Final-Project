namespace Application.Common.Errors;

public static class AppErrors
{
   
    public const string Unauthorized = "auth.unauthorized";
    public const string Forbidden = "auth.forbidden";
    public const string NotFound = "common.not_found";
    public const string Conflict = "common.conflict";
    public const string Validation = "common.validation";
    public const string BadRequest = "common.bad_request";
    public const string Unexpected = "common.unexpected";


    public static string UserUnauthorized() => "User is not authorized.";
    public static string ForbiddenAction() => "You don't have permission to perform this action.";
    public static string PlaylistNotFound(int id) => $"Playlist with id {id} was not found.";
    public static string PlaylistDeletedDuringRead() => "Playlist was deleted during read.";
    public static string SongNotFound(int id) => $"Song with id {id} was not found.";
    public static string ValidationFailed() => "Request validation failed.";
    public static string NameRequired() => "Name is required.";
    public static string PageOutOfRange() => "Page must be >= 1.";
    public static string PageSizeOutOfRange() => "Page size must be > 0.";
    public static string OrderOutOfRange() => "Target order is out of range.";
    public static string BadRequestReason(string reason) => $"Bad request: {reason}.";

    public static string NoPublicPlaylists() => "No public playlists exist.";
    public static string NoPublicPlaylistsWithSongs() => "No public playlists with songs exist.";


}
