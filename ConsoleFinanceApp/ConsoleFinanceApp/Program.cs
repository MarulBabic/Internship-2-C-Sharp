
namespace ConsoleFinanceApp
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //stvaranje inicijalnih podataka
            var users = InitializeUsers();
            var accounts = InitializeAccounts();
            var transactions = InitializeTransactions();

            //odabir
            while (true)
            {
                Console.WriteLine();
                var choice = InitialPrint();
                switch (choice)
                {
                    case 1:
                        ManageUsers(users,accounts);
                        break;
                    case 2:
                        ManageAccounts();
                        break;
                    case 3:
                        Console.WriteLine("Izlazite iz aplikacije...");
                        return;
                    default:
                        Console.WriteLine("Neispravan unos, pokusajte ponovno");
                        break;

                }
            }

        }

        private static void ManageAccounts()
        {
           
        }

        private static void ManageUsers(Dictionary<int, Tuple<string, string, DateTime>> users, Dictionary<int, Dictionary<string,double>> accounts)
        {
            Console.WriteLine("1 - Unos novog korisnika");
            Console.WriteLine("2 - Brisanje korisnika");
            Console.WriteLine("    a) po id-u");
            Console.WriteLine("    b) po imenu i prezimenu");
            Console.WriteLine("3 - Uređivanje korisnika");
            Console.WriteLine("    a) po id-u");
            Console.WriteLine("4 - Pregled korisnika");
            Console.WriteLine("    a) ispis svih korisnika abecedno po prezimenu");
            Console.WriteLine("    b) svih onih koji imaju više od 30 godina");
            Console.WriteLine("    c) svih onih koji imaju barem jedan račun u minusu");


            var choice = 0;
            int.TryParse(Console.ReadLine(), out choice);

            switch (choice) {
                case 1:
                    AddNewUser(users,accounts);
                    break;
                    case 2:
                    DeleteUser(users);                    
                    break;
                case 3:
                    EditUser(users);
                    break;
                case 4:
                    ViewAllUsers(users,accounts);
                    break;
                default:
                    Console.WriteLine("Neispravan unos");
                    break;



            }
        }

        private static void ViewAllUsers(Dictionary<int, Tuple<string, string, DateTime>> users, Dictionary<int, Dictionary<string, double>> accounts)
        {
            Console.WriteLine("1 - ispis svih korisnika abecedno po prezimenu");
            Console.WriteLine("2 - ispis svih korisnika starijih od 30 godina");
            Console.WriteLine("3 - ispis svih korisnika sa barem 1 računom u minusu");

            var choice = 0;
            while (true)
            {
                int.TryParse(Console.ReadLine(), out choice);
                if(choice < 1 || choice > 3)
                {
                    Console.WriteLine("Pogresan odabir, pokušajte ponovo");
                    continue;
                }

                switch (choice) {
                    case 1:
                        PrintUsersByLastname(users);
                        break;
                    case 2:
                        PrintUsersOver30(users);
                        break;
                    case 3:
                        PrintUsersWithNegative(users,accounts);
                        break;
                }
                break;
            }
        }

        private static void PrintUsersWithNegative(Dictionary<int, Tuple<string, string, DateTime>> users, Dictionary<int, Dictionary<string, double>> accounts)
        {

            foreach (var user in users) {
                if (accounts.ContainsKey(user.Key) && accounts[user.Key].Any(account => account.Value < 0))
                {
                    Console.WriteLine($"ID: {user.Key}, Ime: {user.Value.Item1}, Prezime: {user.Value.Item2}, Datum rođenja: {user.Value.Item3:yyyy-MM-dd}");
                }
            }

        }

        private static void PrintUsersOver30(Dictionary<int, Tuple<string, string, DateTime>> users)
        {

            var currentDate = DateTime.Now;
            var filteredUsers = users.Where(u => (currentDate.Year - u.Value.Item3.Year) > 30 ||
                                                 (currentDate.Year - u.Value.Item3.Year == 30 &&
                                                  currentDate.Month >= u.Value.Item3.Month &&
                                                  currentDate.Day >= u.Value.Item3.Day));

            Console.WriteLine("Korisnici stariji od 30 godina:");
            foreach (var user in filteredUsers)
            {
                Console.WriteLine($"ID: {user.Key}, Ime: {user.Value.Item1}, Prezime: {user.Value.Item2}, Datum rođenja: {user.Value.Item3:yyyy-MM-dd}");
            }

        }

        private static void PrintUsersByLastname(Dictionary<int, Tuple<string, string, DateTime>> users)
        {
            var sortedUsers = users.OrderBy(u => u.Value.Item2);
            Console.WriteLine("Korisnici poredani abecedno po prezimenu:");
            foreach (var user in sortedUsers)
            {
                Console.WriteLine($"ID: {user.Key}, Ime: {user.Value.Item1}, Prezime: {user.Value.Item2}, Datum rođenja: {user.Value.Item3:yyyy-MM-dd}");
            }
        }

        private static void EditUser(Dictionary<int, Tuple<string, string, DateTime>> users)
        {
            Console.WriteLine("Unesite id korisnika kojeg želite urediti");

            while (true)
            {
                var id = 0;
                int.TryParse(Console.ReadLine(),out id);

                if (!users.ContainsKey(id))
                {
                    Console.WriteLine($"Nepostoji korisnik sa id-om {id}, pokusajte sa drugim id-om");
                    continue;
                }

                Console.Write("Ime: ");
                var firstName = Console.ReadLine();
                Console.Write("Prezime: ");
                var lastName = Console.ReadLine();

                DateTime dateTime = EnterDateOfBirth();

                users[id] = new Tuple<string, string, DateTime>(firstName, lastName,dateTime);
                Console.WriteLine($"Korisnik sa id-om {id} ureden");
                break;

            }
        }

        private static void DeleteUser(Dictionary<int, Tuple<string, string, DateTime>> users)
        {
            Console.WriteLine("Brisanje korisnika po: ");
            Console.WriteLine(" 1 - id");
            Console.WriteLine(" 2 - ime i prezime");

            var choice = 0;
            int.TryParse(Console.ReadLine(), out choice);

            switch (choice) {
                case 1:
                    DeleteUserById(users);
                    break;
                case 2:
                    DeleteUserByName(users);
                    break;
                default:
                    Console.WriteLine("Neispravan unos");
                    break;
            }

        }

        private static void DeleteUserByName(Dictionary<int, Tuple<string, string, DateTime>> users)
        {

            Console.WriteLine("Unesite ime i prezime korisnika");

            while (true)
            {
                string[] name = Console.ReadLine().Split(" ");


                if (name.Length < 2)
                {
                    Console.WriteLine("Pogrešan unos, Molimo unesite i ime i prezime");
                    continue;
                }

                string firstName = name[0];
                string lastName = name[1];
                int? userIdToDelete = null;

                foreach (var user in users)
                {
                    if (user.Value.Item1 == firstName && user.Value.Item2 == lastName)
                    {
                        userIdToDelete = user.Key;
                        break;
                    }
                }

                if (userIdToDelete.HasValue)
                {
                    users.Remove(userIdToDelete.Value);
                    Console.WriteLine($"Korisnik {firstName} {lastName} je uspješno izbrisan");
                    break;
                }
                else
                {
                    Console.WriteLine($"Korisnik s imenom {firstName} {lastName} nije pronađen.");
                    break;
                }
            }
        }

        private static void DeleteUserById(Dictionary<int, Tuple<string, string, DateTime>> users)
        {
            Console.WriteLine("Unesite id korisnika kojeg zelite izbrisati");
            var id = 0;

            while (true)
            {
                int.TryParse(Console.ReadLine(), out id);
                if (!users.ContainsKey(id))
                {
                    Console.WriteLine("Korisnik sa id-om {0} ne postoji, pokusajte sa drugim id-om", id);
                    continue;
                }
                else
                {
                    Console.WriteLine("Brišete korisnika sa id-em {0}",id);
                    users.Remove(id);
                    break;
                }
            }
        }

        private static void AddNewUser(Dictionary<int, Tuple<string, string, DateTime>> users, Dictionary<int, Dictionary<string, double>> accounts)
        {
            var newUserId = users.Keys.Max() + 1;

            Console.WriteLine("Unesite ime korisnika");
            var firstName = Console.ReadLine();

            Console.WriteLine("Unesite prezime korisnika");
            var lastName = Console.ReadLine();

            DateTime birthDate = EnterDateOfBirth();

            users.Add(newUserId, Tuple.Create(firstName, lastName, birthDate));
            

            var userAccounts = new Dictionary<string, double>()
            {
                 { "Tekući", 100.00 },
                 { "Žiro", 0.00 },
                 { "Prepaid", 0.00 }
            };

            accounts.Add(newUserId,userAccounts);

            Console.WriteLine($"Novi korisnik {firstName} {lastName} uspješno dodan s ID-em {newUserId}.");
        }


        private static DateTime EnterDateOfBirth()
        {
            while (true)
            {
                Console.WriteLine("Datum rođenja:");
                Console.Write("Dan: ");
                var day = 0;
                int.TryParse(Console.ReadLine(), out day);
                if (day < 1 || day > 31)
                {
                    continue;
                }

                Console.Write("Mjesec: ");
                var month = 0;
                int.TryParse(Console.ReadLine(), out month);
                if (month < 1 || month > 12)
                {
                    continue;
                }

                Console.Write("Godina: ");
                var year = 0;
                int.TryParse(Console.ReadLine(), out year);
                if (year < 1900 || year > DateTime.Now.Year)
                {
                    continue;
                }
                return new DateTime(year, month, day);
            }
        }
        private static int InitialPrint()
        {
            Console.WriteLine("1 - Korisnici\n2 - Računi\n3 - Izlaz iz aplikacije");
            Console.WriteLine();

            int choice = 0;
            int.TryParse(Console.ReadLine(), out choice);

            return choice;
        }

        private static Dictionary<int, Tuple<string,string,DateTime>> InitializeUsers()
        {
            return new Dictionary<int, Tuple<String, string, DateTime>>()
            {
                {1, Tuple.Create("Marko","Markic",new DateTime(2001,7,1)) },
                {2, Tuple.Create("Ante","Antic",new DateTime(2001,8,2)) },
                {3, Tuple.Create("Ivan","Ivic",new DateTime(1991,9,3)) }
            };
        }

        private static Dictionary<int, Dictionary<string, double>> InitializeAccounts()
        {
            return new Dictionary<int, Dictionary<string, double>>()
            {
                { 1, new Dictionary<string, double>() { { "Tekući", 100.00 }, { "Žiro", 0.00 }, { "Prepaid", 0.00 } } },
                { 2, new Dictionary<string, double>() { { "Tekući", -200.00 }, { "Žiro", 0.00 }, { "Prepaid", 0.00 } } },
                { 3, new Dictionary<string, double>() { { "Tekući", 50.00 }, { "Žiro", 0.00 }, { "Prepaid", 0.00 } } }
            };
        }

        private static Dictionary<int, Tuple<int, double, string, string, string, DateTime>> InitializeTransactions()
        {
            return new Dictionary<int, Tuple<int, double, string, string, string, DateTime>>()
            {
                {1, Tuple.Create(1, 100.00,"Početno stanje","prihod","plaća",DateTime.Now)},
                {2, Tuple.Create(1, -15.00,"Kupovina","rashod","hrana",DateTime.Now)},
                {3, Tuple.Create(1, 200.00,"isplata ","prihod","honorar",DateTime.Now)}
            };
        }

    }
                
}
