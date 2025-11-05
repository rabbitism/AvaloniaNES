using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using AvaloniaNES.Device.BUS;
using AvaloniaNES.Models;
using AvaloniaNES.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaNES.Views;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _renderTimer;
    private readonly NESStatus _status = App.Services.GetRequiredService<NESStatus>();
    private readonly Bus _bus = App.Services.GetRequiredService<Bus>();
    public MainWindow()
    {
        InitializeComponent();
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
        
        m_Video.Source = _bus.PPU!.GetScreen();
        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // 60FPS
        };
        _renderTimer.Tick += OnRenderFrame;
        _renderTimer.Start();

        Task.Run(() =>
        {
            while (true)
            {
                if (_status.HasLoadRom)
                {
                    // Run Clock
                    if (_status.BusState == BUS_STATE.RUN)
                    {
                        do
                        {
                            _bus.Clock();
                        } while (!_bus.PPU!.FrameCompleted);
                        _bus.PPU!.FrameCompleted = false;
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Environment.Exit(0);
    }

    private void OnRenderFrame(object? sender, EventArgs e)
    {
        if (_status.HasLoadRom)
        {
            // update image
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                // method 1
                //var temp = m_Video.Source;
                // m_Video.Source = null;
                // m_Video.Source = _bus.PPU!.GetScreen();
            
                // method 2
                m_Video.InvalidateMeasure();
                m_Video.InvalidateArrange();
                m_Video.InvalidateVisual();
            }, DispatcherPriority.Render);
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.HandleKeyDown(e.Key);
        }
    }
    
    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.HandleKeyUp(e.Key);
        }
    }
}