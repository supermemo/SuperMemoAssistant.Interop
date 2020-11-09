﻿#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#endregion




namespace SuperMemoAssistant.Interop.Plugins
{
  using System;
  using System.Runtime.Remoting;
  using System.Threading.Tasks;
  using System.Windows;
  using Anotar.Serilog;
  using Extensions;
  using PluginManager.Interop.Plugins;
  using Serilog;
  using Services;
  using Services.Configuration;
  using Services.IO.Diagnostics;
  using Services.IO.HotKeys;
  using Services.IO.Keyboard;
  using SuperMemo;
  using SuperMemo.Core;
  using Sys.Remoting;

  /// <inheritdoc cref="SMAPluginBase{TPlugin}" />
  public abstract class SMAPluginBase<TPlugin> : PluginBase<TPlugin, ISMAPlugin, ISuperMemoAssistant>, ISMAPlugin
    where TPlugin : SMAPluginBase<TPlugin>
  {
    #region Properties & Fields - Non-Public

    private ActionProxy<SMCollection> _onCollectionSelectedProxy;
    private ActionProxy               _onSMStartedProxy;
    private ActionProxy               _onSMStartingProxy;
    private ActionProxy               _onSMStoppedProxy;

    /// <summary>Whether this object was disposed</summary>
    protected bool IsDisposed { get; private set; }

    #endregion




    #region Constructors

    static SMAPluginBase()
    {
      RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
    }

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2214:Do not call overridable methods in constructors",
                                                     Justification = "<Pending>")]
    protected SMAPluginBase()
      : base(RemotingServicesEx.GenerateIpcServerChannelName())
    {
      try
      {
        // Required for logging
        Svc.App                 = CreateApplication();
        Svc.SharedConfiguration = new ConfigurationService(SMAFileSystem.SharedConfigDir);

        Svc.Logger = LoggerFactory.Create(AssemblyName, Svc.SharedConfiguration, ConfigureLogger);
        ReloadAnotarLogger();

        Svc.KeyboardHotKey = KeyboardHookService.Instance;
#pragma warning disable CS0618 // Type or member is obsolete
        Svc.KeyboardHotKeyLegacy = KeyboardHotKeyService.Instance;
#pragma warning restore CS0618 // Type or member is obsolete
        Svc.Configuration = new PluginConfigurationService(this);
        Svc.HotKeyManager = HotKeyManager.Instance.Initialize(Svc.Configuration, Svc.KeyboardHotKey);

        LogTo.Debug("Plugin {AssemblyName} initialized", AssemblyName);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception while initializing {Name}", GetType().Name);
        throw;
      }
    }


    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public sealed override void Dispose()
    {
      base.Dispose();
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>Implements the cleanup logic</summary>
    /// <param name="disposing">Whether this method is called by the <see cref="Dispose()" /> or the destructor</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IsDisposed)
        return;

      try
      {
        KeyboardHotKeyService.Instance.Dispose();

        Svc.App?.Dispatcher.InvokeShutdown();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Plugin threw an exception while disposing.");
      }

      Svc.Logger.Shutdown();

      IsDisposed = true;
    }

    #endregion




    #region Properties & Fields - Public

    /// <summary>The SuperMemo Assistant remote service</summary>
    public ISuperMemoAssistant SMA => Service;

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public virtual bool HasSettings => false;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    protected override void LogError(Exception ex, string message)
    {
      LogTo.Error(ex, message);
    }

    /// <inheritdoc />
    protected override void LogInformation(string message)
    {
      LogTo.Information(message);
    }

    /// <inheritdoc />
    [LogToErrorOnException]
    public override void OnInjected()
    {
      if (SessionGuid == Guid.Empty)
        throw new NullReferenceException($"{nameof(SessionGuid)} is empty");

      if (SMA == null)
        throw new NullReferenceException($"{nameof(SMA)} is null");

      if (PluginMgr == null)
        throw new NullReferenceException($"{nameof(PluginMgr)} is null");

      Svc.Plugin = this;
      Svc.SMA    = SMA;

      OnPluginInitialized();

      if (Svc.SM?.UI?.ElementWdw?.IsAvailable ?? false)
      {
        Svc.CollectionConfiguration = new CollectionConfigurationService(Svc.SM.Collection, this);

        OnSMStarted();

        return;
      }

      Svc.SMA.OnCollectionSelectedEvent += _onCollectionSelectedProxy = new ActionProxy<SMCollection>(OnCollectionSelected);
      Svc.SMA.OnSMStartedEvent          += _onSMStartedProxy          = new ActionProxy(OnSMStarted);
      Svc.SMA.OnSMStartingEvent         += _onSMStartingProxy         = new ActionProxy(OnSMStarting);
      Svc.SMA.OnSMStoppedEvent          += _onSMStoppedProxy          = new ActionProxy(OnSMStopped);
    }

    /// <inheritdoc />
    public virtual RemoteTask<object> OnMessage(PluginMessage msg, params object[] parameters)
    {
      switch (msg)
      {
        case PluginMessage.OnLoggerConfigUpdated:
          return OnLoggerConfigUpdatedAsync();

        default:
          LogTo.Debug("Received unknown message {Msg}. Is plugin up to date ?", msg);
          break;
      }

      return Task.FromResult<object>(null);
    }

    /// <inheritdoc />
    public virtual void ShowSettings()
    {
      // Ignored -- override for desired behavior
    }

    #endregion




    #region Methods

    private async Task<object> OnLoggerConfigUpdatedAsync()
    {
      try
      {
        await Svc.Logger.ReloadConfigFromFileAsync(Svc.SharedConfiguration).ConfigureAwait(false);

        return true;
      }
      catch (Exception ex)
      {
        LogTo.Warning(ex, "Exception caught while reloading logger config");
        return false;
      }
    }

    // See https://github.com/Fody/Anotar/issues/114
    private void ReloadAnotarLogger()
    {
      // SMAPluginBase's logger
      Logger.ReloadAnotarLogger<SMAPluginBase<TPlugin>>();

      // TPlugin's logger
      Logger.ReloadAnotarLogger<TPlugin>();
    }

    /// <summary>
    ///   Triggered when the collection to be loaded in SM has been selected. If overriden, make sure to call base method.
    ///   <see cref="ISuperMemoAssistant.OnCollectionSelectedEvent" />
    /// </summary>
    /// <param name="col">The selected collection</param>
    protected virtual void OnCollectionSelected(SMCollection col)
    {
      Svc.CollectionConfiguration = new CollectionConfigurationService(col, this);

      if (_onCollectionSelectedProxy != null)
      {
        Svc.SMA.OnCollectionSelectedEvent -= _onCollectionSelectedProxy;
        _onCollectionSelectedProxy        =  null;
      }
    }

    /// <summary>
    ///   Triggered when the SM process is created. If overriden, make sure to call base method.
    ///   <see cref="ISuperMemoAssistant.OnSMStartingEvent" />
    /// </summary>
    protected virtual void OnSMStarting()
    {
      if (_onSMStartingProxy != null)
      {
        Svc.SMA.OnSMStartingEvent -= _onSMStartingProxy;
        _onSMStartingProxy        =  null;
      }
    }
    
    /// <summary>
    ///   Triggered when the SM process is fully started, and the collection loaded. If overriden, make sure to call base
    ///   method. <see cref="ISuperMemoAssistant.OnSMStartedEvent" />
    /// </summary>
    protected virtual void OnSMStarted()
    {
      if (_onSMStartedProxy != null)
      {
        Svc.SMA.OnSMStartedEvent -= _onSMStartedProxy;
        _onSMStartedProxy = null;
      }
    }

    /// <summary>
    ///   Triggered when the SM process has been stopped. Make sure to provide a visual feedback for long-running tasks. If
    ///   overriden, make sure to call base method. <see cref="ISuperMemoAssistant.OnSMStoppedEvent" />
    /// </summary>
    /// <remarks>
    ///   Warning: While SMA only allows a single instance of its executable to be run, the user can open the collection that
    ///   was just closed by running the SuperMemo executable directly.
    /// </remarks>
    protected virtual void OnSMStopped()
    {
      if (_onSMStoppedProxy != null)
      {
        Svc.SMA.OnSMStoppedEvent -= _onSMStoppedProxy;
        _onSMStoppedProxy        =  null;
      }
    }

    /// <summary>
    ///   Creates the WPF application. Override to use a custom Application implementation
    /// </summary>
    /// <returns></returns>
    protected virtual Application CreateApplication()
    {
      return new PluginApp();
    }

    /// <summary>Configures the Logger. Override to customize the logger.</summary>
    /// <param name="loggerConfiguration"></param>
    /// <returns></returns>
    protected virtual LoggerConfiguration ConfigureLogger(LoggerConfiguration loggerConfiguration)
    {
      return loggerConfiguration;
    }

    #endregion




    #region Enums

    /// <summary>
    ///   Defines when to attach the debugger TODO: Debug condition is part the Interop library. Find a fix to use the DEBUG
    ///   condition of the plugin
    /// </summary>
    protected enum DebuggerAttachStrategy
    {
      /// <summary>The debugger won't be automatically attached</summary>
      Never,

      /// <summary>Automatically attach the debugger when plugin is in debug mode</summary>
      InDebugConfiguration,

      /// <summary>Always attach the debugger</summary>
      Always
    }

    #endregion
  }
}
