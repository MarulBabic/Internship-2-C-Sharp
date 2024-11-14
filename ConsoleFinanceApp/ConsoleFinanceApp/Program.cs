
using System.Globalization;
using System.Security.Principal;

namespace ConsoleFinanceApp
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //stvaranje inicijalnih podataka
            var users = InitializeUsers();
            var accounts = InitializeAccounts();

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
                        ManageAccounts(users,accounts);
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

        private static readonly HashSet<string> IncomeCategories = new HashSet<string> { "placa", "honorar", "poklon", "stipendija", "dividende", "prijenos" };
        private static readonly HashSet<string> ExpenseCategories = new HashSet<string> { "hrana", "prijevoz", "sport", "stanarina", "zdravstvo, prijenos"};


        private static void PrintInput()
        {
            Console.Write("\nUnos: ");
        }


        private static void ManageAccounts(Dictionary<int, Tuple<string, string, DateTime>> users, Dictionary<int, Dictionary<string, List<Tuple<int, double, string, string, string, DateTime>>>> accounts)
        {
            Console.WriteLine("\nUnesite ime i prezime korisnika");
            while (true) {
                PrintInput();
                string[] name = Console.ReadLine().Split(" ");
                if (name.Length < 2) {
                    Console.WriteLine("Unesite i ime i prezime");
                    continue;
                }
                
                var firstName = name[0];
                var lastName = name[1];
                int ? userId = null;

                foreach ( var user in users)
                {
                    if(user.Value.Item1 == firstName && user.Value.Item2 == lastName)
                    {
                        userId = user.Key;
                        break;
                    }
                }

                if (userId == null)
                {
                    Console.WriteLine("Korisnik nije pronađen. Pokušajte ponovno");
                    continue;
                }

                ManageUserAccount(users, accounts, userId.Value);
                break;
            }

        }

        private static void ManageUserAccount(Dictionary<int, Tuple<string, string, DateTime>> users, Dictionary<int, Dictionary<string, List<Tuple<int, double, string, string, string, DateTime>>>> accounts, int userId)
        {
           
                if (!accounts.ContainsKey(userId))
                {
                    Console.WriteLine("Korisnik nema račune.");
                    return;
                }

                var userAccounts = accounts[userId];
                Console.WriteLine($"\nOdaberite račun za upravljanje za korisnika {users[userId].Item1} {users[userId].Item2}:");

                while (true)
                {
                    Console.WriteLine("1 - Tekući račun");
                    Console.WriteLine("2 - Žiro račun");
                    Console.WriteLine("3 - Prepaid račun");
                    Console.WriteLine("4 - Prijenos novca sa jednog na drugi racun");
                    Console.WriteLine("5 - Slanje novca drugim korisnicima");

                    var choice = 0;
                     PrintInput();
                    if (int.TryParse(Console.ReadLine(), out choice) && (choice >= 1 && choice <= 5))
                    {
                        var selectedAccount = string.Empty;
                        switch (choice){
                            case 1:
                                selectedAccount = "Tekuci";
                                break;
                            case 2:
                                selectedAccount = "Ziro";
                                break;
                            case 3:
                                selectedAccount = "Prepaid";
                                break;
                            case 4:
                                var fromAccount = GetFromAccount();
                                var toAccount = GetToAccount(fromAccount);
                                TransferMoney(userAccounts[fromAccount], userAccounts[toAccount]);
                                return;
                            case 5:
                                SendMoneyToOtherUser(users, accounts, userId);
                                return;
                             default:
                                 Console.WriteLine("Pogresan unos, pokusaj ponovo");
                                 break;
                        }

                        if (choice >= 1 && choice <= 3)
                        {
                            ManageTransactionsForAccount(userAccounts[selectedAccount]);
                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Nevažeći odabir. Pokušajte ponovo.");
                        continue;
                    }
                }
            



        }

        private static void SendMoneyToOtherUser(Dictionary<int, Tuple<string, string, DateTime>> users, Dictionary<int, Dictionary<string, List<Tuple<int, double, string, string, string, DateTime>>>> accounts, int userId)
        {
            Console.WriteLine("\nIzaberite korisnika kojem zelite poslati novac(ime prezime)\n");
            foreach (var user in users)
            {
                Console.WriteLine($"{user.Value.Item1} {user.Value.Item2}");
            }

            PrintInput();
            var nameAndSurname = Console.ReadLine().Trim().ToLower();
            var targetUser = users.Values.FirstOrDefault(user =>(user.Item1 + " " + user.Item2).ToLower() == nameAndSurname);

            if (targetUser == null)
            {
                Console.WriteLine("Korisnik s tim imenom i prezimenom nije pronađen.");
                return;
            }

            int targetUserId = users.First(u => u.Value.Item1 + " " + u.Value.Item2 == targetUser.Item1 + " " + targetUser.Item2).Key;

            if (targetUserId == userId)
            {
                Console.WriteLine("Za prijenos novca na vas drugi racun izaberite 4");
                return;
            }

            var fromAccount = GetFromAccount();  
            var balance = ShowCurrentAccountBalance(accounts[userId][fromAccount]);
            Console.WriteLine($"\nTrenutno stanje na racunu je {balance:F2}");

            Console.WriteLine("Unesite iznos za prijenos: ");
            var amount = 0.0;
            while (true)
            {
                PrintInput();
                if (double.TryParse(Console.ReadLine(), out amount) && amount > 0)
                {
                    if (amount <= balance)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Nemate dovoljno sredstava na računu za ovaj prijenos.");
                    }
                }
                else
                {
                    Console.WriteLine("Neispravan iznos, pokušajte ponovo.");
                }
            }

            var toAccount = GetToAccount("");

            accounts[userId][fromAccount].Add(Tuple.Create(accounts[userId][fromAccount].Count + 1, amount, "Transfer prema korisniku " + targetUser.Item1, "rashod", "prijenos", DateTime.Now));
            accounts[targetUserId][toAccount].Add(Tuple.Create(accounts[targetUserId][toAccount].Count + 1, amount, "Primljeni transfer od korisnika " + users[userId].Item1, "prihod", "prijenos", DateTime.Now));
            Console.WriteLine($"\nUspješno ste poslali {amount:F2} eura korisniku {targetUser.Item1} {targetUser.Item2} na {toAccount} račun.\n");
            return;
        }

        private static void TransferMoney(List<Tuple<int, double, string, string, string, DateTime>> fromAccount, List<Tuple<int, double, string, string, string, DateTime>> toAccount)
        {
            var balanceBefore = ShowCurrentAccountBalance(fromAccount);
            Console.WriteLine($"\nStanje racuna prije prijenosa je: {balanceBefore:F2}");
            Console.WriteLine("Unesite iznos za prijenos: ");
            var amount = 0.0;
            var balance = 0.0;

            do
            {
                PrintInput();
                double.TryParse(Console.ReadLine(), out amount);
                if (amount <= 0)
                {
                    Console.WriteLine("Iznos mora biti veci od 0");
                    continue;
                }

                balance = ShowCurrentAccountBalance(fromAccount);

                if (amount > balance)
                {
                    Console.WriteLine("Nemate dovoljno sredstava za prijenos, pokusajte sa manjim iznosom");
                    continue;
                }
            } while (amount <= 0 || amount > balance);

            fromAccount.Add(Tuple.Create(fromAccount.Count + 1, amount, "Prijenos novca", "rashod", "prijenos", DateTime.Now));
            toAccount.Add(Tuple.Create(toAccount.Count + 1, amount, "Prijenos novca", "prihod", "prijenos", DateTime.Now));

            Console.WriteLine("\nPrijenos novca uspješno izvršen.");

            var balanceAfter = ShowCurrentAccountBalance(fromAccount);
            Console.WriteLine($"stanje na računu nakon prijenosa je: {balanceAfter:F2}\n");
        }

        private static string GetToAccount(string fromAccount)
        {
            Console.WriteLine("\nUnesite racun na koji zelite prebaciti sredstva: tekuci, ziro, prepaid");
            var account = "";
            while (true)
            {
                PrintInput();
                account = Console.ReadLine().ToLower();
                account = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(account);
                if (account.Equals(fromAccount))
                {
                    Console.WriteLine("Nemozete prebaciti sredstva na isti racun, odaberite drugi");
                    continue;
                }
                if(account != "Tekuci" && account != "Ziro" && account != "Prepaid")
                {
                    Console.WriteLine("Unesite jedan od mogucih racuna: Tekuci, Ziro, Prepaid");
                    continue;
                }
                break;
            }
            return account;
        }

        private static string GetFromAccount()
        {
            Console.WriteLine("\nUnesite racun sa kojeg zelite prebaciti sredstva: tekuci, ziro, prepaid");
            var account = "";
            do
            {
                PrintInput();
                account = Console.ReadLine().ToLower();
                account = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(account);
                if (account != "Tekuci" && account != "Ziro" && account != "Prepaid")
                {
                    Console.WriteLine("Nevalidan unos. Molimo unesite 'Tekuci', 'Ziro' ili 'Prepaid'.");
                }
            } while (account != "Tekuci" && account != "Ziro" && account != "Prepaid");

            return account;
        }

        private static void ManageTransactionsForAccount(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            while (true)
            {
                PrintChoiceForAccounts();

                var actionChoice = 0;
                PrintInput();
                if (int.TryParse(Console.ReadLine(), out actionChoice) && (actionChoice >= 1 && actionChoice <= 5))
                {
                    switch (actionChoice)
                    {
                        case 1:
                            AddTransaction(accountTransactions); 
                            return;
                        case 2:
                            DeleteTransaction(accountTransactions);
                            return;
                        case 3:
                            EditTransaction(accountTransactions); 
                            return;
                        case 4:
                            ViewTransactions(accountTransactions); 
                            return;
                        case 5:
                            PrintFinancialReport(accountTransactions); 
                            return;
                        default:
                            Console.WriteLine("Nevažeći odabir. Pokušajte ponovo.");
                            break;
                    } 
                }
                else
                {
                    Console.WriteLine("Nevažeći odabir. Pokušajte ponovo.");
                    continue;
                }
            }
        }

        private static void PrintFinancialReport(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            Console.WriteLine("\n1 - prikaz trenutnog stanja racuna");
            Console.WriteLine("2 - prikaz ukupnog broja transakcija");
            Console.WriteLine("3 - prikaz ukupnog iznosa prihoda i rashoda za odabrani mjesec i godinu");
            Console.WriteLine("4 - prikaz postotka udjela rashoda za odabranu kategoriju");
            Console.WriteLine("5 - prikaz prosjecnog iznosa transakcija za odabrani mjesec i godinu");
            Console.WriteLine("6 - prikaz prosjecnog iznosa transakcija za odabranu kategoriju");

            var choice = 0;
            while (true){
                PrintInput();
                int.TryParse(Console.ReadLine(), out choice);

                switch (choice) {
                    case 1:
                        var balance = ShowCurrentAccountBalance(accountTransactions);
                        Console.WriteLine($"Trenutno stanje na računu je: {balance:F2}");
                        return;
                    case 2:
                        ShowTotalTransactionCount(accountTransactions);
                        break;
                    case 3:
                        ShowTotalIncomeAndExpenseForMonthAndYear(accountTransactions);
                        return;
                    case 4:
                        ShowExpensePercentageForCategory(accountTransactions);
                        return;
                    case 5:
                        ShowAverageTransactionAmountForMonthAndYear(accountTransactions);
                        return;
                    case 6:
                        ShowAverageTransactionAmountForCategory(accountTransactions);
                        return;
                    default:
                        Console.WriteLine("Pogresan unos, pokusajte ponovo");
                        break;
                }
            }
        }

        private static void ShowAverageTransactionAmountForCategory(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            Console.WriteLine("\nUnesite kategoriju transakcije:");
            while (true)
            {
                PrintInput();
                var category = Console.ReadLine().ToLower();
                if(!ExpenseCategories.Contains(category) && !IncomeCategories.Contains(category))
                {
                    Console.WriteLine("Molim unesite valjanu kategoriju");
                    continue;
                }

                var transactionCounter = 0;
                var total = 0.0;
                foreach (var transaction in accountTransactions) { 
                    if(transaction.Item5 == category)
                    {
                        transactionCounter++;
                        total += transaction.Item2;
                    }
                }

                if(transactionCounter > 0)
                {
                    Console.WriteLine($"Prosjecni iznos transakcije za kategoriju: {category} je: {total / transactionCounter:F2}");
                    break;
                }
                else
                {
                    Console.WriteLine("Nema transakcija za odabranu kategoriju");
                    break;
                }
            }

        }

        private static void ShowAverageTransactionAmountForMonthAndYear(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            Console.WriteLine("\nUnesite mjesec i godinu za koju zelite ispisati prosjecni iznos transakcije: ");
            Console.Write("Mjesec: ");
            var month = 0;
            while (true)
            {
                int.TryParse(Console.ReadLine(), out month);
                if (month < 1 || month > 12)
                {
                    Console.WriteLine("Unesite valjanu vrijednost za mjesec (1-12)");
                    continue;
                }
                break;
            }
            Console.Write("Godina: ");
            var year = 0;
            while (true)
            {
                int.TryParse(Console.ReadLine(), out year);
                if (year < 1900 || year > DateTime.Now.Year)
                {
                    Console.WriteLine("Unesite valjanu vrijednost za godinu (1900-2024)");
                    continue;
                }
                break;
            }

            var transactionCount = 0;
            var total = 0.0;

            foreach (var transaction in accountTransactions) {
                if (month == transaction.Item6.Month && year == transaction.Item6.Year)
                {
                    transactionCount++;
                    total += transaction.Item2;
                }
            }

            if (transactionCount > 0)
            {
                Console.WriteLine($"Prosjecni iznos transakcije za mjesec: {month} i godinu {year} je: {total / transactionCount:F2}");
            }
            else
            {
                Console.WriteLine("Nema transakcija za odabrani mjesec i godinu");
            }
                
        }

        private static void ShowExpensePercentageForCategory(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            Console.WriteLine("Unesite kategoriju za rashode: hrana,prijevoz,sport,stanarina,zdravstvo");
            while (true)
            {
                PrintInput();
                var category = Console.ReadLine().ToLower();

                if (!ExpenseCategories.Contains(category))
                {
                    Console.WriteLine("Unesite valjanu kategoriju");
                    continue;
                }
                var categoryTransactions = accountTransactions.Where(t => t.Item4 == "rashod" && t.Item5 == category).ToList();

                var totalCategoryExpense = categoryTransactions.Sum(t => t.Item2);

                var totalExpenses = accountTransactions.Where(t => t.Item4 == "rashod").Sum(t => t.Item2);

                if (totalExpenses == 0)
                {
                    Console.WriteLine("Nema rashoda za izračunavanje postotka.");
                }
                else
                {
                    var percentage = (totalCategoryExpense / totalExpenses) * 100;

                    Console.WriteLine($"Postotak rashoda za kategoriju '{category}' je: {percentage:F2}%");
                }
                break;

            }
        }

        private static void ShowTotalIncomeAndExpenseForMonthAndYear(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            Console.WriteLine("Unesite mjesec i godinu za koju zelite ispisati iznos prihoda i rashoda: ");
            Console.Write("Mjesec: ");
            var month = 0;
            while (true)
            {
                int.TryParse(Console.ReadLine(), out month);
                if(month < 1 || month > 12)
                {
                    Console.WriteLine("Unesite valjanu vrijednost za mjesec (1-12)");
                    continue;
                }
                break;
            }
            Console.Write("Godina: ");
            var year = 0;
            while (true)
            {
                int.TryParse(Console.ReadLine(), out year);
                if (year < 1900 || year > DateTime.Now.Year)
                {
                    Console.WriteLine("Unesite valjanu vrijednost za godinu (1900-2024)");
                    continue;
                }
                break;
            }

            var income = 0.0;
            var expense = 0.0;
            foreach (var t in accountTransactions)
            {
                if(month == t.Item6.Month && year == t.Item6.Year)
                {
                    if (t.Item4 == "prihod")
                    {
                        income += t.Item2;
                    }
                    else
                    {
                        expense += t.Item2;
                    }
                }
            }
            Console.WriteLine($"\nUkupni prihodi za mjesec: {month} i godinu {year} je: {income}");
            Console.WriteLine($"Ukupni rashodi za mjesec: {month} i godinu {year} je: {expense}");
            Console.WriteLine($"Ukupno stanje je: {income-expense}");
        }

        private static void ShowTotalTransactionCount(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            var transactionCount = 0;
            foreach (var transaction in accountTransactions)
            {
                transactionCount++;
            }

            Console.WriteLine($"Ukupni broj transackija na racunu je: {transactionCount}");
        }

        private static double ShowCurrentAccountBalance(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            var accountBalance = 0.0;
            foreach (var transaction in accountTransactions)
            {
                if (transaction.Item4 == "prihod")
                {
                    accountBalance += transaction.Item2;
                }
                else
                {
                    accountBalance -= transaction.Item2;
                }
            }
            if(accountBalance < 0.0)
            {
                Console.WriteLine("Upozorenje! Stanje na računu je u minusu");
            }
            return accountBalance;
        }

        private static void ViewTransactions(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            Console.WriteLine("\n1 - pregled svih transakcija");
            Console.WriteLine("2 - pregled transakcija sortiranih po iznosu uzlazno");
            Console.WriteLine("3 - pregled transakcija sortiranih po iznosu silazno");
            Console.WriteLine("4 - pregled transakcija sortiranih po opisu abecedno");
            Console.WriteLine("5 - pregled transakcija sortiranih po datumu uzlazno");
            Console.WriteLine("6 - pregled transakcija sortiranih po datumu silazno");
            Console.WriteLine("7 - pregled svih prihoda");
            Console.WriteLine("8 - pregled svih rashoda");
            Console.WriteLine("9 - pregled transakcija za odabranu kategoriju");
            Console.WriteLine("10 - pregled transakcija za odabrani tip i kategoriju");

            var choice = 0;
            while (true){
                PrintInput();
                int.TryParse(Console.ReadLine(), out choice);

                switch (choice)
                {
                    case 1:
                        ViewAllTransactions(accountTransactions);
                        return;
                    case 2:
                        ViewTransactionsSortedByAmountAsc(accountTransactions);
                        return;
                    case 3:
                        ViewTransactionsSortedByAmountDesc(accountTransactions);
                        return;
                    case 4:
                        ViewTransactionsSortedByDescription(accountTransactions);
                        return;
                    case 5:
                        ViewTransactionsSortedByDateAsc(accountTransactions);
                        return;
                    case 6:
                        ViewTransactionsSortedByDateDesc(accountTransactions);
                        return;
                    case 7:
                        ViewAllIncomeTransactions(accountTransactions);
                        return;
                    case 8:
                        ViewAllExpenseTransactions(accountTransactions);
                        return;
                    case 9:
                        ViewTransactionsByCategory(accountTransactions);
                        return;
                    case 10:
                        ViewTransactionsByTypeAndCategory(accountTransactions);
                        return;
                    default:
                        Console.WriteLine("Pogresan unos, pokusaj ponovo");
                        break;
                }

            }
        }

        private static void PrintTransaction(Tuple<int, double, string, string, string, DateTime> transaction)
        {
            Console.WriteLine($"{transaction.Item4} - {transaction.Item2:F2} - {transaction.Item3} - {transaction.Item5} - {transaction.Item6.ToShortDateString()}");
        }

        private static void ViewTransactionsByTypeAndCategory(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            Console.WriteLine("\nUnesite tip transakcije (prihod ili rashod)");
            var type = string.Empty;
            while (true)
            {
                PrintInput();
                type = Console.ReadLine().ToLower();
                if(type != "prihod" && type != "rashod")
                {
                    Console.WriteLine("Pogresan unos tipa transakcije, pokušajte ponovno");
                    continue;
                }
                break;
            }
            Console.WriteLine(type == "prihod" ? "\nIzaberite kategoriju za prihode (placa,honorar,poklon,stipendija,dividende)" : "\nIzaberite kategoriju za rashode (hrana,prijevoz,sport,stanarina,zdravstvo)");
            while (true)
            {
                PrintInput();
                var category = Console.ReadLine().ToLower();
                if (!ExpenseCategories.Contains(category) && !IncomeCategories.Contains(category))
                {
                    Console.WriteLine("Pogrešan odabir kategorije, pokušajte ponovo");
                    continue;
                }
                if (type == "prihod" && IncomeCategories.Contains(category))
                {
                    foreach (var transaction in accountTransactions)
                    {
                        if (transaction.Item5 == category)
                        {
                            PrintTransaction(transaction);
                        }
                    }
                
                }else if (type == "rashod" && ExpenseCategories.Contains(category))
                {
                    foreach (var transaction in accountTransactions)
                    {
                        if (transaction.Item5 == category)
                        {
                            PrintTransaction(transaction);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Tip i kategorija transakcije su pogresni, pokusajte ponovo");
                    continue;
                }
                break;
            }
        }

        private static void ViewTransactionsByCategory(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            Console.WriteLine("\nUnesite kategoriju za koju zelite vidjeti transakcije");
            while (true)
            {
                PrintInput();
                var category = Console.ReadLine().ToLower();
                if (!ExpenseCategories.Contains(category) && !IncomeCategories.Contains(category)) {
                    Console.WriteLine("Pogrešan odabir kategorije, pokušajte ponovo");
                    continue;
                }
                
                foreach (var transaction in accountTransactions)
                {
                    if (transaction.Item5 == category)
                    {
                        PrintTransaction(transaction);
                    }
                }
                break;
            }
        }

        private static void ViewAllExpenseTransactions(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            foreach (var transaction in accountTransactions)
            {
                if (transaction.Item4 == "rashod")
                {
                    PrintTransaction(transaction);
                }
            }
        }

        private static void ViewAllIncomeTransactions(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            foreach (var transaction in accountTransactions)
            {
                if(transaction.Item4 == "prihod")
                {
                    PrintTransaction(transaction);
                }
            }
        }

        private static void ViewTransactionsSortedByDateDesc(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            var orderedTransactions = accountTransactions.OrderByDescending(x => x.Item6);

            foreach (var transaction in orderedTransactions)
            {
                PrintTransaction(transaction);
            }
        }

        private static void ViewTransactionsSortedByDateAsc(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            var orderedTransactions = accountTransactions.OrderBy(x => x.Item6);

            foreach (var transaction in orderedTransactions)
            {
                PrintTransaction(transaction);
            }
        }

        private static void ViewTransactionsSortedByDescription(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        { 
            var orderedTransactions = accountTransactions.OrderBy(x => x.Item3);

            foreach (var transaction in orderedTransactions)
            {
                PrintTransaction(transaction);
            }
        }

        private static void ViewTransactionsSortedByAmountDesc(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            var orderedTransactions = accountTransactions.OrderByDescending(x => x.Item2);

            foreach (var transaction in orderedTransactions)
            {
                PrintTransaction(transaction);
            }
        }

        private static void ViewTransactionsSortedByAmountAsc(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            var orderedTransactions = accountTransactions.OrderBy(x => x.Item2);

            foreach(var transaction in orderedTransactions)
            {
                PrintTransaction(transaction);
            }
        }

        private static void ViewAllTransactions(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            foreach(var transaction in accountTransactions)
            {
                PrintTransaction(transaction);
            }
        }

        private static void EditTransaction(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            Console.Write("\nUnesite id transakcije koju zelite urediti: ");
            var id = 0;
            PrintInput();
            while(!int.TryParse(Console.ReadLine(), out id))
            {
                Console.WriteLine("Neispravan unos, pokušajte ponovo");
            }

            var editedTransaction = accountTransactions.FirstOrDefault(transaction => transaction.Item1 == id);
           

            if (editedTransaction != null)
            {
                var newTransaction = ValidateTransaction(id, false);

                var index = accountTransactions.FindIndex(transaction => transaction.Item1 == id);
                if (index != -1) {
                    var makeSure = false;
                    if (SecondCheck(makeSure))
                    {
                        accountTransactions[index] = newTransaction;
                        Console.WriteLine("Transakcija uspješno uređena.");
                    }
                    else
                    {
                        Console.WriteLine("Odustajte od izbora, povratak na pocetnik izbornik");
                        return;
                    }
                }
            }
            else
            {
                Console.WriteLine("Transakcija s tim ID-om nije pronađena.");
            }
        }

        private static void DeleteTransaction(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            Console.WriteLine("\n1 - po id-u");
            Console.WriteLine("2 - ispod unesenog iznosa npr(sve transakcije ispod 10 eura)");
            Console.WriteLine("3 - iznad unesenog iznosa");
            Console.WriteLine("4 - brisanje transakcija svih prihoda");
            Console.WriteLine("5 - brisanje transakcija svih rashoda");
            Console.WriteLine("6 - brisanje svih transakcija za odabranu kategoriju");

            var choice = 0;
            while (true){
                PrintInput();
                int.TryParse(Console.ReadLine(), out choice);

                switch (choice){
                    case 1:
                        DeleteById(accountTransactions);
                        return;
                    case 2:
                        DeleteBelowAmount(accountTransactions);
                        return;
                    case 3:
                        DeleteOverAmonut(accountTransactions);
                        return;
                    case 4:
                        DeleteAllIncome(accountTransactions);
                        return;
                    case 5:
                        DeleteAllExpense(accountTransactions);
                        return;
                    case 6:
                        DeleteAllForCategory(accountTransactions);
                        return;
                    default:
                        Console.WriteLine("Pogrešan unos, pokušajte ponovo");
                        break;

                }
            }
        }

        private static void DeleteAllForCategory(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            Console.WriteLine("\nUnesite kategoriju za koju zelite izbrisati transakcije");

            while (true)
            {
                PrintInput();
                var category = Console.ReadLine().ToLower();
                if (!IncomeCategories.Contains(category) && !ExpenseCategories.Contains(category))
                {
                    Console.WriteLine("Kriv unos kategorije, pokusajte ponovo (plaća, honorar, poklon, stipendija, dividende hrana, prijevoz, sport, stanarina, zdravstvo)");
                    continue;
                }

                var makeSure = false;
                if (SecondCheck(makeSure)) { 
                var removeCount = accountTransactions.RemoveAll(t => t.Item5 == category);
                Console.WriteLine(removeCount > 0 ? $"Uspjesno izbrisane sve transakcije za kategoriju: {category}" : "Nema transakcija za odabranu kategoriju");
                break;
                }
                else
                {
                    Console.WriteLine("Odustajete od izbora, povratak na pocetni izbornik");
                    break;
                }
            }
        }

        private static void DeleteAllExpense(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            var makeSure = false;
            if (SecondCheck(makeSure))
            {
                accountTransactions.RemoveAll(account => account.Item4 == "rashod");
                Console.WriteLine("Izbrisane transakcije za rashode");
            }
            else
            {
                Console.WriteLine("Odustajete od izbora, povratak na pocetni izbornik");
                return;
            }
        }

        private static void DeleteAllIncome(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            var makeSure = false;
            if (SecondCheck(makeSure)){
                accountTransactions.RemoveAll(account => account.Item4 == "prihod");
                Console.WriteLine("Izbrisane transakcije za prihode");
            }
            else
            {
                Console.WriteLine("Odustajete od izbora, povratak na pocetni izbornik");
                return;
            }
        }

        private static void DeleteTransactionsByAmount(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions, double amount, bool deleteAbove)
        {
            int removedCount;
            if (deleteAbove)
            {
                removedCount = accountTransactions.RemoveAll(account => account.Item2 > amount);
                Console.WriteLine(removedCount > 0
                    ? $"Uspješno izbrisano {removedCount} transakcija iznad iznosa od {amount}."
                    : $"Nema transakcija iznad iznosa od {amount}.");
            }
            else
            {
                removedCount = accountTransactions.RemoveAll(account => account.Item2 < amount);
                Console.WriteLine(removedCount > 0
                    ? $"Uspješno izbrisano {removedCount} transakcija ispod iznosa od {amount}."
                    : $"Nema transakcija ispod iznosa od {amount}.");
            }
        }

        private static void DeleteOverAmonut(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            Console.WriteLine("\nUnesite iznos iznad kojeg želite izbrisati sve transakcije");
            var amount = 0.0;
            PrintInput();
            while (!double.TryParse(Console.ReadLine(), out amount))
            {
                Console.Write("Pogrešan unos, unesite brojčani iznos transakcije: ");
            }

            var makeSure = false;
            if (SecondCheck(makeSure))
            {
                DeleteTransactionsByAmount(accountTransactions, amount, deleteAbove: true);
            }
            else
            {
                Console.WriteLine("Odustajete od izbora, povratak na pocetni izbornik");
                return;
            }
        }

        private static void DeleteBelowAmount(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            Console.WriteLine("\nUnesite iznos ispod kojeg želite izbrisati sve transakcije");
            var amount = 0.0;
            PrintInput();
            while (!double.TryParse(Console.ReadLine(), out amount))
            {
                Console.Write("Pogrešan unos, unesite brojčani iznos transakcije: ");
            }

            var makeSure = false;
            if(SecondCheck(makeSure)){
                DeleteTransactionsByAmount(accountTransactions, amount, deleteAbove: false);
            }
            else
            {
                Console.WriteLine("Odustajete od izbora, povratak na pocetni izbornik");
                return;
            }
        }

        private static void DeleteById(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            var id = 0;
            Console.Write("\nUnesite id transakcije koju zelite izbrisati: ");
            PrintInput();
            while (!int.TryParse(Console.ReadLine(), out id))
            {
                Console.Write("Pogrešan unos, unesite brojčani id transakcije: ");
            }

            while (true) {
                var transaction = accountTransactions.FirstOrDefault(t => t.Item1 == id);

                if (transaction == null)
                {
                    Console.WriteLine($"Ne postoji transakcija s id-em {id}, pokušajte s drugim id-em");
                    Console.Write("Unesite novi ID transakcije koju želite izbrisati: ");
                    while (!int.TryParse(Console.ReadLine(), out id))
                    {
                        Console.Write("Pogrešan unos, unesite brojčani id transakcije: ");
                    }
                }
                else
                {
                    var makeSure = false;
                    if (SecondCheck(makeSure))
                    {
                        accountTransactions.Remove(transaction);
                        Console.WriteLine($"Transakcija s id-em {id} je uspješno izbrisana.");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Odustajete od izbora, povratak na pocetni izbornik");
                        break;
                    }
                }
            }
        }

        private static bool SecondCheck(bool makeSure)
        {
            Console.WriteLine("\nJeste li sigurni u odabir: da : ne");
            var choice = "";
            while (true) {
                PrintInput();
                choice = Console.ReadLine().ToLower();
                if (choice != "da" && choice != "ne") {
                    Console.WriteLine("Unos mora biti ili da ili ne.");
                    continue;
                }
                break;
            }
            return choice == "da" ? true : false;
        }

        private static void AddTransaction(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            Console.WriteLine("\n1 - trenutno izvšena transakcija");
            Console.WriteLine("2 - ranije izvršena transakcija");

            var choice = 0;
            while (true)
            {
                PrintInput();
                int.TryParse(Console.ReadLine(), out choice);
                switch (choice) {
                    case 1:
                        AddNewTransaction(accountTransactions);
                        return;
                    case 2:
                        AddTransactionFromBefore(accountTransactions);
                        return;
                    default:
                        Console.WriteLine("Pogrešan unos, pokušajte ponovo");
                        break;
                }
            }
        }


        private static Tuple<int, double, string, string, string, DateTime> ValidateTransaction(int newId, bool useCurrentDate)
        {
            var amount = 0.0;
            var description = "";
            var type = "";
            var category = "";
            DateTime date;

            while (true)
            {
                Console.Write("Unesite iznos transakcije: ");
                if (double.TryParse(Console.ReadLine(), out amount) && amount >= 0)
                {
                    break;
                }
                Console.WriteLine("Iznos transakcije mora biti veći ili jednak 0, pokušajte ponovno.");
            }

            while (true)
            {
                Console.Write("Unesite opis transakcije: ");
                description = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(description) && !IsNumber(description))
                {
                    break;
                }
                if (string.IsNullOrWhiteSpace(description))
                {
                    Console.WriteLine("Transakcija mora sadržavati opis.");
                }
                else
                {
                    Console.WriteLine("Opis ne može biti broj. Unesite valjan opis.");
                }
            }

            while (true)
            {
                Console.Write("Unesite tip transakcije (prihod ili rashod): ");
                type = Console.ReadLine().ToLower();
                if (type == "prihod" || type == "rashod")
                {
                    break;
                }
                Console.WriteLine("Unesite valjan tip transakcije (prihod ili rashod).");
            }

            while (true)
            {
                if (type == "prihod")
                {
                    Console.WriteLine("Izaberite kategoriju za prihod: placa, honorar, poklon, stipendija, dividende");
                }
                else
                {
                    Console.WriteLine("Izaberite kategoriju za rashod: hrana, prijevoz, sport, stanarina, zdravstvo");
                }

                category = Console.ReadLine().ToLower();

                if ((type == "prihod" && IncomeCategories.Contains(category)) ||
                    (type == "rashod" && ExpenseCategories.Contains(category)))
                {
                    break;
                }
                Console.WriteLine("Tip i kategorija transakcije nisu valjani, pokušajte ponovo.");
            }
            if (useCurrentDate)
            {
                date = DateTime.Now;
            }
            else
            {
                date = EnterDateOfBirth();
            }
            return Tuple.Create(newId, amount, description, type, category, date);
        }

        private static bool IsNumber(string input)
        {
            double number;
            return double.TryParse(input, out number);
        }

        private static void AddTransactionFromBefore(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            var newId = accountTransactions.Any() ? accountTransactions.Max(t => t.Item1) + 1 : 1;
            var transaction = ValidateTransaction(newId, false);
            accountTransactions.Add(transaction);
            Console.WriteLine("Transakcija uspjesno dodana");
        }

        private static void AddNewTransaction(List<Tuple<int, double, string, string, string, DateTime>> accountTransactions)
        {
            var newId = accountTransactions.Any() ? accountTransactions.Max(t => t.Item1) + 1 : 1;
            var transaction = ValidateTransaction(newId, true);
            accountTransactions.Add(transaction);
            Console.WriteLine("Transakcija uspjesno dodana");
        }

        private static void PrintChoiceForAccounts()
        {
            Console.WriteLine("\n1 - unos nove transakcije");
            Console.WriteLine("2 - brisanje transakcije");
            Console.WriteLine("3 - uređivanje transakcije");
            Console.WriteLine("4 - pregled transakcija");
            Console.WriteLine("5 - financijsko izvješće");
        }
        private static void ManageUsers(Dictionary<int, Tuple<string, string, DateTime>> users, Dictionary<int, Dictionary<string, List<Tuple<int, double, string, string, string, DateTime>>>> accounts)
        {
            Console.WriteLine("\n1 - Unos novog korisnika");
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
            do
            {
                PrintInput();
                int.TryParse(Console.ReadLine(), out choice);

                switch (choice)
                {
                    case 1:
                        AddNewUser(users, accounts);
                        break;
                    case 2:
                        DeleteUser(users);
                        break;
                    case 3:
                        EditUser(users);
                        break;
                    case 4:
                        ViewAllUsers(users, accounts);
                        break;
                    default:
                        Console.WriteLine("Neispravan unos, pokusajte ponovo(1 - 4)");
                        break;
                }
            } while (choice < 1 || choice > 4);
        }

        private static void ViewAllUsers(Dictionary<int, Tuple<string, string, DateTime>> users, Dictionary<int, Dictionary<string, List<Tuple<int, double, string, string, string, DateTime>>>> accounts)
        {
            Console.WriteLine("\n1 - ispis svih korisnika abecedno po prezimenu");
            Console.WriteLine("2 - ispis svih korisnika starijih od 30 godina");
            Console.WriteLine("3 - ispis svih korisnika sa barem 1 računom u minusu");

            var choice = 0;
            while (true)
            {
                PrintInput();
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
                    default:
                        Console.WriteLine("Pogresan unos, pokusajte ponovo");
                        break;
                }
                break;
            }
        }

        private static void PrintUsersWithNegative(Dictionary<int, Tuple<string, string, DateTime>> users, Dictionary<int, Dictionary<string, List<Tuple<int, double, string, string, string, DateTime>>>> accounts)
        {
            bool anyUserWithNegativeBalance = false;

            foreach (var user in users)
            {
                bool hasNegativeBalance = false;

                if (accounts.ContainsKey(user.Key)) {
                    foreach (var account in accounts[user.Key]) {
                        double balance = 0.0;

                        foreach(var transaction in account.Value)
                        {
                            if(transaction.Item4 == "prihod")
                            {
                                balance += transaction.Item2;
                            }
                            else
                            {
                                balance -= transaction.Item2;
                            }
                        }

                        if (balance < 0) {
                            hasNegativeBalance = true;
                            break;
                        }
                    }
                }

                if (hasNegativeBalance)
                {
                    Console.WriteLine($"\nID: {user.Key}, Ime: {user.Value.Item1}, Prezime: {user.Value.Item2}, Datum rođenja: {user.Value.Item3:yyyy-MM-dd}");
                    anyUserWithNegativeBalance = true;
                }
            }
            if (!anyUserWithNegativeBalance)
            {
                Console.WriteLine("\nNema korisnika sa računom u minusu.");
            }
        }

        private static void PrintUsersOver30(Dictionary<int, Tuple<string, string, DateTime>> users)
        {

            var currentDate = DateTime.Now;
            var filteredUsers = users.Where(u => (currentDate.Year - u.Value.Item3.Year) > 30 ||
                                                 (currentDate.Year - u.Value.Item3.Year == 30 &&
                                                  currentDate.Month >= u.Value.Item3.Month &&
                                                  currentDate.Day >= u.Value.Item3.Day));

            Console.WriteLine("\nKorisnici stariji od 30 godina:");
            foreach (var user in filteredUsers)
            {
                Console.WriteLine($"ID: {user.Key}, Ime: {user.Value.Item1}, Prezime: {user.Value.Item2}, Datum rođenja: {user.Value.Item3:yyyy-MM-dd}");
            }

        }

        private static void PrintUsersByLastname(Dictionary<int, Tuple<string, string, DateTime>> users)
        {
            var sortedUsers = users.OrderBy(u => u.Value.Item2);
            Console.WriteLine("\nKorisnici poredani abecedno po prezimenu:");
            foreach (var user in sortedUsers)
            {
                Console.WriteLine($"ID: {user.Key}, Ime: {user.Value.Item1}, Prezime: {user.Value.Item2}, Datum rođenja: {user.Value.Item3:yyyy-MM-dd}");
            }
        }

        private static void EditUser(Dictionary<int, Tuple<string, string, DateTime>> users)
        {
            Console.WriteLine("\nUnesite id korisnika kojeg želite urediti");

            while (true)
            {
                var id = 0;
                PrintInput();
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

                var makeSure = false;
                if (SecondCheck(makeSure))
                {
                    users[id] = new Tuple<string, string, DateTime>(firstName, lastName, dateTime);
                    Console.WriteLine($"Korisnik sa id-om {id} ureden");
                    break;
                }
                else
                {
                    Console.WriteLine("Odustajete od izbora, povratak na pocetni izbornik");
                    break;
                }

            }
        }

        private static void DeleteUser(Dictionary<int, Tuple<string, string, DateTime>> users)
        {
            Console.WriteLine("\nBrisanje korisnika po: ");
            Console.WriteLine(" 1 - id");
            Console.WriteLine(" 2 - ime i prezime");

            var choice = 0;
            do
            {
                PrintInput();
                int.TryParse(Console.ReadLine(), out choice);

                switch (choice)
                {
                    case 1:
                        DeleteUserById(users);
                        break;
                    case 2:
                        DeleteUserByName(users);
                        break;
                    default:
                        Console.WriteLine("Neispravan unos, pokusajte ponovno(1 - 2)");
                        break;
                }
            } while (choice < 1 || choice > 2);

        }

        private static void DeleteUserByName(Dictionary<int, Tuple<string, string, DateTime>> users)
        {
            Console.WriteLine("\nUnesite ime i prezime korisnika");

            while (true)
            {
                PrintInput();
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
                    var makeSure = false;
                    if (SecondCheck(makeSure))
                    {
                        users.Remove(userIdToDelete.Value);
                        Console.WriteLine($"Korisnik {firstName} {lastName} je uspješno izbrisan");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Odustajete od izbora, povratak na pocetni izbornik");
                        break;
                    }
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
            Console.WriteLine("\nUnesite id korisnika kojeg zelite izbrisati");
            var id = 0;

            while (true)
            {
                PrintInput();
                int.TryParse(Console.ReadLine(), out id);
                if (!users.ContainsKey(id))
                {
                    Console.WriteLine("Korisnik sa id-om {0} ne postoji, pokusajte sa drugim id-om", id);
                    continue;
                }
                else
                {
                    var makeSure = false;
                    if (SecondCheck(makeSure))
                    {
                        Console.WriteLine("Brišete korisnika sa id-em {0}", id);
                        users.Remove(id);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Odustajete od izbora, povratak na pocetni izbornik");
                        break;
                    }
                }
            }
        }

        private static void AddNewUser(Dictionary<int, Tuple<string, string, DateTime>> users, Dictionary<int, Dictionary<string, List<Tuple<int, double, string, string, string, DateTime>>>> accounts)
        {
            var newUserId = users.Any() ? users.Keys.Max() + 1 : 1;

            var firstName = "";
            var lastName = "";

            while (true)
            {

                Console.WriteLine("Unesite ime korisnika");
                firstName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(firstName))
                {
                    Console.WriteLine("Ime ne smije biti prazno. Pokušajte ponovno.");
                    continue;
                }
                else if (!firstName.All(char.IsLetter))
                {
                    Console.WriteLine("Ime smije sadržavati samo slova. Pokušajte ponovno.");
                    continue;
                }
                break;
            }
            while (true) { 
                Console.WriteLine("Unesite prezime korisnika");
                lastName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(lastName))  
                {
                    Console.WriteLine("Prezime ne smije biti prazno. Pokušajte ponovno.");
                    continue;  
                }
                else if (!lastName.All(char.IsLetter))  
                {
                    Console.WriteLine("Prezime smije sadržavati samo slova. Pokušajte ponovno.");
                    continue;  
                }

                break;
            }

            DateTime birthDate = EnterDateOfBirth();

            users.Add(newUserId, Tuple.Create(firstName, lastName, birthDate));


            var userAccounts = new Dictionary<string, List<Tuple<int, double, string, string, string, DateTime>>>()
            {
                { "Tekuci", new List<Tuple<int, double, string, string, string, DateTime>>
                    {
                        Tuple.Create(1, 100.00, "Početno stanje", "prihod", "početni depozit", DateTime.Now)
                    }
                },
                { "Ziro", new List<Tuple<int, double, string, string, string, DateTime>>
                    {
                         Tuple.Create(1, 0.00, "Početno stanje", "prihod", "početni depozit", DateTime.Now)
                    }
                },
                { "Prepaid", new List<Tuple<int, double, string, string, string, DateTime>>
                    {
                         Tuple.Create(1, 0.00, "Početno stanje", "prihod", "početni depozit", DateTime.Now)
                    }
                }
            };

            accounts.Add(newUserId,userAccounts);

            Console.WriteLine($"Novi korisnik {firstName} {lastName} uspješno dodan s ID-em {newUserId}.");
        }


        private static DateTime EnterDateOfBirth()
        {
            var day = 0;
            while (true)
            {
                Console.Write("Dan: ");
                int.TryParse(Console.ReadLine(), out day);
                if (day < 1 || day > 31)
                {
                    Console.WriteLine("Unesite valjan dan (1-31)");
                    continue;
                }
                break;
            }

            var month = 0;
            while (true)
            {
                Console.Write("Mjesec: ");
                int.TryParse(Console.ReadLine(), out month);
                if (month < 1 || month > 12)
                {
                    Console.WriteLine("Unesite valjan mjesec (1-12)");
                    continue;
                }
                break;
            }

            var year = 0;
            while (true)
            {
                Console.Write("Godina: ");
                int.TryParse(Console.ReadLine(), out year);
                if (year < 1900 || year > DateTime.Now.Year)
                {
                    Console.WriteLine("Unesite valjanu godinu (1900-2024)");
                    continue;
                }
                break;
            }
            
            return new DateTime(year, month, day);
        }
        private static int InitialPrint()
        {
            Console.WriteLine("1 - Korisnici\n2 - Računi\n3 - Izlaz iz aplikacije");

            int choice = 0;
            PrintInput();
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

        private static Dictionary<int, Dictionary<string, List<Tuple<int, double, string, string, string, DateTime>>>> InitializeAccounts()
        {
            return new Dictionary<int, Dictionary<string, List<Tuple<int, double, string, string, string, DateTime>>>>()
             {
                 { 1, new Dictionary<string, List<Tuple<int, double, string, string, string, DateTime>>>()
                     {
                         { "Tekuci", new List<Tuple<int, double, string, string, string, DateTime>>() {
                            Tuple.Create(1, 100.00, "Početno stanje", "prihod", "placa", new DateTime(2022,1,4)),
                            Tuple.Create(2, 200.00, "Primanje", "prihod", "placa", new DateTime(2022,2,3)),
                            Tuple.Create(3, 50.00, "Clanarina", "rashod", "sport", new DateTime(2022,1,5))
                         }},
                         { "Ziro", new List<Tuple<int, double, string, string, string, DateTime>>() {
                            Tuple.Create(1, 200.00, "Isplata", "prihod", "honorar", new DateTime(2022,1,6))
                         }},
                         { "Prepaid", new List<Tuple<int, double, string, string, string, DateTime>>() {
                            Tuple.Create(1, 50.00, "Uplata", "prihod", "stipendija", DateTime.Now)
                         }}        
                    }
                 },

                 { 2, new Dictionary<string, List<Tuple<int, double, string, string, string, DateTime>>>()
                     {
                        { "Tekuci", new List<Tuple<int, double, string, string, string, DateTime>>(){
                           Tuple.Create(1, 300.00, "Početno stanje", "prihod", "placa", DateTime.Now),
                           Tuple.Create(2, 100.00, "Kupovina", "rashod", "zdravstvo", DateTime.Now)
                        }},
                        { "Ziro", new List<Tuple<int, double, string, string, string, DateTime>>(){
                           Tuple.Create(1, 150.00, "Isplata", "prihod", "placa", DateTime.Now)
                        }},
                        { "Prepaid", new List<Tuple<int, double, string, string, string, DateTime>>(){
                           Tuple.Create(1, 70.00, "Uplata", "prihod", "stipendija", DateTime.Now)
                         }}
                     }
                 },

                { 3, new Dictionary<string, List<Tuple<int, double, string, string, string, DateTime>>>()
                     {
                        { "Tekuci", new List<Tuple<int, double, string, string, string, DateTime>>(){
                           Tuple.Create(1, 300.00, "Početno stanje", "prihod", "placa", DateTime.Now),
                           Tuple.Create(2, 100.00, "Trosak", "rashod", "stanarina", DateTime.Now)
                        }},
                        { "Ziro", new List<Tuple<int, double, string, string, string, DateTime>>(){
                           Tuple.Create(1, 150.00, "isplata", "prihod", "honorar", DateTime.Now)
                        }},
                        { "Prepaid", new List<Tuple<int, double, string, string, string, DateTime>>(){
                           Tuple.Create(1, 60.00, "Uplata", "prihod", "stipendija", DateTime.Now)
                         }}
                     }
                 },

            };
        }

    }
                
}
