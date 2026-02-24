using System;

namespace DiyetisyenOtomasyonu.Infrastructure.DI
{
    /// <summary>
    /// Dependency Injection Service Provider Interface
    /// </summary>
    public interface IServiceProvider
    {
        /// <summary>
        /// Servisi al
        /// </summary>
        T GetService<T>() where T : class;

        /// <summary>
        /// Servisi al (type ile)
        /// </summary>
        object GetService(Type serviceType);

        /// <summary>
        /// Servis kayıtlı mı?
        /// </summary>
        bool IsRegistered<T>() where T : class;

        /// <summary>
        /// Servis kayıtlı mı? (type ile)
        /// </summary>
        bool IsRegistered(Type serviceType);
    }
}
