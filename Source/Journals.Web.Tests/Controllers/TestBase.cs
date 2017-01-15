using System;
using System.IO;
using System.Reactive;
using System.Reactive.Subjects;
using Autofac;
using AutofacSerilogIntegration;
using Serilog;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Xunit.Abstractions;

namespace Journals.Web.Tests.Controllers
{
    public interface ILogObserver
    {

        ITextFormatter Formatter { get; set; }

        bool HasObservers { get; }

        bool IsDisposed { get; }

        void Dispose();

        void OnCompleted();

        void OnError(Exception error);

        void OnNext(LogEvent value);

        IDisposable Subscribe(IObserver<LogEvent> observer);

        IDisposable Subscribe(ITestOutputHelper output);

    }

    public class XunitLogObserver : SubjectBase<LogEvent>, ILogObserver
    {
        private readonly ITextFormatter defaultFormatter = new MessageTemplateTextFormatter(
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}", null);

        private readonly Subject<LogEvent> subject = new Subject<LogEvent>();


        public ITextFormatter Formatter { get; set; }

        public XunitLogObserver(ITextFormatter formatter = null)
        {
            if (formatter == null)
            {
                Formatter = defaultFormatter;
            }
        }

        public override void Dispose()
        {
            subject.Dispose();
        }

        public override void OnCompleted()
        {
            subject.OnCompleted();
        }

        public override void OnError(Exception error)
        {
            subject.OnError(error);
        }

        public override void OnNext(LogEvent value)
        {
            subject.OnNext(value);
        }

        public override IDisposable Subscribe(IObserver<LogEvent> observer)
        {
            return subject.Subscribe(observer);
        }

        public override bool HasObservers
        {
            get { return subject.HasObservers; }
        }

        public override bool IsDisposed
        {
            get { return subject.IsDisposed; }
        }

        public IDisposable Subscribe(ITestOutputHelper output)
        {
            return this.Subscribe(
                logEvent =>
                {

                    using (StringWriter writer = new StringWriter())
                    {
                        Formatter.Format(logEvent, writer);
                        output.WriteLine(writer.ToString());
                    }
                });
        }

    }

    public abstract class TestBase : IDisposable
    {
        
        private readonly Lazy<ILifetimeScope> _containerLazy;

        protected abstract void InitializeContainer(ContainerBuilder builder);

        
        [ThreadStatic]
        private static IContainer _rootContainer;

        protected static IContainer RootContainer
        {
            get
            {
                if (_rootContainer == null)
                {
                    var builder = new ContainerBuilder();

                    builder.RegisterModule<MocksModule>();
                    builder.RegisterType<XunitLogObserver>().As<ILogObserver>().SingleInstance();

                    builder.Register(c => new LoggerConfiguration()
                        .WriteTo.Observers(o => o.Subscribe(c.Resolve<ILogObserver>().OnNext))
                        .Enrich.FromLogContext()
                        .MinimumLevel.Verbose()
                        .CreateLogger()).As<ILogger>();

                    _rootContainer = builder.Build();
                }
                return _rootContainer;
            }
        }

        protected ILifetimeScope Container
        {
            get
            {
                var container = _containerLazy.Value;

                return container;
            }
        }


        private IDisposable _testSubscription = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestBase" /> class.
        /// </summary>
        protected TestBase(ITestOutputHelper output)
        {
            _containerLazy = new Lazy<ILifetimeScope>(CreateContainer);

            _testSubscription = Container.Resolve<ILogObserver>().Subscribe(output);
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
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            Container.Dispose();

            _testSubscription?.Dispose();
        }

    }
}