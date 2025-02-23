namespace MetadataEditor
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            txtArtists = new System.Windows.Forms.TextBox();
            btnCreate = new System.Windows.Forms.Button();
            chkLanEnglish = new System.Windows.Forms.CheckBox();
            chkLanJapanese = new System.Windows.Forms.CheckBox();
            chkLanChinese = new System.Windows.Forms.CheckBox();
            chkLanOther = new System.Windows.Forms.CheckBox();
            chkIsReadTrue = new System.Windows.Forms.CheckBox();
            pctCover = new System.Windows.Forms.PictureBox();
            btnPrev = new System.Windows.Forms.Button();
            groupAlbum = new System.Windows.Forms.GroupBox();
            btnPostDel = new System.Windows.Forms.Button();
            chkClamp = new System.Windows.Forms.CheckBox();
            txtUpscaleTarget = new System.Windows.Forms.TextBox();
            listViewProgress = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabPage1 = new System.Windows.Forms.TabPage();
            listViewSrc = new System.Windows.Forms.ListView();
            srcColFile = new System.Windows.Forms.ColumnHeader();
            srcColSize = new System.Windows.Forms.ColumnHeader();
            srcColDimension = new System.Windows.Forms.ColumnHeader();
            srcColBytesPerPixel = new System.Windows.Forms.ColumnHeader();
            srcUploadStatus = new System.Windows.Forms.ColumnHeader();
            tabPage2 = new System.Windows.Forms.TabPage();
            listViewDst = new System.Windows.Forms.ListView();
            dstColFile = new System.Windows.Forms.ColumnHeader();
            dstColSize = new System.Windows.Forms.ColumnHeader();
            dstColDimension = new System.Windows.Forms.ColumnHeader();
            dstColBytesPerPixel = new System.Windows.Forms.ColumnHeader();
            dstUploadStatus = new System.Windows.Forms.ColumnHeader();
            btnSrcAdd = new System.Windows.Forms.Button();
            txtSrcTitle = new System.Windows.Forms.TextBox();
            txtSrcSubtitle = new System.Windows.Forms.TextBox();
            txtSrcJson = new System.Windows.Forms.TextBox();
            btnPopupCharacters = new System.Windows.Forms.Button();
            txtCharacters = new System.Windows.Forms.TextBox();
            label10 = new System.Windows.Forms.Label();
            btnHp = new System.Windows.Forms.Button();
            txtTitle = new System.Windows.Forms.RichTextBox();
            btnPostMetadata = new System.Windows.Forms.Button();
            btnTierPlus = new System.Windows.Forms.Button();
            btnTierMin = new System.Windows.Forms.Button();
            btnProcess = new System.Windows.Forms.Button();
            btnPost = new System.Windows.Forms.Button();
            btnPopupTags = new System.Windows.Forms.Button();
            btnExplore = new System.Windows.Forms.Button();
            rbOri = new System.Windows.Forms.Panel();
            rbOriAuto = new System.Windows.Forms.RadioButton();
            rbOriPortrait = new System.Windows.Forms.RadioButton();
            rbOriLandscape = new System.Windows.Forms.RadioButton();
            rbCat = new System.Windows.Forms.Panel();
            label9 = new System.Windows.Forms.Label();
            txtTier = new System.Windows.Forms.TextBox();
            txtTags = new System.Windows.Forms.RichTextBox();
            chkIsWipTrue = new System.Windows.Forms.CheckBox();
            label5 = new System.Windows.Forms.Label();
            groupPicture = new System.Windows.Forms.GroupBox();
            lbPage = new System.Windows.Forms.Label();
            btnNext = new System.Windows.Forms.Button();
            btnBrowse = new System.Windows.Forms.Button();
            txtNote = new System.Windows.Forms.TextBox();
            directoryEntry1 = new System.DirectoryServices.DirectoryEntry();
            ((System.ComponentModel.ISupportInitialize)pctCover).BeginInit();
            groupAlbum.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            rbOri.SuspendLayout();
            groupPicture.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 20);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(36, 18);
            label1.TabIndex = 0;
            label1.Text = "Title";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(6, 110);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(47, 18);
            label2.TabIndex = 1;
            label2.Text = "Artists";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(6, 140);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(34, 18);
            label3.TabIndex = 2;
            label3.Text = "Tags";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(6, 200);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(72, 18);
            label4.TabIndex = 3;
            label4.Text = "Languages";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(6, 230);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(63, 18);
            label6.TabIndex = 5;
            label6.Text = "Category";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(6, 350);
            label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(32, 18);
            label7.TabIndex = 6;
            label7.Text = "Tier";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(6, 290);
            label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(39, 18);
            label8.TabIndex = 7;
            label8.Text = "Note";
            // 
            // txtArtists
            // 
            txtArtists.BackColor = System.Drawing.SystemColors.ControlLight;
            txtArtists.Location = new System.Drawing.Point(100, 110);
            txtArtists.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtArtists.Name = "txtArtists";
            txtArtists.Size = new System.Drawing.Size(252, 26);
            txtArtists.TabIndex = 10;
            // 
            // btnCreate
            // 
            btnCreate.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            btnCreate.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Bold);
            btnCreate.Location = new System.Drawing.Point(61, 525);
            btnCreate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new System.Drawing.Size(71, 52);
            btnCreate.TabIndex = 17;
            btnCreate.Text = "SAVE";
            btnCreate.UseVisualStyleBackColor = false;
            btnCreate.Click += BtnCreate_Click;
            // 
            // chkLanEnglish
            // 
            chkLanEnglish.AutoSize = true;
            chkLanEnglish.Location = new System.Drawing.Point(100, 200);
            chkLanEnglish.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            chkLanEnglish.Name = "chkLanEnglish";
            chkLanEnglish.Size = new System.Drawing.Size(44, 22);
            chkLanEnglish.TabIndex = 18;
            chkLanEnglish.Text = "EN";
            chkLanEnglish.UseVisualStyleBackColor = true;
            // 
            // chkLanJapanese
            // 
            chkLanJapanese.AutoSize = true;
            chkLanJapanese.Location = new System.Drawing.Point(160, 200);
            chkLanJapanese.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            chkLanJapanese.Name = "chkLanJapanese";
            chkLanJapanese.Size = new System.Drawing.Size(40, 22);
            chkLanJapanese.TabIndex = 19;
            chkLanJapanese.Text = "JP";
            chkLanJapanese.UseVisualStyleBackColor = true;
            // 
            // chkLanChinese
            // 
            chkLanChinese.AutoSize = true;
            chkLanChinese.Location = new System.Drawing.Point(220, 200);
            chkLanChinese.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            chkLanChinese.Name = "chkLanChinese";
            chkLanChinese.Size = new System.Drawing.Size(44, 22);
            chkLanChinese.TabIndex = 21;
            chkLanChinese.Text = "CH";
            chkLanChinese.UseVisualStyleBackColor = true;
            // 
            // chkLanOther
            // 
            chkLanOther.AutoSize = true;
            chkLanOther.Location = new System.Drawing.Point(280, 200);
            chkLanOther.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            chkLanOther.Name = "chkLanOther";
            chkLanOther.Size = new System.Drawing.Size(63, 22);
            chkLanOther.TabIndex = 22;
            chkLanOther.Text = "Other";
            chkLanOther.UseVisualStyleBackColor = true;
            // 
            // chkIsReadTrue
            // 
            chkIsReadTrue.AutoSize = true;
            chkIsReadTrue.Location = new System.Drawing.Point(162, 380);
            chkIsReadTrue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            chkIsReadTrue.Name = "chkIsReadTrue";
            chkIsReadTrue.Size = new System.Drawing.Size(58, 22);
            chkIsReadTrue.TabIndex = 26;
            chkIsReadTrue.Text = "Read";
            chkIsReadTrue.UseVisualStyleBackColor = true;
            // 
            // pctCover
            // 
            pctCover.ImageLocation = "";
            pctCover.Location = new System.Drawing.Point(0, 0);
            pctCover.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pctCover.Name = "pctCover";
            pctCover.Size = new System.Drawing.Size(620, 840);
            pctCover.TabIndex = 53;
            pctCover.TabStop = false;
            pctCover.MouseHover += PctCover_Hover;
            // 
            // btnPrev
            // 
            btnPrev.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            btnPrev.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Bold);
            btnPrev.Location = new System.Drawing.Point(61, 470);
            btnPrev.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new System.Drawing.Size(71, 52);
            btnPrev.TabIndex = 54;
            btnPrev.Text = "PREV";
            btnPrev.UseVisualStyleBackColor = false;
            btnPrev.Click += BtnPrev_Click;
            // 
            // groupAlbum
            // 
            groupAlbum.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            groupAlbum.Controls.Add(btnPostDel);
            groupAlbum.Controls.Add(chkClamp);
            groupAlbum.Controls.Add(txtUpscaleTarget);
            groupAlbum.Controls.Add(listViewProgress);
            groupAlbum.Controls.Add(tabControl1);
            groupAlbum.Controls.Add(btnSrcAdd);
            groupAlbum.Controls.Add(txtSrcTitle);
            groupAlbum.Controls.Add(txtSrcSubtitle);
            groupAlbum.Controls.Add(txtSrcJson);
            groupAlbum.Controls.Add(btnPopupCharacters);
            groupAlbum.Controls.Add(txtCharacters);
            groupAlbum.Controls.Add(label10);
            groupAlbum.Controls.Add(btnHp);
            groupAlbum.Controls.Add(txtTitle);
            groupAlbum.Controls.Add(btnPostMetadata);
            groupAlbum.Controls.Add(btnTierPlus);
            groupAlbum.Controls.Add(btnTierMin);
            groupAlbum.Controls.Add(btnProcess);
            groupAlbum.Controls.Add(btnPost);
            groupAlbum.Controls.Add(btnPopupTags);
            groupAlbum.Controls.Add(btnExplore);
            groupAlbum.Controls.Add(rbOri);
            groupAlbum.Controls.Add(rbCat);
            groupAlbum.Controls.Add(label9);
            groupAlbum.Controls.Add(txtTier);
            groupAlbum.Controls.Add(txtTags);
            groupAlbum.Controls.Add(chkIsWipTrue);
            groupAlbum.Controls.Add(label5);
            groupAlbum.Controls.Add(groupPicture);
            groupAlbum.Controls.Add(btnNext);
            groupAlbum.Controls.Add(btnBrowse);
            groupAlbum.Controls.Add(btnPrev);
            groupAlbum.Controls.Add(chkIsReadTrue);
            groupAlbum.Controls.Add(chkLanOther);
            groupAlbum.Controls.Add(chkLanChinese);
            groupAlbum.Controls.Add(chkLanJapanese);
            groupAlbum.Controls.Add(chkLanEnglish);
            groupAlbum.Controls.Add(btnCreate);
            groupAlbum.Controls.Add(txtNote);
            groupAlbum.Controls.Add(txtArtists);
            groupAlbum.Controls.Add(label8);
            groupAlbum.Controls.Add(label7);
            groupAlbum.Controls.Add(label6);
            groupAlbum.Controls.Add(label4);
            groupAlbum.Controls.Add(label3);
            groupAlbum.Controls.Add(label2);
            groupAlbum.Controls.Add(label1);
            groupAlbum.Font = new System.Drawing.Font("Calibri", 11.25F);
            groupAlbum.ForeColor = System.Drawing.SystemColors.HighlightText;
            groupAlbum.Location = new System.Drawing.Point(3, -5);
            groupAlbum.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupAlbum.Name = "groupAlbum";
            groupAlbum.Padding = new System.Windows.Forms.Padding(0);
            groupAlbum.Size = new System.Drawing.Size(1051, 870);
            groupAlbum.TabIndex = 0;
            groupAlbum.TabStop = false;
            groupAlbum.DragDrop += FormMain_DragDrop;
            groupAlbum.DragEnter += FormMain_DragEnter;
            // 
            // btnPostDel
            // 
            btnPostDel.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            btnPostDel.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Bold);
            btnPostDel.Location = new System.Drawing.Point(357, 525);
            btnPostDel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnPostDel.Name = "btnPostDel";
            btnPostDel.Size = new System.Drawing.Size(60, 52);
            btnPostDel.TabIndex = 100;
            btnPostDel.Text = "PD";
            btnPostDel.UseVisualStyleBackColor = false;
            btnPostDel.Click += btnPostDel_Click;
            // 
            // chkClamp
            // 
            chkClamp.AutoSize = true;
            chkClamp.Checked = true;
            chkClamp.CheckState = System.Windows.Forms.CheckState.Checked;
            chkClamp.Location = new System.Drawing.Point(6, 500);
            chkClamp.Name = "chkClamp";
            chkClamp.Size = new System.Drawing.Size(51, 22);
            chkClamp.TabIndex = 99;
            chkClamp.Text = "Clm";
            chkClamp.UseVisualStyleBackColor = true;
            // 
            // txtUpscaleTarget
            // 
            txtUpscaleTarget.BackColor = System.Drawing.SystemColors.ControlLight;
            txtUpscaleTarget.Location = new System.Drawing.Point(6, 472);
            txtUpscaleTarget.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtUpscaleTarget.Name = "txtUpscaleTarget";
            txtUpscaleTarget.Size = new System.Drawing.Size(50, 26);
            txtUpscaleTarget.TabIndex = 98;
            txtUpscaleTarget.Text = "1600";
            // 
            // listViewProgress
            // 
            listViewProgress.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1 });
            listViewProgress.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            listViewProgress.Location = new System.Drawing.Point(6, 796);
            listViewProgress.Name = "listViewProgress";
            listViewProgress.Size = new System.Drawing.Size(411, 64);
            listViewProgress.TabIndex = 97;
            listViewProgress.UseCompatibleStateImageBehavior = false;
            listViewProgress.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Width = 400;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new System.Drawing.Point(6, 583);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(411, 207);
            tabControl1.TabIndex = 96;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(listViewSrc);
            tabPage1.Location = new System.Drawing.Point(4, 27);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new System.Windows.Forms.Padding(3);
            tabPage1.Size = new System.Drawing.Size(403, 176);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Files to Process";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // listViewSrc
            // 
            listViewSrc.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { srcColFile, srcColSize, srcColDimension, srcColBytesPerPixel, srcUploadStatus });
            listViewSrc.Location = new System.Drawing.Point(0, 2);
            listViewSrc.Name = "listViewSrc";
            listViewSrc.Size = new System.Drawing.Size(403, 174);
            listViewSrc.TabIndex = 3;
            listViewSrc.UseCompatibleStateImageBehavior = false;
            listViewSrc.View = System.Windows.Forms.View.Details;
            // 
            // srcColFile
            // 
            srcColFile.Text = "File Name";
            srcColFile.Width = 135;
            // 
            // srcColSize
            // 
            srcColSize.Text = "Size";
            // 
            // srcColDimension
            // 
            srcColDimension.Text = "Dim";
            srcColDimension.Width = 80;
            // 
            // srcColBytesPerPixel
            // 
            srcColBytesPerPixel.Text = "Bph";
            srcColBytesPerPixel.Width = 40;
            // 
            // srcUploadStatus
            // 
            srcUploadStatus.Text = "U";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(listViewDst);
            tabPage2.Location = new System.Drawing.Point(4, 27);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new System.Windows.Forms.Padding(3);
            tabPage2.Size = new System.Drawing.Size(403, 176);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Processed Files";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // listViewDst
            // 
            listViewDst.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { dstColFile, dstColSize, dstColDimension, dstColBytesPerPixel, dstUploadStatus });
            listViewDst.Location = new System.Drawing.Point(0, 2);
            listViewDst.Name = "listViewDst";
            listViewDst.Size = new System.Drawing.Size(403, 174);
            listViewDst.TabIndex = 0;
            listViewDst.UseCompatibleStateImageBehavior = false;
            listViewDst.View = System.Windows.Forms.View.Details;
            // 
            // dstColFile
            // 
            dstColFile.Text = "File Name";
            dstColFile.Width = 135;
            // 
            // dstColSize
            // 
            dstColSize.Text = "Size";
            // 
            // dstColDimension
            // 
            dstColDimension.Text = "Dim";
            dstColDimension.Width = 80;
            // 
            // dstColBytesPerPixel
            // 
            dstColBytesPerPixel.Text = "Bph";
            dstColBytesPerPixel.Width = 40;
            // 
            // dstUploadStatus
            // 
            dstUploadStatus.Text = "U";
            // 
            // btnSrcAdd
            // 
            btnSrcAdd.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            btnSrcAdd.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold);
            btnSrcAdd.Location = new System.Drawing.Point(300, 410);
            btnSrcAdd.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnSrcAdd.Name = "btnSrcAdd";
            btnSrcAdd.Size = new System.Drawing.Size(52, 56);
            btnSrcAdd.TabIndex = 95;
            btnSrcAdd.Text = "UPDT";
            btnSrcAdd.UseVisualStyleBackColor = false;
            btnSrcAdd.Click += btnSrcAdd_Click;
            // 
            // txtSrcTitle
            // 
            txtSrcTitle.BackColor = System.Drawing.SystemColors.ControlLight;
            txtSrcTitle.Location = new System.Drawing.Point(6, 440);
            txtSrcTitle.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtSrcTitle.Name = "txtSrcTitle";
            txtSrcTitle.Size = new System.Drawing.Size(286, 26);
            txtSrcTitle.TabIndex = 94;
            // 
            // txtSrcSubtitle
            // 
            txtSrcSubtitle.BackColor = System.Drawing.SystemColors.ControlLight;
            txtSrcSubtitle.Location = new System.Drawing.Point(100, 410);
            txtSrcSubtitle.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtSrcSubtitle.Name = "txtSrcSubtitle";
            txtSrcSubtitle.Size = new System.Drawing.Size(192, 26);
            txtSrcSubtitle.TabIndex = 93;
            // 
            // txtSrcJson
            // 
            txtSrcJson.BackColor = System.Drawing.SystemColors.ControlLight;
            txtSrcJson.Location = new System.Drawing.Point(6, 410);
            txtSrcJson.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtSrcJson.MaxLength = 327670;
            txtSrcJson.Multiline = true;
            txtSrcJson.Name = "txtSrcJson";
            txtSrcJson.Size = new System.Drawing.Size(87, 26);
            txtSrcJson.TabIndex = 92;
            txtSrcJson.TextChanged += txtSrcJson_TextChanged;
            // 
            // btnPopupCharacters
            // 
            btnPopupCharacters.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            btnPopupCharacters.Location = new System.Drawing.Point(300, 320);
            btnPopupCharacters.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnPopupCharacters.Name = "btnPopupCharacters";
            btnPopupCharacters.Size = new System.Drawing.Size(52, 28);
            btnPopupCharacters.TabIndex = 91;
            btnPopupCharacters.Text = "Chars";
            btnPopupCharacters.UseVisualStyleBackColor = false;
            btnPopupCharacters.Click += btnPopupCharacters_Click;
            // 
            // txtCharacters
            // 
            txtCharacters.BackColor = System.Drawing.SystemColors.ControlLight;
            txtCharacters.Location = new System.Drawing.Point(100, 320);
            txtCharacters.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtCharacters.Name = "txtCharacters";
            txtCharacters.Size = new System.Drawing.Size(192, 26);
            txtCharacters.TabIndex = 90;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(6, 320);
            label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(73, 18);
            label10.TabIndex = 89;
            label10.Text = "Characters";
            // 
            // btnHp
            // 
            btnHp.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            btnHp.Location = new System.Drawing.Point(300, 290);
            btnHp.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnHp.Name = "btnHp";
            btnHp.Size = new System.Drawing.Size(52, 28);
            btnHp.TabIndex = 88;
            btnHp.Text = "HP";
            btnHp.UseVisualStyleBackColor = false;
            btnHp.Click += btnHp_Click;
            // 
            // txtTitle
            // 
            txtTitle.BackColor = System.Drawing.SystemColors.ControlLight;
            txtTitle.Location = new System.Drawing.Point(100, 20);
            txtTitle.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtTitle.Name = "txtTitle";
            txtTitle.Size = new System.Drawing.Size(252, 84);
            txtTitle.TabIndex = 85;
            txtTitle.Text = "";
            // 
            // btnPostMetadata
            // 
            btnPostMetadata.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            btnPostMetadata.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            btnPostMetadata.Location = new System.Drawing.Point(138, 545);
            btnPostMetadata.Name = "btnPostMetadata";
            btnPostMetadata.Size = new System.Drawing.Size(137, 32);
            btnPostMetadata.TabIndex = 84;
            btnPostMetadata.Text = "POST METADATA";
            btnPostMetadata.UseVisualStyleBackColor = false;
            btnPostMetadata.Click += btnPostMetadata_Click;
            // 
            // btnTierPlus
            // 
            btnTierPlus.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            btnTierPlus.Location = new System.Drawing.Point(197, 350);
            btnTierPlus.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnTierPlus.Name = "btnTierPlus";
            btnTierPlus.Size = new System.Drawing.Size(30, 26);
            btnTierPlus.TabIndex = 82;
            btnTierPlus.Text = "+";
            btnTierPlus.UseVisualStyleBackColor = false;
            btnTierPlus.Click += btnTierPlus_Click;
            // 
            // btnTierMin
            // 
            btnTierMin.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            btnTierMin.Location = new System.Drawing.Point(162, 350);
            btnTierMin.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnTierMin.Name = "btnTierMin";
            btnTierMin.Size = new System.Drawing.Size(30, 26);
            btnTierMin.TabIndex = 81;
            btnTierMin.Text = "-";
            btnTierMin.UseVisualStyleBackColor = false;
            btnTierMin.Click += btnTierMin_Click;
            // 
            // btnProcess
            // 
            btnProcess.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            btnProcess.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            btnProcess.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            btnProcess.Location = new System.Drawing.Point(6, 525);
            btnProcess.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnProcess.Name = "btnProcess";
            btnProcess.Size = new System.Drawing.Size(50, 52);
            btnProcess.TabIndex = 80;
            btnProcess.Text = "PRO";
            btnProcess.UseVisualStyleBackColor = false;
            btnProcess.Click += btnProcess_Click;
            // 
            // btnPost
            // 
            btnPost.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            btnPost.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Bold);
            btnPost.Location = new System.Drawing.Point(281, 525);
            btnPost.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnPost.Name = "btnPost";
            btnPost.Size = new System.Drawing.Size(71, 52);
            btnPost.TabIndex = 78;
            btnPost.Text = "POST";
            btnPost.UseVisualStyleBackColor = false;
            btnPost.Click += btnPost_Click;
            // 
            // btnPopupTags
            // 
            btnPopupTags.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            btnPopupTags.Location = new System.Drawing.Point(300, 140);
            btnPopupTags.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnPopupTags.Name = "btnPopupTags";
            btnPopupTags.Size = new System.Drawing.Size(52, 56);
            btnPopupTags.TabIndex = 77;
            btnPopupTags.Text = "Tags";
            btnPopupTags.UseVisualStyleBackColor = false;
            btnPopupTags.Click += btnPopupTags_Click;
            // 
            // btnExplore
            // 
            btnExplore.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            btnExplore.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            btnExplore.Location = new System.Drawing.Point(138, 470);
            btnExplore.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnExplore.Name = "btnExplore";
            btnExplore.Size = new System.Drawing.Size(137, 32);
            btnExplore.TabIndex = 76;
            btnExplore.Text = "OPEN FOLDER";
            btnExplore.UseVisualStyleBackColor = false;
            btnExplore.Click += btnExplore_Click;
            // 
            // rbOri
            // 
            rbOri.Controls.Add(rbOriAuto);
            rbOri.Controls.Add(rbOriPortrait);
            rbOri.Controls.Add(rbOriLandscape);
            rbOri.Location = new System.Drawing.Point(100, 260);
            rbOri.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rbOri.Name = "rbOri";
            rbOri.Size = new System.Drawing.Size(252, 25);
            rbOri.TabIndex = 75;
            // 
            // rbOriAuto
            // 
            rbOriAuto.AutoSize = true;
            rbOriAuto.Location = new System.Drawing.Point(120, 0);
            rbOriAuto.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rbOriAuto.Name = "rbOriAuto";
            rbOriAuto.Size = new System.Drawing.Size(48, 22);
            rbOriAuto.TabIndex = 71;
            rbOriAuto.TabStop = true;
            rbOriAuto.Text = "Aut";
            rbOriAuto.UseVisualStyleBackColor = true;
            // 
            // rbOriPortrait
            // 
            rbOriPortrait.AutoSize = true;
            rbOriPortrait.Location = new System.Drawing.Point(0, 0);
            rbOriPortrait.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rbOriPortrait.Name = "rbOriPortrait";
            rbOriPortrait.Size = new System.Drawing.Size(44, 22);
            rbOriPortrait.TabIndex = 69;
            rbOriPortrait.TabStop = true;
            rbOriPortrait.Text = "Ptr";
            rbOriPortrait.UseVisualStyleBackColor = true;
            // 
            // rbOriLandscape
            // 
            rbOriLandscape.AutoSize = true;
            rbOriLandscape.Location = new System.Drawing.Point(60, 0);
            rbOriLandscape.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rbOriLandscape.Name = "rbOriLandscape";
            rbOriLandscape.Size = new System.Drawing.Size(44, 22);
            rbOriLandscape.TabIndex = 70;
            rbOriLandscape.TabStop = true;
            rbOriLandscape.Text = "Lsc";
            rbOriLandscape.UseVisualStyleBackColor = true;
            // 
            // rbCat
            // 
            rbCat.Location = new System.Drawing.Point(100, 230);
            rbCat.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rbCat.Name = "rbCat";
            rbCat.Size = new System.Drawing.Size(252, 25);
            rbCat.TabIndex = 74;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(6, 260);
            label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(79, 18);
            label9.TabIndex = 68;
            label9.Text = "Orientation";
            // 
            // txtTier
            // 
            txtTier.BackColor = System.Drawing.SystemColors.ControlLight;
            txtTier.Location = new System.Drawing.Point(100, 350);
            txtTier.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtTier.Name = "txtTier";
            txtTier.Size = new System.Drawing.Size(54, 26);
            txtTier.TabIndex = 67;
            txtTier.Text = "0";
            // 
            // txtTags
            // 
            txtTags.BackColor = System.Drawing.SystemColors.ControlLight;
            txtTags.Location = new System.Drawing.Point(100, 140);
            txtTags.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtTags.Name = "txtTags";
            txtTags.Size = new System.Drawing.Size(192, 56);
            txtTags.TabIndex = 66;
            txtTags.Text = "";
            // 
            // chkIsWipTrue
            // 
            chkIsWipTrue.AutoSize = true;
            chkIsWipTrue.Location = new System.Drawing.Point(100, 380);
            chkIsWipTrue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            chkIsWipTrue.Name = "chkIsWipTrue";
            chkIsWipTrue.Size = new System.Drawing.Size(52, 22);
            chkIsWipTrue.TabIndex = 61;
            chkIsWipTrue.Text = "WIP";
            chkIsWipTrue.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(6, 380);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(39, 18);
            label5.TabIndex = 60;
            label5.Text = "Flags";
            // 
            // groupPicture
            // 
            groupPicture.Controls.Add(lbPage);
            groupPicture.Controls.Add(pctCover);
            groupPicture.Location = new System.Drawing.Point(424, 20);
            groupPicture.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupPicture.Name = "groupPicture";
            groupPicture.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupPicture.Size = new System.Drawing.Size(620, 840);
            groupPicture.TabIndex = 59;
            groupPicture.TabStop = false;
            // 
            // lbPage
            // 
            lbPage.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lbPage.BackColor = System.Drawing.Color.Transparent;
            lbPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            lbPage.ForeColor = System.Drawing.SystemColors.MenuText;
            lbPage.Location = new System.Drawing.Point(275, 814);
            lbPage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbPage.Name = "lbPage";
            lbPage.Size = new System.Drawing.Size(70, 26);
            lbPage.TabIndex = 56;
            lbPage.Text = "0/0";
            lbPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnNext
            // 
            btnNext.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            btnNext.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Bold);
            btnNext.Location = new System.Drawing.Point(281, 470);
            btnNext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnNext.Name = "btnNext";
            btnNext.Size = new System.Drawing.Size(71, 52);
            btnNext.TabIndex = 58;
            btnNext.Text = "NEXT";
            btnNext.UseVisualStyleBackColor = false;
            btnNext.Click += BtnNext_Click;
            // 
            // btnBrowse
            // 
            btnBrowse.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            btnBrowse.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Bold);
            btnBrowse.Location = new System.Drawing.Point(138, 507);
            btnBrowse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new System.Drawing.Size(137, 32);
            btnBrowse.TabIndex = 57;
            btnBrowse.Text = "BROWSE";
            btnBrowse.UseVisualStyleBackColor = false;
            btnBrowse.Click += BtnBrowse_Click;
            // 
            // txtNote
            // 
            txtNote.BackColor = System.Drawing.SystemColors.ControlLight;
            txtNote.Location = new System.Drawing.Point(100, 290);
            txtNote.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtNote.Name = "txtNote";
            txtNote.Size = new System.Drawing.Size(192, 26);
            txtNote.TabIndex = 16;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.ControlDarkDark;
            ClientSize = new System.Drawing.Size(1056, 867);
            Controls.Add(groupAlbum);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "FormMain";
            Text = "Metadata Editor";
            Load += FormMain_Load;
            ((System.ComponentModel.ISupportInitialize)pctCover).EndInit();
            groupAlbum.ResumeLayout(false);
            groupAlbum.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            rbOri.ResumeLayout(false);
            rbOri.PerformLayout();
            groupPicture.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtArtists;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.CheckBox chkLanEnglish;
        private System.Windows.Forms.CheckBox chkLanJapanese;
        private System.Windows.Forms.CheckBox chkLanChinese;
        private System.Windows.Forms.CheckBox chkLanOther;
        private System.Windows.Forms.CheckBox chkIsReadTrue;
        private System.Windows.Forms.PictureBox pctCover;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.GroupBox groupAlbum;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.GroupBox groupPicture;
        private System.Windows.Forms.Label lbPage;
        private System.Windows.Forms.CheckBox chkIsWipTrue;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RichTextBox txtTags;
        private System.Windows.Forms.TextBox txtTier;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtNote;
        private System.Windows.Forms.Panel rbCat;
        private System.Windows.Forms.Panel rbOri;
        private System.Windows.Forms.RadioButton rbOriPortrait;
        private System.Windows.Forms.RadioButton rbOriLandscape;
        private System.Windows.Forms.Button btnExplore;
        private System.Windows.Forms.Button btnPopupTags;
        private System.Windows.Forms.Button btnPost;
        private System.DirectoryServices.DirectoryEntry directoryEntry1;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.Button btnTierPlus;
        private System.Windows.Forms.Button btnTierMin;
        private System.Windows.Forms.Button btnPostMetadata;
        private System.Windows.Forms.RichTextBox txtTitle;
        private System.Windows.Forms.Button btnHp;
        private System.Windows.Forms.Button btnPopupCharacters;
        private System.Windows.Forms.TextBox txtCharacters;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.RadioButton rbOriAuto;
        private System.Windows.Forms.TextBox txtSrcSubtitle;
        private System.Windows.Forms.TextBox txtSrcJson;
        private System.Windows.Forms.Button btnSrcAdd;
        private System.Windows.Forms.TextBox txtSrcTitle;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ListView listViewSrc;
        private System.Windows.Forms.ColumnHeader srcColFile;
        private System.Windows.Forms.ColumnHeader srcColSize;
        private System.Windows.Forms.ColumnHeader srcColDimension;
        private System.Windows.Forms.ColumnHeader srcColBytesPerPixel;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListView listViewDst;
        private System.Windows.Forms.ColumnHeader dstColFile;
        private System.Windows.Forms.ColumnHeader dstColSize;
        private System.Windows.Forms.ColumnHeader dstColDimension;
        private System.Windows.Forms.ColumnHeader dstColBytesPerPixel;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.ColumnHeader srcUploadStatus;
        private System.Windows.Forms.ColumnHeader dstUploadStatus;
        private System.Windows.Forms.Label labelDebug;
        private System.Windows.Forms.ListView listViewProgress;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.TextBox txtUpscaleTarget;
        private System.Windows.Forms.CheckBox chkClamp;
        private System.Windows.Forms.Button btnPostDel;
    }
}