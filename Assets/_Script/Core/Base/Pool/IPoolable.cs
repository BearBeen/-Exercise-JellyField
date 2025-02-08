public interface IPoolable
{
    /// <summary>
    /// Detachs all parts and return all of them to resource pool
    /// </summary>
    void Destroy();
    /// <summary>
    /// Enabled after get from pool
    /// </summary>
    void OnEnabled();
    /// <summary>
    /// Disabled after return to pool
    /// </summary>
    void OnDisabled();
    /// <summary>
    /// Return to pool, can be resuse
    /// </summary>
    void Recycle();
}
