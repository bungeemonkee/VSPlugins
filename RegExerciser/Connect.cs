using System;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.VisualStudio.CommandBars;
using System.Windows.Interop;

namespace RegExerciser
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

            var commandBar = ((CommandBars)_application.CommandBars)["Code Window"];
            var commandBarPopup = (CommandBarPopup)commandBar.Controls.Add(MsoControlType.msoControlPopup, Missing.Value, Missing.Value, 1, true);
            commandBarPopup.Caption = "RegExerciser";

            _searchWithRegex = commandBarPopup.Controls.Add(MsoControlType.msoControlButton, Missing.Value, Missing.Value, 1, true);
            _searchWithRegex.Caption = "Search With Regular Expression";
            _searchWithRegexEvents = (CommandBarEvents)_application.Events.CommandBarEvents[_searchWithRegex];
            _searchWithRegexEvents.Click += SearchWithRegex;

            _testRegex = commandBarPopup.Controls.Add(MsoControlType.msoControlButton, Missing.Value, Missing.Value, 1, true);
            _testRegex.Caption = "Test Regular Expression";
            _testRegexEvents = (CommandBarEvents)_application.Events.CommandBarEvents[_testRegex];
            _testRegexEvents.Click += TestRegex;
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

        private bool _handleReady = false;
        private IntPtr _handle;
        public IntPtr Handle
        {
            get
            {
                if (!_handleReady)
                {
                    _handle = new IntPtr(_application.MainWindow.HWnd);
                    _handleReady = true;
                }
                return _handle;
            }
        }

        private void TestRegex(object commandBarControl, ref bool handled, ref bool cancelDefault)
        {
            if (_application.ActiveDocument == null)
            {
                return;
            }

            var selection = (TextSelection)_application.ActiveDocument.Selection;
            if (selection.IsEmpty)
            {
                var beginning = selection.ActivePoint.CreateEditPoint();
                while (!beginning.AtStartOfDocument)
                {
                    if (beginning.GetText(-2) == "@\"")
                    {
                        beginning.CharLeft(2);
                        break;
                    }
                    beginning.CharLeft(1);
                }

                var end = beginning.CreateEditPoint();
                end.CharRight(3);
                var previousQuoteCount = 0;
                while (!end.AtEndOfDocument)
                {
                    var text = end.GetText(-1);
                    if (text[0] != '"')
                    {
                        if (previousQuoteCount % 2 == 1)
                        {
                            end.CharLeft(1); // we found the end but went one too far - go back
                            break;
                        }
                        previousQuoteCount = 0;
                    }
                    else
                    {
                        ++previousQuoteCount;
                    }
                    end.CharRight(1);
                }

                selection.MoveToPoint(beginning);
                selection.MoveToPoint(end, true);
            }

            var regex = FromStringLiteral(selection.Text);
            ShowEditor(regex, null, e => ReplaceSelection(selection, ToStringLiteral(e.RegularExpression)));
        }

        private void SearchWithRegex(object commandBarControl, ref bool handled, ref bool cancelDefault)
        {
            if (_application.ActiveDocument == null)
            {
                return;
            }

            var selection = (TextSelection)_application.ActiveDocument.Selection;
            var text = FromStringLiteral(selection.Text);

            ShowEditor(null, text, e => ReplaceSelection(selection, e.TestingText));
        }

        private void ShowEditor(string regex, string test, Action<RegexEditor> replacer)
        {
            var editor = new RegexEditor
                {
                    Replacer = replacer
                };
            if (regex != null)
            {
                editor.RegularExpression = regex;
            }
            if (test != null)
            {
                editor.TestingText = test;
            }

            var helper = new WindowInteropHelper(editor);
            helper.EnsureHandle();
            helper.Owner = Handle;

            editor.ShowDialog();
        }

        private static void ReplaceSelection(TextSelection selection, string text)
        {
            var anchor = selection.AnchorPoint.CreateEditPoint();
            anchor.ReplaceText(selection.Text.Length, text, (int) vsEPReplaceTextOptions.vsEPReplaceTextAutoformat);
        }

        private static string ToStringLiteral(string regex)
        {
            const string format = "@\"{0}\"";
            return string.Format(format, regex.Replace("\"", "\"\""));
        }

        private static string FromStringLiteral(string literal)
        {
            if (literal.StartsWith("@\""))
            {
                return literal.Substring(2, literal.Length - 3).Replace("\"\"", "\"");
            }
            return "Unable to parse regular expression. Highlight a string of the format @\"...\'";
        }

        private DTE2 _application;
        private AddIn _addInInstance;

        private CommandBarControl _testRegex;
        private CommandBarEvents _testRegexEvents;

        private CommandBarControl _searchWithRegex;
        private CommandBarEvents _searchWithRegexEvents;
    }
}