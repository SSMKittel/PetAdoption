using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Pets
{
    public interface IPetRepository
    {
        IEnumerable<Pet> Get();
        IEnumerable<Pet> Get(string origin);
        Pet Get(string origin, string originId);
        void Add(Pet pet);
        void Commit();
    }

    public class Pet
    {
        public Guid Id { get; private set; }
        public string Origin { get; private set; }
        public string OriginId { get; private set; }
        public LocalizedString Location { get; private set; }
        public LocalizedString Type { get; private set; }
        public LocalizedString Status { get; private set; }
        public string Name { get; private set; }
        public LocalizedString Sex { get; private set; }
        public string Breed { get; private set; }
        public string BreedSecondary { get; private set; }
        public string Age { get; private set; }
        public LocalizedString Lifestyle { get; private set; }
        public LocalizedString Training { get; private set; }
        public string Url { get; private set; }

        private IList<LocalizedString> _otherAnimals = new List<LocalizedString>();
        public IEnumerable<LocalizedString> OtherAnimals
        {
            get
            {
                return _otherAnimals;
            }
        }

        private IList<PetEvent> _events = new List<PetEvent>();
        public IEnumerable<PetEvent> Events
        {
            get
            {
                return _events;
            }
        }

        public DateTime LastChanged { get { return _events.Last().Timestamp; } }

        public void ChangeAge(string age)
        {
            if (age == null || age == string.Empty) throw new ArgumentNullException("age");
            if (age != this.Age)
            {
                when(new AgeUpdated(Id, _events.Count + 1, age));
            }
        }

        public void ChangeTraining(LocalizedString training)
        {
            if (training == null) throw new ArgumentNullException("training");
            if (training != this.Training)
            {
                when(new TrainingUpdated(Id, _events.Count + 1, training));
            }
        }

        public void ChangeOtherAnimals(IEnumerable<LocalizedString> otherAnimals)
        {
            if (otherAnimals == null) throw new ArgumentNullException("otherAnimals");
            if (!Enumerable.SequenceEqual(otherAnimals, this.OtherAnimals))
            {
                when(new OtherAnimalsUpdated(Id, _events.Count + 1, otherAnimals));
            }
        }

        public void CorrectBreed(string breed, string breedSecondary)
        {
            if (breed == null || breed == string.Empty) breed = null;
            if (breedSecondary == string.Empty) breedSecondary = null;
            if (breed == null)
            {
                breed = breedSecondary;
                breedSecondary = null;
            }
            if (breed != this.Breed || breedSecondary != this.BreedSecondary)
            {
                when(new BreedUpdated(Id, _events.Count + 1, breed, breedSecondary));
            }
        }

        public void ChangeLifestyle(LocalizedString lifestyle)
        {
            if (lifestyle == null) throw new ArgumentNullException("lifestyle");
            if (lifestyle != this.Lifestyle)
            {
                when(new LifestyleUpdated(Id, _events.Count + 1, lifestyle));
            }
        }

        public void Move(LocalizedString location)
        {
            if (location == null) throw new ArgumentNullException("location");
            if (location != this.Location)
            {
                when(new Moved(Id, _events.Count + 1, location));
            }
        }

        public void Rename(string name)
        {
            if (name == null || name == string.Empty) throw new ArgumentNullException("name");
            if (name != this.Name)
            {
                when(new Renamed(Id, _events.Count + 1, name));
            }
        }

        public void CorrectType(LocalizedString type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (type != this.Type)
            {
                when(new TypeCorrected(Id, _events.Count + 1, type));
            }
        }

        public void CorrectSex(LocalizedString sex)
        {
            if (sex == null) throw new ArgumentNullException("sex");
            if (sex != this.Sex)
            {
                when(new SexCorrected(Id, _events.Count + 1, sex));
            }
        }

        public void ChangeStatus(LocalizedString status)
        {
            if (status == null) throw new ArgumentNullException("status");
            if (status != this.Status)
            {
                when(new StatusChanged(Id, _events.Count + 1, status));
            }
        }

        public void ChangeUrl(string url)
        {
            if (url == null || url == string.Empty) throw new ArgumentNullException("url");
            if (url != this.Url)
            {
                when(new UrlChanged(Id, _events.Count + 1, url));
            }
        }

        public Pet(IEnumerable<PetEvent> stream)
        {
            foreach (PetEvent pe in stream.OrderBy(x => x.Version))
            {
                if (pe is AgeUpdated) when((AgeUpdated) pe);
                else if (pe is BreedUpdated) when((BreedUpdated) pe);
                else if (pe is LifestyleUpdated) when((LifestyleUpdated) pe);
                else if (pe is Moved) when((Moved) pe);
                else if (pe is NewArrival) when((NewArrival) pe);
                else if (pe is OtherAnimalsUpdated) when((OtherAnimalsUpdated) pe);
                else if (pe is Renamed) when((Renamed) pe);
                else if (pe is TypeCorrected) when((TypeCorrected)pe);
                else if (pe is SexCorrected) when((SexCorrected)pe);
                else if (pe is StatusChanged) when((StatusChanged) pe);
                else if (pe is TrainingUpdated) when((TrainingUpdated) pe);
                else if (pe is UrlChanged) when((UrlChanged) pe);
            }
        }

        public Pet(
            string origin, string originId,
            LocalizedString location, LocalizedString status,
            LocalizedString type, string name, LocalizedString sex,
            string breed, string breedSecondary)
        {
            if (origin == null || origin == string.Empty) throw new ArgumentNullException("origin");
            if (originId == null || originId == string.Empty) throw new ArgumentNullException("originId");
            if (location == null) throw new ArgumentNullException("location");
            if (status == null) throw new ArgumentNullException("status");
            if (type == null) throw new ArgumentNullException("type");
            if (name == null || name == string.Empty) throw new ArgumentNullException("name");
            if (sex == null) throw new ArgumentNullException("sex");
            if (breed == null || breed == string.Empty) breed = null;
            if (breedSecondary == string.Empty) breedSecondary = null;
            if (breed == null)
            {
                breed = breedSecondary;
                breedSecondary = null;
            }
            when(new NewArrival(Guid.NewGuid(), _events.Count + 1, origin, originId, location, status, type, name, sex, breed, breedSecondary));
        }

        private void when(NewArrival e)
        {
            this.Id = e.PetId;
            this.Origin = e.Origin;
            this.OriginId = e.OriginId;
            this.Location = e.Location;
            this.Status = e.Status;
            this.Type = e.Type;
            this.Name = e.Name;
            this.Sex = e.Sex;
            this.Breed = e.Breed;
            this.BreedSecondary = e.BreedSecondary;
            this._events.Add(e);
        }

        private void when(AgeUpdated e)
        {
            this.Age = e.Age;
            this._events.Add(e);
        }

        private void when(BreedUpdated e)
        {
            this.Breed = e.Breed;
            this.BreedSecondary = e.BreedSecondary;
            this._events.Add(e);
        }

        private void when(LifestyleUpdated e)
        {
            this.Lifestyle = e.Lifestyle;
            this._events.Add(e);
        }

        private void when(Moved e)
        {
            this.Location = e.Location;
            this._events.Add(e);
        }

        private void when(OtherAnimalsUpdated e)
        {
            this._otherAnimals.Clear();
            foreach (LocalizedString oa in e.OtherAnimals)
            {
                this._otherAnimals.Add(oa);
            }
            this._events.Add(e);
        }

        private void when(Renamed e)
        {
            this.Name = e.Name;
            this._events.Add(e);
        }

        private void when(SexCorrected e)
        {
            this.Sex = e.Sex;
            this._events.Add(e);
        }

        private void when(TypeCorrected e)
        {
            this.Type = e.Type;
            this._events.Add(e);
        }

        private void when(StatusChanged e)
        {
            this.Status = e.Status;
            this._events.Add(e);
        }

        private void when(TrainingUpdated e)
        {
            this.Training = e.Training;
            this._events.Add(e);
        }

        private void when(UrlChanged e)
        {
            this.Url = e.Url;
            this._events.Add(e);
        }
    }

    [DataContract]
    public class PetEvent
    {
        [DataMember]
        public Guid PetId { get; private set; }

        [DataMember]
        public int Version { get; private set; }

        [DataMember]
        public DateTime Timestamp { get; private set; }

        public PetEvent(Guid id, int version)
        {
            if (id == null) throw new ArgumentNullException("id");
            this.PetId = id;
            this.Version = version;
            DateTime temp = DateTime.Now;
            this.Timestamp = temp.AddTicks(-(temp.Ticks % TimeSpan.TicksPerSecond));
        }
    }

    [DataContract]
    public class NewArrival : PetEvent
    {
        [DataMember]
        public string Origin { get; private set; }

        [DataMember]
        public string OriginId { get; private set; }

        [DataMember]
        public LocalizedString Location { get; private set; }

        [DataMember]
        public LocalizedString Status { get; private set; }

        [DataMember]
        public LocalizedString Type { get; private set; }

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public LocalizedString Sex { get; private set; }

        [DataMember]
        public string Breed { get; private set; }

        [DataMember]
        public string BreedSecondary { get; private set; }

        public NewArrival(Guid id, int version,
            string origin, string originId,
            LocalizedString location, LocalizedString status,
            LocalizedString type, string name, LocalizedString sex,
            string breed, string breedSecondary) : base(id, version)
        {
            this.Origin = origin;
            this.OriginId = originId;
            this.Location = location;
            this.Status = status;
            this.Type = type;
            this.Name = name;
            this.Sex = sex;
            this.Breed = breed;
            this.BreedSecondary = breedSecondary;
        }
    }

    [DataContract]
    public class StatusChanged : PetEvent
    {
        [DataMember]
        public LocalizedString Status { get; private set; }

        public StatusChanged(Guid id, int version, LocalizedString status) : base(id, version)
        {
            this.Status = status;
        }
    }

    [DataContract]
    public class Renamed : PetEvent
    {
        [DataMember]
        public string Name { get; private set; }

        public Renamed(Guid id, int version, string name) : base(id, version)
        {
            this.Name = name;
        }
    }

    [DataContract]
    public class Moved : PetEvent
    {
        [DataMember]
        public LocalizedString Location { get; private set; }

        public Moved(Guid id, int version, LocalizedString location) : base(id, version)
        {
            this.Location = location;
        }
    }

    [DataContract]
    public class BreedUpdated : PetEvent
    {
        [DataMember]
        public string Breed { get; private set; }

        [DataMember]
        public string BreedSecondary { get; private set; }

        public BreedUpdated(Guid id, int version, string breed, string breedSecondary) : base(id, version)
        {
            this.Breed = breed;
            this.BreedSecondary = breedSecondary;
        }
    }

    [DataContract]
    public class SexCorrected : PetEvent
    {
        [DataMember]
        public LocalizedString Sex { get; private set; }

        public SexCorrected(Guid id, int version, LocalizedString sex) : base(id, version)
        {
            this.Sex = sex;
        }
    }

    [DataContract]
    public class TypeCorrected : PetEvent
    {
        [DataMember]
        public LocalizedString Type { get; private set; }

        public TypeCorrected(Guid id, int version, LocalizedString type) : base(id, version)
        {
            this.Type = type;
        }
    }

    [DataContract]
    public class AgeUpdated : PetEvent
    {
        [DataMember]
        public string Age { get; private set; }

        public AgeUpdated(Guid id, int version, string age) : base(id, version)
        {
            this.Age = age;
        }
    }

    [DataContract]
    public class LifestyleUpdated : PetEvent
    {
        [DataMember]
        public LocalizedString Lifestyle { get; private set; }

        public LifestyleUpdated(Guid id, int version, LocalizedString lifestyle) : base(id, version)
        {
            this.Lifestyle = lifestyle;
        }
    }

    [DataContract]
    public class TrainingUpdated : PetEvent
    {
        [DataMember]
        public LocalizedString Training { get; private set; }

        public TrainingUpdated(Guid id, int version, LocalizedString training) : base(id, version)
        {
            this.Training = training;
        }
    }

    [DataContract]
    public class OtherAnimalsUpdated : PetEvent
    {
        [DataMember]
        public IEnumerable<LocalizedString> OtherAnimals { get; private set; }

        public OtherAnimalsUpdated(Guid id, int version, IEnumerable<LocalizedString> otherAnimals) : base(id, version)
        {
            this.OtherAnimals = otherAnimals.ToArray();
        }
    }

    [DataContract]
    public class UrlChanged : PetEvent
    {
        [DataMember]
        public string Url { get; private set; }

        public UrlChanged(Guid id, int version, string url) : base(id, version)
        {
            this.Url = url;
        }
    }

}
