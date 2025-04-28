public interface IPoolable
{
    /// <summary>
    /// Detachs all parts and return all of them to resource pool
    /// </summary>
    void Destroy();
    /// <summary>
    /// Enabled after get from pool
    /// </summary>
    void OnGetFomPool();
    /// <summary>
    /// Disabled after return to pool
    /// </summary>
    void OnReturnToPool();
    /// <summary>
    /// Return to pool, can be resuse
    /// </summary>
    void Recycle();
}
