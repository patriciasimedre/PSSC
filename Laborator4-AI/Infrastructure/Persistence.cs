namespace Laborator4_AI.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Laborator4_AI.Domain.Models.ValueObjects;
    using Laborator4_AI.Domain.Models.Entities;
    using Laborator4_AI.Domain.Models;

    /// <summary>
    /// Entity Framework DbContext for exam scheduling system
    /// Manages rooms, reservations, registrations, grades, and contestations
    /// </summary>
    public class SchedulingDbContext : DbContext
    {
        public DbSet<RoomEntity> Rooms { get; set; } = null!;
        public DbSet<RoomReservation> Reservations { get; set; } = null!;
        public DbSet<StudentRegistrationEntity> StudentRegistrations { get; set; } = null!;
        public DbSet<ExamGradeEntity> ExamGrades { get; set; } = null!;
        public DbSet<ContestationEntity> Contestations { get; set; } = null!;
        
        // New entities from the SQL schema
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<StudentGradeEntity> Grades { get; set; } = null!;

        private readonly string _connectionString;

        public SchedulingDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoomEntity>().HasKey(r => r.Number);
            
            modelBuilder.Entity<RoomReservation>().HasKey(r => r.Id);
            modelBuilder.Entity<RoomReservation>()
                .HasIndex(r => new { r.RoomNumber, r.Date });

            modelBuilder.Entity<StudentRegistrationEntity>().HasKey(r => r.Id);
            modelBuilder.Entity<StudentRegistrationEntity>()
                .HasIndex(r => new { r.StudentRegNumber, r.CourseCode, r.ExamDate })
                .IsUnique();

            modelBuilder.Entity<ExamGradeEntity>().HasKey(g => g.Id);
            modelBuilder.Entity<ExamGradeEntity>()
                .HasIndex(g => new { g.StudentRegNumber, g.CourseCode, g.ExamDate })
                .IsUnique();

            modelBuilder.Entity<ContestationEntity>().HasKey(c => c.Id);

            // Configuration for new Student and Grade entities
            modelBuilder.Entity<Student>()
                .HasKey(s => s.StudentId);
            
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.RegistrationNumber)
                .IsUnique();

            modelBuilder.Entity<StudentGradeEntity>()
                .HasKey(g => g.GradeId);

            modelBuilder.Entity<StudentGradeEntity>()
                .HasOne(g => g.Student)
                .WithMany(s => s.Grades)
                .HasForeignKey(g => g.StudentId)
                .HasConstraintName("FK_Grades_Student");
        }

        public void EnsureSeeded()
        {
            // Create database and tables if they don't exist
            Database.EnsureCreated();
            
            if (!Rooms.Any())
            {
                Rooms.AddRange(new[]
                {
                    new RoomEntity { Number = "A101", Capacity = 30 },
                    new RoomEntity { Number = "A201", Capacity = 25 },
                    new RoomEntity { Number = "B105", Capacity = 20 },
                    new RoomEntity { Number = "B205", Capacity = 35 },
                    new RoomEntity { Number = "C301", Capacity = 40 },
                    new RoomEntity { Number = "C401", Capacity = 50 },
                });
                SaveChanges();
            }

            // Seed Students
            if (!Students.Any())
            {
                Students.AddRange(new[]
                {
                    new Student { RegistrationNumber = "LM12345", Name = "Popescu Ion" },
                    new Student { RegistrationNumber = "LM12346", Name = "Ionescu Maria" },
                    new Student { RegistrationNumber = "LM12347", Name = "Dumitrescu Ana" },
                    new Student { RegistrationNumber = "LM12348", Name = "Gheorghiu Mihai" },
                    new Student { RegistrationNumber = "LM12349", Name = "Vasilescu Elena" },
                });
                SaveChanges();
            }

            // Seed Grades
            if (!Grades.Any())
            {
                var students = Students.ToList();
                if (students.Any())
                {
                    Grades.AddRange(new[]
                    {
                        new StudentGradeEntity { StudentId = students[0].StudentId, Exam = 8.50m, Activity = 9.00m, Final = 8.75m },
                        new StudentGradeEntity { StudentId = students[1].StudentId, Exam = 7.25m, Activity = 8.50m, Final = 7.88m },
                        new StudentGradeEntity { StudentId = students[2].StudentId, Exam = 6.00m, Activity = 7.00m, Final = 6.50m },
                        new StudentGradeEntity { StudentId = students[3].StudentId, Exam = 9.00m, Activity = 8.75m, Final = 8.88m },
                        new StudentGradeEntity { StudentId = students[4].StudentId, Exam = 5.50m, Activity = 6.25m, Final = 5.88m },
                    });
                    SaveChanges();
                }
            }
        }
    }

    // Entity models for persistence
    public class RoomEntity
    {
        public string Number { get; set; } = string.Empty;
        public int Capacity { get; set; }
    }

    public class RoomReservation
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int DurationMinutes { get; set; }
        public string CourseCode { get; set; } = string.Empty;
    }

    public class StudentRegistrationEntity
    {
        public int Id { get; set; }
        public string StudentRegNumber { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
    }

    public class ExamGradeEntity
    {
        public int Id { get; set; }
        public string StudentRegNumber { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public decimal Grade { get; set; }
        public DateTime? PublishedAt { get; set; }
    }

    public class ContestationEntity
    {
        public int Id { get; set; }
        public string StudentRegNumber { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime FiledAt { get; set; }
    }

    /// <summary>
    /// Repository for exam scheduling persistence operations
    /// </summary>
    public static class ExamSchedulingRepository
    {
        public static IEnumerable<RoomNumber> FindAvailableRooms(
            SchedulingDbContext db,
            ExamDate date,
            Duration duration,
            Capacity capacity)
        {
            var d = date.Date;
            var reservedRoomNumbers = db.Reservations
                .Where(r => r.Date == d)
                .Select(r => r.RoomNumber)
                .ToHashSet();

            var rooms = db.Rooms
                .Where(r => r.Capacity >= capacity.Value && !reservedRoomNumbers.Contains(r.Number))
                .ToList();

            foreach (var r in rooms)
            {
                if (RoomNumber.TryCreate(r.Number, out var rn, out var _))
                    yield return rn!;
            }
        }

        public static bool ReserveRoom(
            SchedulingDbContext db,
            RoomNumber room,
            ExamDate date,
            Duration duration,
            CourseCode course)
        {
            var d = date.Date;
            
            // Check if already reserved
            var exists = db.Reservations.Any(r => r.RoomNumber == room.Value && r.Date == d);
            if (exists) return false;

            var reservation = new RoomReservation
            {
                RoomNumber = room.Value,
                Date = d,
                DurationMinutes = (int)duration.Value.TotalMinutes,
                CourseCode = course.Value
            };

            db.Reservations.Add(reservation);

            try
            {
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static RoomNumber? GetExamRoom(
            SchedulingDbContext db,
            CourseCode course,
            ExamDate date)
        {
            var reservation = db.Reservations
                .FirstOrDefault(r => r.CourseCode == course.Value && r.Date == date.Date);

            if (reservation == null) return null;

            RoomNumber.TryCreate(reservation.RoomNumber, out var room, out var _);
            return room;
        }

        public static bool ExamExists(
            SchedulingDbContext db,
            CourseCode course,
            ExamDate date)
        {
            return db.Reservations.Any(r => r.CourseCode == course.Value && r.Date == date.Date);
        }
    }

    /// <summary>
    /// Repository for student registration persistence operations
    /// </summary>
    public static class StudentRegistrationRepository
    {
        public static bool PersistRegistration(
            SchedulingDbContext db,
            StudentRegistrationNumber student,
            CourseCode course,
            ExamDate date,
            RoomNumber room)
        {
            // Check if already registered
            var exists = db.StudentRegistrations.Any(r =>
                r.StudentRegNumber == student.Value &&
                r.CourseCode == course.Value &&
                r.ExamDate == date.Date);

            if (exists) return false;

            var registration = new StudentRegistrationEntity
            {
                StudentRegNumber = student.Value,
                CourseCode = course.Value,
                ExamDate = date.Date,
                RoomNumber = room.Value,
                RegisteredAt = DateTime.Now
            };

            db.StudentRegistrations.Add(registration);

            try
            {
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static int GetStudentExamsOnDate(
            SchedulingDbContext db,
            StudentRegistrationNumber student,
            ExamDate date)
        {
            return db.StudentRegistrations
                .Count(r => r.StudentRegNumber == student.Value && r.ExamDate.Date == date.Date);
        }

        public static bool IsStudentRegistered(
            SchedulingDbContext db,
            StudentRegistrationNumber student,
            CourseCode course,
            ExamDate date)
        {
            return db.StudentRegistrations.Any(r =>
                r.StudentRegNumber == student.Value &&
                r.CourseCode == course.Value &&
                r.ExamDate == date.Date);
        }
    }

    /// <summary>
    /// Repository for exam grading persistence operations
    /// </summary>
    public static class ExamGradingRepository
    {
        public static bool PersistGrades(
            SchedulingDbContext db,
            ValidatedExamGrading grading)
        {
            try
            {
                foreach (var sg in grading.StudentGrades)
                {
                    // Check if grade already exists
                    var existing = db.ExamGrades.FirstOrDefault(g =>
                        g.StudentRegNumber == sg.Student.Value &&
                        g.CourseCode == grading.Course.Value &&
                        g.ExamDate == grading.Date.Date);

                    if (existing != null)
                    {
                        existing.Grade = sg.Grade.Value;
                        existing.PublishedAt = DateTime.Now;
                    }
                    else
                    {
                        db.ExamGrades.Add(new ExamGradeEntity
                        {
                            StudentRegNumber = sg.Student.Value,
                            CourseCode = grading.Course.Value,
                            ExamDate = grading.Date.Date,
                            Grade = sg.Grade.Value,
                            PublishedAt = DateTime.Now
                        });
                    }
                }

                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static DateTime? GetGradesPublishedDate(
            SchedulingDbContext db,
            CourseCode course,
            ExamDate date)
        {
            return db.ExamGrades
                .Where(g => g.CourseCode == course.Value && g.ExamDate == date.Date)
                .Select(g => g.PublishedAt)
                .FirstOrDefault();
        }
    }

    /// <summary>
    /// Repository for contestation persistence operations
    /// </summary>
    public static class ContestationRepository
    {
        public static bool PersistContestation(
            SchedulingDbContext db,
            StudentRegistrationNumber student,
            CourseCode course,
            ExamDate date,
            string reason)
        {
            try
            {
                db.Contestations.Add(new ContestationEntity
                {
                    StudentRegNumber = student.Value,
                    CourseCode = course.Value,
                    ExamDate = date.Date,
                    Reason = reason,
                    FiledAt = DateTime.Now
                });

                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Repository for Student and Grade operations
    /// </summary>
    public static class StudentGradeRepository
    {
        public static IEnumerable<Student> GetAllStudents(SchedulingDbContext db)
        {
            return db.Students.Include(s => s.Grades).ToList();
        }

        public static Student? GetStudentByRegistrationNumber(SchedulingDbContext db, string registrationNumber)
        {
            return db.Students
                .Include(s => s.Grades)
                .FirstOrDefault(s => s.RegistrationNumber == registrationNumber);
        }

        public static bool AddStudent(SchedulingDbContext db, string registrationNumber, string name)
        {
            try
            {
                var student = new Student
                {
                    RegistrationNumber = registrationNumber,
                    Name = name
                };

                db.Students.Add(student);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool AddGrade(SchedulingDbContext db, int studentId, decimal? exam = null, decimal? activity = null, decimal? final = null)
        {
            try
            {
                var grade = new StudentGradeEntity
                {
                    StudentId = studentId,
                    Exam = exam,
                    Activity = activity,
                    Final = final
                };

                db.Grades.Add(grade);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool UpdateGrade(SchedulingDbContext db, int gradeId, decimal? exam = null, decimal? activity = null, decimal? final = null)
        {
            try
            {
                var grade = db.Grades.Find(gradeId);
                if (grade == null) return false;

                if (exam.HasValue) grade.Exam = exam;
                if (activity.HasValue) grade.Activity = activity;
                if (final.HasValue) grade.Final = final;

                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static IEnumerable<StudentGradeEntity> GetGradesByStudent(SchedulingDbContext db, int studentId)
        {
            return db.Grades.Where(g => g.StudentId == studentId).ToList();
        }

        public static decimal GetAverageGrade(SchedulingDbContext db, int studentId)
        {
            var grades = db.Grades.Where(g => g.StudentId == studentId && g.Final.HasValue).ToList();
            return grades.Any() ? grades.Average(g => g.Final!.Value) : 0;
        }
    }
}
