﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Transactions;

namespace Bank_gruppprojekt
{
    public class Customer : User, ICustomerBank, IMenuServices, ILogInServices, ILog
    {
        public const int MaxLoginAttempts = 3;
        public List<Account> Accounts { get; set; }

        private static List<Customer> Customers;

        private List<string> logActivity = new List<string>();        

        private double totalBorrowedAmount = 0;

        private const double LoanInterestRate = 0.04;

        public DateTime LastLoanTime { get; set; } = DateTime.MinValue;

        static Customer()
        {

            Customers = new List<Customer>
        {
            new Customer("Ermin", "1111"),
            new Customer("Oskar", "1234"),
            new Customer("Ludde", "3545"),
            new Customer("Isac", "4355")
        };

            Customers[0].Accounts.Add(new Account("USA-account", 2000, "USD"));
            Customers[0].Accounts.Add(new Account("Household", 52000, "SEK"));
            Customers[0].Accounts.Add(new Account("Savings", 9000, "SEK"));

            Customers[1].Accounts.Add(new Account("USA-account", 1500, "USD"));
            Customers[1].Accounts.Add(new Account("Padel", 80000, "SEK"));

            Customers[2].Accounts.Add(new Account("Main", 500, "SEK"));
            Customers[2].Accounts.Add(new Account("Savings", 10000, "SEK"));
            Customers[2].Accounts.Add(new Account("USA-account", 3200, "USD"));
            Customers[2].Accounts.Add(new Account("Trip", 70000, "SEK"));

            Customers[3].Accounts.Add(new Account("Main", 2500, "SEK"));
            Customers[3].Accounts.Add(new Account("USA-account", 1000, "USD"));
            Customers[3].Accounts.Add(new Account("Household", 50000, "SEK"));
            Customers[3].Accounts.Add(new Account("Gym", 300, "SEK"));
            Customers[3].Accounts.Add(new Account("Cs skins", 25000, "SEK"));
        }

        public Customer(string userName, string pin) : base(userName, pin)
        {
            Username = userName;
            Pin = pin;
            Accounts = new List<Account>();            
        }      

        public static void AddUser(Customer customer)
        {
            Customers.Add(customer);
        }

        public static void Deposit(Customer currentCustomer)
        {
            Console.WriteLine("Which account do you want to deposit into?");
            currentCustomer.DisplayAccounts(currentCustomer);

            if (int.TryParse(Console.ReadLine(), out int accountIndex) && accountIndex > 0 && accountIndex <= currentCustomer.Accounts.Count)
            {
                Console.WriteLine("How much money do you want to deposit?");
                if (double.TryParse(Console.ReadLine(), out double deposit))
                {
                    currentCustomer.Accounts[accountIndex - 1].Balance += deposit;
                    Console.WriteLine($"Your new balance for {currentCustomer.Accounts[accountIndex - 1].Accounttype} account is {currentCustomer.Accounts[accountIndex - 1].Balance}{currentCustomer.Accounts[accountIndex - 1].Currency}");

                    currentCustomer.LogDeposit(deposit, currentCustomer.Accounts[accountIndex - 1].Currency);
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid number.");
                }
            }
            else
            {
                Console.WriteLine("Invalid account selection.");
            }
        }
        public void CreateAccount(string accountType, double initialBalance, string currency)
        {            
            Account newAccount = new Account(accountType, initialBalance, currency);
            Accounts.Add(newAccount);

          
            LogDeposit(initialBalance, currency);
        }


        public void DisplayAccounts(Customer customer)
        {
            Console.WriteLine("Accounts:");
            for (int i = 0; i < customer.Accounts.Count; i++)
            {
                Console.WriteLine($"{i + 1}. [{customer.Accounts[i].Accounttype}] {customer.Accounts[i].Balance} {customer.Accounts[i].Currency}");
            }
        }


        public static void Withdraw(Customer currentCustomer)
        {
            Console.WriteLine("Which account do you want to withdraw from?");
            currentCustomer.DisplayAccounts(currentCustomer);

            if (int.TryParse(Console.ReadLine(), out int accountIndex) && accountIndex > 0 && accountIndex <= currentCustomer.Accounts.Count)
            {
                Console.WriteLine("How much do you want to withdraw?");
                if (double.TryParse(Console.ReadLine(), out double withdrawal))
                {
                    if (currentCustomer.Accounts[accountIndex - 1].Balance < withdrawal)
                    {
                        Console.WriteLine("Insufficient funds");
                    }
                    else
                    {
                        currentCustomer.Accounts[accountIndex - 1].Balance -= withdrawal;
                        Console.WriteLine($"Thank you for the withdrawal. Your new balance for {currentCustomer.Accounts[accountIndex - 1].Accounttype} account is {currentCustomer.Accounts[accountIndex - 1].Balance}{currentCustomer.Accounts[accountIndex - 1].Currency}");
                        currentCustomer.LogWithdraw(withdrawal, currentCustomer.Accounts[accountIndex - 1].Currency);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid number.");
                }
            }
            else
            {
                Console.WriteLine("Invalid account selection.");
            }
        }

        public static void ShowBalance(Customer currentCustomer)
        {        
            currentCustomer.DisplayAccounts(currentCustomer);
            Console.WriteLine("\nPress enter to exit to Login");
            Console.ReadLine();
            Console.Clear();
        }

        public static List<Customer> GetCustomerWithAccounts()
        {
            return Customers;
        }      

        public static Customer AuthenticateCustomer(string username, string pin)
        {
            
            if (!int.TryParse(pin, out int pinValue))
            {
                return null;
            }
           
            Customer authenticatedCustomer = Customers.FirstOrDefault(u => u.Username.Trim().Equals(username, StringComparison.OrdinalIgnoreCase) && u.Pin.ToString() == pin);

            if (authenticatedCustomer != null)
            {
                Console.WriteLine($"\t\u001b[32mAuthentication successful for customer: {username}\u001b[0m");
            }
            return authenticatedCustomer;
        }

        public static void Menu(Customer currentCustomer)
        {

              
            int option = 0;

            do
            {
                PrintOptions(currentCustomer);
                try
                {
                    if (int.TryParse(Console.ReadLine(), out option))
                    {
                        Console.Clear();
                        switch (option)
                        {
                            case 1:
                                Deposit(currentCustomer);
                                break;
                            case 2:
                                Withdraw(currentCustomer);
                                break;
                            case 3:
                                ShowBalance(currentCustomer);
                                break;
                            case 4:
                                AddNewAccount(currentCustomer);
                                break;
                            case 5:
                                TransferMoney(currentCustomer);
                                break;
                            case 6:
                                PrintLog(currentCustomer);
                                break;
                            case 7:
                                Loan(currentCustomer);
                                break;
                            case 8:
                                Console.WriteLine("\u001b[31mExiting...\u001b[0m");
                                Console.WriteLine("");
                                Console.WriteLine("Press enter to exit to Login");
                                Console.ReadLine();
                                Console.Clear();
                                AviciiBank art = new AviciiBank();
                                art.PaintBank();

                                break;
                            default:
                                Console.WriteLine("Invalid option. Try again.");
                                break;

                        }                     
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Invalid input. Please enter a valid number. 1-8");                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            } while (option != 8);
        }

        public static void AddNewAccount(Customer currentCustomer)
        {
            Console.WriteLine("Enter the type of the new account:");
           
            string accountType;
            do
            {
                accountType = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(accountType) || accountType.Length < 4)
                {
                    Console.WriteLine("Invalid input. Please enter a valid account type with at least 4 characters.");
                }

            } while (string.IsNullOrWhiteSpace(accountType) || accountType.Length < 4);

            Administrator.DisplayInterest(accountType);           

            Console.WriteLine("Enter the initial balance:");
            if (double.TryParse(Console.ReadLine(), out double initialBalance))
            {
                Console.WriteLine("Enter the currency (e.g., SEK, USD):");
             
                string currency = Console.ReadLine();

                currentCustomer.CreateAccount(accountType, initialBalance, currency);
                Console.WriteLine($"New {accountType} account added with an initial balance of {initialBalance} {currency}");
            
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid number.");
                Console.WriteLine("Press enter to exit to Main Menu");
                Console.ReadLine();
                Console.Clear();

            }
        }

        public static void TransferMoney(Customer currentCustomer)
        {
            Console.WriteLine("Do you want to transfer money between your own accounts or to another user?");
            Console.WriteLine("1. Own accounts");
            Console.WriteLine("2. Another user");

            if (int.TryParse(Console.ReadLine(), out int transferChoice) && (transferChoice == 1 || transferChoice == 2))
            {
                if (transferChoice == 1)
                {
                    TransferBetweenOwnAccounts(currentCustomer);
                }
                else
                {
                    TransferToAnotherUser(currentCustomer);
                }

            }
            else
            {
                Console.WriteLine("Invalid choice. Please enter 1 or 2.");
            }
        }

        public static void TransferBetweenOwnAccounts(Customer currentCustomer)
        {
            Console.WriteLine("Which account do you want to transfer money from?");
            currentCustomer.DisplayAccounts(currentCustomer);

            if (int.TryParse(Console.ReadLine(), out int fromAccountIndex) && fromAccountIndex > 0 && fromAccountIndex <= currentCustomer.Accounts.Count)
            {
                Console.WriteLine("How much money do you want to transfer?");

                if (double.TryParse(Console.ReadLine(), out double transferAmount))
                {
                    Console.WriteLine("To which account do you want to transfer money?");
                    currentCustomer.DisplayAccounts(currentCustomer);

                    if (int.TryParse(Console.ReadLine(), out int toAccountIndex) && toAccountIndex > 0 && toAccountIndex <= currentCustomer.Accounts.Count)
                    {
                        // Check if the source and destination accounts are the same
                        if (fromAccountIndex != toAccountIndex)
                        {
                            // Get the currencies of the source and destination accounts
                            string sourceCurrency = currentCustomer.Accounts[fromAccountIndex - 1].Currency;
                            string targetCurrency = currentCustomer.Accounts[toAccountIndex - 1].Currency;

                            // Check if the source and destination accounts have valid currencies
                            if ((sourceCurrency == "SEK" || sourceCurrency == "USD") && (targetCurrency == "SEK" || targetCurrency == "USD"))
                            {
                                double convertedAmount = transferAmount; // Initialize converted amount

                                // Check if currency conversion is needed
                                if (sourceCurrency != targetCurrency)
                                {
                                    double sourceToTargetRate = Administrator.GetExchangeRate(sourceCurrency, targetCurrency);
                                    double targetToSourceRate = Administrator.GetExchangeRate(targetCurrency, sourceCurrency);

                                    if (sourceToTargetRate == 1.0 || targetToSourceRate == 1.0)
                                    {
                                        Console.WriteLine("Invalid currency exchange rates. Unable to complete the transfer.");
                                        return;
                                    }

                                    convertedAmount = transferAmount * sourceToTargetRate;
                                }

                                if (currentCustomer.Accounts[fromAccountIndex - 1].Balance < transferAmount)
                                {
                                    Console.WriteLine("Insufficient funds for the selected account.");
                                }
                                else
                                {
                                    currentCustomer.Accounts[fromAccountIndex - 1].Balance -= transferAmount;
                                    currentCustomer.Accounts[toAccountIndex - 1].Balance += convertedAmount;

                                    Console.WriteLine($"Transfer successful. New balance for {currentCustomer.Accounts[fromAccountIndex - 1].Accounttype} account is {currentCustomer.Accounts[fromAccountIndex - 1].Balance} {sourceCurrency}");
                                    Console.WriteLine($"New balance for {currentCustomer.Accounts[toAccountIndex - 1].Accounttype} account is {currentCustomer.Accounts[toAccountIndex - 1].Balance} {targetCurrency}");                                 
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid currencies. Transfers are only allowed between SEK and USD accounts.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Transferring money to the same account is not allowed.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid account selection for receiving money.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid number.");
                }
            }
            else
            {
                Console.WriteLine("Invalid account selection for transferring money.");
            }
        }

        private static async Task TransferToAnotherUser(Customer currentCustomer)
        {
            double exchangeRate = Administrator.GetExchangeRate("USD", "SEK");

            Console.WriteLine("Which user do you want to transfer money to?");
            DisplayCustomer(Customers, currentCustomer);

            if (int.TryParse(Console.ReadLine(), out int toCustomerIndex) && toCustomerIndex > 0 && toCustomerIndex <= Customers.Count)
            {
                Customer receiver = Customers[toCustomerIndex - 1];

                Console.WriteLine("Which account do you want to transfer money from?");
                currentCustomer.DisplayAccounts(currentCustomer);

                if (int.TryParse(Console.ReadLine(), out int fromAccountIndex) && fromAccountIndex > 0 && fromAccountIndex <= currentCustomer.Accounts.Count)
                {
                    Console.WriteLine($"How much money do you want to transfer from {currentCustomer.Accounts[fromAccountIndex - 1].Accounttype}?");

                    if (double.TryParse(Console.ReadLine(), out double transferAmount))
                    {
                        if (currentCustomer.Accounts[fromAccountIndex - 1].Balance < transferAmount)
                        {
                            Console.WriteLine("Insufficient funds for the selected account.");
                        }
                        else
                        {
                            Console.WriteLine("Which account do you want to transfer money to?");
                            currentCustomer.DisplayAccounts(receiver);

                            if (int.TryParse(Console.ReadLine(), out int toAccountIndex) && toAccountIndex > 0 && toAccountIndex <= receiver.Accounts.Count)
                            {
                                // Check if the source and destination accounts have valid currencies
                                string sourceCurrency = currentCustomer.Accounts[fromAccountIndex - 1].Currency;
                                string targetCurrency = receiver.Accounts[toAccountIndex - 1].Currency;

                                if ((sourceCurrency == "SEK" || sourceCurrency == "USD") && (targetCurrency == "SEK" || targetCurrency == "USD"))
                                {
                                    double convertedAmount = transferAmount; // Initialize converted amount

                                    // Check if currency conversion is needed
                                    if (sourceCurrency != targetCurrency)
                                    {
                                        double sourceToTargetRate = Administrator.GetExchangeRate(sourceCurrency, targetCurrency);
                                        double targetToSourceRate = Administrator.GetExchangeRate(targetCurrency, sourceCurrency);

                                        if (sourceToTargetRate == 1.0 || targetToSourceRate == 1.0)
                                        {
                                            Console.WriteLine("Invalid currency exchange rates. Unable to complete the transfer.");
                                            return;
                                        }

                                        convertedAmount = transferAmount * sourceToTargetRate;
                                    }

                                    // Perform the transfer
                                    currentCustomer.Accounts[fromAccountIndex - 1].Balance -= transferAmount;
                                    Console.WriteLine($"Thank you for the transfer. Your new balance for {currentCustomer.Accounts[fromAccountIndex - 1].Accounttype} " +
                                        $"account is {currentCustomer.Accounts[fromAccountIndex - 1].Balance} {sourceCurrency}." +
                                        $"\nThe receiver will be able to see the transfer of funds in approximately 15 minutes.");
                                    await Task.Delay(1 * 5 * 1000); // 15 min

                                    receiver.Accounts[toAccountIndex - 1].Balance += convertedAmount;                                  
                                }
                                else
                                {
                                    Console.WriteLine("Invalid currencies. Transfers are only allowed between SEK and USD accounts.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid account selection for receiving money.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a valid number.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid account selection for transferring money.");
                }
            }
            else
            {
                Console.WriteLine("Invalid user selection for transferring money.");
            }
        }
        public static void DisplayCustomer(List<Customer> customers, Customer currentCustomer)
            {
                var otherCustomer = customers.Where(customer => customer.Username != currentCustomer.Username).ToList();
                foreach (var customer in otherCustomer)
                {
                    var index = customers.IndexOf(customer);
                    Console.WriteLine($"{index + 1}. {customers[index].Username}");
                }
            }

        public static void Loan(Customer currentCustomer)
        {
            if ((DateTime.Now - currentCustomer.LastLoanTime).TotalDays < 30)
            {
                Console.WriteLine("You can only apply for one loan once every 30 days. If you need further help contact the bank.");
                return;
            }

            Console.WriteLine("Select the account you want to loan money to:");
            currentCustomer.DisplayAccounts(currentCustomer);

            if (int.TryParse(Console.ReadLine(), out int selectedAccountIndex) && selectedAccountIndex > 0 && selectedAccountIndex <= currentCustomer.Accounts.Count)
            {
                int accountIndex = selectedAccountIndex - 1;

                if (currentCustomer.Accounts[accountIndex].Currency.ToUpper() != "USD")
                {
                    double remainingLoanAmount = currentCustomer.GetMaxLoanAmount(accountIndex) - currentCustomer.totalBorrowedAmount;

                    if (remainingLoanAmount <= 0)
                    {
                        Console.WriteLine("You have reached the maximum loan limit. Unable to process another loan.");
                        return;
                    }

                    Console.WriteLine($"Enter the amount you want to borrow (up to {remainingLoanAmount} {currentCustomer.Accounts[accountIndex].Currency}):");

                    if (double.TryParse(Console.ReadLine(), out double loanAmount) && loanAmount > 0)
                    {
                        if (loanAmount <= remainingLoanAmount)
                        {
                            Console.WriteLine("Enter the number of months for the loan:");
                            if (int.TryParse(Console.ReadLine(), out int loanMonths) && loanMonths > 0)
                            {
                                double totalInterest = loanAmount * LoanInterestRate * loanMonths;

                                if (currentCustomer.Accounts[accountIndex].Currency.ToUpper() == "USD")
                                {
                                    loanAmount *= Administrator.usdToSekRate;
                                    totalInterest *= Administrator.usdToSekRate;
                                }

                                currentCustomer.DepositLoan(loanAmount, accountIndex);
                                currentCustomer.totalBorrowedAmount += loanAmount;

                                Console.WriteLine($"Loan of {loanAmount} {currentCustomer.Accounts[accountIndex].Currency} successfully deposited into your {currentCustomer.Accounts[accountIndex].Accounttype} account.");
                                Console.WriteLine($"You will need to pay {totalInterest} {currentCustomer.Accounts[accountIndex].Currency} in interest for the {loanMonths}-month loan.");

                                Console.WriteLine("\nPress enter to exit to Main Menu");
                                Console.ReadLine();
                                Console.Clear();
                                currentCustomer.LastLoanTime = DateTime.Now;
                            }
                            else
                            {
                                Console.WriteLine("Invalid input for the number of months. Please enter a valid number.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Loan amount exceeds the remaining limit. Remaining allowed loan is {remainingLoanAmount} {currentCustomer.Accounts[accountIndex].Currency}.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input for loan amount. Please enter a valid number.");
                    }
                }
                else
                {
                    Console.WriteLine("Loans cannot be made from accounts with USD currency.");
                }
            }
            else
            {
                Console.WriteLine("Invalid account selection.");
            }
        }

        public double GetMaxLoanAmount(int accountIndex)
        {
            const double loanLimitMultiplier = 5.0;
            double totalBalance = Accounts.Sum(account => account.Currency.ToUpper() == "USD" ? account.Balance * Administrator.usdToSekRate : account.Balance);

            if (Accounts[accountIndex].Currency.ToUpper() == "USD")
            {
                totalBalance *= Administrator.usdToSekRate;
            }

            return totalBalance * loanLimitMultiplier;
        }

        private void DepositLoan(double loanAmount, int accountIndex)
        {
            Accounts[accountIndex].Balance += loanAmount;
        }


        public void LogDeposit(double amount, string currency)
        {
            string logBoi = $"Deposit: {amount}{currency}";
            logActivity.Add(logBoi);
            Console.WriteLine(logBoi);
        }

        public void LogWithdraw(double amount, string currency)
        {
            string logBoi = $"Withdrawl: {amount}{currency}";
            logActivity.Add(logBoi);
            Console.WriteLine(logBoi);
        }


        public static void PrintLog(Customer currentCustomer)
            { currentCustomer.GetLog();
                foreach (var logboi in currentCustomer.logActivity)
                {
                    Console.WriteLine(logboi);
                }
            }

            public List<string> GetLog()
            {
                return logActivity;
            }

        public static void PrintOptions(Customer currentCustomer)
        {
            Console.WriteLine(" ╔══════════════════════════════════╗");
            Console.WriteLine(" ║            Mega Bank             ║");
            Console.WriteLine(" ╠══════════════════════════════════╣");
            Console.WriteLine(" ║ 1. Deposit                       ║");
            Console.WriteLine(" ║ 2. Withdrawal                    ║");
            Console.WriteLine(" ║ 3. Show balance                  ║");
            Console.WriteLine(" ║ 4. Open a new account            ║");
            Console.WriteLine(" ║ 5. Transfer money                ║");
            Console.WriteLine(" ║ 6. Check history of transactions ║");
            Console.WriteLine(" ║ 7. Take a loan                   ║");
            Console.WriteLine(" ║ 8. Exit                          ║");
            Console.WriteLine(" ╚══════════════════════════════════╝");
        }
    }
}

        

    


