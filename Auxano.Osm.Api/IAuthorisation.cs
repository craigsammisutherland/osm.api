namespace Auxano.Osm.Api
{
    /// <summary>
    /// Exposes the authorisation details.
    /// </summary>
    public interface IAuthorisation
    {
        /// <summary>
        /// The secret returned from the Web API.
        /// </summary>
        string Secret { get; }

        /// <summary>
        /// The user ID returned from the Web API.
        /// </summary>
        string UserId { get; }
    }
}