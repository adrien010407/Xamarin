using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TD1.Models;
using Storm.Mvvm;
using System.Windows.Input;
using Xamarin.Forms;
using System;
using TD1.Views;

namespace TD1.ViewModels
{
    public class TodoListViewModel : ViewModelBase
    {
        private string _title;
        private ObservableCollection<Todo> _todos;
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public ObservableCollection<Todo> Todos
        {
            get => _todos;
            set => SetProperty(ref _todos, value);
        }

        public TodoListViewModel()
        {
            Title = "Choses à faire";
            AddCommand = new Command(goAddTodoPage);
            EditCommand = new Command<Todo>(goEditTodoPage);
        }

        private void goAddTodoPage(object _)
        {
            NavigationService.PushAsync(new EditTodoPage());
        }

        private void goEditTodoPage(Todo todo)
        {
            NavigationService.PushAsync(new EditTodoPage(todo.Id));
        }

        public override async Task OnResume()
        {
            await base.OnResume();

            Todos = Store.Todos;
            
           
        }
    }
}
