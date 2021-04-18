﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CarRentalApp.Core.domain;
using CarRentalApp.Core.Utils;
using CarRentalApp.Persistence;
using CarRentalApp.Properties;
using CarRentalApp.View.Forms;

namespace CarRentalApp.View.UserControls
{
    public partial class ClientsPageUserControl : UserControl
    {
        private readonly string _searchTextBoxDefaultText;
        private readonly UnitOfWork _unitOfWork;

        public ClientsPageUserControl()
        {
            InitializeComponent();
            _searchTextBoxDefaultText = searchTextBox.Text.Trim();
            _unitOfWork = UnitOfWork.Instance;
        }

        private void RefreshDataGridView()
        {
            var clients = _unitOfWork.Clients.GetAll().OrderBy(c => c.FirstName).ToList();
            clientBindingSource.DataSource = clients;
        }

        private void Search(string keyword)
        {
            if (keyword == _searchTextBoxDefaultText)
                keyword = string.Empty;

            var allClients = _unitOfWork.Clients.GetAll().ToList();
            var all = searchFilterComboBox.SelectedIndex == searchFilterComboBox.Items.Count - 1;
            var filterBy = searchFilterComboBox.SelectedText;

            List<Client> filteredClients;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                filteredClients = allClients.FindAll(c =>
                    c.FirstName.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
                    (filterBy == firstNameDataGridViewTextBoxColumn.HeaderText || all)
                    || c.LastName.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
                    (filterBy == lastNameDataGridViewTextBoxColumn.HeaderText || all)
                    || c.Cin.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
                    (filterBy == cinDataGridViewTextBoxColumn.HeaderText || all)
                    || c.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
                    (filterBy == emailDataGridViewTextBoxColumn.HeaderText || all)
                    || c.Phone.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
                    (filterBy == phoneDataGridViewTextBoxColumn.HeaderText || all)
                    || c.DriverLicense.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
                    (filterBy == driverLicenseDataGridViewTextBoxColumn.HeaderText || all)
                    || c.Address.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
                    (filterBy == addressDataGridViewTextBoxColumn.HeaderText || all)
                );
            }
            else
            {
                filteredClients = allClients;
            }

            clientDataGridView.DataSource = filteredClients;
        }

        private void OnClientFromClosed(Client theClient)
        {
            if (theClient == null) return;

            RefreshDataGridView();
            foreach (DataGridViewRow row in clientDataGridView.Rows)
            {
                var client = (Client) row.DataBoundItem;
                if (client == null) continue;

                if (theClient.Id == client.Id)
                {
                    row.Selected = true;
                    return;
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            RefreshDataGridView();
            searchFilterComboBox.DataSource = new[]
            {
                firstNameDataGridViewTextBoxColumn.HeaderText,
                lastNameDataGridViewTextBoxColumn.HeaderText,
                cinDataGridViewTextBoxColumn.HeaderText,
                driverLicenseDataGridViewTextBoxColumn.HeaderText,
                emailDataGridViewTextBoxColumn.HeaderText,
                phoneDataGridViewTextBoxColumn.HeaderText,
                addressDataGridViewTextBoxColumn.HeaderText,
                "All"
            };
            searchFilterComboBox.SelectedIndex = searchFilterComboBox.Items.Count - 1;
        }


        private void ClientsPageUserControl_Load(object sender, EventArgs e)
        {
            RefreshDataGridView();
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            Search(searchTextBox.Text.Trim());
        }

        private void SearchTextBox_Enter(object sender, EventArgs e)
        {
            if (searchTextBox.Text.Trim() == _searchTextBoxDefaultText)
                searchTextBox.Text = string.Empty;
        }

        private void SearchTextBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchTextBox.Text))
                searchTextBox.Text = _searchTextBoxDefaultText;
        }

        private void AddNewClientButton_Click(object sender, EventArgs e)
        {
            var addNewClientForm = new ClientForm(FormMode.AddNew, OnClientFromClosed);
            addNewClientForm.Show();
        }


        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Search(searchTextBox.Text.Trim());
        }

        private void RefreshDataGridButton_Click(object sender, EventArgs e)
        {
            RefreshDataGridView();
        }

        private void ClientsDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (clientDataGridView.Rows[e.RowIndex].DataBoundItem == null) return;
            //display row index
            var row = clientDataGridView.Rows[e.RowIndex];
            row.HeaderCell.Value = $"{row.Index + 1}";
        }

        private void ClientsDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var client = (Client) clientDataGridView.Rows[e.RowIndex].DataBoundItem;

            if (client == null) return;

            var clientForm = new ClientForm(FormMode.View, OnClientFromClosed, client);
            clientForm.Show();
        }

        private void ClientDataGridView_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            RefreshDataGridView();
        }

        private void ClientDataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            var client = (Client) e.Row.DataBoundItem;
            var count = client.Rents.Count;

            if (count > 0)
            {
                MessageBox.Show(string.Format(Resources.UserDeletingRow_Client_0_have_1_Rents_delete_theme_first, client.Resume, count),
                    Resources.Unauthorized_delete_action
                    , MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }

            var dialogResult = MessageBox.Show(
                string.Format(Resources.UserDeletingRow_0_1_will_be_deleted, nameof(Client), client.Resume),
                string.Format(Resources.UserDeletingRow__0__delete_confirm, nameof(Client)), MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (dialogResult != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            _unitOfWork.Clients.Remove(client);
            _unitOfWork.Complete();
        }

        private void PrintButton_Click(object sender, EventArgs e)
        {
           ExportUtils.ToPdf(clientDataGridView);
        }


        private void ExportButton_Click(object sender, EventArgs e)
        {
            ExportUtils.ToCsv(clientDataGridView);
        }
    }
}