namespace IdGen
{
    public class DefaultSequenceGenerator : ISequenceGenerator
    {
        private readonly IdStructure _idStructure;
        private int _sequence;
        public DefaultSequenceGenerator(IdStructure idStructure)
        {
            _idStructure = idStructure;
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
            return _sequence >= _idStructure.MaxSequenceIds;
        }
    }
}