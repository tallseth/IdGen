using System.Collections.Generic;

namespace IdGen
{
    /// <summary>
    /// Provides the interface for Id-generators.
    /// </summary>
    public interface IIdGenerator
    {
        /// <summary>
        /// Gets the Id of the generator.
        /// </summary>
        int Id { get; }
        
        /// <summary>
        /// Gets the <see cref="IdGeneratorOptions"/>.
        /// </summary>
        IdGeneratorOptions Options { get; }
        
        /// <summary>
        /// Creates a new Id.
        /// </summary>
        /// <returns>Returns an Id.</returns>
        long CreateId();

        /// <summary>
        /// Creates a group of Ids at once.
        /// </summary>
        /// <param name="number">desired number of Ids</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of Ids</returns>
        IEnumerable<long> CreateManyIds(int number);
    }
}
