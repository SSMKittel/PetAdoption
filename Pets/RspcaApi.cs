using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pets
{
    public class RspcaService
    {
        public const string ORIGIN = "RSPCA";

        private const string LOCATION_ADELAIDE_ANIMAL_HOSPITAL = "149";
        private const string LOCATION_ALDINGA_VET_SERVICES = "160";
        private const string LOCATION_HILLS_VETERINARY_CENTRE = "161";
        private const string LOCATION_LONSDALE_SHELTER = "21";
        private const string LOCATION_MAIN_NORTH_ROAD_VET_SERVICE = "145";
        private const string LOCATION_PET_UNIVERSE_BROADVIEW = "126";
        private const string LOCATION_PETBARN_ELIZABETH = "186";
        private const string LOCATION_PETBARN_HENDON = "107";
        private const string LOCATION_PETBARN_HOLDEN_HILL = "178";
        private const string LOCATION_PETBARN_MELROSE_PARK = "198";
        private const string LOCATION_PETBARN_MILE_END = "108";
        private const string LOCATION_PETBARN_NOARLUNGA = "158";
        private const string LOCATION_PETBARN_NORWOOD = "187";
        private const string LOCATION_PETBARN_PROSPECT = "165";
        private const string LOCATION_PORT_LINCOLN_SHELTER = "23";
        private const string LOCATION_PROSPECT_ROAD_VET = "151";
        private const string LOCATION_SEMAPHORE_VET_CLINIC = "127";
        private const string LOCATION_WHYALLA_SHELTER = "22";

        private static readonly LocalizedString GONE = new LocalizedString("rspca_status_gone");

        private readonly IPetRepository petRepository;
        private readonly HttpClient client = new HttpClient();
        private readonly Regex getPets = new Regex(@"init_animals\s*=\s*(\[.*?\]);\s*var\s+init_recent_animals_listing", RegexOptions.Singleline);

        public RspcaService(IPetRepository repo)
        {
            if (repo == null) throw new ArgumentNullException();
            this.petRepository = repo;
            client.Timeout = TimeSpan.FromSeconds(60);
            client.BaseAddress = new Uri("https://www.adoptapet.com.au/");
        }

        private async Task<IEnumerable<RspcaPet>> fetch()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "search?animal_type=0&state=4&location=0&breed=0&colour=0&sex=0&name=");
            request.Content = new StringContent("animal_type=0&state=4&location=0&breed=0&colour=0&sex=0&name=", Encoding.UTF8);
            HttpResponseMessage response = await client.SendAsync(request);
            string data = await response.Content.ReadAsStringAsync();
            var match = getPets.Match(data);
            data = match.Groups[1].Value;
            return RspcaPet.Parse(data);
        }

        public async Task Update()
        {
            IEnumerable<RspcaPet> api = await fetch();
            updateRspcaPets(api);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        private void updateRspcaPets(IEnumerable<RspcaPet> api)
        {
            HashSet<string> found = new HashSet<string>();
            foreach (RspcaPet p in api.Where(shelterAllowed))
            {
                found.Add(p.api_id);

                Pet pet = petRepository.Get(ORIGIN, p.api_id);
                if (pet == null)
                {
                    pet = p.Create();
                    petRepository.Add(pet);
                }
                else
                {
                    p.Update(pet);
                }
            }

            foreach (Pet pet in petRepository.Get(ORIGIN).Where(p => !found.Contains(p.OriginId)))
            {
                pet.ChangeStatus(GONE);
            }
        }

        private static bool shelterAllowed(RspcaPet pet)
        {
            string location = pet.shelter;
            return location == LOCATION_ADELAIDE_ANIMAL_HOSPITAL
                || location == LOCATION_ALDINGA_VET_SERVICES
                || location == LOCATION_HILLS_VETERINARY_CENTRE
                || location == LOCATION_LONSDALE_SHELTER
                || location == LOCATION_MAIN_NORTH_ROAD_VET_SERVICE
                || location == LOCATION_PET_UNIVERSE_BROADVIEW
                || location == LOCATION_PETBARN_ELIZABETH
                || location == LOCATION_PETBARN_HENDON
                || location == LOCATION_PETBARN_HOLDEN_HILL
                || location == LOCATION_PETBARN_MELROSE_PARK
                || location == LOCATION_PETBARN_MILE_END
                || location == LOCATION_PETBARN_NOARLUNGA
                || location == LOCATION_PETBARN_NORWOOD
                || location == LOCATION_PETBARN_PROSPECT
              //|| location == LOCATION_PORT_LINCOLN_SHELTER
                || location == LOCATION_PROSPECT_ROAD_VET
                || location == LOCATION_SEMAPHORE_VET_CLINIC
              //|| location == LOCATION_WHYALLA_SHELTER
              ;
        }
    }
    
    [DataContract]
    public class RspcaType
    {
        [DataMember(IsRequired = false)] public string id { get; set; }
        [DataMember] public string api_id { get; set; }
        [DataMember(IsRequired = false)] public string type_title { get; set; }
        [DataMember(IsRequired = false)] public string isActive { get; set; }
        [DataMember(IsRequired = false)] public string created_at { get; set; }
        [DataMember(IsRequired = false)] public string updated_at { get; set; }
    }

    [DataContract]
    public class RspcaSize
    {
        [DataMember(IsRequired = false)] public string id { get; set; }
        [DataMember] public string api_id { get; set; }
        [DataMember(IsRequired = false)] public string size { get; set; }
        [DataMember(IsRequired = false)] public string isActive { get; set; }
        [DataMember(IsRequired = false)] public string created_at { get; set; }
        [DataMember(IsRequired = false)] public string updated_at { get; set; }
    }

    [DataContract]
    public class RspcaPhoto
    {
        [DataMember(IsRequired = false)] public string id { get; set; }
        [DataMember(IsRequired = false)] public string animal_id { get; set; }
        [DataMember] public string api_id { get; set; }
        [DataMember(IsRequired = false)] public string image_path { get; set; }
        [DataMember(IsRequired = false)] public string api_path { get; set; }
        [DataMember(IsRequired = false)] public string isDefault { get; set; }
        [DataMember(IsRequired = false)] public string isActive { get; set; }
        [DataMember(IsRequired = false)] public string isDownloaded { get; set; }
        [DataMember(IsRequired = false)] public string created_at { get; set; }
        [DataMember(IsRequired = false)] public string updated_at { get; set; }
    }

    [DataContract]
    public class RspcaState
    {
        [DataMember(IsRequired = false)] public string id { get; set; }
        [DataMember] public string api_id { get; set; }
        [DataMember(IsRequired = false)] public string name { get; set; }
        [DataMember(IsRequired = false)] public string donate_url { get; set; }
        [DataMember(IsRequired = false)] public string isActive { get; set; }
        [DataMember(IsRequired = false)] public string created_at { get; set; }
        [DataMember(IsRequired = false)] public string updated_at { get; set; }
    }

    [DataContract]
    public class RspcaColour
    {
        [DataMember(IsRequired = false)] public string id { get; set; }
        [DataMember] public string api_id { get; set; }
        [DataMember(IsRequired = false)] public string colour { get; set; }
        [DataMember(IsRequired = false)] public string isActive { get; set; }
        [DataMember(IsRequired = false)] public string created_at { get; set; }
        [DataMember(IsRequired = false)] public string updated_at { get; set; }
    }

    [DataContract]
    public class RspcaPet
    {
        [DataMember(IsRequired = false)] public string id { get; set; }
        [DataMember] public string api_id { get; set; }
        [DataMember(IsRequired = false)] public string shelterBuddyId { get; set; }
        [DataMember] public string adoptionCost { get; set; }
        [DataMember(IsRequired = false)] public string description1 { get; set; }
        [DataMember(IsRequired = false)] public string description2 { get; set; }
        [DataMember(IsRequired = false)] public string date_of_birth { get; set; }
        [DataMember(IsRequired = false)] public string readable_age { get; set; }
        [DataMember(IsRequired = false)] public string ageMonths { get; set; }
        [DataMember(IsRequired = false)] public string ageYears { get; set; }
        [DataMember(IsRequired = false)] public string isCrossBreed { get; set; }
        [DataMember(IsRequired = false)] public string breedPrimaryId { get; set; }
        [DataMember(IsRequired = false)] public string breedPrimary { get; set; }
        [DataMember(IsRequired = false)] public string breedSecondaryId { get; set; }
        [DataMember(IsRequired = false)] public string breedSecondary { get; set; }
        [DataMember(IsRequired = false)] public string isDesexed { get; set; }
        [DataMember(IsRequired = false)] public string colourPrimaryId { get; set; }
        [DataMember(IsRequired = false)] public string colourSecondaryId { get; set; }
        [DataMember(IsRequired = false)] public string colour_url { get; set; }
        [DataMember(IsRequired = false)] public string name { get; set; }
        [DataMember(IsRequired = false)] public string hadBehaviourEvaluated { get; set; }
        [DataMember(IsRequired = false)] public string hadHealthChecked { get; set; }
        [DataMember(IsRequired = false)] public string isVaccinated { get; set; }
        [DataMember(IsRequired = false)] public string isWormed { get; set; }
        [DataMember(IsRequired = false)] public string isSpecialNeedsOkay { get; set; }
        [DataMember(IsRequired = false)] public string isLongtermResident { get; set; }
        [DataMember(IsRequired = false)] public string isSeniorPet { get; set; }
        [DataMember(IsRequired = false)] public string isMicrochipped { get; set; }
        [DataMember] public string shelter { get; set; }
        [DataMember(IsRequired = false)] public string search_type_id { get; set; }
        [DataMember(IsRequired = false)] public string search_type { get; set; }
        [DataMember] public string sex { get; set; }
        [DataMember(IsRequired = false)] public RspcaSize size { get; set; }
        [DataMember(IsRequired = false)] public string youTubeVideo { get; set; }
        [DataMember] public string state_id { get; set; }
        [DataMember] public string animal_status { get; set; }
        [DataMember] public string animal_type { get; set; }
        [DataMember(IsRequired = false)] public string public_url { get; set; }
        [DataMember(IsRequired = false)] public string isActive { get; set; }
        [DataMember(IsRequired = false)] public string created_at { get; set; }
        [DataMember(IsRequired = false)] public string updated_at { get; set; }
        [DataMember(IsRequired = false)] public List<RspcaPhoto> photo { get; set; }
        [DataMember(IsRequired = false)] public RspcaState state { get; set; }
        [DataMember(IsRequired = false)] public RspcaColour primary_colour { get; set; }
        [DataMember(IsRequired = false)] public RspcaColour secondary_colour { get; set; }
        [DataMember(IsRequired = false)] public RspcaType type { get; set; }

        public Pet Create()
        {
            if (name == null || name == string.Empty)
            {
                Trace.WriteLine(this);
                Trace.WriteLine(this.public_url);
            }
            Pet p = new Pet(RspcaService.ORIGIN, api_id, loc("location", shelter), loc("status", animal_status), loc("type", animal_type), toUnknown(name), loc("sex", sex), breedPrimary, breedSecondary);
            p.ChangeAge(formattedAge());
            p.ChangeLifestyle(loc("lifestyle", null));
            p.ChangeTraining(loc("training", null));
            p.ChangeUrl(public_url);
            p.ChangeOtherAnimals(Enumerable.Empty<LocalizedString>());
            return p;
        }

        private static string toUnknown(string value)
        {
            if (value == null || value == "") return "Unknown";
            return value;
        }
        
        public void Update(Pet pet)
        {
            pet.Rename(toUnknown(name));
            pet.CorrectSex(loc("sex", sex));
            pet.CorrectBreed(breedPrimary, breedSecondary);
            pet.ChangeAge(formattedAge());
            pet.ChangeLifestyle(loc("lifestyle", null));
            pet.ChangeTraining(loc("training", null));
            pet.ChangeUrl(public_url);
            pet.ChangeOtherAnimals(Enumerable.Empty<LocalizedString>());
            pet.ChangeStatus(loc("status", animal_status));
        }

        private string formattedAge()
        {
            bool hasYear = !(ageYears == null || ageYears == "" || ageYears == "0");
            bool hasMonth = !(ageMonths == null || ageMonths == "" || ageMonths == "0");
            if (hasYear && hasMonth)
            {
                if (ageYears == "1" && ageMonths == "1")
                {
                    return ageYears + " year, " + ageMonths + " month";
                }
                else if (ageYears == "1")
                {
                    return ageYears + " year, " + ageMonths + " months";
                }
                else if (ageMonths == "1")
                {
                    return ageYears + " years, " + ageMonths + " month";
                }
                return ageYears + " years, " + ageMonths + " months";
            }
            else if (hasYear)
            {
                if (ageYears == "1")
                {
                    return ageYears + " year";
                }
                return ageYears + " years";
            }
            else if (hasMonth)
            {
                if (ageMonths == "1")
                {
                    return ageMonths + " month";
                }
                return ageMonths + " months";
            }
            else
            {
                return "Unknown";
            }
        }

        private static LocalizedString loc(string type, string value)
        {
            if (value == null || value == string.Empty) return new LocalizedString("rspca_" + type + "_null");
            else return new LocalizedString("rspca_" + type + "_" + value);
        }

        public static IEnumerable<RspcaPet> Parse(string data)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                return Parse(ms);
            }
        }
        public static IEnumerable<RspcaPet> Parse(Stream stream)
        {
            DataContractJsonSerializer sr = new DataContractJsonSerializer(typeof(List<RspcaPet>));
            return (IEnumerable<RspcaPet>)sr.ReadObject(stream);
        }
    }
}
