using System;

namespace CmdQ.Random
{
    /// <summary>
    /// Represents a pseudo-random number generator, which is a device that produces a sequence of numbers
    /// that meet certain statistical requirements for randomness.
    /// </summary>
    /// <remarks>
    /// This family of random number generators is based on PCG by M.E. O'Neill. Details available at
    /// http://www.pcg-random.org/
    /// </remarks>
    public sealed class PcgRandom : System.Random
    {
        const ulong MULTIPLIER = 6364136223846793005uL;
        const double INVERSE_MAX = 1.0 / uint.MaxValue;
        const int BYTE_WIDTH = sizeof(uint);

        readonly ulong _increment;

        ulong _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="PcgRandom"/> class, usnig a time-dependent default
        /// seed value.
        /// </summary>
        /// <param name="stream">Optional stream to select.</param>
        /// <remarks>
        /// <para>
        /// The default seed value is derived from the system clock and has finite resolution.
        /// As a result, different <see cref="PcgRandom"/> objects that are created in close succession
        /// by a call to the default constructor will have identical default seed values and,
        /// therefore, will produce identical sets of random numbers. This problem can be avoided by
        /// using a single <seealso cref="PcgRandom"/> object to generate all random numbers.
        /// You can also work around it by modifying the seed value returned by the system clock and then
        /// explicitly providing this new seed value to the <see cref="PcgRandom(ulong, ulong)"/> constructor.
        /// </para>
        /// <para>
        /// Call this constructor if you want your random number generator to generate a random sequence
        /// of numbers. To generate a fixed sequence of random numbers that will be the same for different
        /// random number generators, call the <see cref="PcgRandom(ulong)"/> constructor with a fixed seed value.
        /// This <see cref="PcgRandom"/> constructor overload is frequently used when testing apps that use
        /// random numbers.
        /// </para>
        /// <para>
        /// From http://www.pcg-random.org/rng-basics.html#multiple-streams-more-codebooks
        /// "If a random number generator is like a codebook, a generator with multiple streams is like
        /// multiple books. We could also see all those books as volumes in one single masterwork.
        /// For example, if we have a generator with a period of 2^64 that supports 2^63 streams,
        /// it's a lot like having a generator with a period of 2^127, but it may be faster and easier to use."
        /// </para>
        /// </remarks>
        public PcgRandom(ulong stream = 1442695040888963407uL)
            : this((ulong)DateTime.UtcNow.Ticks, stream)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PcgRandom"/> class, using the specified seed value.
        /// </summary>
        /// <param name="seed">A number used to calculate a starting value for the pseudo-random number sequence.</param>
        /// <param name="stream">Optional stream to select.</param>
        /// <remarks>
        /// <para>
        /// Providing an identical <paramref name="seed"/> value to different <see cref="PcgRandom"/> objects causes each
        /// instance to produce identical sequences of random numbers.
        /// This is often done when testing apps that rely on random number generators.
        /// </para>
        /// <para>
        /// From http://www.pcg-random.org/rng-basics.html#multiple-streams-more-codebooks
        /// "If a random number generator is like a codebook, a generator with multiple streams is like
        /// multiple books. We could also see all those books as volumes in one single masterwork.
        /// For example, if we have a generator with a period of 2^64 that supports 2^63 streams,
        /// it's a lot like having a generator with a period of 2^127, but it may be faster and easier to use."
        /// </para>
        /// </remarks>
        public PcgRandom(ulong seed, ulong stream = 1442695040888963407uL)
        {
            _state = seed;
            // Make sure that the increment is odd.
            _increment = stream | 1uL;
        }

        /// <summary>
        /// Get the next random number.
        /// </summary>
        /// <returns>A random number from <c>0</c> to <see cref="uint.MaxValue"/> inclusive.</returns>
        public uint NextUnsigned()
        {
            unchecked
            {
                var oldstate = _state;
                _state = oldstate * MULTIPLIER + _increment;
                var xorShifted = (uint)(((oldstate >> 18) ^ oldstate) >> 27);
                var rot = (int)(oldstate >> 59);
                return (xorShifted >> rot) | (xorShifted << ((-rot) & 31));
            }
        }

        /// <summary>
        /// Get the next random number below a given threshold.
        /// </summary>
        /// <param name="maxValue">Exclusive upper bound.</param>
        /// <returns>A random <see cref="uint"/> less than <paramref name="maxValue"/>.</returns>
        /// <remarks>
        /// Care has been taken to eliminate modulo bias
        /// (see http://funloop.org/post/2015-02-27-removing-modulo-bias-redux.html).
        /// </remarks>
        public uint NextUnsigned(uint maxValue)
        {
            if (maxValue == 0)
            {
                return 0;
            }

            var threshold = (uint.MaxValue - maxValue) % maxValue;

            for (; ; )
            {
                var rand = NextUnsigned();
                if (rand >= threshold)
                {
                    return rand % maxValue;
                }
            }
        }

        #region Overrides

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="F:System.Int32.MaxValue" />.</returns>
        /// <filterpriority>1</filterpriority>
        public override int Next() => (int)(NextUnsigned() >> 1);

        /// <summary>Returns a random integer that is less than the specified maximum.</summary>
        /// <returns>A 32-bit signed integer greater than or equal to 0 and less than <paramref name="maxValue" />;
        /// that is, the range of return values includes 0 but not <paramref name="maxValue" />.
        /// However, if <paramref name="maxValue"/> equals 0, <paramref name="maxValue"/> is returned.</returns>
        /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0. </exception>
        /// <filterpriority>1</filterpriority>
        public override int Next(int maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue), $"{maxValue} is less than 0.");
            }

            return (int)NextUnsigned((uint)maxValue);
        }

        /// <summary>Returns a random integer that is within a specified range.</summary>
        /// <returns>A 32-bit signed integer greater than or equal to <paramref name="minValue" /> and less than <paramref name="maxValue" />;
        /// that is, the range of return values includes <paramref name="minValue" /> but not <paramref name="maxValue" />.
        /// If <paramref name="minValue" /> equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.</returns>
        /// <param name="minValue">The inclusive lower bound of the random number returned. </param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue" /> must be greater than
        /// or equal to <paramref name="minValue" />. </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="minValue" /> is greater than <paramref name="maxValue" />. </exception>
        /// <filterpriority>1</filterpriority>
        public override int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue), $"{nameof(minValue)} is greater than {nameof(maxValue)}.");
            }

            var range = (uint)maxValue - minValue;
            return (int)(NextUnsigned((uint)((uint)maxValue - minValue)) + minValue);
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">An array of bytes to contain random numbers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            uint pack;
            int i;
            for (i = 0; i < buffer.Length - BYTE_WIDTH; i += BYTE_WIDTH)
            {
                pack = NextUnsigned();
                buffer[i + 0] = (byte)(pack & 0xFF);
                pack >>= 8;
                buffer[i + 1] = (byte)(pack & 0xFF);
                pack >>= 8;
                buffer[i + 2] = (byte)(pack & 0xFF);
                pack >>= 8;
                buffer[i + 3] = (byte)(pack & 0xFF);
            }

            for (pack = NextUnsigned(); i < buffer.Length; ++i, pack >>= 8)
            {
                buffer[i] = (byte)(pack & 0xFF);
            }
        }

        /// <summary>Returns a random floating-point number between 0.0 and 1.0.</summary>
        /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        /// <remarks>The actual upper bound of the number returned by this method is 0.99999999976716936.</remarks>
        protected override double Sample()
        {
            for (; ; )
            {
                var re = NextUnsigned();
                if (re != uint.MaxValue)
                {
                    return INVERSE_MAX * re;
                }
            }
        }

        #endregion
    }
}
