﻿using System;
using System.Data;
using Smooth.IoC.Dapper.FastCRUD.Repository.UnitOfWork.Tests.TestHelpers;
using Smooth.IoC.Dapper.Repository.UnitOfWork.Data;
using StructureMap;

namespace Smooth.IoC.Dapper.FastCRUD.Repository.UnitOfWork.Tests.IoC_Example_Installers
{
    public class StructureMapRegistration
    {
        public void Register(IContainer container)
        {
            container.Configure(c=>c.For<IDbFactory>()
            .UseIfNone<StructureMapDbFactory>().Ctor<IContainer>()
            .Is(container).Singleton());
        }

        [NoIoCFluentRegistration]
        internal class StructureMapDbFactory : IDbFactory
        {
            private IContainer _container;

            public StructureMapDbFactory(IContainer container)
            {
                _container = container;
            }

            public T Create<T>() where T : class, ISession
            {
                return _container.GetInstance<T>();
            }

            public T CreateSession<T>() where T : class, ISession
            {
                return _container.GetInstance<T>();
            }

            public T Create<T>(IDbFactory factory, ISession session) where T : class, IUnitOfWork
            {
                return _container.With(factory).With(session).GetInstance<T>();
            }

            public T Create<T>(IDbFactory factory, ISession session, IsolationLevel isolationLevel) where T : class, IUnitOfWork
            {
                return _container.With(factory).With(session).With(isolationLevel).GetInstance<T>();
            }

            public void Release(IDisposable instance)
            {
                _container.Release(instance);
            }
        }
    }
}
