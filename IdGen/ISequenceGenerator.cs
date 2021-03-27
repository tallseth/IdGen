namespace IdGen
{
    public interface ISequenceGenerator
    {
        int GetNextValue();
        void Reset();
        bool IsExhausted();
    }
}