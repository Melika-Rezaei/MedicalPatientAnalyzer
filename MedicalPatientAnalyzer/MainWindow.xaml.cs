using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Data.SqlClient;

namespace MedicalPatientAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class Patient
    {
        public string Name { get; set; }
        public double Height { get; set; } // به سانتی‌متر
        public double Weight { get; set; } // به کیلوگرم
        public double BMI
        {
            get
            {
                double heightInMeters = Height / 100.0;
                return Math.Round(Weight / (heightInMeters * heightInMeters), 2);
            }
        }
    }

    public partial class MainWindow : Window
    {
        private List<Patient> patients = new List<Patient>();
        public MainWindow()
        {
            InitializeComponent();
            dataGridPatients.ItemsSource = patients; 
            LoadPatientsFromDatabase();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text;
            bool heightValid = double.TryParse(txtHeight.Text, out double height);
            bool weightValid = double.TryParse(txtWeight.Text, out double weight);

            if (string.IsNullOrWhiteSpace(name) || !heightValid || !weightValid)
            {
                MessageBox.Show("لطفاً تمام فیلدها را به درستی وارد کنید.");
                return;
            }

            Patient newPatient = new Patient
            {
                Name = name,
                Height = height,
                Weight = weight
            };

            patients.Add(newPatient);
            
            // database
            
            string connectionString = ConfigurationManager.ConnectionStrings["MedicalDB"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Patients (Name, Height, Weight) VALUES (@Name, @Height, @Weight)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", newPatient.Name);
                    command.Parameters.AddWithValue("@Height", newPatient.Height);
                    command.Parameters.AddWithValue("@Weight", newPatient.Weight);
                    command.ExecuteNonQuery();
                }
            }


            // Refresh the DataGrid
            dataGridPatients.Items.Refresh();

            // Clear the input fields
            txtName.Clear();
            txtHeight.Clear();
            txtWeight.Clear();

            

        }


        private void LoadPatientsFromDatabase()
        {
            patients.Clear();

            string connectionString = ConfigurationManager.ConnectionStrings["MedicalDB"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Name, Height, Weight FROM Patients";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            patients.Add(new Patient
                            {
                                Name = reader["Name"].ToString(),
                                Height = Convert.ToDouble(reader["Height"]),
                                Weight = Convert.ToDouble(reader["Weight"])
                            });
                        }
                    }
                }
            }

            dataGridPatients.Items.Refresh();
        }

    }
}

