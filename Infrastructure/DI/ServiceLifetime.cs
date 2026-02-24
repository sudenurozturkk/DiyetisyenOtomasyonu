namespace DiyetisyenOtomasyonu.Infrastructure.DI
{
    /// <summary>
    /// Service lifetime tipleri
    /// </summary>
    public enum ServiceLifetime
    {
        /// <summary>
        /// Her istekte yeni instance
        /// </summary>
        Transient,

        /// <summary>
        /// Scope başına bir instance
        /// </summary>
        Scoped,

        /// <summary>
        /// Uygulama boyunca tek instance (Singleton)
        /// </summary>
        Singleton
    }
}
