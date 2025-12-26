using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using LLMOverlay.Models;
using LLMOverlay.Services;

namespace LLMOverlay
{
    public partial class ContactDirectoryWindow : Window
    {
        private ObservableCollection<Contact> _contacts = new ObservableCollection<Contact>();
        private ObservableCollection<Contact> _filteredContacts = new ObservableCollection<Contact>();

        public ContactDirectoryWindow()
        {
            InitializeComponent();
            LoadContacts();
        }

        private void LoadContacts()
        {
            try
            {
                var contacts = ContactManager.GetAllContacts();
                _contacts = new ObservableCollection<Contact>(contacts);
                _filteredContacts = new ObservableCollection<Contact>(_contacts);
                
                ContactsDataGrid.ItemsSource = _filteredContacts;
                StatusText.Text = $"Loaded {_contacts.Count} contacts";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading contacts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterContacts();
        }

        private void FilterContacts()
        {
            var searchText = SearchTextBox.Text.ToLower();
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                _filteredContacts.Clear();
                foreach (var contact in _contacts)
                {
                    _filteredContacts.Add(contact);
                }
            }
            else
            {
                _filteredContacts.Clear();
                var filtered = _contacts.Where(c => 
                    c.Name.ToLower().Contains(searchText) ||
                    c.BaseModel.ToLower().Contains(searchText) ||
                    (c.Personality?.ToLower().Contains(searchText) ?? false)
                );
                
                foreach (var contact in filtered)
                {
                    _filteredContacts.Add(contact);
                }
            }
            
            StatusText.Text = $"Showing {_filteredContacts.Count} of {_contacts.Count} contacts";
        }

        private void NewContactButton_Click(object sender, RoutedEventArgs e)
        {
            var createWindow = new CreateContactWindow();
            if (createWindow.ShowDialog() == true)
            {
                LoadContacts(); // Refresh the list
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var contactId = button.Tag?.ToString();
            if (string.IsNullOrEmpty(contactId))
            {
                return;
            }

            var contact = ContactManager.GetContactById(contactId);
            
            if (contact != null)
            {
                var editWindow = new CreateContactWindow(contact);
                if (editWindow.ShowDialog() == true)
                {
                    LoadContacts(); // Refresh the list
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var contactId = button.Tag?.ToString();
            if (string.IsNullOrEmpty(contactId))
            {
                return;
            }

            var contact = ContactManager.GetContactById(contactId);
            
            if (contact != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{contact.Name}'?", 
                    "Confirm Delete", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        ContactManager.DeleteContact(contactId);
                        LoadContacts(); // Refresh the list
                        StatusText.Text = $"Contact '{contact.Name}' deleted successfully";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting contact: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ContactsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ContactsDataGrid.SelectedItem is Contact contact)
            {
                // Load the contact as active
                ContactManager.SetActiveContact(contact);
                
                // Find and notify the main window
                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (mainWindow != null)
                {
                    mainWindow.LoadContact(contact);
                }
                
                this.DialogResult = true;
                this.Close();
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Import Contacts",
                Filter = "JSON Files|*.json|All Files|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var json = System.IO.File.ReadAllText(openFileDialog.FileName);
                    var contacts = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<Contact>>(json);
                    
                    if (contacts != null)
                    {
                        ContactManager.ImportContacts(contacts);
                        LoadContacts();
                        StatusText.Text = $"Successfully imported {contacts.Count} contacts";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing contacts: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Export Contacts",
                Filter = "JSON Files|*.json|All Files|*.*",
                FilterIndex = 1,
                FileName = $"contacts_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var contacts = ContactManager.ExportContacts();
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(contacts, Newtonsoft.Json.Formatting.Indented);
                    System.IO.File.WriteAllText(saveFileDialog.FileName, json);
                    
                    StatusText.Text = $"Successfully exported {contacts.Count} contacts";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting contacts: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}