using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pets
{
    public class MemoryPets : IPets
    {
        private readonly IDictionary<Id, Pet> pets = new ConcurrentDictionary<Id, Pet>();

        public void Add(Pet pet)
        {
            pets.Add(pet.Id, pet);
        }

        public void Commit()
        {

        }

        public IEnumerable<Pet> GetAll()
        {
            return pets.Values;
        }

        public Pet Get(Id id)
        {
            Pet pet;
            bool success = pets.TryGetValue(id, out pet);
            return success ? pet : null;
        }
    }
}
