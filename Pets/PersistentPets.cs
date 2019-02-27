using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Pets
{
    public class PersistentPets : IPets
    {
        private const string FILE_NAME = "pets.file";
        private const string TEMP_FILE_NAME = "pets.file.tmp";

        private MemoryPets store = new MemoryPets();

        public PersistentPets()
        {
            if (File.Exists(FILE_NAME))
            {
                Load();
            }
            else if (File.Exists(TEMP_FILE_NAME))
            {
                File.Move(TEMP_FILE_NAME, FILE_NAME);
                Load();
            }
        }

        public IEnumerable<Pet> GetAll()
        {
            return store.GetAll();
        }

        public Pet Get(Id id)
        {
            return store.Get(id);
        }

        private void Load()
        {
            using (FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read))
            using (BinaryReader r = new BinaryReader(fs, Encoding.UTF8))
            {
                uint pets = r.ReadUInt32();
                while (pets > 0)
                {
                    store.Add(Read(r));
                    pets--;
                }
            }
        }

        public void Add(Pet pet)
        {
            store.Add(pet);
        }

        public void Commit()
        {
            IEnumerable<Pet> all = store.GetAll();
            using (FileStream fs = new FileStream(TEMP_FILE_NAME, FileMode.Create))
            using (BinaryWriter w = new BinaryWriter(fs, Encoding.UTF8))
            {
                w.Write(all.Count());
                foreach (Pet p in all)
                {
                    Write(w, p);
                }
            }
            if (File.Exists(FILE_NAME))
            {
                try
                {
                    File.Delete(FILE_NAME);
                }
                catch (IOException)
                {
                    // Ignore
                }
            }
            File.Move(TEMP_FILE_NAME, FILE_NAME);
        }

        private Pet Read(BinaryReader reader)
        {
            Pet p = new Pet(reader.ReadString(), reader.ReadString());
            p.Location = reader.ReadString();
            p.Type = reader.ReadString();
            p.Status = (Status)reader.ReadInt32();
            p.Name = reader.ReadString();
            p.Sex = (Sex)reader.ReadInt32();
            p.Breed = reader.ReadString();
            p.BreedSecondary = reader.ReadString();
            p.Age = reader.ReadString();
            p.Lifestyle = reader.ReadString();
            p.Training = reader.ReadString();

            uint otherAnimalCount = reader.ReadUInt32();
            while (otherAnimalCount > 0)
            {
                p.AddOtherAnimal(reader.ReadString());
                otherAnimalCount--;
            }

            p.LastChanged = DateTime.FromBinary(reader.ReadInt64());
            return p;
        }

        private void Write(BinaryWriter write, Pet p)
        {
            write.Write(p.Id.Origin);
            write.Write(p.Id.OriginId);
            write.Write(p.Location);
            write.Write(p.Type);
            write.Write((int)p.Status);
            write.Write(p.Name);
            write.Write((int)p.Sex);
            write.Write(p.Breed);
            write.Write(p.BreedSecondary);
            write.Write(p.Age);
            write.Write(p.Lifestyle);
            write.Write(p.Training);

            write.Write(p.OtherAnimals.Count());
            foreach (string oa in p.OtherAnimals)
            {
                write.Write(oa);
            }

            write.Write(p.LastChanged.ToBinary());
        }
    }
}
