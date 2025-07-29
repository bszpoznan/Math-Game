namespace Math_Game
{
    public class MathGameLogic
    {
        public List<string> GameHistory { get; private set; } = new List<string>();

        public void ShowMenu()
        {
            Console.WriteLine("Please enter an option to select operation you want to play:");
            Console.WriteLine("1. Addition");
            Console.WriteLine("2. Subtraction");
            Console.WriteLine("3. Multiplication");
            Console.WriteLine("4. Division");
            Console.WriteLine("5. Random Game");
            Console.WriteLine("6. View Game History");
            Console.WriteLine("7. Change Difficulty Level");
            Console.WriteLine("8. Toogle Voice Response");
            Console.WriteLine("9. Exit");
        }


        public int MathOperation(int num1, int num2, char operation)
        {
            switch (operation)
            {
                case '+':
                    GameHistory.Add($"{num1} + {num2} = {num1 + num2}");
                    return num1 + num2;
                case '-':
                    GameHistory.Add($"{num1} - {num2} = {num1 - num2}");
                    return num1 - num2;
                case '*':
                    GameHistory.Add($"{num1} * {num2} = {num1 * num2}");
                    return num1 * num2;
                case '/':
                    while (num1 < 0 || num1 > 100)
                    {
                        try
                        {
                            Console.WriteLine("Please enter a number between 0 and 100.");
                            num1 = Convert.ToInt32(Console.ReadLine());
                        }
                        catch (System.Exception)
                        {
                            //do nothing   
                        }
                    }
                    GameHistory.Add($"{num1} / {num2} = {num1 * num2}");
                    return num1 / num2;
            }
            return 0;
        }

    }
}