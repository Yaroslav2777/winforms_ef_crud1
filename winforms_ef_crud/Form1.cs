using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace winforms_ef_crud
{
    public partial class Form1 : Form
    {
        // Long-lived DbContext for the form lifetime to keep entities tracked and simplify CRUD operations
        private FitnessDbContext _context;

        // Central BindingList that will be used as the DataSource for multiple UI controls to keep them in sync
        private BindingList<FitnessActivity> _bindingList;

        // UI controls (created in InitializeComponent)
        private TabControl tabControl;
        private TabPage tabManage;
        private TabPage tabListView;
        private TabPage tabGrid;

        private ListBox lbActivities;
        private ComboBox cbActivities;
        private TextBox txtTitle;
        private NumericUpDown numDuration;
        private NumericUpDown numCalories;
        private ComboBox cbIntensity;
        private DateTimePicker dtpDate;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;

        private ListView lvActivities;
        private DataGridView dgvActivities;

        public Form1()
        {
            // Call designer-generated InitializeComponent first, then set up custom UI components
            InitializeComponent();
            InitializeCustomComponents();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Recreate database on every run for demonstration purposes
            using (var seedContext = new FitnessDbContext())
            {
                seedContext.Database.EnsureDeleted();
                seedContext.Database.EnsureCreated();

                // Seed initial data if empty
                if (!seedContext.Activities.Any())
                {
                    var now = DateTime.Now.Date;
                    seedContext.Activities.AddRange(
                        new FitnessActivity { Title = "Ранкова пробіжка", Duration = 30, CaloriesBurned = 250, Intensity = "Середня", Date = now.AddDays(-2) },
                        new FitnessActivity { Title = "Йога", Duration = 45, CaloriesBurned = 180, Intensity = "Низька", Date = now.AddDays(-1) },
                        new FitnessActivity { Title = "Інтервальний тренінг", Duration = 20, CaloriesBurned = 300, Intensity = "Висока", Date = now }
                    );
                    seedContext.SaveChanges();
                }
            }

            // Create a DbContext for the form lifetime (keeps change tracking simple)
            _context = new FitnessDbContext();

            // Load data into UI
            RefreshData();
        }

        /// <summary>
        /// Centralized method to load data from EF Core into a BindingList and bind it to all UI controls.
        /// This ensures that the ListBox, ComboBox, ListView, and DataGridView stay in sync after any CRUD operation.
        /// </summary>
        private void RefreshData()
        {
            // Load activities from the database (ordered by Date desc for nicer presentation)
            var list = _context.Activities.OrderByDescending(a => a.Date).ToList();

            // Create a new BindingList from the loaded entities. BindingList provides change notifications for UI controls.
            _bindingList = new BindingList<FitnessActivity>(list);

            // Bind the same BindingList instance to multiple controls so selection/updates reflect everywhere.
            lbActivities.DataSource = _bindingList;
            lbActivities.DisplayMember = "Title";
            lbActivities.ValueMember = "FitnessActivityId";

            // A separate BindingSource wrapper is used for the ComboBox and DataGridView to avoid interfering with ListBox selection
            cbActivities.DataSource = new BindingSource(_bindingList, null);
            cbActivities.DisplayMember = "Title";
            cbActivities.ValueMember = "FitnessActivityId";

            dgvActivities.DataSource = new BindingSource(_bindingList, null);
            dgvActivities.AutoGenerateColumns = true;
            dgvActivities.Refresh();

            // Refresh ListView manually (ListView does not support complex data binding the same way)
            lvActivities.BeginUpdate();
            lvActivities.Items.Clear();
            foreach (var a in _bindingList)
            {
                var item = new ListViewItem(a.FitnessActivityId.ToString());
                item.SubItems.Add(a.Title);
                item.SubItems.Add(a.Duration.ToString());
                item.SubItems.Add(a.CaloriesBurned.ToString());
                item.SubItems.Add(a.Intensity);
                lvActivities.Items.Add(item);
            }
            lvActivities.EndUpdate();
        }

        /// <summary>
        /// Called when the selected item in the ListBox changes. Populates the edit fields with the selected activity data.
        /// </summary>
        private void LbActivities_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = lbActivities.SelectedItem as FitnessActivity;
            if (selected != null)
            {
                // Populate fields for editing
                txtTitle.Text = selected.Title;
                numDuration.Value = Math.Min(numDuration.Maximum, Math.Max(numDuration.Minimum, selected.Duration));
                numCalories.Value = Math.Min(numCalories.Maximum, Math.Max(numCalories.Minimum, selected.CaloriesBurned));
                cbIntensity.SelectedItem = selected.Intensity;
                dtpDate.Value = selected.Date;
            }
        }

        /// <summary>
        /// Create (Add) operation: reads input fields, creates a new entity, saves via EF Core, and refreshes UI.
        /// </summary>
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var activity = new FitnessActivity
            {
                Title = txtTitle.Text.Trim(),
                Duration = (int)numDuration.Value,
                CaloriesBurned = (int)numCalories.Value,
                Intensity = cbIntensity.SelectedItem?.ToString() ?? string.Empty,
                Date = dtpDate.Value.Date
            };

            _context.Activities.Add(activity);
            _context.SaveChanges();

            RefreshData();

            // Select the newly added item in the ListBox
            lbActivities.SelectedItem = _bindingList.FirstOrDefault(a => a.FitnessActivityId == activity.FitnessActivityId);
        }

        /// <summary>
        /// Update operation: modifies the selected entity and saves changes via EF Core, then refreshes UI.
        /// </summary>
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            var selected = lbActivities.SelectedItem as FitnessActivity;
            if (selected == null)
            {
                MessageBox.Show("Оберіть активність для редагування.", "Інформація", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Retrieve the tracked entity from the DbContext to ensure change tracking works reliably
            var tracked = _context.Activities.Find(selected.FitnessActivityId);
            if (tracked == null)
            {
                MessageBox.Show("Вибрана активність не знайдена в контексті.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            tracked.Title = txtTitle.Text.Trim();
            tracked.Duration = (int)numDuration.Value;
            tracked.CaloriesBurned = (int)numCalories.Value;
            tracked.Intensity = cbIntensity.SelectedItem?.ToString() ?? string.Empty;
            tracked.Date = dtpDate.Value.Date;

            _context.SaveChanges();
            RefreshData();

            // Re-select the updated item so the UI stays focused
            lbActivities.SelectedItem = _bindingList.FirstOrDefault(a => a.FitnessActivityId == tracked.FitnessActivityId);
        }

        /// <summary>
        /// Delete operation: removes the selected entity from the database and refreshes UI.
        /// </summary>
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var selected = lbActivities.SelectedItem as FitnessActivity;
            if (selected == null)
            {
                MessageBox.Show("Оберіть активність для видалення.", "Інформація", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show($"Видалити активність '{selected.Title}'?", "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var tracked = _context.Activities.Find(selected.FitnessActivityId);
            if (tracked != null)
            {
                _context.Activities.Remove(tracked);
                _context.SaveChanges();
                RefreshData();
            }
        }

        /// <summary>
        /// Builds all UI controls and wire up events. Kept simple and self-contained to avoid designer files for this demo.
        /// </summary>
        private void InitializeCustomComponents()
        {
            // Form settings
            this.Text = "Фітнес трекер";
            this.ClientSize = new Size(800, 600);

            // Tab control
            tabControl = new TabControl { Dock = DockStyle.Fill };
            tabManage = new TabPage("Керування");
            tabListView = new TabPage("Перегляд ListView");
            tabGrid = new TabPage("Таблиця DataGridView");

            tabControl.TabPages.AddRange(new[] { tabManage, tabListView, tabGrid });
            this.Controls.Add(tabControl);

            // --- Tab: Manage ---
            lbActivities = new ListBox { Location = new Point(10, 10), Size = new Size(240, 400) };
            lbActivities.SelectedIndexChanged += LbActivities_SelectedIndexChanged;

            cbActivities = new ComboBox { Location = new Point(260, 10), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            var lblTitle = new Label { Text = "Назва:", Location = new Point(260, 50), AutoSize = true };
            txtTitle = new TextBox { Location = new Point(320, 46), Width = 300 };

            var lblDuration = new Label { Text = "Тривалість (хв):", Location = new Point(260, 86), AutoSize = true };
            numDuration = new NumericUpDown { Location = new Point(360, 82), Width = 80, Minimum = 1, Maximum = 1000, Value = 30 };

            var lblCalories = new Label { Text = "Калорії:", Location = new Point(460, 86), AutoSize = true };
            numCalories = new NumericUpDown { Location = new Point(520, 82), Width = 80, Minimum = 0, Maximum = 10000, Value = 200 };

            var lblIntensity = new Label { Text = "Інтенсивність:", Location = new Point(260, 122), AutoSize = true };
            cbIntensity = new ComboBox { Location = new Point(360, 118), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cbIntensity.Items.AddRange(new[] { "Низька", "Середня", "Висока" });
            cbIntensity.SelectedIndex = 1; // default: Середня

            var lblDate = new Label { Text = "Дата:", Location = new Point(260, 158), AutoSize = true };
            dtpDate = new DateTimePicker { Location = new Point(320, 154), Width = 140, Format = DateTimePickerFormat.Short };

            btnAdd = new Button { Text = "Додати", Location = new Point(260, 200), Width = 100 };
            btnUpdate = new Button { Text = "Редагувати", Location = new Point(370, 200), Width = 100 };
            btnDelete = new Button { Text = "Видалити", Location = new Point(480, 200), Width = 100 };

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;

            tabManage.Controls.Add(lbActivities);
            tabManage.Controls.Add(cbActivities);
            tabManage.Controls.Add(lblTitle);
            tabManage.Controls.Add(txtTitle);
            tabManage.Controls.Add(lblDuration);
            tabManage.Controls.Add(numDuration);
            tabManage.Controls.Add(lblCalories);
            tabManage.Controls.Add(numCalories);
            tabManage.Controls.Add(lblIntensity);
            tabManage.Controls.Add(cbIntensity);
            tabManage.Controls.Add(lblDate);
            tabManage.Controls.Add(dtpDate);
            tabManage.Controls.Add(btnAdd);
            tabManage.Controls.Add(btnUpdate);
            tabManage.Controls.Add(btnDelete);

            // --- Tab: ListView ---
            lvActivities = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true };
            lvActivities.Columns.Add("ID", 60, HorizontalAlignment.Left);
            lvActivities.Columns.Add("Назва", 220, HorizontalAlignment.Left);
            lvActivities.Columns.Add("Хвилини", 80, HorizontalAlignment.Right);
            lvActivities.Columns.Add("Калорії", 80, HorizontalAlignment.Right);
            lvActivities.Columns.Add("Інтенсивність", 120, HorizontalAlignment.Left);
            tabListView.Controls.Add(lvActivities);

            // --- Tab: DataGridView ---
            dgvActivities = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false };
            tabGrid.Controls.Add(dgvActivities);
        }
    }
}
