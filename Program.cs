using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OlympusCodeTest
{
    internal class Program
    {
        private static int line_count = 0;
        private static readonly object lockObj = new object();
        private static readonly string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log", "out.txt");
        static void Main(string[] args)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                CountdownEvent countdown = new CountdownEvent(10);
                int thread = 0;
                //1st line
                string FirstLine = $"{line_count}, {thread}, {timestamp}";
                File.AppendAllText(filePath, FirstLine + Environment.NewLine + Environment.NewLine);
                line_count = line_count + 1;

                //concurrent run
                for (int i = 0; i < 10; i++)
                {
                    int thread_id = i + 1;
                    ThreadPool.QueueUserWorkItem(WriteToFile, new { threadId = thread_id, countdown = countdown });
                }

                // wait for all threads
                countdown.Wait();

            }
            catch (UnauthorizedAccessException ex)
            {
                // permissions issues
                Console.WriteLine($"Error: Unauthorized access to the file path. {ex.Message}");
            }
            catch (IOException ex)
            {
                // file locks, read or write errors
                Console.WriteLine($"Error: I/O error occurred. {ex.Message}");
            }
            catch (Exception ex)
            {
                //for all other errors
                Console.WriteLine($"Error: An unexpected error occurred. {ex.Message}");
            }

            Console.WriteLine("Application exiting...");


        }
        static void WriteToFile(object state)
        {
            var context = (dynamic)state;
            int thread_Id = context.threadId;
            CountdownEvent countdown = context.countdown;

            try
            {
                for (int i = 0; i < 10; i++)
                {
                    string line;
                    string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    // Lock 
                    lock (lockObj)
                    {
                        line_count++;
                        line = $"{line_count - 1}, {thread_Id}, {timestamp}";
                        try
                        {
                            Console.WriteLine($"Thread {thread_Id} is writing line {line_count - 1} to file: {filePath}"); // debug
                            File.AppendAllText(filePath, line + Environment.NewLine + Environment.NewLine);
                        }
                        catch (IOException ex)
                        {
                            Console.WriteLine($"Error writing to file there is a problem: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors while writing to the file
                Console.WriteLine($"Error in thread {thread_Id}: {ex.Message}");
            }
            finally
            {
                countdown.Signal();
            }
        }
    }
    
}
