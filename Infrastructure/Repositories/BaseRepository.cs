using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    /// <summary>
    /// Temel Repository sınıfı - Ortak veritabanı işlemleri
    /// </summary>
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly string _tableName;

        protected BaseRepository(string tableName)
        {
            _tableName = tableName;
        }

        protected IDbConnection CreateConnection()
        {
            return DatabaseConfig.Instance.CreateConnection();
        }

        /// <summary>
        /// DataReader'dan Entity oluşturur
        /// </summary>
        protected abstract T MapFromReader(IDataReader reader);

        /// <summary>
        /// Entity'den parametre dictionary oluşturur (INSERT/UPDATE için)
        /// </summary>
        protected abstract Dictionary<string, object> MapToParameters(T entity);

        /// <summary>
        /// INSERT SQL ifadesi
        /// </summary>
        protected abstract string GetInsertSql();

        /// <summary>
        /// UPDATE SQL ifadesi
        /// </summary>
        protected abstract string GetUpdateSql();

        public virtual T GetById(int id)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = $"SELECT * FROM {_tableName} WHERE Id = @id";
                    AddParameter(cmd, "@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }

        public virtual IEnumerable<T> GetAll()
        {
            var list = new List<T>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = $"SELECT * FROM {_tableName}";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            return list;
        }

        public virtual IEnumerable<T> Find(Func<T, bool> predicate)
        {
            return GetAll().Where(predicate);
        }

        public virtual T FirstOrDefault(Func<T, bool> predicate)
        {
            return GetAll().FirstOrDefault(predicate);
        }

        public virtual int Add(T entity)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = GetInsertSql() + "; SELECT LAST_INSERT_ID();";
                    
                    var parameters = MapToParameters(entity);
                    foreach (var param in parameters)
                    {
                        AddParameter(cmd, "@" + param.Key, param.Value);
                    }

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public virtual bool Update(T entity)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = GetUpdateSql();
                    
                    var parameters = MapToParameters(entity);
                    foreach (var param in parameters)
                    {
                        AddParameter(cmd, "@" + param.Key, param.Value);
                    }

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public virtual bool Delete(int id)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = $"DELETE FROM {_tableName} WHERE Id = @id";
                    AddParameter(cmd, "@id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public virtual int Count()
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = $"SELECT COUNT(*) FROM {_tableName}";
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public virtual int Count(Func<T, bool> predicate)
        {
            return GetAll().Count(predicate);
        }

        public virtual bool Any(Func<T, bool> predicate)
        {
            return GetAll().Any(predicate);
        }

        /// <summary>
        /// Özel SQL sorgusu çalıştırır
        /// </summary>
        protected IEnumerable<T> ExecuteQuery(string sql, Dictionary<string, object> parameters = null)
        {
            var list = new List<T>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            AddParameter(cmd, "@" + param.Key, param.Value);
                        }
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Scalar değer döndüren sorgu çalıştırır
        /// </summary>
        protected object ExecuteScalar(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            AddParameter(cmd, "@" + param.Key, param.Value);
                        }
                    }

                    return cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// NonQuery sorgu çalıştırır
        /// </summary>
        protected int ExecuteNonQuery(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            AddParameter(cmd, "@" + param.Key, param.Value);
                        }
                    }

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Parametre ekler
        /// </summary>
        protected void AddParameter(IDbCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(param);
        }
    }
}

