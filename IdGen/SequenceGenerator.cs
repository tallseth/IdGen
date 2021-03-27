namespace IdGen
{
    public class SequenceGenerator : ISequenceGenerator
    {
        private int _sequence;
        private readonly long MASK_SEQUENCE;
        public SequenceGenerator(long maskSequence)
        {
            MASK_SEQUENCE = maskSequence;
        }

        public int GetNextValue()
        {
            return _sequence++;
        }

        public void Reset()
        {
            _sequence = 0;
        }

        public bool IsExhausted()
        {
            return _sequence > MASK_SEQUENCE;
        }
    }
}