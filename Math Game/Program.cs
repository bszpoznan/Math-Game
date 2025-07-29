/* Requirements:

0. You need to create a Math game containing the 4 basic operations
1.The divisions should result on INTEGERS ONLY and dividends should go from 0 to 100. Example: Your app shouldn't present the division 7/2 to the user, since it doesn't result in an integer.
2.Users should be presented with a menu to choose an operation
3. You should record previous games in a List and there should be an option in the menu for the user to visualize a history of previous games.
4.You don't need to record results on a database. Once the program is closed the results will be deleted.

Challenges:
1. Try to implement levels of difficulty.
2. Add a timer to track how long the user takes to finish the game
3. Create a 'Random Game' option where players will presented with questions from random operations.
r. To follow DRY Principle, try using just one method for all games. Additionally, double check your project and try to find opportunities to achieve the sme functionallity with less code, avoiding repetition when possible.


*/

using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using Math_Game;
using Microsoft.CognitiveServices.Speech;

MathGameLogic mathGame = new MathGameLogic();
Random random = new Random();

int firstNumber;
int secondNumber;
int userMenuSelection;
int score = 0;
bool gameOver = false;
bool voiceResponse = false;

DifficultyLevel difficultyLevel = DifficultyLevel.Easy;

while (!gameOver)
{
    userMenuSelection = GetUserMenuSelection(mathGame); ;
    firstNumber = random.Next(0, 101);
    secondNumber = random.Next(0, 101);
    switch (userMenuSelection)
    {
        case 1: // Addition
            score = await PerformOperation(mathGame, firstNumber, secondNumber, '+', score, difficultyLevel, voiceResponse);
            break;
        case 2: // Subtraction
            score = await PerformOperation(mathGame, firstNumber, secondNumber, '-', score, difficultyLevel, voiceResponse);
            break;
        case 3: // Multiplication
            score = await PerformOperation(mathGame, firstNumber, secondNumber, '*', score, difficultyLevel, voiceResponse);
            break;
        case 4: // Division
            while( firstNumber % secondNumber != 0 )
            { 
                firstNumber = random.Next(0, 101);
                secondNumber = random.Next(1, 101); // Ensure second number is not zero
            }   
            score = await PerformOperation(mathGame, firstNumber, secondNumber, '/', score, difficultyLevel, voiceResponse);
            break;
        case 5: // Random Game
            char[] operations = new char[] { '+', '-', '*', '/' };
            int numberOfQuestions = -1;
            int randomOperationIndex = 0;
            do
            {
                Console.WriteLine("Please enter the number of questions you want to answer in the random game:");
            }
            while (!int.TryParse(Console.ReadLine(), out numberOfQuestions) || numberOfQuestions <= 0);
            while (numberOfQuestions-- > 0)
            {
                firstNumber = random.Next(0, 101);
                secondNumber = random.Next(0, 101);
                randomOperationIndex = random.Next(operations.Length);
                if (randomOperationIndex == 3)
                {
                    while( firstNumber % secondNumber != 0 )
                    { 
                        firstNumber = random.Next(0, 101);
                        secondNumber = random.Next(1, 101); // Ensure second number is not zero
                    }          
                }
                score += await PerformOperation(mathGame, firstNumber, secondNumber, operations[randomOperationIndex], score, difficultyLevel);
            }
            break;
        case 6: // View Game History
            Console.WriteLine("Game History:");
            foreach (var entry in mathGame.GameHistory)
            {
                Console.WriteLine(entry);
            }
            break;
        case 7: // Change Difficulty Level
            difficultyLevel = ChangeDifficulty();
            Console.WriteLine($"Difficulty level changed to {difficultyLevel}.");
            break;
        case 8: // Toggle Voice Response
            voiceResponse = !voiceResponse;
            Console.WriteLine($"Voice response is now {(voiceResponse ? "enabled" : "disabled")}.");
            break;
        case 9: // Exit
            gameOver = true;
            Console.WriteLine("Thank you for playing!");
            Console.WriteLine($"Your final score is: {score}");
            break;
        default:
            Console.WriteLine("Invalid selection. Please try again.");
            break;
    }
}


static DifficultyLevel ChangeDifficulty()
{
    int userSelection = 0;
    Console.WriteLine("Please enter a difficulty level:");
    Console.WriteLine("1. Easy");
    Console.WriteLine("2. Medium");
    Console.WriteLine("3. Hard");
    while (!int.TryParse(Console.ReadLine(), out userSelection) || userSelection < 1 || userSelection > 3)
    {
        Console.WriteLine("Invalid selection. Please enter a number between 1 and 3.");
    }
    switch (userSelection)
    {
        case 1:
            return DifficultyLevel.Easy;
        case 2:
            return DifficultyLevel.Medium;
        case 3:
            return DifficultyLevel.Hard;
    }
    return DifficultyLevel.Easy; // Default case

}

static void DisplayMathGameQuestion(int num1, int num2, char operation)
{
    Console.WriteLine($"{num1} {operation} {num2} = ??");
}

static int GetUserMenuSelection(MathGameLogic mathGame)
{
    int selection = -1;
    mathGame.ShowMenu();
    while (!int.TryParse(Console.ReadLine(), out selection) || selection < 1 || selection > 9)
    {
        Console.WriteLine("Invalid selection. Please enter a number between 1 and 9.");
    }
    return selection;
}

static async Task<string?> GetVoiceInputAsync()
{
    var speechKey = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY");
    var speechRegion = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION");
    var config = SpeechConfig.FromSubscription(speechKey, speechRegion);

    using var recognizer = new SpeechRecognizer(config);

    var result = await recognizer.RecognizeOnceAsync();

    if (result.Reason == ResultReason.RecognizedSpeech)
    {
        Console.WriteLine($"Recognized: {result.Text}");
        return System.Text.RegularExpressions.Regex.Replace(result.Text, @"[^0-9\-]", "");
    }
    else
    {
        Console.WriteLine("No speech recognized or an error occurred.");
        return null;
    }
}

static async Task<int?> GetUserResponse(DifficultyLevel difficultyLevel, bool voiceResponse = false)
{
    int response = 0;
    int timeout = (int)difficultyLevel;

    Stopwatch stopwatch = new Stopwatch();
    stopwatch.Start();

    string? result = null;
    Task<string?> getUserInputTask;

    if (voiceResponse)
    {
        getUserInputTask = Task.Run(() => GetVoiceInputAsync());
    }
    else
    {
        getUserInputTask = Task.Run(() => Console.ReadLine());
    }

    try
    {
        result = await Task.WhenAny(getUserInputTask, Task.Delay(timeout * 1000)) == getUserInputTask ? getUserInputTask.Result : null;
        stopwatch.Stop();
        if (result != null && int.TryParse(result, out response))
        {
            Console.WriteLine($"Time taken to answer: {stopwatch.Elapsed.ToString("mm\\:ss\\.fff")}");
            return response;
        }
        else
        {
            throw new OperationCanceledException();
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Time's up! You took too long to answer.");
        return null;

    }
}

static int ValidateUserResult(int result, int? userResponse, int score)
{
    if (result == userResponse)
    {
        Console.WriteLine("Congratulation! You answer correctly; You earned 5 points.");
        score += 5;
    }
    else
    {
        Console.WriteLine($"Try again! The correct answer is {result}.");
    }
    return score;
}

static async Task<int> PerformOperation(MathGameLogic mathGame, int num1, int num2, char operation, int score, DifficultyLevel difficulty, bool voiceResponse = false)
{
    int result;
    int? userResponse;
    DisplayMathGameQuestion(num1, num2, operation);
    result = mathGame.MathOperation(num1, num2, operation);
    userResponse = await GetUserResponse(difficulty, voiceResponse);
    score += ValidateUserResult(result, userResponse, score);
    return score;

} 




public enum DifficultyLevel
{
    Easy = 45,
    Medium = 30,
    Hard = 15
}