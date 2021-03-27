using System;

namespace IdGen
{
    /// <summary>
    /// The exception that is thrown when a timestamp overflows (e.g. Id's generated too long after epoch time).
    /// </summary>
    public class TimestampOverflowException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampOverflowException"/> class.
        /// </summary>
        public TimestampOverflowException() : this("Timestamp overflow") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampOverflowException"/> class with a message that describes the error.
        /// </summary>
        /// <param name="message">
        /// The message that describes the exception. The caller of this constructor is required to ensure that this 
        /// string has been localized for the current system culture.
        /// </param>
        public TimestampOverflowException(string message)
            : this(message, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampOverflowException"/> class with a message that describes
        /// the error and underlying exception.
        /// </summary>
        /// <param name="message">
        /// The message that describes the exception. The caller of this constructor is required to ensure that this 
        /// string has been localized for the current system culture.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current <see cref="TimestampOverflowException"/>. If the
        /// innerException parameter is not null, the current exception is raised in a catch block that handles the
        /// inner exception.
        /// </param>
        public TimestampOverflowException(string message, Exception? innerException) : base(message, innerException) { }
    }
}