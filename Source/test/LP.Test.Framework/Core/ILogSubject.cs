using System;
using System.Reactive.Subjects;
using Serilog.Events;
using Serilog.Formatting;
using Xunit.Abstractions;

namespace LP.Test.Framework.Core
{
    public interface ILogSubject : ISubject<LogEvent>
    {

        ITextFormatter Formatter { get; set; }
        
        IDisposable Subscribe(ITestOutputHelper output);

    }
}