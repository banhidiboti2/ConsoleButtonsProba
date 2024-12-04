using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

class Program
{
    static string saveDirectory = "rajzok";
    static char[,] drawing;
    static ConsoleColor[,] colors;
    static string currentFilePath = "";
    static bool isNewDrawing = true;

    public class DrawingElement
    {
        public int X { get; set; }
        public int Y { get; set; }
        public ConsoleColor Color { get; set; }
        public char Character { get; set; }
    }

    public class Draw
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Color { get; set; }
        public char Character { get; set; }
        public string Name { get; set; }
    }

    public class DrawingContext : DbContext
    {
        public DbSet<Draw> Draws { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=drawingsdb;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Draw>()
                .Property(d => d.Color)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<Draw>()
                .Property(d => d.Character)
                .IsRequired();

            modelBuilder.Entity<Draw>()
                .Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);
        }
    }


    static void Main()
    {
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        string[] menuItems = { "Új Rajz", "Rajz Szerkesztés", "Törlés", "Kilépés" };
        int selectedIndex = 0;

        do
        {
            Console.Clear();
            DisplayMenu(menuItems, selectedIndex);

            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = (selectedIndex - 1 + menuItems.Length) % menuItems.Length;
                    break;

                case ConsoleKey.DownArrow:
                    selectedIndex = (selectedIndex + 1) % menuItems.Length;
                    break;

                case ConsoleKey.Enter:
                    Console.Clear();
                    if (selectedIndex == 0)
                    {
                        isNewDrawing = true;
                        currentFilePath = "";
                        RunDrawingOption();
                    }
                    else if (selectedIndex == 1)
                    {
                        LoadDrawingFromDatabase();
                    }
                    else if (selectedIndex == 2)
                    {
                        DeleteDrawing();
                    }
                    else if (selectedIndex == 3)
                    {
                        Environment.Exit(0);
                    }
                    break;
            }
        } while (true);
    }

    static void DisplayMenu(string[] items, int selectedIndex)
    {
        int maxLength = items.Max(item => item.Length);
        int padding = (Console.WindowWidth - maxLength - 6) / 2;
        int topPadding = (Console.WindowHeight - (items.Length * 3)) / 2;

        for (int i = 0; i < items.Length; i++)
        {
            string paddedItem = items[i].PadRight(maxLength);

            Console.WriteLine(new string(' ', padding) + "----------------------");
            if (i == selectedIndex)
            {
                Console.Write(new string(' ', padding) + "|  ");
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(paddedItem);
                Console.ResetColor();
                Console.WriteLine("  |");
            }
            else
            {
                Console.WriteLine(new string(' ', padding) + $"|  {paddedItem}  |");
            }
            Console.WriteLine(new string(' ', padding) + "----------------------");
        }
    }

    static void DeleteDrawing()
    {
        try
        {
            using (var context = new DrawingContext())
            {
                var drawings = context.Draws.Select(d => d.Name).Distinct().ToList();

                if (drawings.Count == 0)
                {
                    Console.WriteLine("Nincs mentett rajz.");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine("Válassz rajzot törléshez:");
                for (int i = 0; i < drawings.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {drawings[i]}");
                }

                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= drawings.Count)
                {
                    string selectedName = drawings[choice - 1];
                    var itemsToDelete = context.Draws.Where(d => d.Name == selectedName).ToList();

                    context.Draws.RemoveRange(itemsToDelete);
                    context.SaveChanges();

                    Console.WriteLine("Rajz sikeresen törölve.");
                }
                else
                {
                    Console.WriteLine("Érvénytelen választás.");
                }

                Console.ReadKey();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hiba történt: {ex.Message}");
            Console.ReadKey();
        }
    }

    static void LoadDrawingFromDatabase()
    {
        try
        {
            using (var context = new DrawingContext())
            {
                var drawings = context.Draws.Select(d => d.Name).Distinct().ToList();

                if (drawings.Count == 0)
                {
                    Console.WriteLine("Nincs mentett rajz.");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine("Válassz rajzot:");
                for (int i = 0; i < drawings.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {drawings[i]}");
                }

                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= drawings.Count)
                {
                    string selectedName = drawings[choice - 1];
                    var itemsToLoad = context.Draws.Where(d => d.Name == selectedName).ToList();

                    ClearDrawing();

                    foreach (var item in itemsToLoad)
                    {
                        drawing[item.Y, item.X] = item.Character;
                        colors[item.Y, item.X] = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), item.Color);
                    }

                    DisplayDrawing();
                    Console.WriteLine("Rajz sikeresen betöltve.");
                }
                else
                {
                    Console.WriteLine("Érvénytelen választás.");
                }

                Console.ReadKey();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hiba történt: {ex.Message}");
            Console.ReadKey();
        }
    }

    static void ClearDrawing()
    {
        int ww = Console.WindowWidth;
        int wh = Console.WindowHeight;
        drawing = new char[wh, ww];
        colors = new ConsoleColor[wh, ww];

        for (int i = 0; i < wh; i++)
        {
            for (int j = 0; j < ww; j++)
            {
                drawing[i, j] = ' ';
                colors[i, j] = ConsoleColor.Black;
            }
        }
    }

    static void RunDrawingOption()
    {
        int x = 0, y = 0;
        ConsoleKey key;
        int ww = Console.WindowWidth;
        int wh = Console.WindowHeight;
        bool isRunning = true;
        char currentChar = '█';
        ConsoleColor currentColor = ConsoleColor.Gray;

        if (isNewDrawing)
        {
            drawing = new char[wh, ww];
            colors = new ConsoleColor[wh, ww];
            for (int i = 0; i < wh; i++)
            {
                for (int j = 0; j < ww; j++)
                {
                    drawing[i, j] = ' ';
                    colors[i, j] = ConsoleColor.Black;
                }
            }
        }
        else
        {
            DisplayDrawing();
        }

        do
        {
            Console.SetCursorPosition(x, y);
            key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.UpArrow && y > 0) y--;
            if (key == ConsoleKey.DownArrow && y < wh - 1) y++;
            if (key == ConsoleKey.LeftArrow && x > 0) x--;
            if (key == ConsoleKey.RightArrow && x < ww - 1) x++;

            switch (key)
            {
                case ConsoleKey.D1:
                    currentColor = ConsoleColor.Red;
                    break;

                case ConsoleKey.D2:
                    currentColor = ConsoleColor.Green;
                    break;

                case ConsoleKey.D3:
                    currentColor = ConsoleColor.Blue;
                    break;

                case ConsoleKey.D4:
                    currentColor = ConsoleColor.Gray;
                    break;

                case ConsoleKey.D5:
                    currentChar = '▓';
                    break;

                case ConsoleKey.D6:
                    currentChar = '▒';
                    break;

                case ConsoleKey.D7:
                    currentChar = '░';
                    break;

                case ConsoleKey.D8:
                    currentChar = '█';
                    break;

                case ConsoleKey.Spacebar:
                    Console.SetCursorPosition(x, y);
                    Console.ForegroundColor = currentColor;
                    Console.Write(currentChar);
                    Console.ResetColor();
                    drawing[y, x] = currentChar;
                    colors[y, x] = currentColor;
                    break;
            }

            if (key == ConsoleKey.Escape)
            {
                isRunning = false;
                Console.Clear();
                Console.WriteLine("Szeretnéd elmenteni a rajzot? (i/n)");
                string? saveResponse = Console.ReadLine();
                if (saveResponse?.ToLower() == "i")
                {
                    Console.WriteLine("Adj meg egy nevet a rajzhoz:");
                    string? drawingName = Console.ReadLine();
                    if (!string.IsNullOrEmpty(drawingName))
                    {
                        SaveDrawingToDatabase(drawingName);
                    }
                }
            }

            Console.SetCursorPosition(0, wh - 1);
            Console.ResetColor();
            Console.Write($"Szín: {currentColor}, Karakter: {currentChar}   ");

        } while (isRunning);
    }

    static void SaveDrawingToDatabase(string drawingName)
    {
        try
        {
            using (var context = new DrawingContext())
            {
                context.Database.EnsureCreated();

                int ww = Console.WindowWidth;
                int wh = Console.WindowHeight;

                Console.WriteLine($"Saving drawing '{drawingName}' to database...");

                for (int i = 0; i < wh; i++)
                {
                    for (int j = 0; j < ww; j++)
                    {
                        if (drawing[i, j] != ' ')
                        {
                            var draw = new Draw
                            {
                                X = j,
                                Y = i,
                                Color = colors[i, j].ToString(),
                                Character = drawing[i, j],
                                Name = drawingName
                            };
                            context.Draws.Add(draw);
                            Console.WriteLine($"Saving: X={draw.X}, Y={draw.Y}, Color={draw.Color}, Character={draw.Character}, Name={draw.Name}");
                        }
                    }
                }

                context.SaveChanges();
                Console.WriteLine("Rajz elmentve az adatbázisba.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hiba történt: {ex.Message}");
            Console.ReadKey();
        }
    }

    static void DisplayDrawing()
    {
        Console.Clear();
        int ww = Console.WindowWidth;
        int wh = Console.WindowHeight;

        for (int i = 0; i < wh; i++)
        {
            for (int j = 0; j < ww; j++)
            {
                if (drawing[i, j] != ' ')
                {
                    Console.SetCursorPosition(j, i);
                    Console.ForegroundColor = colors[i, j];
                    Console.Write(drawing[i, j]);
                }
            }
        }

        Console.SetCursorPosition(0, wh - 1);
        Console.ResetColor();
        Console.ReadKey();
    }
}

