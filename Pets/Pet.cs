using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Pets
{
    internal class PetUtils
    {
        internal static Sex ParseSex(string str)
        {
            if (str == "Male" || str == "male")
            {
                return Sex.Male;
            }
            else if (str == "Female" || str == "female")
            {
                return Sex.Female;
            }
            else
            {
                return Sex.Unspecified;
            }
        }

        internal static Status ParseStatus(string str)
        {
            if (str == "Adopted" || str == "adopted")
            {
                return Status.Adopted;
            }
            else if (str == "Available" || str == "available")
            {
                return Status.Available;
            }
            else if (str == "Hold" || str == "hold")
            {
                return Status.Hold;
            }
            else
            {
                return Status.Unspecified;
            }
        }
    }

    public interface IPets
    {
        IEnumerable<Pet> GetAll();
        Pet Get(Id id);
        void Add(Pet pet);
        void Commit();
    }

    public enum Sex
    {
        Unspecified,
        Male,
        Female
    }

    public enum Status
    {
        Unspecified,
        Available,
        Hold,
        Adopted
    }

    public struct Id : IComparable<Id>
    {
        public readonly string Origin;
        public readonly string OriginId;

        public Id(string origin, string originId)
        {
            if (origin == null || origin == string.Empty) throw new ArgumentNullException("origin");
            if (originId == null || originId == string.Empty) throw new ArgumentNullException("originId");
            this.Origin = origin;
            this.OriginId = originId;
        }

        public int CompareTo(Id other)
        {
            int c = Origin.CompareTo(other.Origin);
            if (c != 0)
            {
                return c;
            }
            return OriginId.CompareTo(other.OriginId);
        }
    }

    public class Pet
    {
        public readonly Id Id;
        public DateTime LastChanged { get; set; }

        private string location;
        private string type;
        private Status status;
        private string name;
        private Sex sex;
        private string breed;
        private string breedSecondary;
        private string age;
        private string lifestyle;
        private string training;
        private ISet<string> otherAnimals = new SortedSet<string>();

        public string Location
        {
            get { return location; }
            set
            {
                if (location != value)
                {
                    this.location = value;
                    this.LastChanged = DateTime.Now;
                }
            }
        }

        public string Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    this.type = value;
                    this.LastChanged = DateTime.Now;
                }
            }
        }

        public Status Status
        {
            get { return status; }
            set
            {
                if (status != value)
                {
                    this.status = value;
                    this.LastChanged = DateTime.Now;
                }
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    this.name = value;
                    this.LastChanged = DateTime.Now;
                }
            }
        }

        public Sex Sex
        {
            get { return sex; }
            set
            {
                if (sex != value)
                {
                    this.sex = value;
                    this.LastChanged = DateTime.Now;
                }
            }
        }

        public string Breed
        {
            get { return breed; }
            set
            {
                if (breed != value)
                {
                    this.breed = value;
                    this.LastChanged = DateTime.Now;
                }
            }
        }

        public string BreedSecondary
        {
            get { return breedSecondary; }
            set
            {
                if (breedSecondary != value)
                {
                    this.breedSecondary = value;
                    this.LastChanged = DateTime.Now;
                }
            }
        }

        public string Age
        {
            get { return age; }
            set
            {
                if (age != value)
                {
                    this.age = value;
                    this.LastChanged = DateTime.Now;
                }
            }
        }

        public string Lifestyle
        {
            get { return lifestyle; }
            set
            {
                if (lifestyle != value)
                {
                    this.lifestyle = value;
                    this.LastChanged = DateTime.Now;
                }
            }
        }

        public string Training
        {
            get { return training; }
            set
            {
                if (training != value)
                {
                    this.training = value;
                    this.LastChanged = DateTime.Now;
                }
            }
        }

        public IEnumerable<string> OtherAnimals
        {
            get
            {
                return otherAnimals;
            }
        }

        public void AddOtherAnimal(string animal)
        {
            if (otherAnimals.Add(animal))
            {
                this.LastChanged = DateTime.Now;
            }
        }

        public void RemoveOtherAnimal(string animal)
        {
            if (otherAnimals.Remove(animal))
            {
                this.LastChanged = DateTime.Now;
            }
        }

        public void ClearOtherAnimals()
        {
            if (otherAnimals.Count != 0)
            {
                this.otherAnimals.Clear();
                this.LastChanged = DateTime.Now;
            }
        }

        public Pet(string origin, string originId) : this(new Id(origin, originId))
        {
        }

        public Pet(Id id)
        {
            this.Id = id;
        }
    }
}
