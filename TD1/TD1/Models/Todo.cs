using Storm.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace TD1.Models
{
    public class Todo : NotifierBase
    {
        public Guid Id { get; set; }

        private string _text;

        public string Text {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public DateTime CreationDate { get; set; }

        public ICommand DeleteCommand { get; set; }

        public Todo()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.Now;
            DeleteCommand = new Command(DeleteAction);
        }

        public Todo(string text) : this()
        {
            Text = text;
        }

        public Todo(Todo todo) : this()
        {
            Id = todo.Id;
            Text = todo.Text;
        }

        private void DeleteAction(object _)
        {
            Store.Todos.Remove(this);
        }
    }
}
