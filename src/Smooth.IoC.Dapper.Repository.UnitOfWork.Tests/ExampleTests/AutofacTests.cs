using System;
using System.Linq;
using System.Reflection;
using Autofac;
using NUnit.Framework;
using Smooth.IoC.Dapper.FastCRUD.Repository.UnitOfWork.Tests.IoC_Example_Installers;
using Smooth.IoC.Dapper.FastCRUD.Repository.UnitOfWork.Tests.TestHelpers;
using Smooth.IoC.Dapper.Repository.UnitOfWork.Data;

namespace Smooth.IoC.Dapper.FastCRUD.Repository.UnitOfWork.Tests.ExampleTests
{
    [TestFixture]
    public class AutofacTests
    {
        private static IContainer _container;

        [SetUp]
        public void TestSetup()
        {
            if (_container == null)
            {
                var builder = new ContainerBuilder();
                Assert.DoesNotThrow(() =>
                {
                    new AutofacRegistrar().Register(builder);
                    builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).AsImplementedInterfaces()
                    .Where(t => t.GetInterfaces().Any(i=>i!=typeof(IDisposable)) && t.GetCustomAttribute<NoIoCFluentRegistration>() == null);
                    _container = builder.Build();
                });
                Assert.That(_container.IsRegistered<ITestSession>(), Is.True);
            }
        }

        [Test, Category("Integration")]
        public static void Install_1_Resolves_ISession()
        {
            var dbFactory = _container.Resolve<IDbFactory>();
            ITestSession session = null;
            Assert.DoesNotThrow(() => session = dbFactory.Create<ITestSession>());
            Assert.That(session, Is.Not.Null);
        }


        [Test, Category("Integration")]
        public static void Install_2_Resolves_IUnitOfWork()
        {
            var dbFactory = _container.Resolve<IDbFactory>();
            using (var session = dbFactory.Create<ITestSession>())
            {
                IUnitOfWork uow = null;
                Assert.DoesNotThrow(()=> uow = session.UnitOfWork());
                Assert.That(uow, Is.Not.Null);
            }
        }

        [Test, Category("Integration")]
        public static void Install_4_Resolves_WithSameConnection()
        {
            var dbFactory = _container.Resolve<IDbFactory>();
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
            Assert.DoesNotThrow(() => repo = _container.Resolve<IBraveRepository>());
            Assert.That(repo, Is.Not.Null);
        }
    }
}
