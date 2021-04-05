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
        /// <returns>Returns an Id based on the <see cref="IIdGenerator"/>'s epoch, generatorid and sequence.</returns>
        /// <exception cref="InvalidSystemClockException">Thrown when clock going backwards is detected.</exception>
        /// <exception cref="TimestampOverflowException">Thrown when timestamp overflows allotted bits.</exception>
        /// <exception cref="SequenceOverflowException">Thrown when sequence overflows.</exception>
        /// <remarks>Note that this method MAY throw an one of the documented exceptions.</remarks>
        long CreateId();

        /// <summary>
        /// Creates a group of Ids, given a number of desired ID's
        /// </summary>
        /// <param name="number">The number of ID's desired</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of ID's of length <param name="number"/></returns>
        /// <remarks>Note that this method MAY throw any one of the exceptions documented for <see cref="CreateId"/></remarks>
        IEnumerable<long> CreateManyIds(int number);
    }
}
