using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

class Program
{
    static int correctAnswers = 0;
    static int wrongAnswers = 0;
    static List<string> results = new List<string>();

    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to the C# Study Program!");
        List<Question> questions = LoadQuestionsFromFile("..\\..\\..\\questions.txt");
        
        if (questions.Count == 0)
        {
            Console.WriteLine("No questions found. Exiting...");
            return;
        }

        foreach (var question in questions)
        {
            if (question.IsMultipleChoice)
                AskMultipleChoiceQuestion(question);
            else
                AskFillInTheBlankQuestion(question);
        }

        DisplayResults();
        SaveResultsToFile("..\\..\\..\\results.txt");
    }

    static List<Question> LoadQuestionsFromFile(string filePath)
    {
        List<Question> questions = new List<Question>();
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split('|');
                if (parts.Length >= 2)
                {
                    if (parts.Length == 2) // Fill-in-the-blank
                    {
                        questions.Add(new Question(parts[0], parts[1], false));
                    }
                    else if (parts.Length >= 4) // Multiple-choice (question|answer|opt1|opt2|...)
                    {
                        // Take all parts after the correct answer as options
                        string[] options = new string[parts.Length - 2];
                        Array.Copy(parts, 2, options, 0, parts.Length - 2);
                        questions.Add(new Question(parts[0], parts[1], true, options));
                    }
                    else
                    {
                        Console.WriteLine($"Skipping malformed line: {line}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading questions: {ex.Message}");
        }
        return questions;
    }

    static void AskFillInTheBlankQuestion(Question question)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\nFill in the blank: {question.Text}");
        Console.ResetColor();

        string userAnswer = GetTimedInput(20);
        CheckAnswer(question, userAnswer);
    }

    static void AskMultipleChoiceQuestion(Question question)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\nMultiple Choice: {question.Text}");
        if (question.Options != null && question.Options.Length > 0)
        {
            for (int i = 0; i < question.Options.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {question.Options[i]}");
            }
        }
        else
        {
            Console.WriteLine("No options available for this question!");
        }
        Console.ResetColor();

        string userAnswer = GetTimedInput(20);
        if (int.TryParse(userAnswer, out int choice) && choice > 0 && choice <= question.Options.Length)
            CheckAnswer(question, question.Options[choice - 1]);
        else
            CheckAnswer(question, "Invalid input");
    }

    static string GetTimedInput(int seconds)
    {
        string input = "";
        DateTime endTime = DateTime.Now.AddSeconds(seconds);
        Console.WriteLine($"You have {seconds} seconds to answer...");

        while (DateTime.Now < endTime && string.IsNullOrEmpty(input))
        {
            if (Console.KeyAvailable)
                input = Console.ReadLine();
            Thread.Sleep(100);
        }

        return string.IsNullOrEmpty(input) ? "Time's up!" : input;
    }

    static void CheckAnswer(Question question, string userAnswer)
    {
        bool isCorrect = userAnswer.Trim().Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
        if (isCorrect)
        {
            correctAnswers++;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Correct!");
        }
        else
        {
            wrongAnswers++;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Wrong! Correct answer: {question.CorrectAnswer}");
        }
        Console.ResetColor();
        results.Add($"{question.Text} | Your answer: {userAnswer} | Correct: {isCorrect}");
    }

    static void DisplayResults()
    {
        Console.WriteLine($"\nQuiz Complete! Correct: {correctAnswers}, Wrong: {wrongAnswers}");
    }

    static void SaveResultsToFile(string filePath)
    {
        try
        {
            File.WriteAllLines(filePath, results);
            Console.WriteLine($"Results saved to {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving results: {ex.Message}");
        }
    }
}

class Question
{
    public string Text { get; }
    public string CorrectAnswer { get; }
    public bool IsMultipleChoice { get; }
    public string[] Options { get; }

    public Question(string text, string correctAnswer, bool isMultipleChoice, string[] options = null)
    {
        Text = text;
        CorrectAnswer = correctAnswer;
        IsMultipleChoice = isMultipleChoice;
        Options = options ?? Array.Empty<string>();
    }
}