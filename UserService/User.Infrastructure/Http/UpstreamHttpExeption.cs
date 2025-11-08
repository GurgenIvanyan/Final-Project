/*namespace User.Infrastructure.Http
{
    /// <summary>
    /// Исключение для ошибок, возвратившихся из внешнего сервиса (Playlist.Api).
    /// Обрабатывается в ExceptionHandlingMiddleware как ProblemDetails.
    /// </summary>
    public sealed class UpstreamHttpException : Exception
    {
        public int StatusCode { get; }
        public string? Title { get; }
        public string? Detail { get; }
        public string? Type { get; }
        public string? Instance { get; }

        public UpstreamHttpException(int statusCode, string? title, string? detail = null, string? type = null, string? instance = null)
            : base($"{title ?? "Upstream error"} (status {statusCode})")
        {
            StatusCode = statusCode;
            Title = title;
            Detail = detail;
            Type = type;
            Instance = instance;
        }
    }
}*/
