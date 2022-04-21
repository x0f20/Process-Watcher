using System;
using System.Diagnostics;
using System.Timers;

/// <summary>
/// Provide a service to watch a process when they start
/// </summary>
public class ProcessWatcher : IDisposable
{
    #region Events

    /// <summary>
    /// A event that invoke when the target process has been started
    /// </summary>
    public event EventHandler<Process> OnProcessStart;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the process timer
    /// </summary>
    private Timer ProcessTimer { get; set; }

    /// <summary>
    /// Check if the processwatcher is disposed or not
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Check if the processwatcher is initialized or not
    /// </summary>
    public bool Initialized { get; private set; }

    /// <summary>
    /// Set or get the interval for the timer
    /// </summary>
    public double Interval { get; set; }

    /// <summary>
    /// Get the target process name
    /// </summary>
    public readonly string ProcessName;

    #endregion

    #region Constructor

    /// <summary>
    /// The constructor for <see cref="ProcessWatcher"/> class
    /// </summary>
    /// <param name="processName">the target process name</param>
    public ProcessWatcher(string processName)
    {
        ProcessName = processName;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Initialize the process watcher
    /// </summary>
    public void Init()
    {
        if (!Initialized)
        {
            ProcessTimer = new Timer()
            {
                Interval = this.Interval
            };

            ProcessTimer.Elapsed += ProcessTimer_Elapsed;
            Initialized = true;
        }
    }

    /// <summary>
    /// Start the process watcher
    /// </summary>
    public void Start()
    {
        if (!Initialized)
            return;

        ProcessTimer.Start();
    }

    /// <summary>
    /// Releases all the resources used in the process watcher
    /// </summary>
    public void Dispose()
    {
        if (Initialized && !Disposed)
        {
            ProcessTimer.Stop();
            ProcessTimer.Dispose();
            GC.SuppressFinalize(this);

            Disposed = true;
            Initialized = false;
        }
    }

    private void ProcessTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        Process[] TargetProc = Process.GetProcessesByName(ProcessName);

        if (TargetProc.Length < 1 ||
            TargetProc[0].MainWindowHandle == IntPtr.Zero)
            return;

        Process TargetedProc = TargetProc[0];
        TargetedProc.EnableRaisingEvents = true;
        TargetedProc.Exited += (_, __) => ProcessTimer.Start();

        OnProcessStart?.Invoke(sender, TargetedProc);
        ProcessTimer.Stop();
    }

    #endregion
}