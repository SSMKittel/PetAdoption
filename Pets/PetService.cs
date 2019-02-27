using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pets
{
    public class PetService
    {
        private readonly IPets petRepository;
        private readonly AwlService awlService;
        private readonly RspcaService rspcaService;

        public PetService(IPets repo)
        {
            if (repo == null) throw new ArgumentNullException();
            this.petRepository = repo;
            this.awlService = new AwlService(repo);
            this.rspcaService = new RspcaService(repo);
        }

        public async Task Update()
        {
            try
            {
                Task awl = awlService.Update();
                Task rspca = rspcaService.Update();
                await awl;
                await rspca;
                petRepository.Commit();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                throw;
            }
        }
    }
}
