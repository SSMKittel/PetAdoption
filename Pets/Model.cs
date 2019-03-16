using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Pets
{
    internal class Model : INotifyPropertyChanged
    {
        private IList<Pet> _pets = new List<Pet>(0);
        private bool _cat = true;
        private bool _dog = true;
        private bool _rabbit = true;
        private bool _other = true;
        private DateTime? _lastUpdated;

        public IList<Pet> Pets { get { return _pets; } set { _pets = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Pets")); } }
        public bool Cat { get { return _cat; } set { _cat = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Cat")); } }
        public bool Dog { get { return _dog; } set { _dog = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Dog")); } }
        public bool Rabbit { get { return _rabbit; } set { _rabbit = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Rabbit")); } }
        public bool Other { get { return _other; } set { _other = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Other")); } }
        public DateTime? LastUpdated { get { return _lastUpdated; } set { _lastUpdated = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LastUpdated")); } }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}