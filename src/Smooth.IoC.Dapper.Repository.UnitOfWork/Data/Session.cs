﻿using System;
using System.Data;
using Dapper.FastCrud;
using Smooth.IoC.Dapper.Repository.UnitOfWork.Helpers;

namespace Smooth.IoC.Dapper.Repository.UnitOfWork.Data
{
    public abstract class Session<TConnection> : DbConnection , ISession
        where TConnection : System.Data.Common.DbConnection
    {
        private readonly IDbFactory _factory;
        public SqlDialect SqlDialect { get; private set; }

        protected Session(IDbFactory factory, string connectionString) : base(factory)
        {
            _factory = factory;
            SetDialect();
            if (factory != null && !string.IsNullOrWhiteSpace(connectionString))
            {
                Connect(connectionString);
            }
        }

        private void SetDialect()
        {
            var type = typeof(TConnection).FullName.ToLowerInvariant();
            if (type.Contains(".sqlclient") || type.Contains(".mssql"))
            {
                SqlDialect = SqlDialect.MsSql;
            }
            else if (type.Contains(".sqlite"))
            {
                SqlDialect = SqlDialect.SqLite;
            }
            else if (type.Contains(".mysqlclient") || type.Contains(".mysql"))
            {
                SqlDialect = SqlDialect.MySql;
            }
            else if (type.Contains(".pgsql")|| type.Contains(".postgresql"))
            {
                SqlDialect = SqlDialect.PostgreSql;
            }
            else 
            {
                SqlDialect = SqlDialect.MsSql;
            }
        }

        protected void Connect(string connectionString)
        {
            if (Connection != null)
            {
                return;
            }
            Connection = CreateInstanceHelper.Resolve<TConnection>(connectionString);
            Connection?.Open();
        }

        public IUnitOfWork UnitOfWork()
        {
            var uow= _factory.Create<IUnitOfWork>(_factory, this);
            uow.SqlDialect = SqlDialect;
            return uow;
        }

        public IUnitOfWork UnitOfWork(IsolationLevel isolationLevel)
        {
            var uow = _factory.Create<IUnitOfWork>(_factory, this, isolationLevel);
            uow.SqlDialect = SqlDialect;
            return uow;
        }

        public Guid Guid { get; } = Guid.NewGuid();
    }
}
