using Microsoft.AspNetCore.SignalR.Client;
using StudentFrontEnd.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class StudentHubClient
{
    private HubConnection _hubConnection;
    public List<StudentsVm> Students { get; set; } = new List<StudentsVm>();

    public event Action OnStudentListChanged;

    public async Task InitializeHubConnection()
    {
        if (_hubConnection == null)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/studentHub")
                .Build();

            _hubConnection.On<StudentsVm>("StudentAdded", (student) =>
            {
                Students.Add(student);
                OnStudentListChanged?.Invoke();
            });

            _hubConnection.On<StudentsVm>("StudentUpdated", (student) =>
            {
                var existingStudent = Students.Find(s => s.Studentid == student.Studentid);
                if (existingStudent != null)
                {
                    existingStudent.Studentname = student.Studentname;
                    existingStudent.Studentage = student.Studentage;
                    existingStudent.Email = student.Email;
                    OnStudentListChanged?.Invoke();
                }
            });

            _hubConnection.On<int>("StudentDeleted", (studentId) =>
            {
                Students.RemoveAll(s => s.Studentid == studentId);
                OnStudentListChanged?.Invoke();
            });

            await _hubConnection.StartAsync();
        }
    }

    public async Task AddStudent(HttpClient http, StudentsVm student)
    {
        await http.PostAsJsonAsync("api/students", student);
    }

    public async Task UpdateStudent(HttpClient http, StudentsVm student)
    {
        await http.PutAsJsonAsync($"api/students/{student.Studentid}", student);
    }

    public async Task DeleteStudent(HttpClient http, int studentId)
    {
        await http.DeleteAsync($"api/students/{studentId}");
    }
}