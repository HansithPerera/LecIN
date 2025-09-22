using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lecin.ViewModels;

public class AppShellViewModel(string role)
{
    public bool IsAdmin { get; } = role == "Admin";
    public bool IsTeacher { get; } = role == "Teacher";
    public bool IsStudent { get; } = role == "Student";
}
