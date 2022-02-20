namespace Ruler.Engine.Location
{
    /// <summary>
    ///     Provides a location.
    /// </summary>
    public interface ILocationProvider
    {
        /// <summary>
        ///     Retrieves a location using the location provider.
        /// </summary>
        string GetLocation(params object?[] args);
    }
}