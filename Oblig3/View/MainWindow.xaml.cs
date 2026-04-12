using Microsoft.EntityFrameworkCore;
using Oblig3.Models;
using System.Collections.ObjectModel;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Oblig3.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dat154Context db = new();
        private readonly ObservableCollection<Student> Students;

        public MainWindow()
        {
            InitializeComponent();

            db.Students.Load();
            db.Courses.Load();
            db.Grades.Load();

            MainGrid.ItemsSource = db.Students.Local
                .OrderBy(s => s.Studentname)
                .ToList();

            GradeDropdown.ItemsSource = new List<string> { "A", "B", "C", "D", "E", "F" };
            GradeDropdown.SelectedIndex = 5;

            var courses = db.Courses.Local.OrderBy(c => c.Coursename).ToList();
            CourseDropdown.ItemsSource = courses;
            EnrollCourseComboBox.ItemsSource = courses;

            ShowAllStudents();

            AddStudentBox.ItemsSource = db.Students.Local.OrderBy(s => s.Studentname).ToList();
            AddStudentBox.DisplayMemberPath = "Studentname";
            EnrollCourseComboBox.ItemsSource = db.Courses.Local.OrderBy(c => c.Coursename).ToList();
            AddGradeBox.ItemsSource = new List<string> { "A", "B", "C", "D", "E", "F" };
        }

        private void ShowAllStudents()
        {
            var result = db.Grades
                .Include(g => g.Student)
                .Include(g => g.CoursecodeNavigation)
                .OrderBy(g => g.Student.Studentname)
                .Select(g => new
                {
                    StudentName = g.Student.Studentname,
                    CourseName = g.CoursecodeNavigation.Coursename,
                    Grade = g.Grade1
                })
                .ToList();
            MainGrid.ItemsSource = result;
        }

        private void SearchStudent_Click(object sender, RoutedEventArgs e)
        {
            var result = db.Grades
                .Include(g => g.Student)
                .Include(g => g.CoursecodeNavigation)
                .Where(g => g.Student.Studentname.Contains(SearchBox.Text))
                .OrderBy(g => g.Student.Studentname)
                .Select(g => new
                {
                    StudentName = g.Student.Studentname,
                    CourseName = g.CoursecodeNavigation.Coursename,
                    Grade = g.Grade1
                })
                .ToList();

            MainGrid.ItemsSource = result;
        }

        private void ShowCourseStudents_Click (object sender, RoutedEventArgs e)
        {
            var selectedCourse = (Course)CourseDropdown.SelectedItem;

            var result = db.Grades
                .Where(g => g.Coursecode == selectedCourse.Coursecode)
                .OrderBy(g => g.Student.Studentname)
                .Select(g => new
                {
                    StudentName = g.Student.Studentname,
                    CourseName = g.CoursecodeNavigation.Coursename,
                    Grade = g.Grade1
                })
                .ToList();
            MainGrid.ItemsSource = result;
        }

        private void ShowGrades_Click (object sender, RoutedEventArgs e)
        {
            var gradeOrder = new List<string> { "A", "B", "C", "D", "E", "F" };
            var selectedGrade = (string)GradeDropdown.SelectedItem;
            int selectedIndex = gradeOrder.IndexOf(selectedGrade);
            var allowedGrades = gradeOrder.Take(selectedIndex + 1).ToList();

            int selectedGradeIndex = gradeOrder.IndexOf(selectedGrade);

            var result = db.Grades
                .Where(g => allowedGrades.Contains(g.Grade1))
                .OrderBy(g => g.Grade1)
                .Select(g => new
                {
                    StudentName = g.Student.Studentname,
                    CourseName = g.CoursecodeNavigation.Coursename,
                    Grade = g.Grade1
                })
                .ToList();

            MainGrid.ItemsSource = result;
        }

        private void ShowFailed_Click (object sender, RoutedEventArgs e)
        {
            var result = db.Grades
                .Where(g => g.Grade1 == "F")
                .OrderBy(g => g.Student.Studentname)
                .Select(g => new
                {
                    StudentName = g.Student.Studentname,
                    CourseName = g.CoursecodeNavigation.Coursename,
                    Grade = g.Grade1
                })
                .ToList();

            MainGrid.ItemsSource = result;
        }

        private void ShowManageEnrollments_Click(object sender, RoutedEventArgs e)
        {
            EnrollPanel.Visibility = EnrollPanel.Visibility == Visibility.Collapsed
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void AddEnrollment_Click(object sender, RoutedEventArgs e)
        {
            var student = (Student)AddStudentBox.SelectedItem;
            var course = (Course)EnrollCourseComboBox.SelectedItem;
            var gradeValue = AddGradeBox.SelectedItem as String;

            if (student == null || course == null)
            {
                MessageBox.Show("Please select both a student and a course.");
                return;
            }

            db.Grades.Add(new Grade
            {
                Studentid = student.Id,
                Coursecode = course.Coursecode,
                Grade1 = gradeValue
            });

            db.SaveChanges();
            MessageBox.Show($"Enrollment added: {student.Studentname} in {course.Coursename} with grade {gradeValue}");
        }

        private void RemoveEnrollment_Click(object sender, RoutedEventArgs e)
        {
            var student = (Student)AddStudentBox.SelectedItem;
            var course = (Course)EnrollCourseComboBox.SelectedItem;

            if (student == null || course == null)
            {
                MessageBox.Show("Please select both a student and a course.");
                return;
            }

            var enrollment = db.Grades
                .FirstOrDefault(g => g.Studentid == student.Id && g.Coursecode == course.Coursecode);

            if (enrollment != null)
            {
                db.Grades.Remove(enrollment);
                db.SaveChanges();
                MessageBox.Show($"Enrollment removed: {student.Studentname} from {course.Coursename}");

            }
            else
            {
                MessageBox.Show("Enrollment not found.");
            }
        }
    }
}
