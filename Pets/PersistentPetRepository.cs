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
    public class PersistentPetRepository : MemoryPetRepository
    {
        private const string FILE_NAME = "pets.json";
        private const string TEMP_FILE_NAME = "pets.json.tmp";
        private readonly DataContractJsonSerializer serializer;

        public PersistentPetRepository() : base()
        {
            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
            settings.EmitTypeInformation = EmitTypeInformation.AsNeeded;
            settings.KnownTypes = new[] {
                typeof(AgeUpdated),
                typeof(BreedUpdated),
                typeof(LifestyleUpdated),
                typeof(Moved),
                typeof(NewArrival),
                typeof(OtherAnimalsUpdated),
                typeof(PetEvent),
                typeof(Renamed),
                typeof(SexCorrected),
                typeof(TypeCorrected),
                typeof(StatusChanged),
                typeof(TrainingUpdated),
                typeof(UrlChanged)
            };
            this.serializer = new DataContractJsonSerializer(typeof(List<PetEvent>), settings);

            if (File.Exists(FILE_NAME))
            {
                load();
            }
            else if (File.Exists(TEMP_FILE_NAME))
            {
                File.Move(TEMP_FILE_NAME, FILE_NAME);
                load();
            }
        }

        private void load()
        {
            using (FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read))
            {
                foreach (IEnumerable<PetEvent> grp in parse(fs).GroupBy(x => x.PetId))
                {
                    Add(new Pet(grp));
                }
            }
        }

        private IEnumerable<PetEvent> parse(Stream stream)
        {
            return (IEnumerable<PetEvent>)serializer.ReadObject(stream);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public override void Commit()
        {
            PetEvent[] events = Get().SelectMany(x => x.Events).ToArray();
            write(events);
        }

        private void write(PetEvent[] data)
        {
            using (FileStream fs = new FileStream(TEMP_FILE_NAME, FileMode.Create))
            {
                serializer.WriteObject(fs, data);
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
    }
}
