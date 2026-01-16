using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PlatformaImprumut
{
    public class Database
    {
        public List<User> Users { get; set; } = new List<User>();
        public List<Item> Items { get; set; } = new List<Item>();
        public List<LoanRequest> Requests { get; set; } = new List<LoanRequest>();
        public List<string> Categories { get; set; } = new List<string> 
        {"Unelte", 
        "Carti", 
        "Jocuri"};

        private static readonly string FilePath = "date_platforma.json";

        public void Save()
        {
            try {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            } catch (Exception ex) { Console.WriteLine("Eroare salvare: " + ex.Message); }
        }

        public static Database Load()
        {
            if (!File.Exists(FilePath)) return new Database();
            try {
                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<Database>(json) ?? new Database();
            } 
            catch 
            { 
                return new Database(); 
            }
        }
    }
}
