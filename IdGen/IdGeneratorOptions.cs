using System;

namespace IdGen
{
    /// <summary>
    /// Represents the options an <see cref="IdGenerator"/> can be configured with.
    /// </summary>
    public class IdGeneratorOptions
    {
        /// <summary>
        /// Returns the default epoch.
        /// </summary>
        public static readonly DateTime DefaultEpoch = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Returns a default instance of <see cref="IdGeneratorOptions"/>.
        /// </summary>
        public static readonly IdGeneratorOptions Default = new IdGeneratorOptions();

        /// <summary>
        /// Gets the <see cref="IdStructure"/> of the generated ID's
        /// </summary>
        public IdStructure IdStructure { get; }

        /// <summary>
        /// Gets the <see cref="ITimeSource"/> to use when generating ID's.
        /// </summary>
        public ITimeSource TimeSource { get; }
        
        /// <summary>
        /// Gets the <see cref="ISequenceGenerator"/> to use when generating ID's.
        /// </summary>
        public ISequenceGenerator SequenceGenerator { get; }

        /// <summary>
        /// Gets the <see cref="SequenceOverflowStrategy"/> to use when generating ID's.
        /// </summary>
        public SequenceOverflowStrategy SequenceOverflowStrategy { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGeneratorOptions"/> class.
        /// </summary>
        /// <param name="idStructure">The <see cref="IdStructure"/> for ID's to be generated.</param>
        /// <param name="timeSource">The <see cref="ITimeSource"/> to use when generating ID's.</param>
        /// <param name="sequenceOverflowStrategy">The <see cref="SequenceOverflowStrategy"/> to use when generating ID's.</param>
        /// <param name="sequenceGenerator">The <see cref="ISequenceGenerator"/> to use when generating ID's.</param>
        public IdGeneratorOptions(
            IdStructure? idStructure = null,
            ITimeSource? timeSource = null,
            SequenceOverflowStrategy sequenceOverflowStrategy = SequenceOverflowStrategy.Throw,
            ISequenceGenerator? sequenceGenerator = null)
        {
            IdStructure = idStructure ?? IdStructure.Default;
            TimeSource = timeSource ?? new DefaultTimeSource(DefaultEpoch);
            SequenceOverflowStrategy = sequenceOverflowStrategy;
            SequenceGenerator = sequenceGenerator ?? new SequenceGenerator(IdStructure);
        }
    }
}
