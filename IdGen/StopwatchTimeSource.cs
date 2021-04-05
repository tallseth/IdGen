using System;
using System.Diagnostics;

namespace IdGen
{
    /// <summary>
    /// Provides time data to an <see cref="IdGenerator"/>. This timesource uses a <see cref="Stopwatch"/> for timekeeping.
    /// </summary>
    public class StopwatchTimeSource : ITimeSource
    {
        private static readonly Stopwatch _sw = new Stopwatch();
        private static readonly DateTimeOffset _initialized = DateTimeOffset.UtcNow;
        
        private readonly TimeSpan _offset;
        
        private StopwatchTimeSource(DateTimeOffset epoch, TimeSpan tickDuration)
        {
            Epoch = epoch;
            _offset = (_initialized - Epoch);
            TickDuration = tickDuration;

            // Start (or resume) stopwatch
            _sw.Start();
        }
        
        /// <summary>
        /// Initializes a new <see cref="ITimeSource"/> object.
        /// </summary>
        /// <param name="epoch">The epoch to use as an offset from now,</param>
        /// <param name="tickDuration">The duration of a single tick for this timesource.</param>
        public static ITimeSource GetInstance(DateTimeOffset epoch, TimeSpan tickDuration)
        {
            return new StopwatchTimeSource(epoch, tickDuration);
        }

        /// <summary>
        /// Initializes a new <see cref="ITimeSource"/> object with a millisecond TickDuration
        /// </summary>
        /// <param name="epoch">The epoch to use as an offset from now,</param>
        public static ITimeSource GetInstance(DateTimeOffset epoch)
        {
            return GetInstance(epoch, TimeSpan.FromMilliseconds(1));
        }

        /// <summary>
        /// Gets the epoch of the <see cref="ITimeSource"/>.
        /// </summary>
        public DateTimeOffset Epoch { get; }

        /// <summary>
        /// Returns the duration of a single tick.
        /// </summary>
        public TimeSpan TickDuration { get; }

        /// <summary>
        /// Returns the current number of ticks for the <see cref="DefaultTimeSource"/>.
        /// </summary>
        /// <returns>The current number of ticks to be used by an <see cref="IdGenerator"/> when creating an Id.</returns>
        public long GetTicks() => (_offset.Ticks + _sw.Elapsed.Ticks) / TickDuration.Ticks;
    }
}
