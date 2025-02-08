
public class Singleton<T> where T : new()
{

    private static T _instance;
    private static readonly object m_Lock = new object();

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (m_Lock)
                {
                    if (_instance == null)
                    {
                    }
                    _instance = new T();
                }
            }
            return _instance;
        }
    }
}