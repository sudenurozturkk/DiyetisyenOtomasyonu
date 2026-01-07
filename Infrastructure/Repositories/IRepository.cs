using System;
using System.Collections.Generic;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    /// <summary>
    /// Generic Repository Interface - CRUD işlemleri için temel arayüz
    /// 
    /// OOP Principle: Abstraction - Veri erişim detayları soyutlanır
    /// OOP Principle: Interface Segregation - Minimal ve odaklı arayüz
    /// Design Pattern: Repository Pattern - Data access logic separation
    /// SOLID: Dependency Inversion - High-level modules depend on abstractions
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// ID'ye göre entity getirir
        /// </summary>
        T GetById(int id);

        /// <summary>
        /// Tüm entity'leri getirir
        /// </summary>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Koşula göre entity'leri filtreler
        /// </summary>
        IEnumerable<T> Find(Func<T, bool> predicate);

        /// <summary>
        /// Koşula uyan ilk entity'yi getirir
        /// </summary>
        T FirstOrDefault(Func<T, bool> predicate);

        /// <summary>
        /// Yeni entity ekler
        /// </summary>
        int Add(T entity);

        /// <summary>
        /// Entity günceller
        /// </summary>
        bool Update(T entity);

        /// <summary>
        /// Entity siler
        /// </summary>
        bool Delete(int id);

        /// <summary>
        /// Kayıt sayısını döndürür
        /// </summary>
        int Count();

        /// <summary>
        /// Koşula göre kayıt sayısını döndürür
        /// </summary>
        int Count(Func<T, bool> predicate);

        /// <summary>
        /// Koşula uyan kayıt var mı kontrol eder
        /// </summary>
        bool Any(Func<T, bool> predicate);
    }
}

