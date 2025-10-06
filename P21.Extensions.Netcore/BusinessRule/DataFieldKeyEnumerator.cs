using System.Collections;

namespace P21.Extensions.BusinessRule;

[Serializable]
public class DataFieldKeyEnumerator : IEnumerator
{
    public DataFieldKey[] keys;
    private int position = -1;

    public DataFieldKeyEnumerator(DataFieldKey[] fields) => keys = fields;

    public bool MoveNext()
    {
        ++position;
        return position < keys.Length;
    }

    public void Reset() => position = -1;

    object IEnumerator.Current => Current;

    public DataFieldKey Current
    {
        get
        {
            try
            {
                return keys[position];
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
