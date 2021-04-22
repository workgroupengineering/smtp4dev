using Chromely;
using Chromely.Core.Host;
using Gtk;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Chromely.Core;
using Chromely.Core.Configuration;
using Chromely.Core.Infrastructure;
using System.Linq;
using System.Diagnostics;

namespace Rnwood.Smtp4dev.Desktop
{
    public class DesktopApp : ChromelyBasicApp
    {
        private IServiceProvider serviceProvider;
        private IPlatform platform;
        private Thread gtkThread;
        private IChromelyWindow windowService;
        private StatusIcon statusIcon;

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            switch (ChromelyRuntime.Platform)
            {
                case ChromelyPlatform.Windows:
                    services.AddSingleton<IPlatform, WindowsPlatform>();
                    break;
                default:
                    throw new NotImplementedException($"Desktop app is not currently supported on platform {ChromelyRuntime.Platform}");
            }

        }


        public override void Initialize(IServiceProvider serviceProvider)
        {
            base.Initialize(serviceProvider);
            this.serviceProvider = serviceProvider;

            this.windowService = this.serviceProvider.GetService<IChromelyWindow>();

            this.platform = (IPlatform)serviceProvider.GetService(typeof(IPlatform));

            

            if (!Environment.GetCommandLineArgs().Any(a => a.StartsWith("--type=")))
            {
                if (Debugger.IsAttached)
                {
                   this.platform.ShowConsoleWindow();
                }


                gtkThread = new Thread(() =>
                {

                    Application.Init();
                    statusIcon = new StatusIcon("icon.svg");
                    statusIcon.TooltipText = "smtp4dev";
                    statusIcon.Activate += StatusIcon_Activate;
                    statusIcon.PopupMenu += OnTrayIconPopup;

                    this.windowService.NativeHost.HostSizeChanged += NativeHost_HostSizeChanged;
                    this.windowService.NativeHost.HostClose += NativeHost_HostClose;
                    this.windowService.NativeHost.HostCreated += NativeHost_HostCreated;


                    Application.Run();
                });
                gtkThread.Start();

            }
        }

        private void NativeHost_HostClose(object sender, CloseEventArgs e)
        {
            
        }

        private void NativeHost_HostCreated(object sender, CreatedEventArgs e)
        {
            this.platform.HideWindow(this.windowService.Handle);
        }

        private void StatusIcon_Activate(object sender, EventArgs e)
        {
            this.platform.ShowWindow(this.windowService.Handle);
            this.windowService.Restore();
        }

        private void NativeHost_HostSizeChanged(object sender, SizeChangedEventArgs e)
        {

            if (windowService.NativeHost.GetWindowState() == WindowState.Minimize)
            {
                this.platform.HideWindow(windowService.Handle);
            }
        }

        void OnTrayIconPopup(object o, PopupMenuArgs args)
        {
            Menu popupMenu = new Menu();
            ImageMenuItem menuItemShow = new ImageMenuItem("Show smtp4dev");
            menuItemShow.Image = new Gtk.Image(Stock.Open, IconSize.Menu); ;
            popupMenu.Add(menuItemShow);
            menuItemShow.Activated += StatusIcon_Activate;


            ImageMenuItem menuItemQuit = new ImageMenuItem("Quit");
            menuItemQuit.Image = new Gtk.Image(Stock.Quit, IconSize.Menu); ;
            popupMenu.Add(menuItemQuit);
            // Quit the application when quit has been clicked.
            menuItemQuit.Activated += delegate { Application.Quit(); };

            popupMenu.ShowAll();

            statusIcon.PresentMenu(popupMenu, (uint) args.Args[0],(uint) args.Args[1]);
        }


        internal static void Run(string[] args, Uri baseUrl)
        {
            var config = DefaultConfiguration.CreateForRuntimePlatform();
            config.AppName = "smtp4dev";
            config.WindowOptions = new WindowOptions
            {
                Title = "smtp4dev",
                RelativePathToIconFile = "icon.ico"
            };
            config.StartUrl = baseUrl.ToString();

            AppBuilder
            .Create()
            .UseConfig<IChromelyConfiguration>(config)
            .UseApp<DesktopApp>()
            .Build()
            .Run(args);
        }

        internal static void Exit()
        {
            Application.Invoke((s, ea) => Application.Quit());
        }
    }
}
