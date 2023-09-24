using System.Globalization;
using Microsoft.Data.Sqlite;

internal class Program
{
    static string connectionString = @"Data Source=test-tracker.db";

    private static void Main(string[] args)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText =
                @"CREATE TABLE IF NOT EXISTS drinking_water (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Date TEXT,
            Quantity INTEGER
        )";

            tableCmd.ExecuteNonQuery();

            connection.Close();
        }

        GetUserInput();
    }

    static void GetUserInput()
    {
        Console.Clear();
        bool closeApp = false;

        while (!closeApp)
        {
            Console.WriteLine("\n\nMain Menu");
            Console.WriteLine("\nWhat would you like to do?");
            Console.WriteLine("\nType 0 to Close Application");
            Console.WriteLine("Type 1 to View All Records");
            Console.WriteLine("Type 2 to Insert Record");
            Console.WriteLine("Type 3 to Delete Record");
            Console.WriteLine("Type 4 to Update Record");
            Console.WriteLine("------------------------------------------\n");

            int userInput = int.Parse(Console.ReadLine().ToLower().Trim());

            switch (userInput)
            {
                case 0:
                    Console.WriteLine("\nGoodbye:\n");
                    closeApp = true;
                    Environment.Exit(0);
                    break;
                case 1:
                    GetAllRecords();
                    break;
                case 2:
                    Insert();
                    Console.WriteLine("Record Successfully inserted");
                    break;
                case 3:
                    Delete();
                    break;
                case 4:
                    Update();
                    break;
                default:
                    Console.WriteLine("Invalid Command. Please type a number from 0 to 4:\n");
                    break;
            }
        }
    }

    private static void GetAllRecords()
    {
        Console.Clear();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = $"SELECT * FROM drinking_water";

            List<DrinkingWater> tableData = new();

            SqliteDataReader reader = tableCmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    if (DateTime.TryParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-UK"), DateTimeStyles.None, out DateTime date))
                    {
                        tableData.Add(
                            new DrinkingWater
                            {
                                Id = reader.GetInt32(0),
                                Date = date,
                                Quantity = reader.GetInt32(2)
                            }
                        );
                    }
                    else
                    {
                        Console.WriteLine($"Invalid date format found: {reader.GetString(1)}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No records found");
            }

            connection.Close();

            Console.WriteLine("----------------------------------\n");
            foreach (var dw in tableData)
            {
                Console.WriteLine($"{dw.Id} - {dw.Date.ToString("dd-MM-yyyy")} - Quantity: {dw.Quantity}");
            }
            Console.WriteLine("-----------------------------------\n");
        }
    }

    private static void Insert()
    {
        string date = GetDateInput();

        int quantity = GetNumberInput("\n\nPlease insert the number of glasses or other unit of measure of your choice (no decimals allowed)\n\n");

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText =
                $"INSERT INTO drinking_water(date, quantity) VALUES('{date}', {quantity})";

            tableCmd.ExecuteNonQuery();
            connection.Close();
        }
    }

    private static void Delete()
    {
        Console.Clear();
        GetAllRecords();

        int recordId = GetNumberInput("\n\nPlease type the Id of the record you want to delete");

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = $"DELETE FROM drinking_water WHERE Id = '{recordId}'";

            int rowCount = tableCmd.ExecuteNonQuery();

            if (rowCount == 0)
            {
                Console.WriteLine($"\n\nRecord with Id {recordId} does not exist \n\n");
                Delete();
            }

            connection.Close();
        }

        Console.WriteLine($"\n\nRecord with Id {recordId} was deleted\n\n");
        Console.ReadLine();
        GetUserInput();
    }

    private static void Update()
    {
        Console.Clear();
        GetAllRecords();

        int recordId = GetNumberInput("\n\nPlease type the Id of the record you want to update");

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM drinking_water WHERE Id = {recordId})";
            int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (checkQuery == 0)
            {
                Console.WriteLine($"\n\nRecord with Id {recordId} does not exist \n\n");
                connection.Close();
                Update();
            }

            string date = GetDateInput();
            int quantity = GetNumberInput("\n\nPlease insert the number of glasses or other unit of measure of your choice (no decimals allowed)\n\n");

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = $"UPDATE drinking_water SET date = '{date}', quantity = {quantity} WHERE Id = {recordId}";

            tableCmd.ExecuteNonQuery();
            connection.Close();
        }

        Console.WriteLine($"\n\nRecord with Id {recordId} was updated\n\n");
        Console.ReadLine();
        GetUserInput();
    }

    internal static string GetDateInput()
    {
        Console.WriteLine("\n\nPlease insert the date: (Format: dd-mm-yy). Type 0 to return to Main Menu.\n\n");
        string dateInput = Console.ReadLine();

        if (dateInput.Length == 0)
            GetUserInput();

        while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-UK"), DateTimeStyles.None, out DateTime date))
        {
            Console.WriteLine("\n\nInvalid data. (Format: dd-mm-yy). Type 0 to return to main menu or try again:\n\n");
            dateInput = Console.ReadLine();
        }

        return dateInput;
    }

    internal static int GetNumberInput(string message)
    {
        Console.WriteLine(message);
        string numberInput = Console.ReadLine().ToLower().Trim();

        while (!int.TryParse(numberInput, out _) || Convert.ToInt32(numberInput) < 0)
        {
            Console.WriteLine("\n\nInvalid Input. Enter a number");
            numberInput = Console.ReadLine();
        }

        int finalInput = Convert.ToInt32(numberInput);
        return finalInput;
    }
}

public class DrinkingWater
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int Quantity { get; set; }
}