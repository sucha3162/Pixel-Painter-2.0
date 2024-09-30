using MyTestVueApp.Server.Entities;

namespace MyTestVueApp.Server.Interfaces
{
    /// <summary>
    /// Interface defines the SQL service.
    /// </summary>
    public interface IArtAccessService
    {
        /// <summary>
        /// Gets a list of all paintings from the database.
        /// </summary>
        /// <returns>A list of all paintings</returns>
        public IEnumerable<WorkOfArt> GetAllArt();
        public WorkOfArt GetArtById(int id);
    }
}