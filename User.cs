﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Bank_gruppprojekt
{
    public class User
    {
        public string Username { get; set; }
        public string Pin { get; set; }
        public double MaxLoan { get; set; }

        public const int MaxLoginAttempts = 3;

        public User(string userName, string pin)
        {
            Username = userName;
            Pin = pin;
        } 
        
        public static User LogIn()
        {           
            Console.WriteLine("\t \tWelcome to the bank");
            AviciiBank art = new AviciiBank();
            art.PaintBank();
            int loginAttempts = 0;
            User authenticatedUser = null;

            while (loginAttempts < MaxLoginAttempts)
            {
                try
                {

                    Console.Write("\t \tEnter username: ");
                    string username = Console.ReadLine();
                    Console.Write("\t \tEnter PIN: ");
                    string pin = MaskPassword();

                    authenticatedUser = Customer.AuthenticateCustomer(username, pin);

                    if (authenticatedUser == null)
                    {
                        authenticatedUser = Administrator.AuthenticateAdministrator(username, pin);
                    }

                    if (authenticatedUser != null)
                    {
                        loginAttempts = 0;
                        Thread.Sleep(3000);
                        Console.Clear();
                        if (authenticatedUser is Customer)
                        {
                            Customer.Menu((Customer)authenticatedUser);
                        }
                        else if (authenticatedUser is Administrator)
                        {
                            Administrator.Menu((Administrator)authenticatedUser);
                        }                        
                    }
                    else
                    {
                        Console.WriteLine($"\t\u001b[31mAuthentication failed for user '{username}'. Attempts left: {MaxLoginAttempts - loginAttempts - 1}\u001b[0m");
                        loginAttempts++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Console.WriteLine($"\t\u001b[31mAuthentication failed. Attempts left: {MaxLoginAttempts - loginAttempts - 1}\u001b[0m");
                    loginAttempts++;
                }
                if (loginAttempts == MaxLoginAttempts)
                {
                    
                    art.FadeBank();

                    Console.WriteLine("Too many unsuccessful login attempts. You are now locked out");
                    
                    Thread.Sleep(2000);
                    break;
                }
            }
            return authenticatedUser;
        }
        private static string MaskPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // Ignore any key that isn't a valid password character
                if (!char.IsControl(key.KeyChar))
                {
                    password += key.KeyChar;
                    Console.Write("*"); // Print asterisk for each character
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine(); // Move to the next line after pressing Enter
            return password;
        }
    }
}

