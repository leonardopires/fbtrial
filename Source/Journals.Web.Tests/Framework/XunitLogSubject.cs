using System;
using System.IO;
using System.Reactive.Subjects;
using Serilog.Events;
using Serilog.Formatting;
using Xunit.Abstractions;

namespace Journals.Web.Tests.Framework
{
    /// <summary>
    /// Implements a class that listens the log subsystem for occurrences of <see cref="LogEvent"/> and writes them on Xunit test output.
    /// </summary>
    public class XunitLogSubject : SubjectBase<LogEvent>, ILogSubject
    {
        /// <summary>
        /// The underlying subject object that we use to listen and observe for events.
        /// </summary>
        private readonly Subject<LogEvent> subject = new Subject<LogEvent>();


        /// <summary>
        /// Gets or sets the formatter used to display the log information in the Xunit test interface.
        /// </summary>
        /// <value>The formatter.</value>
        public ITextFormatter Formatter { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitLogSubject"/> class.
        /// </summary>
        /// <param name="formatter">The formatter used to display the log information in the Xunit test output.</param>
        public XunitLogSubject(ITextFormatter formatter = null)
        {
            Formatter = formatter;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the subject and unsubscribes all observers.
        /// </summary>
        public override void Dispose()
        {
            subject.Dispose();
        }

        /// <summary>
        /// Notifies all subscribed observers about the end of the sequence.
        /// </summary>
        public override void OnCompleted()
        {
            subject.OnCompleted();
        }

        /// <summary>
        /// Notifies all subscribed observers about the specified exception.
        /// </summary>
        /// <param name="error">The exception to send to all currently subscribed observers.</param>
        public override void OnError(Exception error)
        {
            subject.OnError(error);
        }

        /// <summary>
        /// Notifies all subscribed observers about the arrival of the specified element in the sequence.
        /// </summary>
        /// <param name="value">The value to send to all currently subscribed observers.</param>
        public override void OnNext(LogEvent value)
        {
            subject.OnNext(value);
        }

        /// <summary>
        /// Subscribes an observer to the subject.
        /// </summary>
        /// <param name="observer">Observer to subscribe to the subject.</param>
        /// <returns>Disposable object that can be used to unsubscribe the observer from the subject.</returns>
        public override IDisposable Subscribe(IObserver<LogEvent> observer)
        {
            return subject.Subscribe(observer);
        }

        /// <summary>
        /// Indicates whether the subject has observers subscribed to it.
        /// </summary>
        /// <value><c>true</c> if this instance has observers; otherwise, <c>false</c>.</value>
        public override bool HasObservers
        {
            get { return subject.HasObservers; }
        }

        /// <summary>
        /// Indicates whether the subject has been disposed.
        /// </summary>
        /// <value><c>true</c> if this instance is disposed; otherwise, <c>false</c>.</value>
        public override bool IsDisposed
        {
            get { return subject.IsDisposed; }
        }

        /// <summary>
        /// Creates a subscription in this subject that writes in the specified <see cref="ITestOutputHelper"/> whenever <see cref="OnNext"/> is called.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <returns><see cref="IDisposable"/></returns>
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
}