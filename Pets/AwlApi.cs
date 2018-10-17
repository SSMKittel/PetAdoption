using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Pets
{
    public class AwlService
    {
        public const string ORIGIN = "AWL";

        private static readonly LocalizedString GONE = new LocalizedString("awl_status_gone");

        private readonly IPetRepository petRepository;
        private readonly HttpClient client = new HttpClient();
        private readonly Regex getToken = new Regex("\"token\":\"([^'\"]+)\"", RegexOptions.Singleline);

        public AwlService(IPetRepository repo)
        {
            if (repo == null) throw new ArgumentNullException();
            this.petRepository = repo;
            client.Timeout = TimeSpan.FromSeconds(60);
            client.BaseAddress = new Uri("https://awl.org.au/");
        }

        private async Task<AwlApi> fetch()
        {
            AwlApi first = await fetchOnce();
            if (first.message != null && first.message != "")
            {
                string token = await client.GetStringAsync("adopt");
                token = getToken.Match(token).Groups[1].Value;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                return await fetchOnce();
            }

            return first;
        }

        private async Task<AwlApi> fetchOnce()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/pets");
            request.Content = new StringContent("type=all&lifestyle=all&size=all&age=all&gender=all", Encoding.UTF8);
            HttpResponseMessage response = await client.SendAsync(request);
            string data = await response.Content.ReadAsStringAsync();

            return AwlApi.parse(data);
        }

        public async Task Update()
        {
            AwlApi api = await fetch();
            updateAwlPets(api);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        private void updateAwlPets(AwlApi api)
        {
            HashSet<string> found = new HashSet<string>();
            foreach (AwlPet p in api.data)
            {
                found.Add(p.unique_id);

                Pet pet = petRepository.Get(ORIGIN, p.unique_id);
                if (pet == null)
                {
                    pet = p.create();
                    petRepository.Add(pet);
                }
                else
                {
                    p.update(pet);
                }
            }

            foreach (Pet pet in petRepository.Get(ORIGIN).Where(p => !found.Contains(p.OriginId)))
            {
                pet.ChangeStatus(GONE);
            }
        }
    }

    [DataContract]
    public class AwlTrait
    {
        [DataMember(IsRequired = false)] public string other_cats { get; set; }
        [DataMember(IsRequired = false)] public string other_animals { get; set; }
        [DataMember(IsRequired = false)] public string children { get; set; }
        [DataMember(IsRequired = false)] public string other_dogs { get; set; }
        [DataMember(IsRequired = false)] public string other_rabbits { get; set; }
    }

    [DataContract]
    public class AwlPet
    {
        [DataMember] public string unique_id { get; set; }
        [DataMember] public string type { get; set; }
        [DataMember] public string status { get; set; }
        [DataMember] public string name { get; set; }
        [DataMember(IsRequired = false)] public string summary { get; set; }
        [DataMember(IsRequired = false)] public string notes { get; set; }
        [DataMember(IsRequired = false)] public string features { get; set; }
        [DataMember] public string location { get; set; }
        [DataMember] public string price { get; set; }
        [DataMember(IsRequired = false)] public string breed { get; set; }
        [DataMember(IsRequired = false)] public string breed_secondary { get; set; }
        [DataMember] public string gender { get; set; }
        [DataMember(IsRequired = false)] public string colour { get; set; }
        [DataMember(IsRequired = false)] public string colour_secondary { get; set; }
        [DataMember(IsRequired = false)] public string coat { get; set; }
        [DataMember(IsRequired = false)] public string age { get; set; }
        [DataMember(IsRequired = false)] public string age_actual { get; set; }
        [DataMember(IsRequired = false)] public string size { get; set; }
        [DataMember(IsRequired = false)] public string lifestyle { get; set; }
        [DataMember(IsRequired = false)] public string training { get; set; }
        [DataMember(IsRequired = false)] public AwlTrait traits { get; set; }
        [DataMember(IsRequired = false)] public string microchipped { get; set; }
        [DataMember(IsRequired = false)] public string microchip_number { get; set; }
        [DataMember(IsRequired = false)] public string microchipped_tattoo { get; set; }
        [DataMember(IsRequired = false)] public string desexed { get; set; }
        [DataMember(IsRequired = false)] public string desexed_tattoo { get; set; }
        [DataMember(IsRequired = false)] public string image { get; set; }
        [DataMember(IsRequired = false)] public List<string> images { get; set; }
        [DataMember(IsRequired = false)] public string url { get; set; }

        public Pet create()
        {
            Pet p = new Pet(AwlService.ORIGIN, unique_id, loc("location", location), loc("status", status), loc("type", type), name, loc("sex", gender), breed, breed_secondary);
            p.ChangeAge(age_actual);
            p.ChangeLifestyle(loc("lifestyle", lifestyle));
            p.ChangeTraining(loc("training", training));
            p.ChangeUrl(url);
            p.ChangeOtherAnimals(animalHandling());
            return p;
        }

        public void update(Pet pet)
        {
            pet.Rename(name);
            pet.CorrectType(loc("type", type));
            pet.CorrectSex(loc("sex", gender));
            pet.CorrectBreed(breed, breed_secondary);
            pet.ChangeAge(age_actual);
            pet.ChangeLifestyle(loc("lifestyle", lifestyle));
            pet.ChangeTraining(loc("training", training));
            pet.ChangeUrl(url);
            pet.ChangeOtherAnimals(animalHandling());
            pet.ChangeStatus(loc("status", status));
        }

        private List<LocalizedString> animalHandling()
        {
            List<LocalizedString> animals = new List<LocalizedString>();
            if (traits != null)
            {
                if (traits.other_cats != null && traits.other_cats != string.Empty)
                {
                    animals.Add(loc("other_cats", traits.other_cats));
                }
                if (traits.other_dogs != null && traits.other_dogs != string.Empty)
                {
                    animals.Add(loc("other_dogs", traits.other_dogs));
                }
                if (traits.other_rabbits != null && traits.other_rabbits != string.Empty)
                {
                    animals.Add(loc("other_rabbits", traits.other_rabbits));
                }
                if (traits.other_animals != null && traits.other_animals != string.Empty)
                {
                    animals.Add(loc("other_animals", traits.other_animals));
                }
            }
            return animals;
        }

        private static LocalizedString loc(string type, string value)
        {
            if (value == null || value == string.Empty) return new LocalizedString("awl_" + type + "_null");
            else return new LocalizedString("awl_" + type + "_" + value);
        }
    }

    [DataContract]
    public class AwlApi
    {
        public static AwlApi parse(string data)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                return parse(ms);
            }
        }
        public static AwlApi parse(Stream stream)
        {
            DataContractJsonSerializer sr = new DataContractJsonSerializer(typeof(AwlApi));
            return (AwlApi) sr.ReadObject(stream);
        }

        [DataMember(IsRequired = false)] public string message { get; set; }
        [DataMember(IsRequired = false)] public List<AwlPet> data { get; set; }
    }

}
