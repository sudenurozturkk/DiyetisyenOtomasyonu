using System;
using System.Collections.Generic;
using System.Linq;

namespace DiyetisyenOtomasyonu.Infrastructure.DI
{
    /// <summary>
    /// Basit Dependency Injection Container
    /// Singleton, Transient ve Scoped lifetime desteği
    /// </summary>
    public class ServiceContainer : IServiceProvider
    {
        private static ServiceContainer _instance;
        private static readonly object _lock = new object();

        private readonly Dictionary<Type, ServiceRegistration> _registrations = new Dictionary<Type, ServiceRegistration>();
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();

        private ServiceContainer() { }

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static ServiceContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ServiceContainer();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Servis kaydet
        /// </summary>
        public void Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TService : class
            where TImplementation : class, TService
        {
            var registration = new ServiceRegistration
            {
                ServiceType = typeof(TService),
                ImplementationType = typeof(TImplementation),
                Lifetime = lifetime,
                Factory = null
            };

            _registrations[typeof(TService)] = registration;
        }

        /// <summary>
        /// Servis kaydet (factory ile)
        /// </summary>
        public void Register<TService>(Func<TService> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TService : class
        {
            var registration = new ServiceRegistration
            {
                ServiceType = typeof(TService),
                ImplementationType = typeof(TService),
                Lifetime = lifetime,
                Factory = () => factory()
            };

            _registrations[typeof(TService)] = registration;
        }

        /// <summary>
        /// Instance kaydet (singleton)
        /// </summary>
        public void RegisterInstance<TService>(TService instance) where TService : class
        {
            _singletons[typeof(TService)] = instance;
            _registrations[typeof(TService)] = new ServiceRegistration
            {
                ServiceType = typeof(TService),
                ImplementationType = typeof(TService),
                Lifetime = ServiceLifetime.Singleton,
                Factory = null
            };
        }

        /// <summary>
        /// Servisi al
        /// </summary>
        public T GetService<T>() where T : class
        {
            return (T)GetService(typeof(T));
        }

        /// <summary>
        /// Servisi al (type ile)
        /// </summary>
        public object GetService(Type serviceType)
        {
            // Singleton instance kontrolü
            if (_singletons.ContainsKey(serviceType))
            {
                return _singletons[serviceType];
            }

            if (!_registrations.ContainsKey(serviceType))
            {
                throw new InvalidOperationException($"Service {serviceType.Name} is not registered");
            }

            var registration = _registrations[serviceType];

            // Factory varsa kullan
            if (registration.Factory != null)
            {
                var instance = registration.Factory();
                if (registration.Lifetime == ServiceLifetime.Singleton)
                {
                    _singletons[serviceType] = instance;
                }
                return instance;
            }

            // Reflection ile instance oluştur
            var implementationType = registration.ImplementationType;
            var constructors = implementationType.GetConstructors();

            if (constructors.Length == 0)
            {
                throw new InvalidOperationException($"No public constructor found for {implementationType.Name}");
            }

            // En uzun constructor'ı seç (en fazla dependency)
            var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
            var parameters = constructor.GetParameters();
            var args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                args[i] = GetService(paramType);
            }

            var instance2 = Activator.CreateInstance(implementationType, args);

            if (registration.Lifetime == ServiceLifetime.Singleton)
            {
                _singletons[serviceType] = instance2;
            }

            return instance2;
        }

        /// <summary>
        /// Servis kayıtlı mı?
        /// </summary>
        public bool IsRegistered<T>() where T : class
        {
            return IsRegistered(typeof(T));
        }

        /// <summary>
        /// Servis kayıtlı mı? (type ile)
        /// </summary>
        public bool IsRegistered(Type serviceType)
        {
            return _registrations.ContainsKey(serviceType) || _singletons.ContainsKey(serviceType);
        }

        /// <summary>
        /// Tüm kayıtları temizle
        /// </summary>
        public void Clear()
        {
            _registrations.Clear();
            _singletons.Clear();
        }

        private class ServiceRegistration
        {
            public Type ServiceType { get; set; }
            public Type ImplementationType { get; set; }
            public ServiceLifetime Lifetime { get; set; }
            public Func<object> Factory { get; set; }
        }
    }
}
