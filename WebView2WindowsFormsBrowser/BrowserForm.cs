// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace WebView2WindowsFormsBrowser
{
    public partial class BrowserForm : Form
    {
        private CoreWebView2NewWindowRequestedEventArgs _newWindowRequestedEventArgs;
        private CoreWebView2Deferral _newWindowDeferral;
        private WebView2 _webView2Control;
        private CoreWebView2Environment _environment;

        public BrowserForm()
        {
            InitializeComponent();
            InitializeWebView();            
        }

        private void InitializeWebView()
        {
            // Create the cache directory 
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string cacheFolder = Path.Combine(localAppData, "WindowsFormsWebView2");

            // Create the environment manually
            Task<CoreWebView2Environment> task = CoreWebView2Environment.CreateAsync(null, cacheFolder, null);

            // Do this so the task is continued on the UI Thread
            TaskScheduler ui = TaskScheduler.FromCurrentSynchronizationContext();

            task.ContinueWith(t =>
            {
                _environment = task.Result;

                // Create the web view 2 control and add it to the form. 
                _webView2Control = new WebView2();

                _webView2Control.Location = new System.Drawing.Point(0, 96);
                _webView2Control.Name = "webView2Control";
                _webView2Control.Size = new System.Drawing.Size(788, 410);
                _webView2Control.TabIndex = 7;
                _webView2Control.Text = "webView2Control";
                _webView2Control.CoreWebView2Ready += WebView2Control_CoreWebView2Ready;
                _webView2Control.NavigationStarting += WebView2Control_NavigationStarting;
                _webView2Control.NavigationCompleted += WebView2Control_NavigationCompleted;
                _webView2Control.SourceChanged += WebView2Control_SourceChanged;
                _webView2Control.KeyDown += WebView2Control_KeyDown;
                _webView2Control.KeyUp += WebView2Control_KeyUp;

                Controls.Add(_webView2Control);

                _webView2Control.EnsureCoreWebView2Async(_environment);
            }, ui);
        }


        public BrowserForm(CoreWebView2NewWindowRequestedEventArgs args, CoreWebView2Deferral deferral) :this()
        {
            _newWindowRequestedEventArgs = args;
            _newWindowDeferral = deferral;
        }

        private void UpdateTitleWithEvent(string message)
        {
            string currentDocumentTitle = _webView2Control?.CoreWebView2?.DocumentTitle ?? "Uninitialized";
            this.Text = currentDocumentTitle + " (" + message + ")";
        }

        #region Event Handlers
        private void WebView2Control_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            UpdateTitleWithEvent("NavigationStarting");
        }

        private void WebView2Control_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            UpdateTitleWithEvent("NavigationCompleted");
        }

        private void WebView2Control_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            txtUrl.Text = _webView2Control.Source.AbsoluteUri;
        }

        private void WebView2Control_CoreWebView2Ready(object sender, EventArgs e)
        {
            HandleResize();

            _webView2Control.Source = new Uri("https://www.bing.com/");

            _webView2Control.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;
            _webView2Control.CoreWebView2.HistoryChanged += CoreWebView2_HistoryChanged;
            _webView2Control.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
            _webView2Control.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Image);
            _webView2Control.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            UpdateTitleWithEvent("CoreWebView2Ready");

            if (_newWindowRequestedEventArgs != null)
            {
                _newWindowRequestedEventArgs.NewWindow = _webView2Control.CoreWebView2;
                _newWindowRequestedEventArgs.Handled = true;
                _newWindowDeferral.Complete();
                _newWindowRequestedEventArgs = null;
                _newWindowDeferral = null;
            }
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            CoreWebView2Deferral deferral = e.GetDeferral();
            BrowserForm newWindow = new BrowserForm(e, deferral);
            newWindow.Show();
        }

        private void WebView2Control_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateTitleWithEvent($"AcceleratorKeyUp key={e.KeyCode}");
        }

        private void WebView2Control_KeyDown(object sender, KeyEventArgs e)
        {
            UpdateTitleWithEvent($"AcceleratorKeyDown key={e.KeyCode}");
        }

        private void CoreWebView2_HistoryChanged(object sender, object e)
        {
            // No explicit check for webView2Control initialization because the events can only start
            // firing after the CoreWebView2 and its events exist for us to subscribe.
            btnBack.Enabled = _webView2Control.CoreWebView2.CanGoBack;
            btnForward.Enabled = _webView2Control.CoreWebView2.CanGoForward;
            UpdateTitleWithEvent("HistoryChanged");
        }

        private void CoreWebView2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            this.txtUrl.Text = _webView2Control.Source.AbsoluteUri;
            UpdateTitleWithEvent("SourceChanged");
        }

        private void CoreWebView2_DocumentTitleChanged(object sender, object e)
        {
            this.Text = _webView2Control.CoreWebView2.DocumentTitle;
            UpdateTitleWithEvent("DocumentTitleChanged");
        }
        #endregion

        #region UI event handlers
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            _webView2Control.Reload();
        }

        private void BtnGo_Click(object sender, EventArgs e)
        {
            _webView2Control.Source = new Uri(txtUrl.Text);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            _webView2Control.GoBack();
        }

        private void btnEvents_Click(object sender, EventArgs e)
        {
            (new EventMonitor(_webView2Control)).Show(this);
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            _webView2Control.GoForward();
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            HandleResize();
        }

        private void xToolStripMenuItem05_Click(object sender, EventArgs e)
        {
            _webView2Control.ZoomFactor = 0.5;
        }

        private void xToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _webView2Control.ZoomFactor = 1.0;
        }

        private void xToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            _webView2Control.ZoomFactor = 2.0;
        }

        private void xToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Zoom factor: {_webView2Control.ZoomFactor}", "WebView Zoom factor");
        }
        #endregion

        private void HandleResize()
        {
            if (_webView2Control == null)
                return;

            // Resize the webview
            _webView2Control.Size = this.ClientSize - new System.Drawing.Size(_webView2Control.Location);

            // Move the Events button
            btnEvents.Left = this.ClientSize.Width - btnEvents.Width;
            // Move the Go button
            btnGo.Left = this.btnEvents.Left - btnGo.Size.Width;

            // Resize the URL textbox
            txtUrl.Width = btnGo.Left - txtUrl.Left;
        }
    }
}
