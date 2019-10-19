using System;
using System.Collections.ObjectModel;
using Unity;

namespace CloudBackuper
{
    class AppState
    {
        protected object _lock = new object();
        public ObservableCollection<Line> Status = new ObservableCollection<Line>();

        internal class Line : IDisposable
        {
            private AppState owner;
            public string Data;

            public Line(string data="")
            {
                // TODO: Избавиться от этого странного взаимодействия
                owner = Program.container.Resolve<AppState>();

                Data = data;
                lock (owner._lock)
                {
                    owner.Status.Add(this);
                }
            }

            public void Dispose()
            {
                lock (owner._lock)
                {
                    owner.Status.Remove(this);
                }
            }
        }
    }
}