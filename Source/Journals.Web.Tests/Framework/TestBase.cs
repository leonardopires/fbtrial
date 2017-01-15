﻿using System;
using System.Reflection;
using System.Threading;
using Autofac;
using Serilog;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Xunit.Abstractions;

namespace Journals.Web.Tests.Framework
{
    public abstract class TestBase : IDisposable
    {
        
        private readonly Lazy<ILifetimeScope> container;

        protected abstract void InitializeContainer(ContainerBuilder builder);

        private static int _instanceCount = 0;

        
        private static readonly RecyclableLazy<IContainer> rootContainer = new RecyclableLazy<IContainer>(() =>
        {
            var builder = new ContainerBuilder();

            builder.RegisterAssemblyModules<InitializationModule>(Assembly.GetExecutingAssembly());

            builder.Register(
                        c =>
                            new MessageTemplateTextFormatter(
                                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}",
                                null))
                    .As<ITextFormatter>();

            builder.RegisterType<XunitLogSubject>().As<ILogSubject>().SingleInstance();

            builder.Register(
                        c => new LoggerConfiguration()
                            .WriteTo.Observers(o => o.Subscribe(c.Resolve<ILogSubject>().OnNext))
                            .Enrich.FromLogContext()
                            .MinimumLevel.Verbose()
                            .CreateLogger()).As<ILogger>();

            return builder.Build();
        });

        protected static IContainer RootContainer => rootContainer.Value;

        protected ILifetimeScope Container => this.container.Value;


        private readonly IDisposable _testSubscription = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestBase" /> class.
        /// </summary>
        protected TestBase(ITestOutputHelper output)
        {
            Interlocked.Increment(ref _instanceCount);

            container = new Lazy<ILifetimeScope>(CreateContainer);

            _testSubscription = Container.Resolve<ILogSubject>().Subscribe(output);
        }

        /// <summary>
        /// Creates the container.
        /// </summary>
        /// <returns>
        ///   <see cref="Autofac.IContainer" />
        /// </returns>
        private ILifetimeScope CreateContainer()
        {
            return RootContainer.BeginLifetimeScope(InitializeContainer);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            Container.Dispose();
            _testSubscription?.Dispose();

            int count = Interlocked.Decrement(ref _instanceCount);

            if (count == 0)
            {
                rootContainer.Reset();
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

    }
}