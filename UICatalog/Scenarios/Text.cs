﻿using NStack;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Terminal.Gui;
using Terminal.Gui.TextValidateProviders;



namespace UICatalog.Scenarios {
	[ScenarioMetadata (Name: "Text Input Controls", Description: "Tests all text input controls")]
	[ScenarioCategory ("Controls")]
	[ScenarioCategory ("Mouse and Keyboard")]
	[ScenarioCategory ("Text and Formatting")]
	public class Text : Scenario {
		public override void Setup ()
		{
			// TextField is a simple, single-line text input control
			var textField = new TextField ("TextField with test text. Unicode shouldn't 𝔹Aℝ𝔽!") {
				X = 1,
				Y = 0,
				Width = Dim.Percent (50) - 1,
				Height = 2
			};
			textField.TextChanging += TextField_TextChanging;

			void TextField_TextChanging (TextChangingEventArgs e)
			{
				textField.Autocomplete.AllSuggestions = Regex.Matches (e.NewText.ToString (), "\\w+")
					.Select (s => s.Value)
					.Distinct ().ToList ();
			}
			Win.Add (textField);

			var labelMirroringTextField = new Label (textField.Text) {
				X = Pos.Right (textField) + 1,
				Y = Pos.Top (textField),
				Width = Dim.Fill (1) - 1
			};
			Win.Add (labelMirroringTextField);

			textField.TextChanged += (prev) => {
				labelMirroringTextField.Text = textField.Text;
			};

			// TextView is a rich (as in functionality, not formatting) text editing control
			var textView = new TextView () {
				X = 1,
				Y = Pos.Bottom (textField),
				Width = Dim.Percent (50) - 1,
				Height = Dim.Percent (30),
			};
			textView.Text = "TextView with some more test text. Unicode shouldn't 𝔹Aℝ𝔽!" ;
			textView.DrawContent += TextView_DrawContent;

			// This shows how to enable autocomplete in TextView.
			void TextView_DrawContent (Rect e)
			{
				textView.Autocomplete.AllSuggestions = Regex.Matches (textView.Text.ToString (), "\\w+")
					.Select (s => s.Value)
					.Distinct ().ToList ();
			}
			Win.Add (textView);

			var labelMirroringTextView = new Label () {
				X = Pos.Right (textView) + 1,
				Y = Pos.Top (textView),
				Width = Dim.Fill (1) - 1,
				Height = Dim.Height (textView) - 1,
			};
			Win.Add (labelMirroringTextView);

			// Use ContentChanged to detect if the user has typed something in a TextView.
			// The TextChanged property is only fired if the TextView.Text property is
			// explicitly set
			textView.ContentsChanged += (a) => {
				labelMirroringTextView.Enabled = !labelMirroringTextView.Enabled;
				labelMirroringTextView.Text = textView.Text;
			};

			// By default TextView is a multi-line control. It can be forced to 
			// single-line mode.
			var chxMultiline = new CheckBox ("Multiline") {
				X = Pos.Left (textView),
				Y = Pos.Bottom (textView), 
				Checked = true
			};
			chxMultiline.Toggled += (b) => textView.Multiline = b;
			Win.Add (chxMultiline);

			var chxWordWrap = new CheckBox ("Word Wrap") {
				X = Pos.Right (chxMultiline) + 2,
				Y = Pos.Top (chxMultiline)
			};
			chxWordWrap.Toggled += (b) => textView.WordWrap = b;
			Win.Add (chxWordWrap);

			// TextView captures Tabs (so users can enter /t into text) by default;
			// This means using Tab to navigate doesn't work by default. This shows
			// how to turn tab capture off.
			var chxCaptureTabs = new CheckBox ("Capture Tabs") {
				X = Pos.Right (chxWordWrap) + 2,
				Y = Pos.Top (chxWordWrap),
				Checked = true
			};

			Key keyTab = textView.GetKeyFromCommand (Command.Tab);
			Key keyBackTab = textView.GetKeyFromCommand (Command.BackTab);
			chxCaptureTabs.Toggled += (b) => { 
				if (b) {
					textView.AddKeyBinding (keyTab, Command.Tab);
					textView.AddKeyBinding (keyBackTab, Command.BackTab);
				} else {
					textView.ClearKeybinding (keyTab);
					textView.ClearKeybinding (keyBackTab);
				}
				textView.WordWrap = b; 
			};
			Win.Add (chxCaptureTabs);

			var hexEditor = new HexView (new MemoryStream (Encoding.UTF8.GetBytes ("HexEditor Unicode that shouldn't 𝔹Aℝ𝔽!"))) {
				X = 1,
				Y = Pos.Bottom (chxMultiline) + 1,
				Width = Dim.Percent (50) - 1,
				Height = Dim.Percent (30),
			};
			Win.Add (hexEditor);

			var labelMirroringHexEditor = new Label () {
				X = Pos.Right (hexEditor) + 1,
				Y = Pos.Top (hexEditor),
				Width = Dim.Fill (1) - 1,
				Height = Dim.Height (hexEditor) - 1,
			};
			var array = ((MemoryStream)hexEditor.Source).ToArray ();
			labelMirroringHexEditor.Text = Encoding.UTF8.GetString (array, 0, array.Length);
			hexEditor.Edited += (kv) => {
				hexEditor.ApplyEdits ();
				var array = ((MemoryStream)hexEditor.Source).ToArray (); 
				labelMirroringHexEditor.Text = Encoding.UTF8.GetString (array, 0, array.Length);
			};
			Win.Add (labelMirroringHexEditor);

			var dateField = new DateField (System.DateTime.Now) {
				X = 1,
				Y = Pos.Bottom (hexEditor) + 1,
				Width = 20,
				IsShortFormat = false
			};
			Win.Add (dateField);

			var labelMirroringDateField = new Label (dateField.Text) {
				X = Pos.Right (dateField) + 1,
				Y = Pos.Top (dateField),
				Width = Dim.Width (dateField),
				Height = Dim.Height (dateField),
			};
			Win.Add (labelMirroringDateField);

			dateField.TextChanged += (prev) => {
				labelMirroringDateField.Text = dateField.Text;
			};

			_timeField = new TimeField (DateTime.Now.TimeOfDay) {
				X = Pos.Right (labelMirroringDateField) + 5,
				Y = Pos.Bottom (hexEditor) + 1,
				Width = 20,
				IsShortFormat = false
			};
			Win.Add (_timeField);

			_labelMirroringTimeField = new Label (_timeField.Text) {
				X = Pos.Right (_timeField) + 1,
				Y = Pos.Top (_timeField),
				Width = Dim.Width (_timeField),
				Height = Dim.Height (_timeField),
			};
			Win.Add (_labelMirroringTimeField);

			_timeField.TimeChanged += TimeChanged;

			// MaskedTextProvider - uses .NET MaskedTextProvider
			var netProviderLabel = new Label ("NetMaskedTextProvider [ 999 000 LLL >LLL| AAA aaa ]") {
				X = Pos.Left (dateField),
				Y = Pos.Bottom (dateField) + 1
			};
			Win.Add (netProviderLabel);

			var netProvider = new NetMaskedTextProvider ("999 000 LLL > LLL | AAA aaa");

			var netProviderField = new TextValidateField (netProvider) {
				X = Pos.Right (netProviderLabel) + 1,
				Y = Pos.Y (netProviderLabel),
			};

			Win.Add (netProviderField);

			// TextRegexProvider - Regex provider implemented by Terminal.Gui
			var regexProvider = new Label ("TextRegexProvider [ ^([0-9]?[0-9]?[0-9]|1000)$ ]") {
				X = Pos.Left (netProviderLabel),
				Y = Pos.Bottom (netProviderLabel) + 1
			};
			Win.Add (regexProvider);

			var provider2 = new TextRegexProvider ("^([0-9]?[0-9]?[0-9]|1000)$") { ValidateOnInput = false };
			var regexProviderField = new TextValidateField (provider2) {
				X = Pos.Right (regexProvider) + 1,
				Y = Pos.Y (regexProvider),
				Width = 30,
				TextAlignment = TextAlignment.Centered
			};

			Win.Add (regexProviderField);
		}

		TimeField _timeField;
		Label _labelMirroringTimeField;

		private void TimeChanged (DateTimeEventArgs<TimeSpan> e)
		{
			_labelMirroringTimeField.Text = _timeField.Text;
		}
	}
}
