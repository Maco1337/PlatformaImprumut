using System;
using System.Linq;
using System.Collections.Generic;

namespace PlatformaImprumut
{
    class Program
    {  // baza de date a aplicatiei
        static Database db = Database.Load();
         // utilizatorul curent sau null daca nu e logat
        static User currentUser = null;

        static void Main(string[] args)
        {
            //verificam daca exista deja un admin
            if (!db.Users.Any(u => u.Role == "Admin"))
            {
                // cream un admin implicit daca nu exista
                db.Users.Add(new Admin { Username = "admin", Password = "123" });
                db.Save();
            }

            //bucla aplicatiei
            while (true)
            {
                Console.Clear();
                //nimeni logat=> meniu login
                if (currentUser == null) LoginMenu();
                //utilizatorul este admin => meniu de admin
                else if (currentUser is Admin) AdminMenu();
                //altfel=> meniul de utilizator normal
                else UserMenu();
            }
        }

        //meniu logare
        static void LoginMenu()
        {
            Console.WriteLine("Platforma imput");
            Console.WriteLine("1. Login | 2. Inregistrare | 3. Iesire");
            string opt = Console.ReadLine();

            //iesire
            if (opt == "3") Environment.Exit(0);

            Console.Write("Username: "); string u = Console.ReadLine();
            Console.Write("Parola: "); string p = Console.ReadLine();

            //login
            if (opt == "1") {
                //cautare utilozator in lista
                currentUser = db.Users.FirstOrDefault(x => x.Username == u && x.Password == p);
                if (currentUser == null) { Console.WriteLine("Date gresite"); Console.ReadKey(); }
            } 
            //inregistrare
            else 
            {
                db.Users.Add(new RegularUser { Username = u, Password = p });
                db.Save();
                Console.WriteLine("Cont creat"); Console.ReadKey();
            }
        }

        //meniu utilizator
        static void UserMenu()
        {
            Console.WriteLine($"\n Utilizator: {currentUser.Username}");
            Console.WriteLine("1. Pune obiect la imprumut");
            Console.WriteLine("2. Cauta obiecte (pe categorii)");
            Console.WriteLine("3. Cereri primite (pentru obiectele tale)");
            Console.WriteLine("4. Status cereri trimise (de tine)");
            Console.WriteLine("5. Logout");
            string opt = Console.ReadLine();

            switch (opt)
            {
                case "1": AddItemFlow(); break; // utilizatorul publica un obiect
                case "2": SearchItemFlow(); break; // utilizatorul cauta obiecte si poate trimite cereri
                case "3": HandleReceivedRequests(); break; // accepta sau respinge cereri
                case "4": ViewSentRequests(); break;      // afișează cererile și statusul lor
                case "5": currentUser = null; break;     //accepta sau respinge cereri
            }
        }

        //adaugare obiect
        static void AddItemFlow()
        {
            Console.WriteLine("\n Alege categoria");
            // afisam categoriile existente
            for (int i = 0; i < db.Categories.Count; i++) Console.WriteLine($"{i + 1}. {db.Categories[i]}");
            
            Console.Write("Numar categorie: ");
            //verificam alegerea utilizatorului
            if (int.TryParse(Console.ReadLine(), out int catIdx) && catIdx > 0 && catIdx <= db.Categories.Count)
            {
                string selectedCat = db.Categories[catIdx - 1];
                Console.Write("Nume obiect: "); string nume = Console.ReadLine();
                Console.Write("Descriere scurta: "); string desc = Console.ReadLine();

                // cream obiectul si îl adaugam în lista
                db.Items.Add(new Item { Name = nume, Description = desc, Category = selectedCat, OwnerUsername = currentUser.Username });
                db.Save();
                Console.WriteLine("Succes!");
            }
            Console.ReadKey();
        }

         // cautare obiecte si trimitere cerere
        static void SearchItemFlow()
        {
            Console.WriteLine("\n Categorii disponibile");
            for (int i = 0; i < db.Categories.Count; i++) Console.WriteLine($"{i + 1}. {db.Categories[i]}");

            Console.Write("Alege categoria: ");
            if (int.TryParse(Console.ReadLine(), out int catIdx) && catIdx > 0 && catIdx <= db.Categories.Count)
            {
                string selectedCat = db.Categories[catIdx - 1];
                //filtram obiectele disponibile
                var filteredItems = db.Items.Where(i => i.Category == selectedCat && i.IsAvailable && i.OwnerUsername != currentUser.Username).ToList();

                if (!filteredItems.Any()) Console.WriteLine("Nimic disponibil aici");
                else {
                    for (int i = 0; i < filteredItems.Count; i++) Console.WriteLine($"{i + 1}. {filteredItems[i]} (Proprietar: {filteredItems[i].OwnerUsername})");
                    Console.Write("\nAlege numarul pt imprumut: ");
                    if (int.TryParse(Console.ReadLine(), out int itemIdx) && itemIdx > 0 && itemIdx <= filteredItems.Count)
                    {
                        var target = filteredItems[itemIdx - 1];
                        // cream cererea de împrumut
                        db.Requests.Add(new LoanRequest { ItemId = target.Id, ItemName = target.Name, RequesterUsername = currentUser.Username, OwnerUsername = target.OwnerUsername });
                        db.Save();
                        Console.WriteLine("Cerere trimisa");
                    }
                }
            }
            Console.ReadKey();
        }

        //cereri primite pentru obiectele proprii
        static void HandleReceivedRequests()
        {
            Console.WriteLine("\n Cereri pentru obiectele tale");
            var received = db.Requests.Where(r => r.OwnerUsername == currentUser.Username && r.Status == "Pending").ToList();

            if (!received.Any()) { Console.WriteLine("Nu ai cereri noi"); }
            else {
                for (int i = 0; i < received.Count; i++)
                    Console.WriteLine($"{i + 1}. {received[i].RequesterUsername} vrea sa imprumute: {received[i].ItemName}");

                Console.Write("\n Alege numarul cererii: ");
                if (int.TryParse(Console.ReadLine(), out int rIdx) && rIdx > 0 && rIdx <= received.Count)
                {
                    var request = received[rIdx - 1];
                    Console.WriteLine("1. Accepta | 2. Respinge | 3. Anuleaza");
                    string decizie = Console.ReadLine();

                    if (decizie == "1") {
                        request.Status = "Acceptata";
                        // marcam obiectul ca fiind ocupat/indisponibil
                        var item = db.Items.FirstOrDefault(it => it.Id == request.ItemId);
                        if (item != null) item.IsAvailable = false;
                        Console.WriteLine("Ai acceptat cererea");
                    } 
                    else if (decizie == "2") {
                        request.Status = "Respinsa";
                        Console.WriteLine("Ai respins cererea.");
                    }
                    db.Save();
                }
            }
            Console.ReadKey();
        }

        // afisare status cereri trimise
        static void ViewSentRequests()
        {
            Console.WriteLine("\n Status cereri trimise de tine");
            var sent = db.Requests.Where(r => r.RequesterUsername == currentUser.Username).ToList();

            if (!sent.Any()) { Console.WriteLine("Nu ai trimis nicio cerere"); }
            else {
                foreach (var r in sent)
                {
                    // afisare colorata simbolic in text
                    Console.WriteLine($"- Obiect: {r.ItemName} | Catre: {r.OwnerUsername} | Status: [{r.Status}]");
                }
            }
            Console.WriteLine("\nApasa orice tasta pentru inapoi");
            Console.ReadKey();
        }

        // meniu admin
        static void AdminMenu()
        {
            Console.WriteLine("Admin");
            Console.WriteLine("1. Utilizatori | 2. Adauga Categorie | 3. Logout");
            string opt = Console.ReadLine();
            if (opt == "1") {
                foreach (var u in db.Users) Console.WriteLine($"- {u.Username} ({u.Role})");
                Console.ReadKey();
            } else if (opt == "2") {
                Console.Write("Nume categorie: ");
                db.Categories.Add(Console.ReadLine());
                db.Save();
            } else 
                //logout
                currentUser = null;
        }
    }
}
