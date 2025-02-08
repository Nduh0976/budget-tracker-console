﻿using BudgetTracker.Console.Constants;
using BudgetTracker.Models;
using System.Text;

namespace BudgetTracker.Console
{
    public static class MenuHelper
    {
        public static string GetWelcomeMessage(string activeUserName)
        {
            var currentDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            var welcomeMessage = new StringBuilder();

            // Add top border
            welcomeMessage.AppendLine(new string('*', 50));
            welcomeMessage.AppendLine("*          Welcome to Budget Tracker!            *");
            welcomeMessage.AppendLine("*        Take control of your finances!          *");
            welcomeMessage.AppendLine(new string('*', 50));

            // Add date
            welcomeMessage.AppendLine($"\nToday is: {currentDate}");

            // Add active user info if available
            if (!string.IsNullOrEmpty(activeUserName))
            {
                welcomeMessage.AppendLine($"Active User: {activeUserName}");
            }
            else
            {
                welcomeMessage.AppendLine("No active user found. Select or create a user.");
            }

            // Add bottom border
            welcomeMessage.AppendLine(new string('*', 50));

            return welcomeMessage.ToString();
        }

        public static string GetMenu(int selectedOption, IList<string> userMenuItems)
        {
            var menuBuilder = new StringBuilder();
            var selectedOptionMarker = $"✅  {ForeColorConfig.GreenForeColor}";

            for (var i = 0; i < userMenuItems.Count; i++)
            {
                var marker = selectedOption == i ? selectedOptionMarker : "    ";
                menuBuilder.AppendLine($"{marker}{userMenuItems[i]}{ForeColorConfig.ForeColorReset}");
            }

            return menuBuilder.ToString();
        }
    }
}
