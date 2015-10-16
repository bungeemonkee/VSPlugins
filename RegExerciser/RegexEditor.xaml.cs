using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace RegExerciser
{
    /// <summary>
    /// Interaction logic for RegexEditor.xaml
    /// </summary>
    public partial class RegexEditor : Window
    {
        public RegexEditor()
        {
            InitializeComponent();

            var offset = 0;
            var options = Enum.GetNames(typeof(RegexOptions));
            foreach (var option in options.OrderBy(s => s, StringComparer.InvariantCultureIgnoreCase))
            {
                var optVal = (RegexOptions)Enum.Parse(typeof(RegexOptions), option);
                if (optVal == RegexOptions.None) continue;

                var opt = new CheckBox
                    {
                        Content = option
                    };
                opt.Checked += (sender, args) => ChangeOption(sender, optVal);
                opt.Unchecked += (sender, args) => ChangeOption(sender, optVal);
                OptionsContainer.Children.Add(opt);
            }

            ReplaceButton.Visibility = (Replacer != null) ? Visibility.Visible : Visibility.Collapsed;

            UpdateMatches();
        }

        public string RegularExpression
        {
            get { return RegularExpressionInput.Text; }
            set { RegularExpressionInput.Text = value; }
        }

        public string TestingText
        {
            get { return TestingTextInput.Text; }
            set { TestingTextInput.Text = value; }
        }

        public Action<RegexEditor> Replacer
        {
            get { return _replacer; }
            set
            {
                if (_replacer == value) return;
                _replacer = value;
                ReplaceButton.Visibility = (value != null) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #region private

        private RegexOptions _options = RegexOptions.None;
        private Action<RegexEditor> _replacer;

        private void ChangeOption(object sender, RegexOptions option)
        {
            var box = sender as CheckBox;
            if (box == null) return;

            if (box.IsChecked.HasValue && box.IsChecked.Value)
            {
                _options = _options | option;
            }
            else if (_options.HasFlag(option))
            {
                _options = _options & ~option;
            }

            UpdateMatches();
        }

        private void ReplaceButton_Click(object sender, EventArgs e)
        {
            if (Replacer != null)
            {
                Replacer.Invoke(this);
            }
            Close();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void RegularExpressionInput_TextChanged(object sender, EventArgs e)
        {
            UpdateMatches();
        }

        private void TestingTextInput_TextChanged(object sender, EventArgs e)
        {
            UpdateMatches();
        }

        private void UpdateMatches()
        {
            Regex regex;
            try
            {
                regex = new Regex(RegularExpressionInput.Text, _options);
            }
            catch (ArgumentException ex)
            {
                Results.Text = string.Format("The regular expression is not valid: {0}", ex.Message);
                return;
            }

            // multiline mode is broken by windows ridiculous insistence that a newline is "\r\n"
            var text = Environment.NewLine != "\n"
                           ? TestingTextInput.Text.Replace(Environment.NewLine, "\n")
                           : TestingTextInput.Text;
            var matches = regex.Matches(text);
            if (matches.Count == 0)
            {
                Results.Text = "The regular expression does not match the text.";
                return;
            }
            var sb = new StringBuilder();
            foreach (var match in matches.Cast<Match>())
            {
                sb.AppendFormat("Match: {0}{1}", match.Value, Environment.NewLine);
                foreach (var capture in match.Captures.Cast<Capture>())
                {
                    sb.AppendFormat("\tCapture: {0}{1}", capture.Value, Environment.NewLine);
                }
            }
            Results.Text = sb.ToString();
        }

        #endregion
    }
}
