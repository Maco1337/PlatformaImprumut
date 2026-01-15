using System;
using System.Linq;
using System.Collections.Generic;

namespace PlatformaImprumut
{
    class Program
    {
        static Database db = Database.Load();
        static User currentUser = null;

        static void Main(string[] args)
        {
            if (!db.Users.Any(u => u.Role == "Admin"))
            {
                db.Users.Add(new Admin { Username = "admin", Password = "123" });
                db.Save();
            }

            while (true)
            {
                Console.Clear();
                if (currentUser == null) LoginMenu();
                else if (currentUser is Admin) AdminMenu();
                else UserMenu();
            }
        }

        // ================= LOGIN / REGISTER =================
        static void LoginMenu()
        {
            Console.WriteLine("=== PLATFORMA IMPRUMUT ===");
            Console.WriteLine("1. Login | 2. Inregistrare | 3. Iesire");
            string opt = Console.ReadLine();

            if (opt == "3")
                Environment.Exit(0);

            Console.Write("Username: ");
            string u = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(u))
            {
                Console.WriteLine("Username invalid!");
                Console.ReadKey();
                return;
            }

            if (opt == "1") // LOGIN
            {
                Console.Write("Parola: ");
                string p = Console.ReadLine();

                currentUser = db.Users.FirstOrDefault(x => x.Username == u && x.Password == p);
                if (currentUser == null)
                {
                    Console.WriteLine("Date gresite!");
                    Console.ReadKey();
                }
            }
            else if (opt == "2") // REGISTER
            {
                if (db.Users.Any(x => x.Username == u))
                {
                    Console.WriteLine("Username deja existent!");
                    Console.ReadKey();
                    return;
                }

                string password = ReadValidPassword();

                db.Users.Add(new RegularUser { Username = u, Password = password });
                db.Save();
                Console.WriteLine("Cont creat cu succes!");
                Console.ReadKey();
            }
        }

        // ================= USER MENU =================
        static void UserMenu()
        {
            Console.WriteLine($"\n--- Utilizator: {currentUser.Username} ---");
            Console.WriteLine("1. Pune obiect la imprumut");
            Console.WriteLine("2. Cauta obiecte (pe categorii)");
            Console.WriteLine("3. Cereri primite (pentru obiectele tale)");
            Console.WriteLine("4. Status cereri trimise (de tine)");
            Console.WriteLine("5. Logout");
            string opt = Console.ReadLine();

            switch (opt)
            {
                case "1": AddItemFlow(); break;
                case "2": SearchItemFlow(); break;
                case "3": HandleReceivedRequests(); break;
                case "4": ViewSentRequests(); break;
                case "5": currentUser = null; break;
            }
        }

        static void AddItemFlow()
        {
            Console.WriteLine("\n--- ALEGE CATEGORIA ---");
            for (int i = 0; i < db.Categories.Count; i++)
                Console.WriteLine($"{i + 1}. {db.Categories[i]}");

            Console.Write("Numar categorie: ");
            if (int.TryParse(Console.ReadLine(), out int catIdx) &&
                catIdx > 0 && catIdx <= db.Categories.Count)
            {
                string selectedCat = db.Categories[catIdx - 1];
                Console.Write("Nume obiect: ");
                string nume = Console.ReadLine();
                Console.Write("Descriere scurta: ");
                string desc = Console.ReadLine();

                db.Items.Add(new Item
                {
                    Name = nume,
                    Description = desc,
                    Category = selectedCat,
                    OwnerUsername = currentUser.Username
                });

                db.Save();
                Console.WriteLine("Succes!");
            }
            Console.ReadKey();
        }

        static void SearchItemFlow()
        {
            Console.WriteLine("\n--- CATEGORII DISPONIBILE ---");
            for (int i = 0; i < db.Categories.Count; i++)
                Console.WriteLine($"{i + 1}. {db.Categories[i]}");

            Console.Write("Alege categoria: ");
            if (int.TryParse(Console.ReadLine(), out int catIdx) &&
                catIdx > 0 && catIdx <= db.Categories.Count)
            {
                string selectedCat = db.Categories[catIdx - 1];
                var filteredItems = db.Items
                    .Where(i => i.Category == selectedCat &&
                                i.IsAvailable &&
                                i.OwnerUsername != currentUser.Username)
                    .ToList();

                if (!filteredItems.Any())
                {
                    Console.WriteLine("Nimic disponibil aici.");
                }
                else
                {
                    for (int i = 0; i < filteredItems.Count; i++)
                        Console.WriteLine($"{i + 1}. {filteredItems[i]} (Proprietar: {filteredItems[i].OwnerUsername})");

                    Console.Write("\nAlege numarul pt imprumut: ");
                    if (int.TryParse(Console.ReadLine(), out int itemIdx) &&
                        itemIdx > 0 && itemIdx <= filteredItems.Count)
                    {
                        var target = filteredItems[itemIdx - 1];
                        db.Requests.Add(new LoanRequest
                        {
                            ItemId = target.Id,
                            ItemName = target.Name,
                            RequesterUsername = currentUser.Username,
                            OwnerUsername = target.OwnerUsername
                        });
                        db.Save();
                        Console.WriteLine("Cerere trimisa!");
                    }
                }
            }
            Console.ReadKey();
        }

        // ================= CERERI PRIMITE =================
        static void HandleReceivedRequests()
        {
            Console.WriteLine("\n--- CERERI PENTRU OBIECTELE TALE ---");
            var received = db.Requests
                .Where(r => r.OwnerUsername == currentUser.Username && r.Status == "Pending")
                .ToList();

            if (!received.Any())
            {
                Console.WriteLine("Nu ai cereri noi.");
            }
            else
            {
                for (int i = 0; i < received.Count; i++)
                    Console.WriteLine($"{i + 1}. {received[i].RequesterUsername} vrea {received[i].ItemName}");

                Console.Write("\nAlege cererea: ");
                if (int.TryParse(Console.ReadLine(), out int rIdx) &&
                    rIdx > 0 && rIdx <= received.Count)
                {
                    var request = received[rIdx - 1];
                    Console.WriteLine("1. Accepta | 2. Respinge");
                    string decizie = Console.ReadLine();

                    if (decizie == "1")
                    {
                        request.Status = "Acceptata";
                        var item = db.Items.FirstOrDefault(it => it.Id == request.ItemId);
                        if (item != null) item.IsAvailable = false;
                        Console.WriteLine("Cerere acceptata!");
                    }
                    else if (decizie == "2")
                    {
                        request.Status = "Respinsa";
                        Console.WriteLine("Cerere respinsa.");
                    }
                    db.Save();
                }
            }
            Console.ReadKey();
        }

        // ================= CERERI TRIMISE =================
        static void ViewSentRequests()
        {
            Console.WriteLine("\n--- CERERI TRIMISE ---");
            var sent = db.Requests
                .Where(r => r.RequesterUsername == currentUser.Username)
                .ToList();

            if (!sent.Any())
                Console.WriteLine("Nu ai trimis cereri.");
            else
                foreach (var r in sent)
                    Console.WriteLine($"Obiect: {r.ItemName} | Catre: {r.OwnerUsername} | Status: {r.Status}");

            Console.ReadKey();
        }

        // ================= ADMIN =================
        static void AdminMenu()
        {
            Console.WriteLine("--- ADMIN ---");
            Console.WriteLine("1. Utilizatori | 2. Adauga categorie | 3. Logout");
            string opt = Console.ReadLine();

            if (opt == "1")
            {
                foreach (var u in db.Users)
                    Console.WriteLine($"{u.Username} ({u.Role})");
                Console.ReadKey();
            }
            else if (opt == "2")
            {
                Console.Write("Nume categorie: ");
                db.Categories.Add(Console.ReadLine());
                db.Save();
            }
            else
                currentUser = null;
        }

        // ================= VALIDARE PAROLA =================
        static string ReadValidPassword()
        {
            while (true)
            {
                Console.Write("Parola (minim 5 caractere, fara spatii): ");
                string p = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(p))
                {
                    Console.WriteLine("Parola nu poate fi goala.");
                    continue;
                }

                if (p.Length < 5)
                {
                    Console.WriteLine("Parola prea scurta.");
                    continue;
                }

                if (p.Contains(" "))
                {
                    Console.WriteLine("Parola nu trebuie sa contina spatii.");
                    continue;
                }

                return p;
            }
        }
    }
}
