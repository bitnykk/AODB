using System;
using System.IO;
using System.Threading;
using System.Timers;

public class LoadingBar
{
    private int _totalCount = 0;
    private int _processedCount = 0;
    private string[] _anim = new string[4]{ "|", "/", "-", "\\", };
    private int _animIndex = 0;
    private System.Timers.Timer _updateTimer;
    private readonly int _updateInterval = 200;  // Interval in milliseconds

    public LoadingBar(int totalCount)
    {
        Console.SetIn(TextReader.Null);

        _totalCount = totalCount; 
        _updateTimer = new System.Timers.Timer(_updateInterval);
        _updateTimer.Elapsed += OnTimedEvent;
        _updateTimer.AutoReset = true;  // Repeat the event
        _updateTimer.Enabled = false;  // Start the timer only when needed
        _updateTimer.Start();
    }

    private void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        if (_processedCount >= _totalCount)
        {
            _updateTimer.Stop();  // Stop the timer when done
            _processedCount = _totalCount;  // Ensure completion status
        }

        Update();  // Update the loading bar
    }

    public void Increment()
    {
        Interlocked.Increment(ref _processedCount);
    }

    private void Update()
    {
        if (_processedCount > _totalCount)
            return;

        Console.CursorVisible = false;
        Console.SetCursorPosition(0, Console.CursorTop);

        UpdateText(_processedCount, _totalCount);
        UpdateBar(_processedCount, _totalCount);
    }

    private void UpdateText(int current, int total)
    {
        float progress = (float)Math.Round(((float)current / total) * 100f, 2);  // Calculate percentage done
        Console.Write($"Item {current.ToString().PadRight(6)}   of {total.ToString().PadRight(6)}    ({progress.ToString().PadRight(5)}%)");
    }

    private void UpdateBar(int current, int total)
    {
        int barLength = 30;  // Length of the loading bar
        int filledLength = (int)((float)barLength * current / total);
        string filledBar = new string('█', filledLength);  // Create the loading bar
        string emptyBar = new string('-', barLength - filledLength);


        Console.Write($"[".PadLeft(5));
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(filledBar);
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(emptyBar);
        Console.Write($"]");

        var symb = "";

        if (current == total)
            symb = "✓";
        else
        {
            _animIndex = (_animIndex + 1) % _anim.Length;
            symb = _anim[_animIndex];
        }

        Console.Write(symb.PadLeft(2));
    }
}
