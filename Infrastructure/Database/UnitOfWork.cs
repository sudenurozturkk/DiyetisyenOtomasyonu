using System;
using System.Data;
using DiyetisyenOtomasyonu.Infrastructure.Database;
using DiyetisyenOtomasyonu.Infrastructure.Exceptions;

namespace DiyetisyenOtomasyonu.Infrastructure.Database
{
    /// <summary>
    /// Unit of Work Pattern
    /// Transaction yönetimi ve veri bütünlüğü için
    /// </summary>
    public class UnitOfWork : IDisposable
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private bool _disposed = false;

        public UnitOfWork()
        {
            _connection = DatabaseConfig.Instance.CreateConnection();
            _transaction = _connection.BeginTransaction();
        }

        /// <summary>
        /// Transaction içindeki connection
        /// </summary>
        public IDbConnection Connection => _connection;

        /// <summary>
        /// Transaction
        /// </summary>
        public IDbTransaction Transaction => _transaction;

        /// <summary>
        /// Değişiklikleri kaydet (commit)
        /// </summary>
        public void Commit()
        {
            try
            {
                if (_transaction != null)
                {
                    _transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                Rollback();
                throw new DatabaseException("Transaction commit hatası", ex);
            }
        }

        /// <summary>
        /// Değişiklikleri geri al (rollback)
        /// </summary>
        public void Rollback()
        {
            try
            {
                if (_transaction != null)
                {
                    _transaction.Rollback();
                }
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Transaction rollback hatası", ex);
            }
        }

        /// <summary>
        /// Parametre ekle
        /// </summary>
        public IDbDataParameter CreateParameter(string name, object value)
        {
            var param = _connection.CreateCommand().CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            return param;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
