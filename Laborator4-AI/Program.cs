using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Laborator4_AI.Domain.Models.Commands;
using Laborator4_AI.Domain.Models.Events;
using Laborator4_AI.Domain.Models.ValueObjects;
using Laborator4_AI.Domain.Workflows;
using Laborator4_AI.Infrastructure;

namespace Laborator4_AI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║    SISTEM DE GESTIONARE A SESIUNII DE EXAMENE - DEMONSTRAȚIE   ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // MySQL connection string
            var connectionString = "Server=127.0.0.1;Database=student_management;User=root;Port=3306;";
            
            var db = new SchedulingDbContext(connectionString);
            
            try
            {
                db.EnsureSeeded();
                Console.WriteLine($"🐬 Database: MySQL - student_management");
                Console.WriteLine($"🏫 Available rooms: {db.Rooms.Count()}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database connection failed: {ex.Message}");
                Console.WriteLine("💡 Make sure MySQL is running and the database 'student_management' exists.");
                Console.WriteLine("   You can create it with: CREATE DATABASE student_management;");
                Console.WriteLine();
                Console.WriteLine("Apasă orice tastă pentru a ieși...");
                Console.ReadKey();
                return;
            }

            var coursesCatalog = new Dictionary<string, DateTime>
            {
                { "PSSC", DateTime.Today.AddDays(-30) },
                { "BD", DateTime.Today.AddDays(-25) },
                { "POO", DateTime.Today.AddDays(-20) },
                { "MATH1", DateTime.Today.AddDays(-15) }
            };

            var students = new HashSet<string> { "LM12345", "LM12346", "LM12347", "LM12348", "LM12349" };

            PrintSection("WORKFLOW 1: PROGRAMARE EXAMENE");
            RunExamSchedulingWorkflow(db, coursesCatalog);

            PrintSection("WORKFLOW 2: ÎNSCRIERE STUDENȚI");
            RunStudentRegistrationWorkflow(db, students);

            PrintSection("WORKFLOW 3: PUBLICARE NOTE");
            RunPublishGradesWorkflow(db, students);

            PrintSection("WORKFLOW 4: CONTESTAȚII");
            RunContestationWorkflow(db);

            PrintSection("WORKFLOW 5: GESTIONARE STUDENȚI ȘI NOTE");
            RunStudentGradeWorkflow(db);

            Console.WriteLine();
            Console.WriteLine("✅ DEMONSTRAȚIE COMPLETĂ!");
            Console.WriteLine();
            Console.WriteLine("Apasă orice tastă...");
            Console.ReadKey();
        }

        static void RunExamSchedulingWorkflow(SchedulingDbContext db, Dictionary<string, DateTime> coursesCatalog)
        {
            var workflow = new ScheduleExamWorkflow();
            // Use June 2026 for valid exam session
            var futureDate = new DateTime(2026, 6, 15);

            Console.WriteLine("\n🟢 Test 1: Programare examen PSSC (VALID)");
            var cmd1 = new ScheduleExamCommand
            {
                CourseCode = "PSSC",
                ProposedDate1 = futureDate.ToString("yyyy-MM-dd"),
                Duration = "120",
                ExpectedStudents = "25"
            };

            var result1 = workflow.Execute(cmd1,
                code => coursesCatalog.ContainsKey(code.Value),
                code => coursesCatalog[code.Value],
                (date, duration, capacity) => ExamSchedulingRepository.FindAvailableRooms(db, date, duration, capacity),
                (room, date, duration) =>
                {
                    CourseCode.TryCreate(cmd1.CourseCode, out var c, out var _);
                    return ExamSchedulingRepository.ReserveRoom(db, room, date, duration, c!);
                });
            PrintExamSchedulingResult(result1);

            Console.WriteLine("\n🔴 Test 2: Cod curs invalid");
            var cmd2 = new ScheduleExamCommand { CourseCode = "invalid", ProposedDate1 = futureDate.ToString("yyyy-MM-dd"), Duration = "120", ExpectedStudents = "25" };
            var result2 = workflow.Execute(cmd2, code => false, code => DateTime.Today, (d, dur, c) => Enumerable.Empty<RoomNumber>(), (r, d, dur) => false);
            PrintExamSchedulingResult(result2);
        }

        static void RunStudentRegistrationWorkflow(SchedulingDbContext db, HashSet<string> students)
        {
            var workflow = new RegisterStudentWorkflow();
            
            // Check if there are any reservations
            if (!db.Reservations.Any())
            {
                Console.WriteLine("\n⚠️  No exams scheduled. Skipping student registration tests.");
                return;
            }
            
            var examDate = db.Reservations.First().Date.ToString("yyyy-MM-dd");

            Console.WriteLine("\n🟢 Test 1: Înscriere LM12345 (VALID)");
            var cmd1 = new RegisterStudentCommand { StudentRegistrationNumber = "LM12345", CourseCode = "PSSC", ExamDate = examDate };
            var result1 = workflow.Execute(cmd1,
                student => students.Contains(student.Value),
                (course, date) => (ExamSchedulingRepository.ExamExists(db, course, date), ExamSchedulingRepository.GetExamRoom(db, course, date)),
                (student, date) => StudentRegistrationRepository.GetStudentExamsOnDate(db, student, date),
                (course, date) => ExamSchedulingRepository.GetExamRoom(db, course, date),
                (student, course, date) => StudentRegistrationRepository.IsStudentRegistered(db, student, course, date),
                (student, course, date, room) => StudentRegistrationRepository.PersistRegistration(db, student, course, date, room));
            PrintStudentRegistrationResult(result1);

            foreach (var studentId in new[] { "LM12346", "LM12347" })
            {
                var cmd = new RegisterStudentCommand { StudentRegistrationNumber = studentId, CourseCode = "PSSC", ExamDate = examDate };
                workflow.Execute(cmd,
                    student => students.Contains(student.Value),
                    (course, date) => (ExamSchedulingRepository.ExamExists(db, course, date), ExamSchedulingRepository.GetExamRoom(db, course, date)),
                    (student, date) => StudentRegistrationRepository.GetStudentExamsOnDate(db, student, date),
                    (course, date) => ExamSchedulingRepository.GetExamRoom(db, course, date),
                    (student, course, date) => StudentRegistrationRepository.IsStudentRegistered(db, student, course, date),
                    (student, course, date, room) => StudentRegistrationRepository.PersistRegistration(db, student, course, date, room));
            }
        }

        static void RunPublishGradesWorkflow(SchedulingDbContext db, HashSet<string> students)
        {
            var workflow = new PublishGradesWorkflow();
            
            // Check if there are any reservations
            if (!db.Reservations.Any())
            {
                Console.WriteLine("\n⚠️  No exams scheduled. Skipping grading tests.");
                return;
            }
            
            var examDate = db.Reservations.First().Date.ToString("yyyy-MM-dd");

            Console.WriteLine("\n🟢 Test 1: Publicare note (VALID)");
            var cmd1 = new PublishGradesCommand
            {
                CourseCode = "PSSC",
                ExamDate = examDate,
                StudentGrades = new List<StudentGradeInput>
                {
                    new() { StudentRegistrationNumber = "LM12345", Grade = "8.50" },
                    new() { StudentRegistrationNumber = "LM12346", Grade = "6.00" },
                    new() { StudentRegistrationNumber = "LM12347", Grade = "4.50" }
                }
            };
            var result1 = workflow.Execute(cmd1,
                (course, date) => ExamSchedulingRepository.ExamExists(db, course, date),
                (student, course, date) => StudentRegistrationRepository.IsStudentRegistered(db, student, course, date),
                grading => ExamGradingRepository.PersistGrades(db, grading));
            PrintGradingResult(result1);
        }

        static void RunContestationWorkflow(SchedulingDbContext db)
        {
            var workflow = new FileContestationWorkflow();
            
            // Check if there are any reservations
            if (!db.Reservations.Any())
            {
                Console.WriteLine("\n⚠️  No exams scheduled. Skipping contestation tests.");
                return;
            }
            
            var examDate = db.Reservations.First().Date.ToString("yyyy-MM-dd");

            Console.WriteLine("\n🟢 Test 1: Contestație (VALID)");
            var cmd1 = new FileContestationCommand
            {
                StudentRegistrationNumber = "LM12347",
                CourseCode = "PSSC",
                ExamDate = examDate,
                Reason = "Consider că problema 3 a fost evaluată incorect."
            };
            var result1 = workflow.Execute(cmd1,
                (student, course, date) => StudentRegistrationRepository.IsStudentRegistered(db, student, course, date),
                (course, date) => ExamGradingRepository.GetGradesPublishedDate(db, course, date),
                (student, course, date, reason) => ContestationRepository.PersistContestation(db, student, course, date, reason));
            PrintContestationResult(result1);
        }

        static void PrintSection(string title)
        {
            Console.WriteLine($"\n════ {title} ════");
        }

        static void PrintExamSchedulingResult(IExamSchedulingEvent evt)
        {
            if (evt is ExamScheduledEvent success)
            {
                Console.WriteLine($"   ✅ Curs: {success.Course}, Data: {success.Date}, Sala: {success.Room}");
            }
            else if (evt is ExamSchedulingFailedEvent failure)
            {
                Console.WriteLine($"   ❌ {failure.CourseCode}: {string.Join("; ", failure.Reasons)}");
            }
        }

        static void PrintStudentRegistrationResult(IStudentRegistrationEvent evt)
        {
            if (evt is StudentRegisteredEvent success)
            {
                Console.WriteLine($"   ✅ Student: {success.Student}, Curs: {success.Course}, Sala: {success.Room}");
            }
            else if (evt is StudentRegistrationFailedEvent failure)
            {
                Console.WriteLine($"   ❌ {failure.StudentRegistrationNumber}: {string.Join("; ", failure.Reasons)}");
            }
        }

        static void PrintGradingResult(IExamGradingEvent evt)
        {
            if (evt is GradesPublishedEvent success)
            {
                Console.WriteLine($"   ✅ Curs: {success.Course}, Total: {success.TotalStudents}, Promovați: {success.PassedStudents} ({success.PassRate}%)");
            }
            else if (evt is ExamGradingFailedEvent failure)
            {
                Console.WriteLine($"   ❌ {failure.CourseCode}: {string.Join("; ", failure.Reasons)}");
            }
        }

        static void PrintContestationResult(IContestationEvent evt)
        {
            if (evt is ContestationFiledEvent success)
            {
                Console.WriteLine($"   ✅ Student: {success.Student}, Curs: {success.Course}, Motiv: {success.Reason}");
            }
            else if (evt is ContestationFailedEvent failure)
            {
                Console.WriteLine($"   ❌ {failure.StudentRegistrationNumber}: {string.Join("; ", failure.Reasons)}");
            }
        }

        static void RunStudentGradeWorkflow(SchedulingDbContext db)
        {
            Console.WriteLine("\n🟢 Test 1: Afișare studenți existenți");
            var students = StudentGradeRepository.GetAllStudents(db);
            foreach (var student in students)
            {
                var avgGrade = StudentGradeRepository.GetAverageGrade(db, student.StudentId);
                Console.WriteLine($"   📚 Student: {student.Name} ({student.RegistrationNumber}) - Media: {avgGrade:F2}");
                
                foreach (var grade in student.Grades)
                {
                    Console.WriteLine($"      📝 Examen: {grade.Exam?.ToString("F2") ?? "N/A"}, " +
                                    $"Activitate: {grade.Activity?.ToString("F2") ?? "N/A"}, " +
                                    $"Finală: {grade.Final?.ToString("F2") ?? "N/A"}");
                }
            }

            Console.WriteLine("\n🟢 Test 2: Adăugare student nou");
            var success = StudentGradeRepository.AddStudent(db, "LM12350", "Popa Alexandru");
            if (success)
            {
                Console.WriteLine("   ✅ Student nou adăugat cu succes!");
                
                var newStudent = StudentGradeRepository.GetStudentByRegistrationNumber(db, "LM12350");
                if (newStudent != null)
                {
                    var gradeSuccess = StudentGradeRepository.AddGrade(db, newStudent.StudentId, 9.25m, 8.75m, 9.00m);
                    if (gradeSuccess)
                    {
                        Console.WriteLine("   ✅ Note adăugate cu succes!");
                    }
                }
            }

            Console.WriteLine("\n🟢 Test 3: Căutare student specific");
            var specificStudent = StudentGradeRepository.GetStudentByRegistrationNumber(db, "LM12345");
            if (specificStudent != null)
            {
                Console.WriteLine($"   🔍 Găsit: {specificStudent.Name} ({specificStudent.RegistrationNumber})");
                var grades = StudentGradeRepository.GetGradesByStudent(db, specificStudent.StudentId);
                Console.WriteLine($"   📊 Numărul de note: {grades.Count()}");
            }
        }
    }
}
