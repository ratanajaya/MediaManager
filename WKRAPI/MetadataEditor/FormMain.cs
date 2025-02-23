using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using MetadataEditor.Models;
using MetadataEditor.Services;
using SharedLibrary.Models;
using SharedLibrary.Helpers;
using C = SharedLibrary.Constants;
using CoreAPI.AL.Models;
using System.Runtime.Versioning;
using Newtonsoft.Json;
using SixLabors.ImageSharp;

namespace MetadataEditor;

#pragma warning disable CS8618
#pragma warning disable CS8622
[SupportedOSPlatform("windows")]
public partial class FormMain : Form
{
    string _currentRootFolder; //for folder picker dialog, filled with the previous path of the selected folder
    int _currentFileIndex = 0; //For album display
    AppLogic _al;
    AlbumInfoProvider _ai;
    MetadataEditorConfig _config;

    AlbumViewModel _viewModel = new() {
            Album = new Album(),
            Path = "",
        };
    List<SourceAndContent> _tempSourceAndContents = new();
    List<FileDisplayModel> _srcFileDisplays;
    List<FileDisplayModel>? _dstFileDisplays = null;
    string? _cachedFolderNext;
    string? _cachedFolderPrev;

    Dictionary<string, string> _shortDisplayMap = new Dictionary<string, string>() {
            { "Ptr", C.Orientation.Portrait },
            { "Lsc", C.Orientation.Landscape },
            { "Aut", C.Orientation.Auto },
            { "EN", C.Language.English },
            { "JP", C.Language.Japanese },
            { "CH", C.Language.Chinese },
            { "Other", C.Language.Other }
        };

    #region Initialization
    public FormMain(AppLogic appLogic, AlbumInfoProvider ai, MetadataEditorConfig config) {
        InitializeComponent();

        _al = appLogic;
        _ai = ai;
        _config = config;

        lbPage.Parent = pctCover;
        pctCover.MouseWheel += PctCover_MouseWheel;
        pctCover.PreviewKeyDown += AnyControlPreviewKeyDown;

        groupAlbum.AllowDrop = true;

        RenderCategoryRadioButtons();
    }

    void RenderCategoryRadioButtons() {
        for(int i=0; i < _ai.Categories.Count(); i++) {
            var category = _ai.Categories[i];

            var rb = new RadioButton();
            rb.Text = category.First().ToString();
            rb.Name = "rbCat" + category;
            rb.Parent = rbCat;
            rb.Location = new System.Drawing.Point(0 + (i * 60), 0);
            rb.Size = new System.Drawing.Size(48, 22);
        }
    }

    private void FormMain_Load(object sender, EventArgs e) {
        txtTags.Text = "";
        if(_config.Args.Any()) {
            var path = _config.Args[0];
            var viewModel = Task.Run(() => _al.GetAlbumViewModelAsync(path, _viewModel.Album)).GetAwaiter().GetResult();
            AssignViewModel(viewModel, int.Parse(txtUpscaleTarget.Text)).GetAwaiter().GetResult();
        }
    }

    #endregion

    #region Browse folder for album
    private async void BtnBrowse_Click(object sender, EventArgs e) {
        CommonOpenFileDialog dialog = new CommonOpenFileDialog();
        dialog.InitialDirectory = _currentRootFolder ?? _config.BrowsePath;
        dialog.IsFolderPicker = true;
        if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
            await AssignAlbumPath(dialog.FileName);
        }
    }

    private async Task AssignAlbumPath(string path) {
        var viewModel = await _al.GetAlbumViewModelAsync(path, _viewModel.Album);
        await AssignViewModel(viewModel, int.Parse(txtUpscaleTarget.Text));
    }

    private async Task AssignViewModel(AlbumViewModel vm, int upscaleTarget) {
        //TODO refactor - MERGE 1
        _viewModel = vm;
        _currentRootFolder = vm.Path.Replace(vm.Path.Split('\\').Last(), "");
        _srcFileDisplays = _al.GetFileDisplayModels(vm.Path, vm.AlbumFiles, upscaleTarget, chkClamp.Checked);
        _dstFileDisplays = null;
        _tempSourceAndContents = await _al.GetOrInitializeSourceAndContents(vm.Path);
        DisplayAlbum();
    }

    private void FormMain_DragEnter(object sender, DragEventArgs e) {
        if(e.Data!.GetDataPresent(DataFormats.FileDrop) && Directory.Exists(((string[])e.Data.GetData(DataFormats.FileDrop)!)[0]))
            e.Effect = DragDropEffects.Copy;
    }

    private async void FormMain_DragDrop(object sender, DragEventArgs e) {
        if(!e.Data!.GetDataPresent(DataFormats.FileDrop)) return;
        var path = ((string[])e.Data.GetData(DataFormats.FileDrop)!)[0];
        if(!Directory.Exists(path)) return;

        await AssignAlbumPath(path);
    }
    #endregion

    #region Display Album
    void DisplayAlbum() {
        ClearControls();

        txtTitle.Text = _viewModel.Album.Title;
        txtArtists.Text = String.Join(",", _viewModel.Album.Artists);
        txtTags.Text = JsonConvert.SerializeObject(new SplitTagModel {
            Povs = _viewModel.Album.Povs,
            Focuses = _viewModel.Album.Focuses,
            Others = _viewModel.Album.Others,
            Rares = _viewModel.Album.Rares,
            Qualities = _viewModel.Album.Qualities
        });
        txtCharacters.Text = _viewModel.Album.GetCharactersDisplay();
        txtNote.Text = _viewModel.Album.Note ?? "";

        foreach(string lan in _viewModel.Album.Languages) {
            string controlName = "chkLan" + lan;
            ((CheckBox)this.Controls.Find(controlName, true)[0]).Checked = true;
        }
        SetRadioButton("rbCat", _viewModel.Album.Category);
        txtTier.Text = _viewModel.Album.Tier.ToString();
        SetRadioButton("rbOri", _viewModel.Album.Orientation);
        chkIsWipTrue.Checked = _viewModel.Album.IsWip;
        chkIsReadTrue.Checked = _viewModel.Album.IsRead;

        UpdateFileDisplay();

        _currentFileIndex = 0;

        DisplayImageOnPctCover();

        lbPage.Text = (_currentFileIndex + 1) + "/" + _viewModel.AlbumFiles.Count;

        _cachedFolderNext = null;
        _cachedFolderPrev = null;
    }

    void DisplayImageOnPctCover() {
        var extension = Path.GetExtension(_viewModel.AlbumFiles[_currentFileIndex]);

        if(extension == ".webp") {
            using(var image = SixLabors.ImageSharp.Image.Load(_viewModel.AlbumFiles[_currentFileIndex])) {
                // Convert the ImageSharp image to a System.Drawing.Bitmap
                using(MemoryStream ms = new MemoryStream()) {
                    image.SaveAsBmp(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    Bitmap bitmap = new Bitmap(ms);

                    // Assign the bitmap to the PictureBox
                    pctCover.Image = bitmap;
                }
            }
            pctCover.SizeMode = PictureBoxSizeMode.Zoom;
        }
        else if(_ai.SuitableImageFormats.Contains(extension)) {
            using(var fs = File.Open(_viewModel.AlbumFiles[_currentFileIndex], FileMode.Open, FileAccess.Read, FileShare.Read)) {
                pctCover.Image = System.Drawing.Image.FromStream(fs);
            }
            pctCover.SizeMode = PictureBoxSizeMode.Zoom;
        }
        else {
            pctCover.Image = null;
        }
    }

    void UpdateFileDisplay() {
        listViewSrc.Items.Clear();
        listViewSrc.Items.AddRange(GetListViewItems(_srcFileDisplays, _viewModel.Path));

        listViewDst.Items.Clear();
        if(_dstFileDisplays != null) {
            listViewDst.Items.AddRange(GetListViewItems(_dstFileDisplays, _viewModel.Path));
            tabControl1.SelectedIndex = 1;
        }
        else
            tabControl1.SelectedIndex = 0;
    }

    private ListViewItem[] GetListViewItems(List<FileDisplayModel> files, string rootPath) {
        string rootPathWithSlash = $"{rootPath}{Path.DirectorySeparatorChar}";

        return files.Select(a => {
            var lv = new ListViewItem(a.Path.Replace(rootPathWithSlash, String.Empty));
            lv.SubItems.Add(a.SizeDisplay);
            lv.SubItems.Add(a.DimensionDisplay);
            lv.SubItems.Add(a.BytesPer100PixelDisplay);
            lv.SubItems.Add(a.UploadStatus);
            return lv;
        }).ToArray();
    }

    void SetRadioButton(string v, string category) {
        RadioButton rb = (RadioButton)this.Controls[0].Controls[v]!.Controls[v + category]!;
        rb.Select();
    }

    void ClearControls() {
        var skippedControlNames = new string[] {
            "txtUpscaleTarget",
            "chkClamp"
        };

        foreach(Control control in this.Controls[0].Controls) {
            if(skippedControlNames.Contains(control.Name))
                continue;

            if(control is TextBox) {
                TextBox txtbox = (TextBox)control;
                txtbox.Text = string.Empty;
            }
            else if(control is ListView) {
                ((ListView)control).Items.Clear();
            }
            else if(control is CheckBox) {
                CheckBox chkbox = (CheckBox)control;
                chkbox.Checked = false;
            }
            else if(control is RadioButton) {
                RadioButton rdbtn = (RadioButton)control;
                rdbtn.Checked = false;
            }
            else if(control is DateTimePicker) {
                DateTimePicker dtp = (DateTimePicker)control;
                dtp.Value = DateTime.Now;
            }
        }
    }
    #endregion

    #region SourceAndContent
    private void btnSrcAdd_Click(object sender, EventArgs e) {
        AddSourceAndContentFromUIToList();
    }

    private void AddSourceAndContentFromUIToList() {
        try {
            //Somehow only newtonsoft deserializer works
            var newSnc = JsonConvert.DeserializeObject<SourceAndContent>(txtSrcJson.Text)!;
            newSnc.Source.SubTitle = txtSrcSubtitle.Text;

            var existingSnc = _tempSourceAndContents.FirstOrDefault(a => a.Source.Url == newSnc.Source.Url);

            if(existingSnc != null) {
                existingSnc.Source = newSnc.Source;
                existingSnc.Comments = newSnc.Comments;
            }
            else {
                _tempSourceAndContents.Add(newSnc);
            }
        }
        catch(Exception ex) {
            MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK);
        }
    }

    private void txtSrcJson_TextChanged(object sender, EventArgs e) {
        if(string.IsNullOrWhiteSpace(txtSrcJson.Text))
            return;

        try {
            var snc = JsonConvert.DeserializeObject<SourceAndContent>(txtSrcJson.Text)!;

            txtSrcTitle.Text = snc.Source.Title;
            txtSrcSubtitle.Text = "";

            AddSourceAndContentFromUIToList();
        }
        catch(Exception ex) {
            MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK);
        }
    }
    #endregion

    #region Save #album.json
    private async void BtnCreate_Click(object sender, EventArgs e) {
        RetrieveAlbumVmValueFromUI();
        string retval = await _al.SaveAlbumJson(_viewModel);
    }

    private void RetrieveAlbumVmValueFromUI() {
        _viewModel.Album.Title = txtTitle.Text;
        _viewModel.Album.Artists = txtArtists.Text.Split(',').ToList();
        _viewModel.Album.Category = GetFromRadioButton("rbCat");
        _viewModel.Album.Tier = int.Parse(txtTier.Text);
        _viewModel.Album.Orientation = GetFromRadioButton("rbOri");
        _viewModel.Album.Characters = txtCharacters.Text.Split(',').Select(x => x.Trim()).ToList();
        _viewModel.Album.Note = txtNote.Text;
        _viewModel.Album.Languages = GetLansFromForm();
        _viewModel.Album.IsWip = chkIsWipTrue.Checked;
        _viewModel.Album.IsRead = chkIsReadTrue.Checked;

        var splitTag = JsonConvert.DeserializeObject<SplitTagModel>(txtTags.Text)!;
        _viewModel.Album.Povs = splitTag.Povs;
        _viewModel.Album.Focuses = splitTag.Focuses;
        _viewModel.Album.Others = splitTag.Others;
        _viewModel.Album.Rares = splitTag.Rares;
        _viewModel.Album.Qualities = splitTag.Qualities;

        _viewModel.Album.ValidateAndCleanup();
    }

    private string GetFromRadioButton(string v) {
        foreach(Control c in this.Controls[0].Controls[v]!.Controls) {
            if(((RadioButton)c).Checked) {
                if(v == "rbCat")
                    return c.Name.Replace("rbCat", "");
                else
                    return _shortDisplayMap[c.Text];
            }
        }
        return "";
    }

    List<string> GetLansFromForm() {
        List<string> result = new List<string>();

        foreach(Control c in this.Controls[0].Controls) {
            if(c.Name.Contains("chkLan")) {
                if(((CheckBox)c).Checked) {
                    result.Add(_shortDisplayMap[c.Text]);
                }
            }
        }

        return result;
    }
    #endregion

    #region Next/Prev
    string GetRelativeFolder(string currentFolder, int step) {
        string rootFolder = Path.GetDirectoryName(currentFolder)!;
        string[] allFolders = Directory.GetDirectories(rootFolder).OrderByAlphaNumeric(a => a).ToArray();

        int relativeFolderIndex = (Array.IndexOf(allFolders, currentFolder) + step) % allFolders.Length;
        string result = allFolders[relativeFolderIndex];
        return result;
    }

    private async void BtnNext_Click(object sender, EventArgs e) {
        try {
            //TODO refactor - MERGE 1
            string fullPath = string.IsNullOrEmpty(_cachedFolderNext) ? GetRelativeFolder(_viewModel.Path, 1) : _cachedFolderNext;
            _currentRootFolder = fullPath.Replace(fullPath.Split('\\').Last(), "");
            _viewModel = await _al.GetAlbumViewModelAsync(fullPath, _viewModel.Album);
            _srcFileDisplays = _al.GetFileDisplayModels(_viewModel.Path, _viewModel.AlbumFiles, int.Parse(txtUpscaleTarget.Text), chkClamp.Checked);
            _dstFileDisplays = null;
            _tempSourceAndContents = new();
            DisplayAlbum();
        }
        catch(Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }

    private async void BtnPrev_Click(object sender, EventArgs e) {
        try {
            //TODO refactor - MERGE 1
            string fullPath = string.IsNullOrEmpty(_cachedFolderPrev) ? GetRelativeFolder(_viewModel.Path, -1) : _cachedFolderPrev;
            _currentRootFolder = fullPath.Replace(fullPath.Split('\\').Last(), "");
            _viewModel = await _al.GetAlbumViewModelAsync(fullPath, _viewModel.Album);
            _srcFileDisplays = _al.GetFileDisplayModels(_viewModel.Path, _viewModel.AlbumFiles, int.Parse(txtUpscaleTarget.Text), chkClamp.Checked);
            _dstFileDisplays = null;
            _tempSourceAndContents = new();
            DisplayAlbum();
        }
        catch(Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }

    private void PctCover_Hover(object sender, EventArgs e) {
        pctCover.Focus();
    }

    private void PctCover_MouseWheel(object sender, MouseEventArgs e) {
        if(e.Delta > 0) {
            MovePage(-1);
        }
        else {
            MovePage(1);
        }
    }

    private void AnyControlPreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
        if(e.KeyCode == Keys.A) {
            MovePage(-1);
        }
        else if(e.KeyCode == Keys.D) {
            MovePage(1);
        }
    }

    void MovePage(int step) {
        if(_viewModel.AlbumFiles == null) { return; }
        try {
            _currentFileIndex = _currentFileIndex + step;
            if(_currentFileIndex < 0) {
                _currentFileIndex = _viewModel.AlbumFiles.Count - 1;
            }
            else {
                _currentFileIndex = _currentFileIndex % _viewModel.AlbumFiles.Count;
            }

            DisplayImageOnPctCover();
            lbPage.Text = (_currentFileIndex + 1) + "/" + _viewModel.AlbumFiles.Count;
        }
        catch(Exception ex) {
            pctCover.Image = null;
            lbPage.Text = "";

            Console.WriteLine(ex.ToString());
        }
    }
    #endregion

    #region Other UI Actions
    private void btnTierMin_Click(object sender, EventArgs e) {
        int res;
        int val = int.TryParse(txtTier.Text, out res) ? res : 0;
        txtTier.Text = (val - 1).ToString();
    }

    private void btnTierPlus_Click(object sender, EventArgs e) {
        int res;
        int val = int.TryParse(txtTier.Text, out res) ? res : 0;
        txtTier.Text = (val + 1).ToString();
        chkIsReadTrue.Checked = true;
    }

    public void UpdateTags(SplitTagModel splitTag) {
        txtTags.Text = JsonConvert.SerializeObject(splitTag);
    }

    public List<string> ChangeSelectedCharacters(string character) {
        txtCharacters.Text = txtCharacters.Text.Trim();
        List<string> characters = txtCharacters.Text.Split(new[] { "," }, StringSplitOptions.None).Select(x => x.Trim()).ToList();
        characters.Remove("");
        if(characters.Contains(character)) { //Remove if exist
            characters.Remove(character);
        }
        else { //Add if not exist
            characters.Add(character);
        }
        characters.Sort();
        txtCharacters.Text = string.Join(", ", characters);

        return characters;
    }

    private void btnHp_Click(object sender, EventArgs e) {
        txtNote.Text = "HP";
    }

    private void CbTags_SelectedIndexChanged(object sender, EventArgs e) { }

    private void btnPopupTags_Click(object sender, EventArgs e) {
        FormTags formTags = new FormTags(this, _ai, JsonConvert.DeserializeObject<SplitTagModel>(txtTags.Text)!);
        formTags.StartPosition = FormStartPosition.Manual;
        formTags.ShowInTaskbar = false;
        formTags.ShowIcon = false;
        formTags.ControlBox = false;
        formTags.Text = String.Empty;

        var location = this.Location;
        var marginLeft = btnPopupTags.Left + btnPopupTags.Width;
        var marginTop = btnPopupTags.Top + btnPopupTags.Height;
        location.Offset(marginLeft, marginTop);
        formTags.Location = location;
        formTags.Show(btnPopupTags);
    }

    private void btnPopupCharacters_Click(object sender, EventArgs e) {
        FormCharacters formCharacters = new FormCharacters(this, _ai.Characters.ToList(), _viewModel.Album.Characters);
        formCharacters.StartPosition = FormStartPosition.Manual;
        formCharacters.ShowInTaskbar = false;
        formCharacters.ShowIcon = false;
        formCharacters.ControlBox = false;
        formCharacters.Text = String.Empty;

        var location = this.Location;
        var marginLeft = btnPopupCharacters.Left + btnPopupCharacters.Width;
        var marginTop = btnPopupCharacters.Top + btnPopupCharacters.Height;
        location.Offset(marginLeft, marginTop);
        formCharacters.Location = location;
        formCharacters.Show(btnPopupCharacters);
    }

    private void btnExplore_Click(object sender, EventArgs e) {
        try {
            string fileName = _viewModel.Path;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "explorer.exe";
            startInfo.Arguments = "\"" + fileName + "\"";
            Process.Start(startInfo);
        }
        catch(Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }

    private void UpdateProgress(string message) {
        var lv = new ListViewItem();
        lv.Text = message;
        lv.SubItems.Add(message);
        listViewProgress.Items.Add(lv);

        lv.EnsureVisible();
    }

    private async void btnPost_Click(object sender, EventArgs e) {
        if(_viewModel.Path == null) { return; }
        RetrieveAlbumVmValueFromUI();

        var saveTask = _al.SaveAlbumJson(_viewModel);

        UpdateProgress("Posting Album...");
        var progress = new Progress<FileDisplayModel>(model => {
            if(_dstFileDisplays != null) {
                var uploadedFile = _dstFileDisplays.First(fd => fd.Path == model.Path);
                uploadedFile.UploadStatus = model.UploadStatus;
            }
            else {
                var uploadedFile = _srcFileDisplays.First(fd => fd.Path == model.Path);
                uploadedFile.UploadStatus = model.UploadStatus;
            }

            UpdateFileDisplay();
        });

        var libRelPath = await _al.PostAlbumJsonOffline(_viewModel, _tempSourceAndContents, progress);
        var saveTaskRetval = await saveTask;

        UpdateProgress($"Post Finished | {libRelPath}");
    }

    private async void btnPostMetadata_Click(object sender, EventArgs e) {
        if(_viewModel.Path == null) { return; }

        RetrieveAlbumVmValueFromUI();
        await _al.SaveAlbumJson(_viewModel);
        var libRelPath = _al.PostAlbumMetadataOffline(_viewModel);

        UpdateProgress("Post Metadata Finished!");
        MessageBox.Show(libRelPath, "Post Metadata Success", MessageBoxButtons.OK);
    }

    private async void btnProcess_Click(object sender, EventArgs e) {
        try {
            var progress = new Progress<string>(msg => {
                UpdateProgress(msg);
            });

            var (processedFileDisplays, report) = await _al.CorrectPages(
                _viewModel.Path,
                _srcFileDisplays.Select(a => a.CorrectionModel).ToList(),
                int.Parse(txtUpscaleTarget.Text),
                chkClamp.Checked,
                progress
            );

            _dstFileDisplays = processedFileDisplays;

            _viewModel.AlbumFiles = processedFileDisplays.Select(a => a.Path).ToList();
            UpdateFileDisplay();

            var failedFileCount = report.Count(a => !a.Success);
            if(failedFileCount > 0) {
                var failedFileMsg = failedFileCount > 0 ? $"{failedFileCount} failed files" : "";
                MessageBox.Show(failedFileMsg, "Error", MessageBoxButtons.OK);
            }
            UpdateProgress("Correction Finished!");

        }
        catch(Exception ex) {
            MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK);
        }
    }

    private async void btnPostDel_Click(object sender, EventArgs e) {
        if(_viewModel.Path == null) { return; }

        var libRelPath = await _al.PostAlbumAndImmediatelyDelete(_viewModel);

        MessageBox.Show(libRelPath, "Insert Delete Success", MessageBoxButtons.OK);
    }
    #endregion
}