using System;
using System.Reflection;
using System.Web;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.VisualStudio.CommandBars;

namespace VSEncoderizer
{
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2
    {
        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
        }

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            _application = (DTE2)application;
            _addInInstance = (AddIn)addInInst;

            _contextMenu = ((CommandBars)_application.CommandBars)["Code Window"];
            _encoderizer = (CommandBarPopup)_contextMenu.Controls.Add(MsoControlType.msoControlPopup, Missing.Value, Missing.Value, 1, true);
            _encoderizer.Caption = "VS Encoderizer";

            _javascriptStringEncode = _encoderizer.Controls.Add(MsoControlType.msoControlButton, Missing.Value, Missing.Value, 1, true);
            _javascriptStringEncode.Caption = "Javascript String Encode";
            _javascriptStringEncodeEvents = (CommandBarEvents)_application.Events.CommandBarEvents[_javascriptStringEncode];
            _javascriptStringEncodeEvents.Click += JavaScriptStringEncode;

            _urlPathEncode = _encoderizer.Controls.Add(MsoControlType.msoControlButton, Missing.Value, Missing.Value, 1, true);
            _urlPathEncode.Caption = "Url Path Encode";
            _urlPathEncodeEvents = (CommandBarEvents)_application.Events.CommandBarEvents[_urlPathEncode];
            _urlPathEncodeEvents.Click += UrlPathEncode;

            _urlDecode = _encoderizer.Controls.Add(MsoControlType.msoControlButton, Missing.Value, Missing.Value, 1, true);
            _urlDecode.Caption = "Url Decode";
            _urlDecodeEvents = (CommandBarEvents)_application.Events.CommandBarEvents[_urlDecode];
            _urlDecodeEvents.Click += UrlDecode;

            _urlEncode = _encoderizer.Controls.Add(MsoControlType.msoControlButton, Missing.Value, Missing.Value, 1, true);
            _urlEncode.Caption = "Url Encode";
            _urlEncodeEvents = (CommandBarEvents)_application.Events.CommandBarEvents[_urlEncode];
            _urlEncodeEvents.Click += UrlEncode;

            _htmlAttributeEncode = _encoderizer.Controls.Add(MsoControlType.msoControlButton, Missing.Value, Missing.Value, 1, true);
            _htmlAttributeEncode.Caption = "Html Attribute Encode";
            _htmlAttributeEncodeEvents = (CommandBarEvents)_application.Events.CommandBarEvents[_htmlAttributeEncode];
            _htmlAttributeEncodeEvents.Click += HtmlAttributeEncode;

            _htmlDecode = _encoderizer.Controls.Add(MsoControlType.msoControlButton, Missing.Value, Missing.Value, 1, true);
            _htmlDecode.Caption = "Html Decode";
            _htmlDecodeEvents = (CommandBarEvents)_application.Events.CommandBarEvents[_htmlDecode];
            _htmlDecodeEvents.Click += HtmlDecode;

            _htmlEncode = _encoderizer.Controls.Add(MsoControlType.msoControlButton, Missing.Value, Missing.Value, 1, true);
            _htmlEncode.Caption = "Html Encode";
            _htmlEncodeEvents = (CommandBarEvents)_application.Events.CommandBarEvents[_htmlEncode];
            _htmlEncodeEvents.Click += HtmlEncode;
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }

        private void HtmlEncode(object commandBarControl, ref bool handled, ref bool cancelDefault)
        {
            ReplaceSeletedText(HttpUtility.HtmlEncode);
        }

        private void HtmlDecode(object commandBarControl, ref bool handled, ref bool cancelDefault)
        {
            ReplaceSeletedText(HttpUtility.HtmlDecode);
        }

        private void HtmlAttributeEncode(object commandBarControl, ref bool handled, ref bool cancelDefault)
        {
            ReplaceSeletedText(HttpUtility.HtmlAttributeEncode);
        }

        private void UrlEncode(object commandBarControl, ref bool handled, ref bool cancelDefault)
        {
            ReplaceSeletedText(HttpUtility.UrlEncode);
        }

        private void UrlDecode(object commandBarControl, ref bool handled, ref bool cancelDefault)
        {
            ReplaceSeletedText(HttpUtility.UrlDecode);
        }

        private void UrlPathEncode(object commandBarControl, ref bool handled, ref bool cancelDefault)
        {
            ReplaceSeletedText(HttpUtility.UrlPathEncode);
        }

        private void JavaScriptStringEncode(object commandBarControl, ref bool handled, ref bool cancelDefault)
        {
            ReplaceSeletedText(HttpUtility.JavaScriptStringEncode);
        }

        private void ReplaceSeletedText(Func<string, string> processor)
        {
            if (_application.ActiveDocument == null)
            {
                return;
            }

            var document = (TextDocument)_application.ActiveDocument.Object("");
            var selection = document.Selection;
            if (!String.IsNullOrWhiteSpace(selection.Text))
            {
                var anchor = selection.AnchorPoint.CreateEditPoint();
                anchor.ReplaceText(selection.Text.Length, processor.Invoke(selection.Text), (int)vsEPReplaceTextOptions.vsEPReplaceTextAutoformat);
            }
        }

        private DTE2 _application;
        private AddIn _addInInstance;

        private CommandBar _contextMenu;
        private CommandBarPopup _encoderizer;

        private CommandBarControl _htmlEncode;
        private CommandBarEvents _htmlEncodeEvents;

        private CommandBarControl _htmlDecode;
        private CommandBarEvents _htmlDecodeEvents;

        private CommandBarControl _htmlAttributeEncode;
        private CommandBarEvents _htmlAttributeEncodeEvents;

        private CommandBarControl _urlEncode;
        private CommandBarEvents _urlEncodeEvents;

        private CommandBarControl _urlDecode;
        private CommandBarEvents _urlDecodeEvents;

        private CommandBarControl _urlPathEncode;
        private CommandBarEvents _urlPathEncodeEvents;

        private CommandBarControl _javascriptStringEncode;
        private CommandBarEvents _javascriptStringEncodeEvents;
    }
}