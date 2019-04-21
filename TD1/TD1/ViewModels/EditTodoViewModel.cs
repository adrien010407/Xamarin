using Storm.Mvvm;
using System;
using System.Linq;
using System.Windows.Input;
using TD1.Models;
using Xamarin.Forms;

namespace TD1.ViewModels
{
    public class EditTodoViewModel : ViewModelBase
    {
        private string _buttonText;
        private bool _isNew;
        private string _title;
        private Todo _todo;
        public ICommand EditTodoCommand { get; set; }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string ButtonText
        {
            get => _buttonText;
            set => SetProperty(ref _buttonText, value);
        }

        public Todo Todo
        {
            get => _todo;
            set => SetProperty(ref _todo, value);
        }

        public bool IsNew
        {
            get => _isNew;
            private set => SetProperty(ref _isNew, value);
        }

        public EditTodoViewModel(Guid id = default(Guid))
        {
            if (id == Guid.Empty)
            {
                Title = "Ajouter un Todo";
                EditTodoCommand = new Command(AddTodo);
                Todo = new Todo();
                ButtonText = "Créer";
            } else
            {
                Title = "Modifier un Todo";
                EditTodoCommand = new Command(EditTodo);
                Todo = new Todo(Store.Todos.Single(e => e.Id == id));
                ButtonText = "Modifier";
            }
        }

        private void EditTodo(object _)
        {
            Store.Todos.Single(e => e.Id == Todo.Id).Text = Todo.Text;
            NavigationService.PopAsync();
        }

        private void AddTodo(object _)
        {
            Store.Todos.Add(Todo);
            NavigationService.PopAsync();
        }
    }
}