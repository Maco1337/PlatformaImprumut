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

        //calea fisierului unde se salveaza datele aplicatiei
        private static readonly string FilePath = "date_platforma.json";

        //salvam datele in fisierul JSON
        public void Save()
        {
            try {
                //obict->text
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                //scriem JSON-ul in fisier
                File.WriteAllText(FilePath, json);
            } 
            // afisam mesaj in caz de eroare
            catch (Exception ex) 
            { 
                Console.WriteLine("Eroare salvare: " + ex.Message); 
            }
        }

        public static Database Load()
        {
            //ptr eroare returnam o baza de date goala
            if (!File.Exists(FilePath)) return new Database();
            try {
                string json = File.ReadAllText(FilePath);
                //text->obiect
                return JsonSerializer.Deserialize<Database>(json) ?? new Database();
            } 
            catch 
            { 
                //ptr eroare pornim cu o baza de date goala
                return new Database(); 
            }
        }
    }
}
