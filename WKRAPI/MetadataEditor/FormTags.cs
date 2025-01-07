using MetadataEditor.Models;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace MetadataEditor;

#pragma warning disable CS8618
[SupportedOSPlatform("windows")]
public partial class FormTags : Form
{
    AlbumInfoProvider _ai;

    readonly List<Button> _povButtons = [];
    readonly List<Button> _focusButtons = [];
    readonly List<Button> _otherButtons = [];
    readonly List<Button> _rareButtons = [];
    readonly List<Button> _qualityButtons = [];

    SplitTagModel _splitTag;

    readonly int colCount = 4;
    readonly int buttonHeight = 30;
    readonly int buttonWidth = 110;
    readonly Color defaultColor = SystemColors.Control;
    readonly Color selectedColor = Color.Aqua;
    readonly FormMain formMain;

    public FormTags(FormMain formMain, AlbumInfoProvider ai, SplitTagModel splitTag) {
        _ai = ai;
        _splitTag = splitTag;

        InitializeComponent();

        this.formMain = formMain;

        DrawButtonFromSplitTags();
        RecolorSelectedTagButtons();
    }

    void ReziseWindowToWrapItems(int row, int col) {
        int height = buttonHeight * row;
        int width = buttonWidth * col;

        int heightPadding = 2 * (row + 1);
        int widthPadding = 4 * (col + 1); //each button has 4 pixels margin

        this.Height = height + heightPadding;
        this.Width = width + widthPadding;
    }

    void DrawButtonFromSplitTags() {
        int row = -1;

        void DrawButtons(string[] tags, List<Button> buttons, EventHandler onClick) {
            int col = -1;

            for(int i = 0; i < tags.Length; i++) {
                Button button = new Button();
                if(i % colCount == 0) {
                    row++;
                    col = -1;
                }
                col++;

                button.Height = buttonHeight;
                button.Width = buttonWidth;
                button.Location = new Point(col * buttonWidth, row * buttonHeight);
                button.Text = tags[i];
                button.Font = new Font("Calibri", 11.25f, FontStyle.Regular);
                button.Click += onClick;

                buttons.Add(button);
                this.Controls.Add(button);
            }
        }

        DrawButtons(_ai.Povs, _povButtons, (sender, e) => {
            var name = ((Button)sender!).Text;
            if(!_splitTag.Povs.Contains(name)){
                _splitTag.Povs.Add(name);
            }
            else {
                _splitTag.Povs.Remove(name);
            }

            RecolorSelectedTagButtons();
            formMain.UpdateTags(_splitTag);
        });

        row++;
        DrawButtons(_ai.Focuses, _focusButtons, (sender, e) => {
            var name = ((Button)sender!).Text;
            if(!_splitTag.Focuses.Contains(name)) {
                _splitTag.Focuses.Add(name);
            }
            else {
                _splitTag.Focuses.Remove(name);
            }

            RecolorSelectedTagButtons();
            formMain.UpdateTags(_splitTag);
        });

        row++;
        DrawButtons(_ai.Others, _otherButtons, (sender, e) => {
            var name = ((Button)sender!).Text;
            if(!_splitTag.Others.Contains(name)) {
                _splitTag.Others.Add(name);
            }
            else {
                _splitTag.Others.Remove(name);
            }

            RecolorSelectedTagButtons();
            formMain.UpdateTags(_splitTag);
        });

        row++;
        DrawButtons(_ai.Rares, _rareButtons, (sender, e) => {
            var name = ((Button)sender!).Text;
            if(!_splitTag.Rares.Contains(name)) {
                _splitTag.Rares.Add(name);
            }
            else {
                _splitTag.Rares.Remove(name);
            }

            RecolorSelectedTagButtons();
            formMain.UpdateTags(_splitTag);
        });

        row++;
        DrawButtons(_ai.Qualities, _qualityButtons, (sender, e) => {
            var name = ((Button)sender!).Text;
            if(!_splitTag.Qualities.Contains(name)) {
                _splitTag.Qualities.Add(name);
            }
            else {
                _splitTag.Qualities.Remove(name);
            }

            RecolorSelectedTagButtons();
            formMain.UpdateTags(_splitTag);
        });

        row++;
        ReziseWindowToWrapItems(row, colCount);
    }

    void RecolorSelectedTagButtons() {
        foreach(var button in _povButtons) {
            button.BackColor = _splitTag.Povs.Contains(button.Text) ? selectedColor : defaultColor;
        }
        foreach(var button in _focusButtons) {
            button.BackColor = _splitTag.Focuses.Contains(button.Text) ? selectedColor : defaultColor;
        }
        foreach(var button in _otherButtons) {
            button.BackColor = _splitTag.Others.Contains(button.Text) ? selectedColor : defaultColor;
        }
        foreach(var button in _rareButtons) {
            button.BackColor = _splitTag.Rares.Contains(button.Text) ? selectedColor : defaultColor;
        }
        foreach(var button in _qualityButtons) {
            button.BackColor = _splitTag.Qualities.Contains(button.Text) ? selectedColor : defaultColor;
        }
    }

    private void FormTags_Deactivate(object sender, EventArgs e) {
        this.Close();
    }
}