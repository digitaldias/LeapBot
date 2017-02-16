using LEAPBot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LEAPBot.Domain.Contracts
{
    public interface ILeapRestClient
    {
        Task<IEnumerable<MasterClass>> GetMasterClasses();
        Task <IEnumerable<Speaker>> GetMasterClassSpeakers(int masterClassNumber);
        Task <IEnumerable<Speaker>> GetAllSpeakers();
        Task<Speaker> GetSpeakerByPartialName(string speakerName);
    }
}
