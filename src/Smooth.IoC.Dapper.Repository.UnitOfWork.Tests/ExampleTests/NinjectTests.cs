﻿using System;
using System.Data;
using System.Linq;
using System.Reflection;
using Dapper.FastCrud;
using Ninject;
using Ninject.Extensions.Conventions;
using NUnit.Framework;
using Smooth.IoC.Dapper.FastCRUD.Repository.UnitOfWork.Tests.IoC_Example_Installers;
using Smooth.IoC.Dapper.FastCRUD.Repository.UnitOfWork.Tests.TestHelpers;
using Smooth.IoC.Dapper.Repository.UnitOfWork.Data;

namespace Smooth.IoC.Dapper.FastCRUD.Repository.UnitOfWork.Tests.ExampleTests
{
    [TestFixture]
    public class NinjectTests
    {
        private static IKernel _kernel;

        [SetUp]
        public void TestSetup()
        {
            if (_kernel == null)
            {
                _kernel = new StandardKernel();
                Assert.DoesNotThrow(() =>
                {
                    _kernel.Bind(x =>
                    {
                        x.FromThisAssembly()
                            .SelectAllClasses()
                            .Where(t => t.GetInterfaces().Any(i => i != typeof(IDisposable)) && t.GetCustomAttribute<NoIoCFluentRegistration>() == null)
                            .BindDefaultInterface()
                            .Configure(c=>c.InTransientScope());

                    });
                    new NinjectBinder().Bind(_kernel);
                });
            }
        }

        [Test, Category("Integration")]
        public static void Install_1_Resolves_ISession()
        {
            var dbFactory = _kernel.Get<IDbFactory>();
            ITestSession session = null;
            Assert.DoesNotThrow(() => session = dbFactory.Create<ITestSession>());
            Assert.DoesNotThrow(() => session.Dispose());
            Assert.That(session, Is.Not.Null);
        }


        [Test, Category("Integration")]
        public static void Install_2a_Resolves_IUnitOfWork()
        {
            var dbFactory = _kernel.Get<IDbFactory>();
            using (var session = dbFactory.Create<ITestSession>())
            {
                IUnitOfWork uow = null;
                Assert.DoesNotThrow(()=> uow = session.UnitOfWork());
                Assert.That(uow, Is.Not.Null);
            }
        }
        [Test, Category("Integration")]
        public static void Install_2b_Resolves_IUnitOfWorkWithIsolation()
        {
            var dbFactory = _kernel.Get<IDbFactory>();
            using (var session = dbFactory.Create<ITestSession>())
            {
                IUnitOfWork uow = null;
                Assert.DoesNotThrow(() => uow = session.UnitOfWork(IsolationLevel.Serializable));
                Assert.That(uow, Is.Not.Null);
            }
        }

        [Test, Category("Integration")]
        public static void Install_4_Resolves_WithSameConnection()
        {
            var dbFactory = _kernel.Get<IDbFactory>();
            using (var session = dbFactory.Create<ITestSession>())
            {
                using (var uow = session.UnitOfWork())
                {
                    Assert.That(uow.Connection, Is.EqualTo(session.Connection));
                }
            }
        }

        [Test, Category("Integration")]
        public static void Install_5_Resolves_IBravoRepository()
        {
            IBraveRepository repo = null;
            Assert.DoesNotThrow(() => repo = _kernel.Get<IBraveRepository>());
            Assert.That(repo, Is.Not.Null);
        }
    }
}


