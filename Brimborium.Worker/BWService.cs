using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Text;

namespace Brimborium.Worker;

public class BWService<TOptions>
    where TOptions : class {
    protected readonly ILogger Logger;
    
    private IDisposable? _OnChangeDisposable;

    public BWService(
        IOptionsMonitor<TOptions> optionsMonitor,
        ILogger logger) {
        this.Logger = logger;
        this._OnChangeDisposable = optionsMonitor.OnChange(this.OptionsOnChange);
        this.OptionsOnChange(optionsMonitor.CurrentValue, string.Empty);
    }

    protected virtual void OptionsOnChange(TOptions options, string? name) {
    }
}
