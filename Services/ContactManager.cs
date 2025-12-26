using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LLMOverlay.Models;
using Newtonsoft.Json;

namespace LLMOverlay.Services
{
    public static class ContactManager
    {
        private static readonly string ContactsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LLMOverlay", "contacts");
        private static Contact? _activeContact;
        private static List<Contact> _contacts = new List<Contact>();

        static ContactManager()
        {
            Initialize();
        }

        private static void Initialize()
        {
            // Ensure contacts directory exists
            if (!Directory.Exists(ContactsDirectory))
            {
                Directory.CreateDirectory(ContactsDirectory);
            }

            LoadContacts();
        }

        private static void LoadContacts()
        {
            _contacts = new List<Contact>();

            try
            {
                var contactFiles = Directory.GetFiles(ContactsDirectory, "*.json");
                foreach (var file in contactFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var contact = JsonConvert.DeserializeObject<Contact>(json);
                        if (contact != null)
                        {
                            _contacts.Add(contact);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading contact from {file}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading contacts: {ex.Message}");
            }
        }

        public static List<Contact> GetAllContacts()
        {
            return _contacts.OrderByDescending(c => c.LastActive).ToList();
        }

        public static List<Contact> GetRecentContacts(int count = 2)
        {
            return _contacts.OrderByDescending(c => c.LastActive).Take(count).ToList();
        }

        public static Contact? GetContactById(string? id)
        {
            return _contacts.FirstOrDefault(c => c.Id == id);
        }

        public static Contact? GetActiveContact()
        {
            return _activeContact;
        }

        public static void SetActiveContact(Contact? contact)
        {
            if (contact != null)
            {
                _activeContact = contact;
                contact.UpdateLastActive();
                SaveContact(contact);
            }
        }

        public static void SaveContact(Contact? contact)
        {
            if (contact == null) return;

            try
            {
                var filePath = Path.Combine(ContactsDirectory, $"{contact.Id}.json");
                var json = JsonConvert.SerializeObject(contact, Formatting.Indented);
                File.WriteAllText(filePath, json);

                // Update internal list if contact exists
                var existingContact = _contacts.FirstOrDefault(c => c.Id == contact.Id);
                if (existingContact != null)
                {
                    _contacts.Remove(existingContact);
                }
                _contacts.Add(contact);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving contact {contact.Id}: {ex.Message}");
                throw;
            }
        }

        public static void DeleteContact(string id)
        {
            try
            {
                var filePath = Path.Combine(ContactsDirectory, $"{id}.json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                var contact = _contacts.FirstOrDefault(c => c.Id == id);
                if (contact != null)
                {
                    _contacts.Remove(contact);
                }

                if (_activeContact?.Id == id)
                {
                    _activeContact = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting contact {id}: {ex.Message}");
                throw;
            }
        }

        public static Contact CreateContact(string name, string baseModel)
        {
            var contact = new Contact
            {
                Name = name,
                BaseModel = baseModel
            };

            SaveContact(contact);
            return contact;
        }

        public static void ImportContacts(List<Contact> contacts)
        {
            foreach (var contact in contacts)
            {
                // Generate new ID to avoid conflicts
                contact.Id = Guid.NewGuid().ToString();
                SaveContact(contact);
            }
        }

        public static List<Contact> ExportContacts()
        {
            return new List<Contact>(_contacts);
        }
    }
}