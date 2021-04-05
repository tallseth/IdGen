namespace IdGen.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a sequence overflows (e.g. too many Id's generated within the same timespan (ms)).
    /// </summary>
    public class SequenceOverflowException : IdGenException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceOverflowException"/> class with a message that describes the error.
        /// </summary>
        public SequenceOverflowException() : base("Sequence overflow. Refusing to generate id for rest of tick") { }
    }
}