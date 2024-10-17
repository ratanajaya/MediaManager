using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetadataEditor;

[SupportedOSPlatform("windows")]
public partial class FormCharacters : Form
{
    List<Button> characterButtons;

    int colCount = 3;
    int buttonHeight = 30;
    int buttonWidth = 110;
    Color defaultColor = SystemColors.Control;
    Color selectedColor = Color.Aqua;
    FormMain formMain;

    public FormCharacters(FormMain formMain, List<string> allCharacters, List<string> selectedCharacters) {
        InitializeComponent();

        this.formMain = formMain;

        ReziseWindowToWrapItems(allCharacters);
        DrawButtonFromCharacters(allCharacters, selectedCharacters);
        RefreshSelectedCharacterButtons(selectedCharacters);
    }

    void ReziseWindowToWrapItems(List<string> allCharacters) {
        int rowCount = (int)Math.Ceiling((float)allCharacters.Count / (float)colCount);
        int height = buttonHeight * rowCount;
        int width = buttonWidth * colCount;

        int heightPadding = 2 * (rowCount + 1);
        int widthPadding = 4 * (colCount + 1); //each button has 4 pixels margin

        this.Height = height + heightPadding;
        this.Width = width + widthPadding;
    }

    void DrawButtonFromCharacters(List<string> allTags, List<string> selectedCharacters) {
        characterButtons = new List<Button>();
        int row = -1;
        int col = -1;
        for(int i = 0; i < allTags.Count; i++) {
            Button button = new Button();
            if(i % colCount == 0) {
                row++;
                col = -1;
            }
            col++;

            button.Height = buttonHeight;
            button.Width = buttonWidth;
            button.Location = new Point(col * buttonWidth, row * buttonHeight);
            button.Text = allTags[i];
            button.Font = new Font("Calibri", 11.25f, FontStyle.Regular);
            button.Click += ButtonCharacters_Click;

            characterButtons.Add(button);
            this.Controls.Add(button);
        }
    }

    void RefreshSelectedCharacterButtons(List<string> selectedCharacters) {
        foreach(var button in characterButtons) {
            button.BackColor = selectedCharacters.Contains(button.Text) ? selectedColor : defaultColor;
        }
    }

    private void ButtonCharacters_Click(object sender, EventArgs e) {
        var selectedTags = formMain.ChangeSelectedCharacters(((Button)sender).Text);
        RefreshSelectedCharacterButtons(selectedTags);
    }

    private void FormCharacters_Deactivate(object sender, EventArgs e) {
        foreach(var button in characterButtons) {
            button.Click -= ButtonCharacters_Click;
        }
        this.Close();
    }
}