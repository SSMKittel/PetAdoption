using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pets
{
    public class MemoryPetRepository : IPetRepository
    {
        private readonly IDictionary<MprKey, Pet> _pets;

        public MemoryPetRepository()
        {
            _pets = new ConcurrentDictionary<MprKey, Pet>();
        }

        public void Add(Pet pet)
        {
            MprKey key = new MprKey() { Origin = pet.Origin, OriginId = pet.OriginId };
            _pets.Add(key, pet);
        }

        public IEnumerable<Pet> Get()
        {
            return _pets.Values;
        }

        public IEnumerable<Pet> Get(string origin)
        {
            return _pets.Values.Where(p => p.Origin == origin);
        }

        public Pet Get(string origin, string originId)
        {
            MprKey key = new MprKey() { Origin = origin, OriginId = originId };
            Pet pet;
            bool success = _pets.TryGetValue(key, out pet);
            return success ? pet : null;
        }

        public virtual void Commit()
        {

        }
    }

    struct MprKey
    {
        public string Origin { get; set; }
        public string OriginId { get; set; }
    }
}
