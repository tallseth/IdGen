namespace IdGen.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a timestamp overflows (e.g. Id's generated too long after epoch time).
    /// </summary>
    public class TimestampOverflowException : IdGenException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampOverflowException"/> class.
        /// </summary>
        public TimestampOverflowException() : base("Timestamp overflow") { }
    }
}